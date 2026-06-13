print 'Loading Table: devices';

drop table if exists [#devices2];

create table [#devices2]
    (
      [source_id]        [varchar](25) null
    , [site_id]          [varchar](25) not null
    , [is_active]        [char](1) null
    , [device_type]      [char](1) null
    , [address]          [nvarchar](50) null
    , [print_queue_name] [varchar](80) null
    , [description]      [nvarchar](50) null
    , [tray]             [char](1) null
    , [pcl_type]         [char](1) null);

if '$(load_data)' = 'sample'
   or ('$(load_data)' = 'live'
       and @does_ibex_exist = 1)
    begin

        insert into [#devices2]
            ([source_id]
           , [site_id]
           , [is_active]
           , [device_type]
           , [address]
           , [print_queue_name]
           , [description]
           , [tray]
           , [pcl_type]
            )
        execute ('execute dbo.export_ibex_devices');
    end;

if(
  select count(*)
  from [#devices2]) > 0
    begin

        begin transaction;

/****************************************
        load temporary tables for staging
****************************************/

        update [source] set    
            [source_id] = [source].[site_id] + '|' + [source].[source_id]
        from   [#devices2] as [source];

        alter table [#devices2]
        add [id]        [int] identity(1, 1)
          , [target_id] [int];

/********************************
        get max id for seed value
********************************/

        set @max_id = null;

        select @max_id = max([id])
        from   [dbo].[devices];

        set @max_id = isnull(@max_id, 0);

        update [source] set    
            [target_id] = [source].[id] + @max_id
        from   [#devices2] as [source];

/*************************************
        begin loading permanent tables
*************************************/

        set identity_insert [dbo].[devices] on;

        insert into [dbo].[devices]
            ([id]
           , [site_id]
           , [is_active]
           , [device_type]
           , [address]
           , [print_queue_name]
           , [description]
           , [tray]
           , [pcl_type]
            )
        select [source].[target_id]
             , isnull([internal_site].[id], -1) as [site_id]
             , case [is_active]
                   when 'Y'
                       then 1
                   else 0
               end as                              [is_active]
             , [device_type]
             , [address]
             , [print_queue_name]
             , [description]
             , [tray]
             , [pcl_type]
        from   [#devices2] as [source]
               outer apply [dbo].[get_internal_id]
            ('pulsecheck', 'sites', [source].[site_id]) as [internal_site]
        where  [source].[site_id] > 0;

        set identity_insert [dbo].[devices] off;

/***************************************
        loading [external_ids] reference
***************************************/

        insert into [dbo].[external_ids]
            ([internal_id]
           , [vendor]
           , [entity]
           , [external_id]
            )
        select [source].[target_id]
             , 'pulsecheck'
             , 'devices'
             , [source].[source_id]
        from   [#devices2] as [source];

/****************
        end table
****************/

        commit transaction;
    end;

drop table if exists [#devices2];