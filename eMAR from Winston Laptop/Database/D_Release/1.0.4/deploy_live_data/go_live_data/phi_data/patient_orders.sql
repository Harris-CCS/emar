print 'Loading Table: patient_orders';

drop table if exists [#patient_orders];

create table [#patient_orders]
    (
        [patient_external]        [varchar](50)   not null
      , [add_user]                [varchar](50)   not null
      , [add_datetime]            [varchar](50)   not null
      , [order_physician_user]    [varchar](50)   not null
      , [begin_datetime]          [varchar](50)   not null
      , [end_datetime]            [varchar](50)   null
      , [ndc]                     [varchar](32)   null
      , [drug_id]                 [varchar](32)   null
      , [brand_name]              [nvarchar](255) null
      , [dose]                    [varchar](50)   null
      , [medication_unit]         [varchar](20)   null
      , [medication_route]        [varchar](50)   null
      , [priority]                [tinyint]       not null
      , [frequency_schedule_id]   [int]           null
      , [prn]                     [bit]           not null
      , [point_in_time]           [bit]           not null
      , [order_status]            [varchar](10)   not null
      , [order_notes]             [nvarchar](max) null
      , [time_zone_name]          [sysname]       null
      , [site_id]                 [int]           null
      , [medication_unit_id]      [int]           null
      , [medication_route_id]     [int]           null
      , [patient_id]              [bigint]        null
      , [add_user_id]             [int]           null
      , [order_physician_user_id] [int]           null
      , [medication_id]           [int]           null
        default 0
    );

if '$(load_data)' = 'sample'
    or ('$(load_data)' = 'live'
        and @does_ibex_exist = 1)
    begin

        insert into [#patient_orders]
        (
            [patient_external]
          , [add_user]
          , [add_datetime]
          , [order_physician_user]
          , [begin_datetime]
          , [end_datetime]
          , [ndc]
          , [drug_id]
          , [brand_name]
          , [dose]
          , [medication_unit]
          , [medication_route]
          , [priority]
          , [frequency_schedule_id]
          , [prn]
          , [point_in_time]
          , [order_status]
          , [order_notes]
        )
        execute ('execute dbo.export_ibex_patient_orders');
    end;

if (
             select
                 count(*)
             from [#patient_orders]
    ) > 0
    begin

        begin transaction;

        truncate table [#medication_items];
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
        from [#patient_orders];

        --set medication id's
        execute [dbo].[update_medication_id_list];

        update [target] set
            [medication_id] = [source].[medication_id]
        from [#medication_items] [source]
            inner join [#patient_orders] [target]
                on [source].[ndc] = [target].[ndc]
                    and [source].[brand_name] = [target].[brand_name]
                    and [source].[drug_id] = [target].[drug_id]
        where [source].[medication_id] > 0;


        -- get patient_id , site_id, timezone
        update [source] set
            [patient_id]     = [internal_patient_id].[id]
          , [site_id]        = [patients].[site_id]
          , [time_zone_name] = [site].[time_zone_name]
        from [#patient_orders] as [source]
            outer apply [dbo].[get_internal_id]
            ('pulsecheck', 'patients', [source].[patient_external]) as [internal_patient_id]
            left join [dbo].[patients] as [patients]
                on [patients].[id] = [internal_patient_id].[id]
            left join [dbo].[sites] as [site]
                on [site].[id] = [patients].[site_id];

        -- get add_user_id
        update [source] set
            [add_user_id] = isnull([internal_add_user_id].[id], 0)
        from [#patient_orders] as [source]
            outer apply [dbo].[get_internal_id]
            ('pulsecheck', 'users', [source].[add_user]) as [internal_add_user_id];

        -- get order_physician_user_id
        update [source] set
            [order_physician_user_id] = isnull([internal_add_user_id].[id], 0)
        from [#patient_orders] as [source]
            outer apply [dbo].[get_internal_id]
            ('pulsecheck', 'users', [source].[order_physician_user]) as [internal_add_user_id];

        -- get medication_route_id
        update [source] set
            [medication_route_id] = [mr].[id]
        from [#patient_orders] as [source]
            cross apply [dbo].[get_code_share_site]([source].[site_id], 'medication_routes') as [mr_site]
            inner join [dbo].[medication_routes] as [mr]
                on [mr].[site_id] = [mr_site].[site_id]
                    and [mr].[code] = [source].[medication_route];

        -- get medication_unit_id
        update [source] set
            [medication_unit_id] = [mu].[id]
        from [#patient_orders] as [source]
            cross apply [dbo].[get_code_share_site]([source].[site_id], 'medication_units') as [mr_site]
            inner join [dbo].[medication_units] as [mu]
                on [mu].[site_id] = [mr_site].[site_id]
                    and [mu].[name] = [source].[medication_unit];

        /****************************************
                load temporary tables for staging
        ****************************************/

        alter table [#patient_orders]
        add [id] [bigint] identity (1, 1)
        , [target_id] [bigint];

        /********************************
                get max id for seed value
        ********************************/

        set @max_id = null;

        select
            @max_id = max([id])
        from [dbo].[patient_orders];

        set @max_id = isnull(@max_id, 0);

        update [source] set
            [target_id] = [source].[id] + @max_id
          , [dose]      =
                case
                    when isnumeric([dose]) = 0 then null
                    else [dose]
                end
        from [#patient_orders] as [source];

        /*************************************
                begin loading permanent tables
        *************************************/

        -- set identity_insert [dbo].[patient_orders] on;

        insert into [dbo].[patient_orders]
        (
            [patient_id]
          , [add_user_id]
          , [add_datetime]
          , [order_physician_user_id]
          , [begin_datetime]
          , [end_datetime]
          , [dose]
          , [medication_unit_id]
          , [medication_route_id]
          , [priority]
          , [frequency_schedule_id]
          , [prn]
          , [point_in_time]
          , [order_status]
          , [order_notes]
          , [medication_id]
        )
        select
            [patient_id]
          , [add_user_id]
          , [dbo].[ibex_date_to_offset_date]([add_datetime], [time_zone_name])   as [add_datetime]
          , [order_physician_user_id]
          , [dbo].[ibex_date_to_offset_date]([begin_datetime], [time_zone_name]) as [begin_datetime]
          , [dbo].[ibex_date_to_offset_date]([end_datetime], [time_zone_name])   as [end_datetime]
          , [dose]
          , [medication_unit_id]
          , [medication_route_id]
          , [priority]
          , null [frequency_schedule_id]
          , [prn]
          , [point_in_time]
          , [order_status]
          , [order_notes]
          , [medication_id]
        from [#patient_orders] as [source]
        where [medication_id] > 0
        order by [brand_name]
               , [patient_id];

        -- set identity_insert [dbo].[patient_orders] off;

        /***************************************
                loading [external_ids] reference
        ***************************************/
        /****************
                end table
        ****************/

        commit transaction;
    end;

drop table if exists [#patient_orders];