create procedure [dbo].[generate_order_administrations] 
      @override_offset           int = null
    , @override_patient_order_id int = null
    , @is_debug                  bit = 0
as

/*************************************************************************************************************
This procedure is designed to run without any input parameters
input parameters are provided for special situational cases

      @override_offset
        Default @override_offset current_day + 1
            This parameter provides the ability to run a specific day if needed
            +1 = Tomorrow
             0 = Today
            -1 = Yesterday

    , @override_patient_order_id
        This parameter provides the ability to run the process on a single patient_order

    , @is_debug
        This parameter prvides some simple output used to generate order_administrations: useful for debugging
*************************************************************************************************************/
    begin

        set nocount on;

        declare 
            @period_begin          datetime2(0)
          , @period_end            datetime2(0)
          , @SCHEDULE_FUTURE_ITEMS varchar(25)
          , @last_scheduled_date   datetime2(0)
          , @frequency_schedule_id int
          , @patient_order_id      int;

        set @override_offset = isnull(@override_offset, 1);
        set @period_begin = dateadd(day, @override_offset, cast(getdate() as date));
        set @is_debug = isnull(@is_debug, 0);

        declare 
            @order_administrations table
            (
              [frequency_schedule_id]   [int] null
            , [frequency_type_name]     [sysname] null
            , [frequency_schedule_name] [sysname] null
            , [site_id]                 [int] null
            , [patient_order_id]        [bigint] null
            , [proposed_scheduled_date] [date] null
            , [existing_scheduled_date] [date] null
            , [period_begin]            [datetime2](07) null
            , [future_days]             [tinyint]
            , [period_end]              [datetime2](0) null
            , [last_scheduled_date]     [datetime2](0) null
            , [on_hold]                 [bit] null
            , [missed_dose]             [bit] null
            , [order_status]            [varchar](10));

        declare 
            @tmp_schedule table
            (
              [point_in_time]                     bit
            , [administration_scheduled_datetime] datetimeoffset(7)
            , [stop_scheduled_datetime]           datetimeoffset(7));

        declare 
            @OrderStatus table
            (
              [valid_status] varchar(10));

        insert into @OrderStatus([valid_status])
        values('OnHold'),('OnGoing'),('Pending');

        declare 
            @future_item_days table
            (
              [site_id]     int
            , [future_days] [tinyint]);

        insert into @future_item_days
            ([site_id]
           , [future_days]
            )
        select [sop].[site_id]
             , case isnumeric([sop].[option_value])
                   when 1
                       then [sop].[option_value]
                   else 3
               end as [future_days]
        from   [dbo].[options] as [opt]
               inner join [dbo].[site_options] as [sop] on [opt].[id] = [sop].[option_id]
        where  [opt].[name] = 'SCHEDULE_FUTURE_ITEMS';

        with cte_orders
             as (select    [po].[frequency_schedule_id] as                                                               [frequency_schedule_id]
                         , [ft].[name] as                                                                                [frequency_type_name]
                         , [fs].[name] as                                                                                [frequency_schedule_name]
                         , @period_begin as                                                                              [period_begin]
                         , [fi].[future_days]
                         , dateadd(minute, -1, cast(dateadd(day, [fi].[future_days], @period_begin) as datetime2(0))) as [period_end]
                         , cast([po].[begin_datetime] as datetime2(0)) as                                                [order_start_datetime]

/**********************************************************************
 Generate a fake order_end (when value is null) for where clause usage 
**********************************************************************/
                         , cast(isnull([po].[end_datetime], dateadd(year, 5, getdate())) as datetime2(0)) as             [order_end_datetime]
                         , null as                                                                                       [existing_scheduled_date]
                         , [last_scheduled_date]
                         , [po].[id] as                                                                                  [patient_order_id]
                         , 0 as                                                                                          [on_hold]
                         , 0 as                                                                                          [missed_dose]
                         , [p].[site_id]
                         , [po].[order_status]
                 from      [patients] as [p]
                           inner join [patient_orders] as [po] on [po].[patient_id] = [p].[id]
                           inner join @future_item_days as [fi] on [p].[site_id] = [fi].[site_id]
                           inner join [frequency_schedules] as [fs] on [po].[frequency_schedule_id] = [fs].[id]
                           inner join [frequency_types] as [ft] on [fs].[frequency_type_id] = [ft].[id]
                           outer apply
                 (
                     select top 1 [ca_oa].[administration_scheduled_datetime] as [last_scheduled_date]
                     from         [order_administrations] as [ca_oa]
                     where        [po].[id] = [ca_oa].[patient_order_id]
                     order by [ca_oa].[administration_scheduled_datetime] desc
                 ) as [last_administration]
                 where [po].[frequency_schedule_id] <> 0
                       and [ft].[name] in('Daily', 'Weekly', 'Interval')
                       and [po].[order_status] in
                 (
                     select [valid_status]
                     from   @OrderStatus
                 ))

             insert into @order_administrations
                 ([frequency_schedule_id]
                , [frequency_type_name]
                , [frequency_schedule_name]
                , [site_id]
                , [patient_order_id]
                , [proposed_scheduled_date]
                , [existing_scheduled_date]
                , [period_begin]
                , [future_days]
                , [period_end]
                , [last_scheduled_date]
                , [on_hold]
                , [missed_dose]
                , [order_status]
                 )
             select    [gen].[frequency_schedule_id]
                     , [gen].[frequency_type_name]
                     , [gen].[frequency_schedule_name]
                     , [gen].[site_id]
                     , [gen].[patient_order_id]
                     , [proposed_date].[scheduled_date]
                     , [existing_date].[scheduled_date]
                     , [gen].[period_begin]
                     , [gen].[future_days]
                     , [gen].[period_end]
                     , [gen].[last_scheduled_date]
                     , [gen].[on_hold]
                     , [gen].[missed_dose]
                     , [gen].[order_status]
             from      [cte_orders] as [gen]
                       outer apply
             (
                 select distinct 
                        [calendar].[the_date] as [scheduled_date]
                 from   [dbo].[frequency_calendar] as [calendar]
                 where  [calendar].[the_date] between [gen].[period_begin] and [gen].[period_end]
             ) as [proposed_date]
                       outer apply
             (
                 select distinct 
                        cast([ca_oa].[administration_scheduled_datetime] as date) as [scheduled_date]
                 from   [order_administrations] as [ca_oa]
                 where  [gen].[patient_order_id] = [ca_oa].[patient_order_id]
                        and cast([ca_oa].[administration_scheduled_datetime] as date) = [proposed_date].[scheduled_date]
             ) as [existing_date]
             where [gen].[order_start_datetime] <= [gen].[period_end]
                   and [gen].[order_end_datetime] >= @period_begin;

        if @override_patient_order_id is not null
            begin
                delete @order_administrations
                where  [patient_order_id] <> @override_patient_order_id;
            end;

        declare csr cursor local fast_forward
        for select distinct [pending_orders].[frequency_schedule_id]
                          , [pending_orders].[patient_order_id]
                          , [pending_orders].[period_end]
                          , [pending_orders].[last_scheduled_date]
            from @order_administrations as [pending_orders]
            order by [patient_order_id];
        open csr;

        fetch next from csr into 
            @frequency_schedule_id
          , @patient_order_id
          , @period_end
          , @last_scheduled_date;

        while @@FETCH_STATUS = 0
            begin
                delete @tmp_schedule;

                if @last_scheduled_date is not null
                    begin
                        if not exists
                            (
                            select null
                            from @order_administrations as [pending_orders]
                            where [patient_order_id] = @patient_order_id
                              and existing_scheduled_date is null
                            )
                            begin
                                set @period_begin = @last_scheduled_date;
                            end;
                    end;

                if @is_debug = 1
                    begin
                        select @frequency_schedule_id as [@frequency_schedule_id]
                             , @patient_order_id as      [@patient_order_id     ]
                             , @period_begin as          [@period_begin         ]
                             , @period_end as            [@period_end           ]
                             , @last_scheduled_date as   [@last_scheduled_date  ];
                    end;

                insert into @tmp_schedule
                    ([point_in_time]
                   , [administration_scheduled_datetime]
                   , [stop_scheduled_datetime]
                    )
                execute [dbo].[get_frequency_schedule_items] 
                    @frequency_schedule_id
                  , @period_begin
                  , @period_end;

                insert into [dbo].[order_administrations]
                    ([patient_order_id]
                   , [point_in_time]
                   , [on_hold]
                   , [missed_dose]
                   , [administration_scheduled_datetime]
                   , [administration_system_datetime]
                   , [administering_user_id]
                   , [administration_datetime]
                   , [stop_scheduled_datetime]
                   , [stop_input_datetime]
                   , [stop_user_id]
                   , [stop_datetime]
                   , [acknowledge_user_id]
                   , [acknowledge_datetime]
                    )
                select [pending_orders].[patient_order_id]
                     , [schedule].[point_in_time]
                     , case
                           when [pending_orders].[order_status] = 'Held'
                               then 1
                           else 0
                       end as  [on_hold]
                     , [pending_orders].[missed_dose]
                     , [schedule].[administration_scheduled_datetime]
                     , null as [administration_system_datetime]
                     , null as [administering_user_id]
                     , null as [administration_datetime]
                     , null as [stop_scheduled_datetime]
                     , null as [stop_input_datetime]
                     , null as [stop_user_id]
                     , null as [stop_datetime]
                     , null as [acknowledge_user_id]
                     , null as [acknowledge_datetime]
                from   @tmp_schedule as [schedule]
                       cross join @order_administrations as [pending_orders]
                where  [pending_orders].[patient_order_id] = @patient_order_id
                       and [pending_orders].[existing_scheduled_date] is null
                       and cast([schedule].[administration_scheduled_datetime] as datetime2(0)) between [pending_orders].[period_begin] and [pending_orders].[period_end]
                       and cast([schedule].[administration_scheduled_datetime] as date) = [pending_orders].[proposed_scheduled_date];

                --if @is_debug = 1
                --    begin
                --        select [point_in_time]
                --             , [administration_scheduled_datetime]
                --             , [stop_scheduled_datetime]
                --        from   @tmp_schedule;
                --    end;

                set @last_scheduled_date = null;

                fetch next from csr into 
                    @frequency_schedule_id
                  , @patient_order_id
                  , @period_end
                  , @last_scheduled_date;
            end;

        close csr;

        deallocate csr;

        if @is_debug = 1
            begin

                select [oa].[frequency_schedule_id]
                     , [oa].[frequency_type_name]
                     , [oa].[frequency_schedule_name]
                     , [oa].[site_id]
                     , [oa].[patient_order_id]
                     , [oa].[proposed_scheduled_date]
                     , [oa].[existing_scheduled_date]
                     , [oa].[period_begin]
                     , [oa].[future_days]
                     , [oa].[period_end]
                     , [oa].[last_scheduled_date]
                     , [oa].[on_hold]
                     , [oa].[missed_dose]
                     , [oa].[order_status]
                from   @order_administrations as [oa];
            end;
    end;

go

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'Procedure Purpose: Generate Future Order Administrations
This procedure is designed to run without any input parameters
input parameters are provided for special situational cases
'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'PROCEDURE'
  , @level1name = N'generate_order_administrations';
go