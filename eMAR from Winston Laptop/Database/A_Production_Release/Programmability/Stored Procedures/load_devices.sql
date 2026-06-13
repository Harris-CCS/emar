create procedure [dbo].[load_devices]
as
begin

    set nocount on;

    print 'Loading Table: devices';

    create table [#devices]
        (
            [target_id]        [int]          null
          , [source_id]        [varchar](25)  null
          , [site]             [varchar](25)  null
          , [status]           [char](1)      null
          , [device_type]      [char](1)      null
          , [address]          [nvarchar](50) null
          , [print_queue_name] [varchar](80)  null
          , [description]      [nvarchar](50) null
          , [tray]             [char](1)      null
          , [pcl_type]         [char](1)      null
          , [site_id]          [int]          null
          , [is_active]        [bit]          default 0
          , [existing_record]  [bit]          default 0
        );


    insert into [#devices]
    (
        [source_id]
      , [site]
      , [status]
      , [device_type]
      , [address]
      , [print_queue_name]
      , [description]
      , [tray]
      , [pcl_type]
    )
    execute ('execute dbo.export_ibex_devices');

    if (
                 select
                     count(*)
                 from [#devices]
        ) > 0
        begin

            begin transaction;

            /****************************************
                    load temporary tables for staging
            ****************************************/

            update [source] set
                [source_id] = [source].[site] + '|' + [source].[source_id]
              , [is_active] =
                    case [status]
                        when 'Y' then 1
                        else 0
                    end
            from [#devices] as [source];

            update [source] set
                [site_id] = isnull([internal_site].[id], -1)
            from [#devices] as [source]
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
                               , 'devices'            [entity]
                               , [source].[source_id] [external_id]
                             from [#devices] as [source]
                                 inner join [dbo].[devices] [target]
                                     on [source].[site_id] = [target].[site_id]
                                         and [source].[description] = [target].[description]
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
                left join [#devices] as [source]
                    on [ei].[external_id] = [source].[source_id]
                left join [dbo].[devices] [target]
                    on [ei].[internal_id] = [target].[id]
            where [ei].[vendor] = 'pulsecheck'
                and [ei].[entity] = 'devices'
                and [source].[source_id] is null
                and [target].[id] is not null;

            --- Delete Bad External ID Records
            delete [ei]
            from [dbo].[external_ids] [ei]
                left join [#devices] as [source]
                    on [ei].[external_id] = [source].[source_id]
                left join [dbo].[devices] [target]
                    on [ei].[internal_id] = [target].[id]
            where [ei].[vendor] = 'pulsecheck'
                and [ei].[entity] = 'devices'
                and ([source].[source_id] is null
                    or [target].[id] is null);

            --- Match target_id's with external_ids table
            update [source] set
                [target_id]       = [internal_site].[id]
              , [existing_record] = 1
            from [#devices] as [source]
                cross apply [dbo].[get_internal_id]
                ('pulsecheck', 'devices', [source].[source_id]) as [internal_site];

            --- Update Matching Target Records
            update [target] set
                [site_id]          = [source].[site_id]
              , [address]          = [source].[address]
              , [description]      = [source].[description]
              , [is_active]        = [source].[is_active]
              , [print_queue_name] = [source].[print_queue_name]
              , [tray]             = [source].[tray]
              , [device_type]      = [source].[device_type]
              , [pcl_type]         = [source].[pcl_type]
            from [#devices] as [source]
                cross apply [dbo].[get_internal_id]('pulsecheck', 'devices', [source].[source_id]) [gii]
                inner join [dbo].[devices] [target]
                    on [gii].[id] = [target].[id];

            --- Generate New id's for insert records
            with cte_data
                as (
                             select
                                 ident_current('[dbo].[devices]') +
                                 row_number() over (order by [source].[source_id]) new_id
                               , [source].[target_id]
                             from [#devices] as [source]
                                 outer apply [dbo].[get_internal_id]('pulsecheck', 'devices', [source].[source_id]) [gii]
                                 left join [dbo].[devices] [target]
                                     on [gii].[id] = [target].[id]
                             where [target].[id] is null
                    )
            update [data] set
                [target_id] = [data].[new_id]
            from cte_data [data];

            /*************************************
                    begin loading permanent tables
            *************************************/
            set identity_insert [dbo].[devices] on;

            insert into [dbo].[devices]
            (
                [id]
              , [site_id]
              , [is_active]
              , [device_type]
              , [address]
              , [print_queue_name]
              , [description]
              , [tray]
              , [pcl_type]
            )
            select
                [source].[target_id]
              , [site_id]
              , [is_active]
              , [device_type]
              , [address]
              , [print_queue_name]
              , [description]
              , [tray]
              , [pcl_type]
            from [#devices] as [source]
            where [site_id] > 0
                  and [target_id] is not null
                  and [existing_record] = 0;

            set identity_insert [dbo].[devices] off;

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
              , 'devices'
              , [source].[source_id]
            from [#devices] as [source]
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
            --  devices: orphans will be marked is_active = 0
            --
            update [target] set
                [is_active] = 0
            from [dbo].[devices] [target]
                outer apply [dbo].[get_external_id]('pulsecheck', 'devices', [target].[id]) [gii]
                left join [#devices] as [source]
                    on [gii].[id] = [source].[source_id]
            where [source].[source_id] is null;

            commit transaction;
        end;

    drop table if exists [#devices];
end;
go
/*
begin transaction

delete [dbo].[external_ids]
              where [entity]='devices'


execute [dbo].[load_devices]
select * from [devices] [d] where [d].[description] like 'Impl%'

go
rollback transaction
*/