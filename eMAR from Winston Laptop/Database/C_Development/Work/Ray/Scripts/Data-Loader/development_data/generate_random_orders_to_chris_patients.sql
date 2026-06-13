if '$(load_data)' in('sample', 'live')
    begin

        print 'Loading Table: generate_random_orders_to_chris_patients.sql';

        drop table if exists [#patient_orders2];

        create table [#patient_orders2]
            (
              [id]                          [bigint] null
            , [patient_id]                  [bigint] null
            , [add_user_id]                 [int] null
            , [add_datetime]                [datetimeoffset](7) null
            , [order_physician_user_id]     [int] null
            , [begin_datetime]              [datetimeoffset](7) null
            , [end_datetime]                [datetimeoffset](7) null
            , [dose]                        [decimal](11, 2) null
            , [medication_unit_id]          [int] null
            , [medication_route_id]         [int] null
            , [priority]                    [tinyint] null
            , [frequency_schedule_id]       [int] null
            , [prn]                         [bit] null
            , [point_in_time]               [bit] null
            , [order_status]                [varchar](10) null
            , [order_notes]                 [nvarchar](max) null
            , [medication_id]               [int] null
            , [antimicrobial_indication_id] [int] null);
    
        with cte_source
             as (select row_number() over(partition by [p].[site_id]
                        order by [reps].sequence
                               , [p].[id]) as [id]
                      , [p].[site_id]
                      , [p].[id] as           [patient_id]
                 from   [dbo].[patients] as [p]
                        cross join [dbo].[frequency_minutes] as [reps]
                 where  [p].[middle_name] = 'Chris'
                        and [site_id] = @dev_custom_data_site_id
                        and [reps].sequence between 1 and 10)
             insert into [#patient_orders2]
                 ([id]
                , [patient_id]
                , priority
                , add_datetime
                , begin_datetime
                 )
             select [id]
                  , [patient_id]
                  , 0
                  , dateadd(day,-1,getdate()) at time zone 'Cuba Standard Time' as [add_datetime]
                  , cast(cast(getdate() as date) as datetime) at time zone 'Central Standard Time' as [begin_datetime]
             from   [cte_source];

        with cte_source
             as (select row_number() over(
                        order by [reps].sequence
                               , [src].[id]) as [id]
                      , [src].[id] as           [source_id]
                 from   [users] as [src]
                        cross join [dbo].[frequency_minutes] as [reps]
                 where  [site_id] = @dev_custom_data_site_id
                        and [type] = 'D'
                        and [reps].sequence between 1 and 36)
             update [target] set    
                 [order_physician_user_id] = [source].[source_id]
               , [order_status] = 'Pending'
             from   [cte_source] [source]
                    inner join [#patient_orders2] [target] on [source].[id] = [target].[id];

        with cte_source
             as (select row_number() over(
                        order by [vals].[order_type] desc
                               , [vals].[sequence]
                               , [vals].[source_id]) as [id]
                               , [vals].[source_id]
                               , [vals].[order_type]
                               , [vals].[prn]
                               , [vals].[point_in_time]
        from(
        select 1 [order_type]
                      , [src].[id] as           [source_id]
                      , case
                            when [src].[frequency_type_id] = 7
                                then 1
                            else 0
                        end as                  [prn]
                      , [point_in_time]
                      , [reps].[sequence]
                 from   [dbo].[frequency_schedules] as [src]
                        cross join [dbo].[frequency_minutes] as [reps]
                 where  [site_id] = @dev_custom_data_site_id
                        and [reps].sequence between 1 and 6
                        and [src].frequency_type_id in(1,2,4)
        union all
                 select 2 [order_type]
                      , [src].[id] as           [source_id]
                      , case
                            when [src].[frequency_type_id] = 7
                                then 1
                            else 0
                        end as                  [prn]
                      , [point_in_time]
                      , [reps].[sequence]
                 from   [dbo].[frequency_schedules] as [src]
                        cross join [dbo].[frequency_minutes] as [reps]
                 where  [site_id] = @dev_custom_data_site_id 
                        and [reps].sequence between 1 and 2
                        and [src].frequency_type_id in(7)
        union all
                 select 3 [order_type]
                      , [src].[id] as           [source_id]
                      , case
                            when [src].[frequency_type_id] = 7
                                then 1
                            else 0
                        end as                  [prn]
                      , [point_in_time]
                      , [reps].[sequence]
                 from   [dbo].[frequency_schedules] as [src]
                        cross join [dbo].[frequency_minutes] as [reps]
                 where  [site_id] = @dev_custom_data_site_id 
                        and [reps].sequence between 1 and 30
                        and [src].frequency_type_id in(8)
        ) vals                
                        )
             update [target] set    
                 [frequency_schedule_id] = [source].[source_id]
               , [point_in_time] = [source].[point_in_time]
               , [prn] = [source].[prn]
               , [order_notes] = ''
             from   [cte_source] [source]
                    inner join [#patient_orders2] [target] on [source].[id] = [target].[id];

        with cte_source
             as (select    row_number() over(
                           order by [reps].sequence
                                  , [src].[medication_id]) as [id]
                         , [src].[medication_id] as           [source_id]
                 from      (select distinct 
                                   [src].[medication_id]
                            from [user_quick_list_items] as [src]
                            where [src].[site_id] = @dev_custom_data_site_id
                                  and [src].[medication_unit_id] is not null
                                  and [src].[medication_route_id] is not null) as [src]
                           cross join [dbo].[frequency_minutes] as [reps]
                 where [reps].sequence between 1 and 6)
             update [target] set    
                 [medication_id] = [source].[source_id]
             from   [cte_source] as [source]
                    inner join [#patient_orders2] as [target] on [source].[id] = [target].[id];

        update [target] set    
            [add_user_id] = [src].[user_id]
          , [medication_route_id] = [src].[medication_route_id]
          , [dose] = [src].[dose]
          , [medication_unit_id] = [src].[medication_unit_id]
          , [order_notes] = [src].[order_notes]
        from   [#patient_orders2] [target]
               inner join [user_quick_list_items] [src] on [src].[medication_id] = [target].[medication_id]
        where  [src].[site_id] = @dev_custom_data_site_id
               and [src].[medication_unit_id] is not null
               and [src].[medication_route_id] is not null;

        insert into [dbo].[patient_orders]
            ([patient_id]
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
           , [antimicrobial_indication_id]
            )
        select [po].[patient_id]
             , [po].[add_user_id]
             , [po].[add_datetime]
             , [po].[order_physician_user_id]
             , [po].[begin_datetime]
             , [po].[end_datetime]
             , [po].[dose]
             , [po].[medication_unit_id]
             , [po].[medication_route_id]
             , [po].[priority]
             , [po].[frequency_schedule_id]
             , [po].[prn]
             , [po].[point_in_time]
             , [po].[order_status]
             , [po].[order_notes]
             , [po].[medication_id]
             , [po].[antimicrobial_indication_id]
        from   [#patient_orders2] as [po];

        exec [dbo].[generate_order_administrations] 0;

        drop table if exists [#patient_orders2];

    end;