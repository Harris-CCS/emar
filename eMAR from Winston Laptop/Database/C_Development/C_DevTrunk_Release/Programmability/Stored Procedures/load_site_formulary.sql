create procedure [dbo].[load_site_formulary]
as
begin

    set nocount on;

	-- Ensure that both temp tables do not exist.
	drop table if exists [#medication_items];
    drop table if exists [#site_formulary];

    create table [#site_formulary]
    (
        [target_id]          [bigint]        null
        , [source_id]          [varchar](25)   null
        , [site]               [varchar](25)   null
        , [ndc]                [varchar](32)   null
        , [drug_id]            [varchar](32)   null
        , [brand_name]         [nvarchar](255) null
        , [hospital_drug_code] [varchar](32)   null
        , [service_code]       [varchar](32)   null
        , [is_inpatient]       [bit]           null
        , [is_outpatient]      [bit]           null
        , [is_pyxis]           [bit]           null
        , [dateadd]            [varchar](14)   null
        , [priority_pick]      [smallint]      null
        , [medication_id]      [int]           null default 0
        , [site_id]            [int]           null
        , [existing_record]    [bit]           null default 0
    );

	create table [#medication_items]
	(
		[medication_id] [int]           default 0
		, [site_id]       [int]           not null default -1
		, [ndc]           [varchar](32)   not null
		, [drug_id]       [varchar](32)   not null
		, [brand_name]    [nvarchar](255) not null
		, [match]         [nvarchar](255) null --- Added for testing / debugging
		, primary key clustered ([ndc] asc, [drug_id] asc, [brand_name] asc, [site_id] asc)
	);


	-- This call to export_ibex_site_formulary gets all rows from
	-- the formulary in PCED.  We need that to know which rows
	-- have been deleted in PCED and therefore need to be deleted
	-- from the formulary in eMAR.  After we've done that delete,
	-- we'll empty out #site_formulary and re-populate it with
	-- the data from a second external SP that gets only the
	-- differences between the PCED and eMAR formulary. That wey 
	-- we aren't updating everything in the eMAR formulary and
	-- firing off the update trigger for everything.
	-- All of the logic between this insert statement
	-- and the delete statement will ne duplicated.
	-- Winston Murdock, 07/16/2021.  EMAR-1014
    insert into [#site_formulary]
    (
        [source_id]
      , [site]
      , [ndc]
      , [drug_id]
      , [brand_name]
      , [hospital_drug_code]
      , [service_code]
      , [is_inpatient]
      , [is_outpatient]
      , [is_pyxis]
      , [dateadd]
    )
    execute ('execute dbo.export_ibex_site_formulary');


    if (
                 select
                     count(*)
                 from [#site_formulary]
        ) > 0
        begin
            --get a distinct list of medications
            insert into [#medication_items]
            (
                [ndc]
              , [drug_id]
              , [brand_name]
            )
            select distinct
                isnull([ndc], '')
              , isnull([drug_id], '')
              , isnull([brand_name], '')
            from [#site_formulary];

            --set medication id's
            execute [dbo].[update_medication_id_list];

            update [target] set
                [medication_id] = [source].[medication_id]
            from [#medication_items] [source]
                inner join [#site_formulary] [target]
                    on [source].[ndc] = [target].[ndc]
                        and [source].[brand_name] = [target].[brand_name]
                        and [source].[drug_id] = [target].[drug_id]
            where [source].[medication_id] > 0;

            -- get internal site_id
            update [source] set
                [site_id] = isnull([internal_site].[id], -1)
            from [#site_formulary] as [source]
                outer apply [dbo].[get_internal_id]
                ('pulsecheck', 'sites', [source].[site]) as [internal_site];

            -- ibex allows duplicate drugs (by ndc) in database
            -- pick priority record by date / then most recent id
            with cte_priority
                as (
                             select
                                 row_number() over (partition by [sq].[site_id]
                                 , [sq].[ndc]
                                 order by [sq].[site_id]
                                 , [sq].[dateadd] desc
                                 --padding varchar values with leading constant ensures a better sort
                                 , right('000000000000000000000'+[sq].[source_id],25) desc
                                 ) as [priority_pick]
                               , [sq].[source_id]
                               , [sq].[site_id]
                               , [sq].[ndc]
                               , [sq].[dateadd]
                             from [#site_formulary] as [sq]
                    )
            update [target] set
                [priority_pick] = [source].[priority_pick]
            from [cte_priority] [source]
                inner join [#site_formulary] [target]
                    on [source].[source_id] = [target].[source_id]
                        and [source].[ndc] = [target].[ndc]
                        and [source].[dateadd] = [target].[dateadd]
                        and [source].[site_id] = [target].[site_id];

            /****************************************
                    load temporary tables for staging
            ****************************************/

            /********************************
             synchornize internal / external id's
            ********************************/
            -- uniqueness by site / medication_id
            update [source] set
                [target_id]       = [target].[id]
              , [existing_record] = 1
            from [#site_formulary] as [source]
                inner join [dbo].[site_formulary] [target]
                    on [source].[site_id] = [target].[site_id]
                        and [source].[ndc] = [target].[ndc];

            --- Generate New id's for insert records
            with cte_data
                as (
                             select
                                 ident_current('[dbo].[site_formulary]') +
                                 row_number() over (order by [source].[source_id]) new_id
                               , [source].[target_id]
                             from [#site_formulary] as [source]
                                 left join [dbo].[site_formulary] [target]
                                     on [source].[site_id] = [target].[site_id]
                                         and [source].[ndc] = [target].[ndc]
                             where [target].[id] is null
                                   and [source].[medication_id] > 0
                    )
            update [data] set
                [target_id] = [data].[new_id]
            from cte_data [data];

            -- delete any bad records
            delete [source]
            from [#site_formulary] as [source]
            where [medication_id] <= 0
                or [priority_pick] <> 1
                or [site_id] <= 0;

            /*************************************
                    begin loading permanent tables
                    delete only
					insert / update are handled below
            *************************************/

			-- Delete any rows in the eMAR formulary that are not in the PCED formulary.
            delete [target]
            from [#site_formulary] as [source]
                right join [dbo].[site_formulary] [target]
                    on [source].[site_id] = [target].[site_id]
                        and [source].[ndc] = [target].[ndc]
            where [source].[target_id] is null;

        /***************************************
                loading [external_ids] reference
        ***************************************/
        /****************
                end table
        ****************/
    end;

	-- Now that we've deleted anything from the eMAR formulary
	-- that had been deleted from the PCED formulary, handle
	-- the inserts and updates.
		
	-- Empty out the tables.
	-- Delete everything from the temp tables.
	-- We go ahead and create them at the top, so we know they will exist.
	DELETE FROM #site_formulary
	DELETE FROM #medication_items

	-- Populate the list of differences between the PCED formulary
	-- and the eMAR formulary.
	insert into [#site_formulary]
    (
        [source_id]
      , [site]
      , [ndc]
      , [drug_id]
      , [brand_name]
      , [hospital_drug_code]
      , [service_code]
      , [is_inpatient]
      , [is_outpatient]
      , [is_pyxis]
      , [dateadd]
    )
    execute ('execute dbo.export_ibex_site_formulary_differences');


	if (
                select
                    count(*)
                from [#site_formulary]
    ) > 0
    begin
        --get a distinct list of medications
        insert into [#medication_items]
        (
            [ndc]
            , [drug_id]
            , [brand_name]
        )
        select distinct
            isnull([ndc], '')
            , isnull([drug_id], '')
            , isnull([brand_name], '')
        from [#site_formulary];

        --set medication id's
        execute [dbo].[update_medication_id_list];

        update [target] set
            [medication_id] = [source].[medication_id]
        from [#medication_items] [source]
            inner join [#site_formulary] [target]
                on [source].[ndc] = [target].[ndc]
                    and [source].[brand_name] = [target].[brand_name]
                    and [source].[drug_id] = [target].[drug_id]
        where [source].[medication_id] > 0;

        -- get internal site_id
        update [source] set
            [site_id] = isnull([internal_site].[id], -1)
        from [#site_formulary] as [source]
            outer apply [dbo].[get_internal_id]
            ('pulsecheck', 'sites', [source].[site]) as [internal_site];

        -- ibex allows duplicate drugs (by ndc) in database
        -- pick priority record by date / then most recent id
        with cte_priority
            as (
                            select
                                row_number() over (partition by [sq].[site_id]
                                , [sq].[ndc]
                                order by [sq].[site_id]
                                , [sq].[dateadd] desc
                                --padding varchar values with leading constant ensures a better sort
                                , right('000000000000000000000'+[sq].[source_id],25) desc
                                ) as [priority_pick]
                            , [sq].[source_id]
                            , [sq].[site_id]
                            , [sq].[ndc]
                            , [sq].[dateadd]
                            from [#site_formulary] as [sq]
                )
        update [target] set
            [priority_pick] = [source].[priority_pick]
        from [cte_priority] [source]
            inner join [#site_formulary] [target]
                on [source].[source_id] = [target].[source_id]
                    and [source].[ndc] = [target].[ndc]
                    and [source].[dateadd] = [target].[dateadd]
                    and [source].[site_id] = [target].[site_id];

        /****************************************
                load temporary tables for staging
        ****************************************/

        /********************************
            synchornize internal / external id's
        ********************************/
        -- uniqueness by site / medication_id
        update [source] set
            [target_id]       = [target].[id]
            , [existing_record] = 1
        from [#site_formulary] as [source]
            inner join [dbo].[site_formulary] [target]
                on [source].[site_id] = [target].[site_id]
                    and [source].[ndc] = [target].[ndc];

        --- Generate New id's for insert records
        with cte_data
            as (
                            select
                                ident_current('[dbo].[site_formulary]') +
                                row_number() over (order by [source].[source_id]) new_id
                            , [source].[target_id]
                            from [#site_formulary] as [source]
                                left join [dbo].[site_formulary] [target]
                                    on [source].[site_id] = [target].[site_id]
                                        and [source].[ndc] = [target].[ndc]
                            where [target].[id] is null
                                and [source].[medication_id] > 0
                )
        update [data] set
            [target_id] = [data].[new_id]
        from cte_data [data];

        -- delete any bad records
        delete [source]
        from [#site_formulary] as [source]
        where [medication_id] <= 0
            or [priority_pick] <> 1
            or [site_id] <= 0;

        /*************************************
                begin loading permanent tables
                delete was handled above
				update / insert
        *************************************/
		--Before doing the update/insert logic
        update [target] set
            [site_id]            = [source].[site_id]
            , [hospital_drug_code] = [source].[hospital_drug_code]
            , [service_code]       = [source].[service_code]
            , [is_inpatient]       = [source].[is_inpatient]
            , [is_outpatient]      = [source].[is_outpatient]
            , [is_pyxis]           = [source].[is_pyxis]
            , [medication_id]      = [source].[medication_id]
            , [ndc]                = [source].[ndc]
        from [#site_formulary] as [source]
            inner join [dbo].[site_formulary] [target]
                on [source].[site_id] = [target].[site_id]
                    and [source].[ndc] = [target].[ndc];

        set identity_insert [dbo].[site_formulary] on;

        insert into [dbo].[site_formulary]
        (
            [id]
            , [site_id]
            , [hospital_drug_code]
            , [service_code]
            , [is_inpatient]
            , [is_outpatient]
            , [is_pyxis]
            , [medication_id]
            , [ndc]
        )
        select
            [target_id]
            , [site_id]
            , [hospital_drug_code]
            , [service_code]
            , [is_inpatient]
            , [is_outpatient]
            , [is_pyxis]
            , [medication_id]
            , [ndc]
        from [#site_formulary] as [source]
        where [target_id] is not null
                and [existing_record] = 0;

        set identity_insert [dbo].[site_formulary] off;

    /***************************************
            loading [external_ids] reference
    ***************************************/
    /****************
            end table
    ****************/

    end;

	-- Drop both temp tables.
    drop table if exists [#medication_items];
    drop table if exists [#site_formulary];


end;

go