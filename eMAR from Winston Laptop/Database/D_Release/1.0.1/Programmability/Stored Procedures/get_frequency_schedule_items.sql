create procedure [dbo].[get_frequency_schedule_items] 
      @frequency_schedule_id [int]               = 0
    , @schedule_period_begin [datetimeoffset](7) = null ---(when auto generating new orders time, this will be the last order administration datetime scheduled on the frequencyID)
    , @schedule_period_end   [datetimeoffset](7) = null
as
    begin

        declare 
            @site_id                [int]
          , @date_period_begin      [date]
          , @date_period_end        [date]
          , @datetime_period_begin  [datetime]
          , @datetime_period_end    [datetime]
          , @time_period_begin      [time](0)
          , @time_period_end_offset [int]
          , @local_time_zone_name   sysname
          , @current_utc_offset     nvarchar(12)
          , @local_current_time     datetime
          , @SCHEDULE_FUTURE_ITEMS  varchar(25)
          , @DAILY_SCHEDULE         int          = 0
          , @future_days            int;

          -- this change is not needed since UI will 
--        if @schedule_period_begin is not null
--        begin
--          select @schedule_period_begin = dateadd(second,-datepart(second,@schedule_period_begin),@schedule_period_begin) ;
--        end;

        select @local_time_zone_name = [site].[time_zone_name]
             , @current_utc_offset = [tz].[current_utc_offset]
        from   [dbo].[sites] as [site]
               inner join [dbo].[frequency_schedules] as [frequency_schedules] on [frequency_schedules].[site_id] = [site].[id]
               left join [sys].[time_zone_info] as [tz] on [site].[time_zone_name] = [tz].[name]
        where  [frequency_schedules].[id] = @frequency_schedule_id;

        select @SCHEDULE_FUTURE_ITEMS = [sop].[option_value]
        from   [dbo].[options] as [opt]
               inner join [dbo].[site_options] as [sop] on [opt].[id] = [sop].[option_id]
        where  [sop].[site_id] = @site_id
               and [opt].[name] = 'SCHEDULE_FUTURE_ITEMS';

        if isnumeric(@SCHEDULE_FUTURE_ITEMS) = 1
            begin
                set @future_days = @SCHEDULE_FUTURE_ITEMS;
            end;
            else
            begin
                set @future_days = 3;
            end;

        if @schedule_period_begin is null
            begin
                set @local_current_time = cast(switchoffset(sysutcdatetime(), @current_utc_offset) as datetime2(0));
                set @schedule_period_begin = @local_current_time at time zone @local_time_zone_name;
                set @date_period_begin = cast(@local_current_time as date);
                set @datetime_period_begin = cast(@local_current_time as datetime);
                set @time_period_begin = cast(@datetime_period_begin as time(0));
            end;
            else
            begin
                set @local_current_time = cast(@schedule_period_begin as datetime2(0));
                set @date_period_begin = cast(@local_current_time as date);
                set @datetime_period_begin = cast(@local_current_time as datetime);
                set @time_period_begin = cast(@schedule_period_begin as time(0));
            end;

        if @schedule_period_end is null
           and @schedule_period_begin is not null
            begin
                set @schedule_period_end = dateadd(day, @future_days - 1, cast(convert(char(8), @datetime_period_begin, 112) + ' 23:59:00' as datetime));
                set @time_period_end_offset = @future_days * 1440;
            end;

        if @schedule_period_end is null
            begin
                set @date_period_end = dateadd(day, @future_days - 1, cast(@local_current_time as date));
                set @datetime_period_end = cast(convert(char(8), @date_period_end, 112) + ' 23:59:00' as datetime);
                set @time_period_end_offset = @future_days * 1440;
            end;
            else
            begin
                set @date_period_end = cast(@schedule_period_end as date);
                set @datetime_period_end = cast(@schedule_period_end as datetime);
                set @time_period_end_offset = datediff(minute, @schedule_period_begin, @schedule_period_end);
            end;

        select    [sched].[point_in_time]
                , [sched].[sched_datetime] at time zone @local_time_zone_name as                                          [sched_datetime_tz]
                , case
                      when [sched].[point_in_time] = 1
                          then null
                      else [sched].[sched_stop_datetime]
                  end at time zone @local_time_zone_name as                                                               [stop_datetime_tz]
        from
        (
            -------- Reccuring Fixed Times
            select dense_rank() over(
                   order by [cal].[the_date]) as                                                                          [sched_order]
                 , [fs].[id] as                                                                                           [frequency_schedules_id]
                 , [fs].[name] as                                                                                         [frequency_schedule_name]
                 , [fs].[point_in_time] as                                                                                [point_in_time]
                 , cast(cast([cal].[the_date] as datetime) + cast([fjd].[frequency_time] as datetime) as datetime2(7)) as [sched_datetime]
                 , cast(@schedule_period_end as datetime2(7)) as                                                          [sched_stop_datetime]
                 , [fs].[frequency_type_recurring]
            from     [dbo].[frequency_schedules] as [fs]
                     inner join [dbo].[frequency_interval_day_times] as [fjd] on [fs].[id] = [fjd].[frequency_schedule_id]
                     inner join [dbo].[frequency_calendar] as [cal] on [fjd].[frequency_day_id] = [cal].[the_day_of_week]
                                                                       or [fjd].[frequency_day_id] = @DAILY_SCHEDULE
                     inner join [dbo].[frequency_types] as [ft] on [ft].[id] = [fs].[frequency_type_id]
            where   [cal].[the_date] between @date_period_begin and @date_period_end
                    and [fs].[id] = @frequency_schedule_id
                    and [fs].[frequency_interval] = 0
                    and [ft].[name] in(N'Daily', N'Weekly')
            union
            -------- one time order
            select 1 as                                          [sched_order]
                 , [fs].[id] as                                  [frequency_schedules_id]
                 , [fs].[name] as                                [frequency_schedule_name]
                 , [fs].[point_in_time] as                       [point_in_time]
                 , cast(@local_current_time as datetime2(7)) as  [sched_datetime]
                 , cast(@schedule_period_end as datetime2(7)) as [sched_stop_datetime]
                 , [fs].[frequency_type_recurring]
            from     [dbo].[frequency_schedules] as [fs]
                     inner join [dbo].[frequency_types] as [ft] on [ft].[id] = [fs].[frequency_type_id]
            where   [fs].[id] = @frequency_schedule_id
                    and [ft].[name] in('One Time', 'STAT', 'Continuous')
            union
            ---------- Reccuring Interval Times
            select       dense_rank() over(
                         order by [cal].[the_date]) as                                                                          [sched_order]
                       , [fs].[id] as                                                                                           [frequency_schedules_id]
                       , [fs].[name] as                                                                                         [frequency_schedule_name]
                       , [fs].[point_in_time] as                                                                                [point_in_time]
                       , cast(cast([cal].[the_date] as datetime) + cast([fjt].[frequency_time] as datetime) as datetime2(7)) as [sched_datetime]
                       , cast(@schedule_period_end as datetime2(7)) as                                                          [sched_stop_datetime]
                       , [fs].[frequency_type_recurring]
            from           [dbo].[frequency_schedules] as [fs]
                           inner join [dbo].[frequency_interval_day_times] as [fjd] on [fs].[id] = [fjd].[frequency_schedule_id]
                           inner join [dbo].[frequency_interval_units] as [fi] on [fi].[id] = [fs].[frequency_interval_unit_id]
                           cross apply
            (
                select cast(dateadd(minute, sequence, '') as time(0)) as [frequency_time]
                from   [dbo].[frequency_minutes]
                where  sequence between datediff(minute, '00:00:00', [fs].[interval_start_time]) and datediff(minute, '00:00:00', [fs].[interval_start_time]) + [fs].[interval_end_minutes]
                       and (sequence - datediff(minute, '00:00:00', [fs].[interval_start_time])) % (case
                                                                                                    --- Hours and Minutes and Days only valid selection for the Reccuring Interval Times
                                                                                                        when [fi].[name] = 'Minutes'
                                                                                                            then [fs].[frequency_interval]
                                                                                                        when [fi].[name] = 'Hours'
                                                                                                            then [fs].[frequency_interval] * 60
                                                                                                        when [fi].[name] = 'Days'
                                                                                                            then [fs].[frequency_interval] * 60 * 24
                                                                                                        else 0
                                                                                                    end) = 0
            ) as [fjt]
                           inner join [dbo].[frequency_calendar] as [cal] on [fjd].[frequency_day_id] = [cal].[the_day_of_week]
                                                                             or [fjd].[frequency_day_id] = @DAILY_SCHEDULE
                           inner join [dbo].[frequency_types] as [ft] on [ft].[id] = [fs].[frequency_type_id]
            where [cal].[the_date] between @date_period_begin and @date_period_end  --Reporting Period
                  and [fs].[id] = @frequency_schedule_id
                  and [ft].[name] in(N'Daily', N'Weekly')
                  and [fs].[frequency_interval] > 0
            union
            ---------- Interval Schedule
            select    1 as                                          [sched_order]
                    , [fs].[id] as                                  [frequency_schedules_id]
                    , [fs].[name] as                                [frequency_schedule_name]
                    , [fs].[point_in_time] as                       [point_in_time]
                    , [fjt].[frequency_time] as                     [sched_datetime]
                    , cast(@schedule_period_end as datetime2(7)) as [sched_stop_datetime]
                    , [fs].[frequency_type_recurring]
            from      [dbo].[frequency_schedules] as [fs]
                      inner join [dbo].[frequency_interval_units] as [fi] on [fi].[id] = [fs].[frequency_interval_unit_id]
                      inner join [dbo].[frequency_types] as [ft] on [ft].[id] = [fs].[frequency_type_id]
                      cross apply
            (
                select cast(@date_period_begin as datetime) + cast(dateadd(minute, sequence, '') as datetime) as [frequency_time]
                     , sequence - datediff(minute, '00:00:00', @time_period_begin) as                            [start_time_offset]
                from   [dbo].[frequency_minutes]
                where  sequence between datediff(minute, '00:00:00', @time_period_begin) and datediff(minute, '00:00:00', @time_period_begin) + @time_period_end_offset
                       and cast(@date_period_begin as datetime) + cast(dateadd(minute, sequence, '') as datetime) between @datetime_period_begin and @datetime_period_end  --Reporting Period
                       and (sequence - datediff(minute, '00:00:00', @time_period_begin)) % (case
                                                                                            --- Hours and Minutes and Days only valid selection for the Reccuring Interval Times
                                                                                                when [fi].[name] = 'Minutes'
                                                                                                    then [fs].[frequency_interval]
                                                                                                when [fi].[name] = 'Hours'
                                                                                                    then [fs].[frequency_interval] * 60
                                                                                                when [fi].[name] = 'Days'
                                                                                                    then [fs].[frequency_interval] * 60 * 24
                                                                                                else 0
                                                                                            end) = 0
            ) as [fjt]
            where [fs].[id] = @frequency_schedule_id
                  and [ft].[name] in(N'Interval')
                  and [fs].[frequency_interval] > 0
        ) as [sched]
        where([sched].[sched_order] % [sched].[frequency_type_recurring] = 1
              or [sched].[frequency_type_recurring] = 1)
             and [sched].[sched_datetime] between @datetime_period_begin and @datetime_period_end
        order by [sched].[sched_datetime];

    end;

go

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'Procedure used to generate schedules based on predefined frequencies'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'PROCEDURE'
  , @level1name = N'get_frequency_schedule_items';
go