create procedure [dbo].[load_medication_units]
as
begin

    set nocount on;

    print 'Loading Table: medication_units';

    drop table if exists [#medication_units];

    create table [#medication_units]
        (
            [target_id]       [int]         null
          , [source_id]       [varchar](25) null
          , [site]            [varchar](25) null
          , [code]            [varchar](50) null
          , [name]            [varchar](50) null
          , [print_name]      [varchar](50) null
          , [is_active]       [bit]         null
          , [misc2]           [varchar](50) null
          , [site_id]         [int]         null
          , [priority]        [int]         null
          , [existing_record] [bit]         default 0
        );

    insert into [#medication_units]
    (
        [source_id]
      , [site]
      , [code]
      , [name]
      , [print_name]
      , [is_active]
      , [misc2]
    )
    execute ('execute dbo.export_ibex_medication_units');

    if (
                 select
                     count(*)
                 from [#medication_units]
        ) > 0
        begin

            /****************************************
                    load temporary tables for staging
            ****************************************/

            -- transform remote data
            update [source] set
                [source_id] = [site] + '|' + [source_id]
              , [priority]  =
                    case
                        when isnumeric([misc2]) = 1 then cast([misc2] as [int])
                        else 0
                    end
            from [#medication_units] as [source];

            -- get internal site_id
            update [source] set
                [site_id] = isnull([internal_site].[id], -1)
            from [#medication_units] as [source]
                outer apply [dbo].[get_internal_id]
                ('pulsecheck', 'sites', [source].[site]) as [internal_site];

        /********************************
         synchornize internal / external id's
        ********************************/
        /*          PointOfView: [dbo].[external_ids] [ei]

                    source_id not null / target_id not null :: update in [TARGET]

                    ------------------------------------------
                    All other cases: Corrupt Record in [external_ids]
                    we want to generate a new [external_ids]
                    after inserting a new [TARGET] record

                    source_id     null / target_id not null :: delete in [dbo].[external_ids] delete in [TARGET]
                    source_id     null / target_id     null :: delete in [dbo].[external_ids]
                    source_id not null / target_id     null :: delete in [dbo].[external_ids]

        */

        begin try
            begin transaction;

            --- if emar unique index/constraint exists validate and create any missing [dbo].[external_ids] [ei]
            with cte_constraint_match
                as (
                             select
                                 [target].[id]        [internal_id]
                               , 'pulsecheck'         [vendor]
                               , 'medication_units'   [entity]
                               , [source].[source_id] [external_id]
                             from [#medication_units] as [source]
                                 inner join [dbo].[medication_units] [target]
                                     on [source].[site_id] = [target].[site_id]
                                         and [source].[code] = [target].[code]
                    )
            insert into [dbo].[external_ids]
            (
                [internal_id]
              , [vendor]
              , [entity]
              , [external_id]
            )
            select
                [cm].[internal_id]
              , [cm].[vendor]
              , [cm].[entity]
              , [cm].[external_id]
            from cte_constraint_match [cm]
                left join [dbo].[external_ids] [ei]
                    on [ei].[internal_id] = [cm].[internal_id]
                        and [ei].[vendor] = [cm].[vendor]
                        and [ei].[entity] = [cm].[entity]
                        and [ei].[external_id] = [cm].[external_id]
            where [ei].[internal_id] is null;

            --- Target Records for Delete
            delete [target]
            from [dbo].[external_ids] [ei]
                left join [#medication_units] as [source]
                    on [ei].[external_id] = [source].[source_id]
                left join [dbo].[medication_units] [target]
                    on [ei].[internal_id] = [target].[id]
            where [ei].[vendor] = 'pulsecheck'
                and [ei].[entity] = 'medication_units'
                and [source].[source_id] is null
                and [target].[id] is not null;

            --- Delete Bad External ID Records
            delete [ei]
            from [dbo].[external_ids] [ei]
                left join [#medication_units] as [source]
                    on [ei].[external_id] = [source].[source_id]
                left join [dbo].[medication_units] [target]
                    on [ei].[internal_id] = [target].[id]
            where [ei].[vendor] = 'pulsecheck'
                and [ei].[entity] = 'medication_units'
                and ([source].[source_id] is null
                    or [target].[id] is null);

            --- Match target_id's with external_ids table
            update [source] set
                [target_id]       = [internal_site].[id]
              , [existing_record] = 1
            from [#medication_units] as [source]
                cross apply [dbo].[get_internal_id]
                ('pulsecheck', 'medication_units', [source].[source_id]) as [internal_site];

            --- Update Matching Target Records
            update [target] set
                [site_id]    = [source].[site_id]
              , [code]       = [source].[code]
              , [name]       = [source].[name]
              , [print_name] = [source].[print_name]
              , [is_active]  = [source].[is_active]
              , [priority]   = [source].[priority]
            from [#medication_units] as [source]
                cross apply [dbo].[get_internal_id]('pulsecheck', 'medication_units', [source].[source_id]) [gii]
                inner join [dbo].[medication_units] [target]
                    on [gii].[id] = [target].[id];

            --- Generate New id's for insert records
            with cte_data
                as (
                             select
                                 ident_current('[dbo].[medication_units]') +
                                 row_number() over (order by [source].[source_id]) new_id
                               , [source].[target_id]
                             from [#medication_units] as [source]
                                 outer apply [dbo].[get_internal_id]('pulsecheck', 'medication_units', [source].[source_id]) [gii]
                                 left join [dbo].[medication_units] [target]
                                     on [gii].[id] = [target].[id]
                             where [target].[id] is null
                    )
            update [data] set
                [target_id] = [data].[new_id]
            from cte_data [data];

            /*************************************
                    begin loading permanent tables
            *************************************/

            set identity_insert [dbo].[medication_units] on;

            insert into [dbo].[medication_units]
            (
                [id]
              , [site_id]
              , [code]
              , [name]
              , [print_name]
              , [is_active]
              , [priority]
            )
            select
                [target_id]
              , [site_id]
              , [code]
              , [name]
              , [print_name]
              , [is_active]
              , [priority]
            from [#medication_units] as [source]
            where [site_id] > 0
                  and [target_id] is not null
                  and [existing_record] = 0;

            set identity_insert [dbo].[medication_units] off;

            /***************************************
                    loading [external_ids] reference
            ***************************************/

            insert into [dbo].[external_ids]
            (
                [internal_id]
              , [vendor]
              , [entity]
              , [external_id]
            )
            select
                [target_id]
              , 'pulsecheck'
              , 'medication_units'
              , [source_id]
            from [#medication_units] as [source]
            where [site_id] > 0
                  and [target_id] is not null
                  and [existing_record] = 0;

            /****************
                    end table
            ****************/
            --
            -- check for emar orphans
            -- the way to deal with orphans has to be determined for each table
            --
            --  medication_routes: orphans will be marked is_active = 0
            --
            update [target] set
                [is_active] = 0
            from [dbo].[medication_units] [target]
                outer apply [dbo].[get_external_id]('pulsecheck', 'medication_routes', [target].[id]) [gii]
                left join [#medication_units] as [source]
                    on [gii].[id] = [source].[source_id]
            where [source].[source_id] is null;
            commit transaction;

        end try
        begin catch

            rollback transaction;
        end catch;
        end;

    drop table if exists [#medication_units];
end;
go
