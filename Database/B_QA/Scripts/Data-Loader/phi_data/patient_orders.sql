print 'Loading Table: patient_orders';

drop table if exists [#patient_orders];

create table [#patient_orders]
    (
      [patient_id]              [varchar](50) not null
    , [add_user_id]             [varchar](50) not null
    , [add_datetime]            [varchar](50) not null
    , [order_physician_user_id] [varchar](50) not null
    , [begin_datetime]          [varchar](50) not null
    , [end_datetime]            [varchar](50) null
    , [ndc]                     [varchar](32) null
    , [drug_id]                 [varchar](32) null
    , [brand_name]              [nvarchar](255) null
    , [dose]                    [varchar](50) null
    , [dose_unit]               [varchar](20) null
    , [medication_route_id]     [varchar](50) null
    , [priority]                [tinyint] not null
    , [frequency_id]            [int] null
    , [prn]                     [bit] not null
    , [point_in_time]           [bit] not null
    , [order_status]            [varchar](10) not null
    , [order_notes]             [nvarchar](max) null);

if '$(load_data)' = 'live'
   and exists
(
    select null
    from   [master].[sys].[databases]
    where  [name] = 'ibex'
)
    begin

        insert into [#patient_orders]
            ([patient_id]
           , [add_user_id]
           , [add_datetime]
           , [order_physician_user_id]
           , [begin_datetime]
           , [end_datetime]
           , [ndc]
           , [drug_id]
           , [brand_name]
           , [dose]
           , [dose_unit]
           , [medication_route_id]
           , [priority]
           , [frequency_id]
           , [prn]
           , [point_in_time]
           , [order_status]
           , [order_notes]
            )
        execute ('execute dbo.export_ibex_patient_orders ''live''');
    end;

if '$(load_data)' = 'sample'
    begin

        bulk insert [#patient_orders] from '$(current_path)Scripts\Data-Loader\sample_data\patient_orders.bcp' with(fieldterminator = '|~', rowterminator = '\n');
    end;

if
(
    select count(*)
    from   [#patient_orders]
) > 0
    begin

        begin transaction;

/****************************************
        load temporary tables for staging
****************************************/

        alter table [#patient_orders]
        add [id]        [bigint] identity(1, 1)
          , [target_id] [bigint];

/********************************
        get max id for seed value
********************************/

        set @max_id = null;

        select @max_id = max([id])
        from   [dbo].[patient_orders];

        set @max_id = isnull(@max_id, 0);

        update [source] set    
              [target_id] = [source].[id] + @max_id
            , [dose] = case when isnumeric([dose])=0 then null else [dose] end
        from   [#patient_orders] as [source];

/*************************************
        begin loading permanent tables
*************************************/

        -- set identity_insert [dbo].[patient_orders] on;

        insert into [dbo].[patient_orders]
            ([patient_id]
           , [add_user_id]
           , [add_datetime]
           , [order_physician_user_id]
           , [begin_datetime]
           , [end_datetime]
           , [ndc]
           , [drug_id]
           , [brand_name]
           , [dose]
           , [dose_unit]
           , [medication_route_id]
           , [priority]
           , [frequency_id]
           , [prn]
           , [point_in_time]
           , [order_status]
           , [order_notes]
            )
        select isnull([internal_patient_id].[id], -1) as     [patient_id]
            , isnull([internal_add_user_id].[id], 0) as          [add_user_id]
            , [dbo].[ibex_date_to_offset_date]([source].[add_datetime], [site].[time_zone_name]) as [add_datetime]
            , isnull([internal_order_physician_user_id].[id], 0) as          [order_physician_user_id]
            , [dbo].[ibex_date_to_offset_date]([source].[begin_datetime], [site].[time_zone_name]) as [begin_datetime]
            , [dbo].[ibex_date_to_offset_date]([source].[end_datetime], [site].[time_zone_name]) as   [end_datetime]
            , [source].[ndc]
            , [source].[drug_id]
            , [source].[brand_name]
            , [source].[dose]
            , [source].[dose_unit]
            , [medication_routes].[id] as                    [medication_routes_id]
            , [source].[priority]
            , [source].[frequency_id]
            , [source].[prn]
            , [source].[point_in_time]
            , [source].[order_status]
            , [source].[order_notes]
        from [#patient_orders] as [source]
            outer apply [dbo].[get_internal_id]('pulsecheck', 'patients', [source].[patient_id]) as [internal_patient_id]
            left join [dbo].[patients] as [patients] on [patients].[id] = [internal_patient_id].[id]
            left join [dbo].[sites] as [site] on [site].[id] = [patients].[site_id]
            outer apply [dbo].[get_internal_id]('pulsecheck', 'users', [source].[add_user_id]) as [internal_add_user_id]
            outer apply [dbo].[get_internal_id]('pulsecheck', 'users', [source].[order_physician_user_id]) as [internal_order_physician_user_id]
                      outer apply
        (
            select top 1 [mr_item].[id]
            from
            (
                select 1 as [type]
                     , [mr].[id]
                     , [patients].[site_id]
                from     [dbo].[medication_routes] as [mr]
                where      [mr].[name] = [source].[medication_route_id]
                           and [mr].[site_id] = [patients].[site_id]
                union
                select 2 as [type]
                     , [mr].[id]
                     , [patients].[site_id]
                from   [dbo].[medication_routes] as [mr]
                where  [mr].[name] = [source].[medication_route_id]
                       and [mr].[site_id] <> [patients].[site_id]
            ) as [mr_item]
            order by [mr_item].[type]
                   , [mr_item].[site_id]
        ) as [medication_routes]
        order by [brand_name],[patient_id];

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