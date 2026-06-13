create procedure [dbo].[load_site_formulary_match]
as
begin

    set nocount on;

    print 'Loading Table: site_formulary_match';

    drop table if exists [#site_formulary_match];

    create table [#site_formulary_match]
        (
            [target_id]        [bigint]        null
          , [source_id]        [varchar](25)   null
          , [site]             [varchar](25)   null
          , [ndc]              [varchar](32)   null
          , [drug_id]          [varchar](32)   null
          , [brand_name]       [nvarchar](255) null
          , [inpatient_match]  [tinyint]       null
          , [outpatient_match] [tinyint]       null
          , [pyxis_match]      [tinyint]       null
          , [priority_pick]    [smallint]      null
          , [medication_id]    [int]           null default 0
          , [site_id]          [int]           null
          , [existing_record]  [bit]           null default 0
        );


    insert into [#site_formulary_match]
    (
        [source_id]
      , [site]
      , [ndc]
      , [drug_id]
      , [brand_name]
      , [inpatient_match]
      , [outpatient_match]
      , [pyxis_match]
    )
    execute ('execute dbo.export_ibex_site_formulary_match');

    if (
                 select
                     count(*)
                 from [#site_formulary_match]
        ) > 0
        begin

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
            from [#site_formulary_match];

            --set medication id's
            execute [dbo].[update_medication_id_list];

            update [target] set
                [medication_id] = [source].[medication_id]
            from [#medication_items] [source]
                inner join [#site_formulary_match] [target]
                    on [source].[ndc] = [target].[ndc]
                        and [source].[brand_name] = [target].[brand_name]
                        and [source].[drug_id] = [target].[drug_id]
            where [source].[medication_id] > 0;

            -- get internal site_id
            update [source] set
                [site_id] = isnull([internal_site].[id], -1)
            from [#site_formulary_match] as [source]
                outer apply [dbo].[get_internal_id]
                ('pulsecheck', 'sites', [source].[site]) as [internal_site];

            -- ibex allows duplicate drugs (by ndc) in database
            -- pick priority record by date / then most recent id
            with cte_priority
                as (
                             select
                                 row_number() over (partition by [sq].[site_id]
                                 , [sq].[medication_id]
                                 order by [sq].[site_id]
                                 --padding varchar values with leading constant ensures a better sort
                                 , right('000000000000000000000' + [sq].[source_id], 25) desc
                                 ) as [priority_pick]
                               , [sq].[source_id]
                               , [sq].[site_id]
                               , [sq].[medication_id]
                             from [#site_formulary_match] as [sq]
                    )
            update [target] set
                [priority_pick] = [source].[priority_pick]
            from [cte_priority] [source]
                inner join [#site_formulary_match] [target]
                    on [source].[source_id] = [target].[source_id]
                        and [source].[medication_id] = [target].[medication_id]
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
            from [#site_formulary_match] as [source]
                inner join [dbo].[site_formulary_match] [target]
                    on [source].[site_id] = [target].[site_id]
                        and [source].[medication_id] = [target].[medication_id];

            --- Generate New id's for insert records
            with cte_data
                as (
                             select
                                 ident_current('[dbo].[site_formulary_match]') +
                                 row_number() over (order by [source].[source_id]) new_id
                               , [source].[target_id]
                             from [#site_formulary_match] as [source]
                                 left join [dbo].[site_formulary_match] [target]
                                     on [source].[site_id] = [target].[site_id]
                                         and [source].[medication_id] = [target].[medication_id]
                             where [target].[id] is null
                                   and [source].[medication_id] > 0
                    )
            update [data] set
                [target_id] = [data].[new_id]
            from cte_data [data];

            -- delete any bad records
            delete [source]
            from [#site_formulary_match] as [source]
            where [medication_id] <= 0
                or [priority_pick] <> 1
                or [site_id] <= 0;

            /*************************************
                    begin loading permanent tables
                    delete / update / insert
            *************************************/
            delete [target]
            from [#site_formulary_match] as [source]
                right join [dbo].[site_formulary_match] [target]
                    on [source].[site_id] = [target].[site_id]
                        and [source].[medication_id] = [target].[medication_id]
            where [source].[target_id] is null;

            update [target] set
                [site_id]          = [source].[site_id]
              , [inpatient_match]  = [source].[inpatient_match]
              , [outpatient_match] = [source].[outpatient_match]
              , [pyxis_match]      = [source].[pyxis_match]
              , [medication_id]    = [source].[medication_id]
            from [#site_formulary_match] as [source]
                inner join [dbo].[site_formulary_match] [target]
                    on [source].[site_id] = [target].[site_id]
                        and [source].[medication_id] = [target].[medication_id];

            set identity_insert [dbo].[site_formulary_match] on;

            insert into [dbo].[site_formulary_match]
            (
                [id]
              , [site_id]
              , [inpatient_match]
              , [outpatient_match]
              , [pyxis_match]
              , [medication_id]
            )
            select
                [target_id]
              , [site_id]
              , [inpatient_match]
              , [outpatient_match]
              , [pyxis_match]
              , [medication_id]
            from [#site_formulary_match] as [source]
            where [target_id] is not null
                  and [existing_record] = 0;

            set identity_insert [dbo].[site_formulary_match] off;

            /***************************************
                    loading [external_ids] reference
            ***************************************/
            /****************
                    end table
            ****************/
        end;

    drop table if exists [#site_formulary_match];

end;
go
