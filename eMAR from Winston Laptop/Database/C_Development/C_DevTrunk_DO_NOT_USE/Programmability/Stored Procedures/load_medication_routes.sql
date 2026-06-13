create procedure [dbo].[load_medication_routes]
as
begin

    set nocount on;

    print 'Loading Table: medication_routes';

    drop table if exists [#medication_routes];

    create table [#medication_routes]
        (
            [target_id]       [int]          null
          , [source_id]       [varchar](25)  null
          , [site]            [varchar](25)  null
          , [code]            [varchar](25)  null
          , [type]            [varchar](25)  null
          , [name]            [nvarchar](50) null
          , [misc2]           [varchar](50)  null
          , [misc3]           [varchar](50)  null
          , [site_id]         [int]          null
          , [priority]        [int]          null
          , [status]          [varchar](25)  null
          , [is_active]       [bit]          null
          , [existing_record] [bit]          default 0
        );

    -- query remote data

    insert into [#medication_routes]
    (
        [source_id]
      , [site]
      , [name]
      , [misc2]
      , [misc3]
      , [status]
      , [code]
    )
    execute ('execute dbo.export_ibex_medication_routes');

    if (
                 select
                     count(*)
                 from [#medication_routes]
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
              , [is_active] =
                    case
                        when [status] = 'A' then 1
                        else 0
                    end
              , [type]      = isnull([misc3], '')
            from [#medication_routes] as [source];

            -- get internal site_id
            update [source] set
                [site_id] = isnull([internal_site].[id], -1)
            from [#medication_routes] as [source]
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

            --- if emar unique index/constraint exists validate and create any missing [dbo].[external_ids] [ei]
            with cte_constraint_match
                as (
                             select
                                 [target].[id]        [internal_id]
                               , 'pulsecheck'         [vendor]
                               , 'medication_routes'  [entity]
                               , [source].[source_id] [external_id]
                             from [#medication_routes] as [source]
                                 inner join [dbo].[medication_routes] [target]
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

            --- Delete Missing Target Records
            delete [target]
            from [dbo].[external_ids] [ei]
                left join [#medication_routes] as [source]
                    on [ei].[external_id] = [source].[source_id]
                left join [dbo].[medication_routes] [target]
                    on [ei].[internal_id] = [target].[id]
            where [ei].[vendor] = 'pulsecheck'
                and [ei].[entity] = 'medication_routes'
                and [source].[source_id] is null
                and [target].[id] is not null;

            --- Delete Bad External ID Records
            delete [ei]
            from [dbo].[external_ids] [ei]
                left join [#medication_routes] as [source]
                    on [ei].[external_id] = [source].[source_id]
                left join [dbo].[medication_routes] [target]
                    on [ei].[internal_id] = [target].[id]
            where [ei].[vendor] = 'pulsecheck'
                and [ei].[entity] = 'medication_routes'
                and ([source].[source_id] is null
                    or [target].[id] is null);

            --- Match target_id's with external_ids table
            update [source] set
                [target_id]       = [internal_site].[id]
              , [existing_record] = 1
            from [#medication_routes] as [source]
                cross apply [dbo].[get_internal_id]
                ('pulsecheck', 'medication_routes', [source].[source_id]) as [internal_site];

            --- Update Matching Target Records
            update [target] set
                [site_id]   = [source].[site_id]
              , [name]      = [source].[name]
              , [priority]  = [source].[priority]
              , [is_active] = [source].[is_active]
              , [type]      = [source].[type]
            from [#medication_routes] as [source]
                cross apply [dbo].[get_internal_id]('pulsecheck', 'medication_routes', [source].[source_id]) [gii]
                inner join [dbo].[medication_routes] [target]
                    on [gii].[id] = [target].[id];

            --- Generate New id's for insert records
            with cte_data
                as (
                             select
                                 ident_current('[dbo].[medication_routes]') +
                                 row_number() over (order by [source].[source_id]) new_id
                               , [source].[target_id]
                             from [#medication_routes] as [source]
                                 outer apply [dbo].[get_internal_id]('pulsecheck', 'medication_routes', [source].[source_id]) [gii]
                                 left join [dbo].[medication_routes] [target]
                                     on [gii].[id] = [target].[id]
                             where [target].[id] is null
                    )
            update [data] set
                [target_id] = [data].[new_id]
            from cte_data [data];


            /*************************************
                    begin loading permanent tables
            *************************************/
            set identity_insert [dbo].[medication_routes] on;

            insert into [dbo].[medication_routes]
            (
                [id]
              , [site_id]
              , [name]
              , [priority]
              , [is_active]
              , [code]
              , [type]
            )
            select
                [target_id]
              , [site_id]
              , [name]
              , [priority]
              , [is_active]
              , [code]
              , [type]
            from [#medication_routes] as [source]
            where [site_id] > 0
                  and [target_id] is not null
                  and [existing_record] = 0;

            set identity_insert [dbo].[medication_routes] off;

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
              , 'medication_routes'
              , [source_id]
            from [#medication_routes] as [source]
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
            from [dbo].[medication_routes] [target]
                outer apply [dbo].[get_external_id]('pulsecheck', 'medication_routes', [target].[id]) [gii]
                left join [#medication_routes] as [source]
                    on [gii].[id] = [source].[source_id]
            where [source].[source_id] is null;

        end;

    drop table if exists [#medication_routes];
end;
go
