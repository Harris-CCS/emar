create procedure [dbo].[load_sites]
as
begin

    set nocount on;

    create table [#sites]
        (
            [target_id]       [int]         null
          , [source_id]       [varchar](25) null
          , [name]            [varchar](60) null
          , [is_active]       [bit]         null
          , [time_zone_name]  [sysname]     null
          , [existing_record] [bit]         default 0
        );

    insert into [#sites]
    (
        [source_id]
      , [name]
      , [is_active]
      , [time_zone_name]
    )
    execute ('execute dbo.export_ibex_sites');

    if (
                 select
                     count(*)
                 from [#sites]
        ) > 0
        begin

            begin transaction;

            /****************************************
                    load temporary tables for staging
            ****************************************/



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
                               , 'sites'              [entity]
                               , [source].[source_id] [external_id]
                             from [#sites] as [source]
                                 inner join [dbo].[sites] [target]
                                     on [source].[name] = [target].[name]
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
                left join [#sites] as [source]
                    on [ei].[external_id] = [source].[source_id]
                left join [dbo].[sites] [target]
                    on [ei].[internal_id] = [target].[id]
            where [ei].[vendor] = 'pulsecheck'
                and [ei].[entity] = 'sites'
                and [source].[source_id] is null
                and [target].[id] is not null;

            --- Delete Bad External ID Records
            delete [ei]
            from [dbo].[external_ids] [ei]
                left join [#sites] as [source]
                    on [ei].[external_id] = [source].[source_id]
                left join [dbo].[sites] [target]
                    on [ei].[internal_id] = [target].[id]
            where [ei].[vendor] = 'pulsecheck'
                and [ei].[entity] = 'sites'
                and ([source].[source_id] is null
                    or [target].[id] is null);

            --- Match target_id's with external_ids table
            update [source] set
                [target_id]       = [internal_site].[id]
              , [existing_record] = 1
            from [#sites] as [source]
                cross apply [dbo].[get_internal_id]
                ('pulsecheck', 'sites', [source].[source_id]) as [internal_site];

            --- Update Matching Target Records
            update [target] set
                [name]           = [source].[name]
              , [is_active]      = [source].[is_active]
              , [time_zone_name] = [source].[time_zone_name]
            from [#sites] as [source]
                cross apply [dbo].[get_internal_id]('pulsecheck', 'sites', [source].[source_id]) [gii]
                inner join [dbo].[sites] [target]
                    on [gii].[id] = [target].[id];

            --- Generate New id's for insert records
            with cte_data
                as (
                             select
                                 ident_current('[dbo].[sites]') +
                                 row_number() over (order by [source].[source_id]) new_id
                               , [source].[target_id]
                             from [#sites] as [source]
                                 outer apply [dbo].[get_internal_id]('pulsecheck', 'sites', [source].[source_id]) [gii]
                                 left join [dbo].[sites] [target]
                                     on [gii].[id] = [target].[id]
                             where [target].[id] is null
                    )
            update [data] set
                [target_id] = [data].[new_id]
            from cte_data [data];

            /*************************************
                    begin loading permanent tables
            *************************************/

            if (
                         select
                             count(*)
                         from [dbo].[sites] [site]
                         where [site].[id] in (0, -1)
                ) <> 2
                begin

                    set identity_insert [dbo].[sites] on;

                    insert into [dbo].[sites]
                    (
                        [id]
                      , [name]
                      , [is_active]
                      , [time_zone_name]
                    )
                    select
                        [val].[site_id]
                      , [val].[name]
                      , [val].[is_active]
                      , [val].[time_zone_name]
                    from (
                    values
                    ('-1', 'Dummy Site for Relational Integrity', '0', 'Central Standard Time')
                    , ('0', 'Dummy Site use up site_id 0', '0', 'Central Standard Time')
                    ) as [val]
                    (
                    [site_id]
                    , [name]
                    , [is_active]
                    , [time_zone_name]
                    )
                        left join [dbo].[sites] [site]
                            on [site].[id] = [val].[site_id]
                    where [site].[id] is null;

                    set identity_insert [dbo].[sites] off;

                end;


            set identity_insert [dbo].[sites] on;

            insert into [dbo].[sites]
            (
                [id]
              , [name]
              , [is_active]
              , [time_zone_name]
            )
            select
                [target_id]
              , [name]
              , [is_active]
              , [time_zone_name]
            from [#sites] as [source]
            where [target_id] is not null
                  and [target_id] > 0
                  and [existing_record] = 0;

            set identity_insert [dbo].[sites] off;

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
                [source].[target_id]
              , 'pulsecheck'
              , 'sites'
              , [source].[source_id]
            from [#sites] as [source]
            where [target_id] is not null
                  and [target_id] > 0
                  and [existing_record] = 0;

            /****************
                    end table
            ****************/

            --
            -- check for emar orphans
            -- the way to deal with orphans has to be determined for each table
            --
            --  sites: orphans will be marked is_active = 0
            --
            update [target] set
                [is_active] = 0
            from [dbo].[sites] [target]
                outer apply [dbo].[get_external_id]('pulsecheck', 'sites', [target].[id]) [gii]
                left join [#sites] as [source]
                    on [gii].[id] = [source].[source_id]
            where [source].[source_id] is null;

            commit transaction;
        end;

    drop table if exists [#sites];
end;
go

/*
begin transaction;
--set rowcount 10;
--delete [sites];
--set rowcount 0;

--set rowcount 0;
----delete [sites];
--delete [ei]
--from [dbo].[external_ids] [ei]
--where [ei].[entity] = 'sites';
--set rowcount 0;

go

execute [dbo].[load_sites];

--select
--    *
--from [dbo].[external_ids] [ei]
--where [ei].[entity] = 'sites'
--order by 1;

select
    *
from [sites] as [source]
where name like 'fdb%'
order by name;

rollback transaction;
---*/

