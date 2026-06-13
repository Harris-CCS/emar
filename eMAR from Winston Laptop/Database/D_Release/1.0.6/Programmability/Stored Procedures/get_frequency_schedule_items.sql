create procedure [dbo].[get_frequency_schedule_items] 
      @frequency_schedule_id [int]               = 0
    , @schedule_period_begin [datetimeoffset](7) = null ---(when auto generating new orders time, this will be the last order administration datetime scheduled on the frequencyID)
    , @schedule_period_end   [datetimeoffset](7) = null
	, @patient_site_id int = null
	, @duration int = null
    , @duration_unit_id int = null
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
          , @future_days            int
		  , @duration_total_minutes int = null
		  , @temp_datetime_end      datetime
		  , @temp_time_end          varchar(8);

		  --*****************************************************
		  -- When adding a quick list item to the cart, the first administration is starting
		  -- when the second administration should be.
		  -- So we're offset by one time interval.
		  -- Things are fine when the "seconds" portion of the start date
		  -- happens to be 00.
		  -- And we don't show seconds in the UI anyways.  It's only hour and minute.
		  -- so figure out how manys econds there are,
		  -- and then subtract that many seconds to get us to 00 seconds.
		  -- Jim Hoos, Winston Murdock, 08/26/2021.

		  -- Get the number of seconds.
		  declare @start_seconds int = datepart(SECOND, @schedule_period_begin)

		  -- Now subtract that many seconds so that this is whatever hour and minute and 00 seconds.
		  set @schedule_period_begin = dateadd(second, -@start_seconds, @schedule_period_begin)
		  --*****************************************************

          -- this change is not needed since UI will 
--        if @schedule_period_begin is not null
--        begin
--          select @schedule_period_begin = dateadd(second,-datepart(second,@schedule_period_begin),@schedule_period_begin) ;
--        end;

		-- If the API told us which site the user is logged into, then we need to use that site's time zone.
		-- Else, use the time zone of the frequency's site.
		-- When a site in one time zone code shares frequencies from another time zone, we run into issues here
		-- because this was pulling the time for the code share "from" site, not for the code share "to" site that the user is logged into.
		-- Winston Murdock, 07/07/2021.
		if (@patient_site_id is null)
		begin
			-- No site was specified.
			-- Use the frequency's site id to get the time zone.
			-- This was the behavior prior to this change.
			select @local_time_zone_name = [site].[time_zone_name]
				 , @current_utc_offset = [tz].[current_utc_offset]
			from   [dbo].[sites] as [site]
				   inner join [dbo].[frequency_schedules] as [frequency_schedules] on [frequency_schedules].[site_id] = [site].[id]
				   left join [sys].[time_zone_info] as [tz] on [site].[time_zone_name] = [tz].[name]
			where  [frequency_schedules].[id] = @frequency_schedule_id;
		end
		else
		begin
			-- Use the specified site id to get the time zone.
			select @local_time_zone_name = [site].[time_zone_name]
				 , @current_utc_offset = [tz].[current_utc_offset]
			from   [dbo].[sites] as [site]
				   left join [sys].[time_zone_info] as [tz] on [site].[time_zone_name] = [tz].[name]
			where  [site].[id] = @patient_site_id;
		end

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

		-- calculate the total duration in minutes if necessary
        if @duration is not null and @duration_unit_id is not null and @schedule_period_end is null
			begin
                declare @duration_in_minutes int, @day_adder int;
				-- get the duration in minutes value.
                select @duration_in_minutes = duration_in_minutes from duration_units where id=@duration_unit_id;
                declare @frequency_interval int, @frequency_interval_units varchar(10), @frequency_type varchar(10)
        			  , @interval_start_time time(0), @interval_end_minutes int, @frequency_type_recurring int;
				-- get the frequency related values based upon the frequency schedule id provided as input
                select @frequency_interval = fs.frequency_interval, @frequency_interval_units = fsu.name, @frequency_type = ft.name
					 , @interval_start_time = fs.interval_start_time, @interval_end_minutes = fs.interval_end_minutes
					 , @frequency_type_recurring = fs.frequency_type_recurring
					from frequency_schedules fs
                    join frequency_interval_units fsu on fs.frequency_interval_unit_id = fsu.id
					join frequency_types ft on fs.frequency_type_id = ft.id
                    where fs.id = @frequency_schedule_id;
                -- check if the duration_in_minutes is 0 (Dose type not time value)
                -- if so then need to calculate @duration_total_minutes using @frequency_schedule_id
                if @duration_in_minutes = 0
                    begin
						---------- Interval Schedule
						if @frequency_type = 'Interval' and @frequency_interval > 0
							-- do we need to use fs.frequency_type_recurring too?
							begin
								-- get the duration minutes using just the frequency schedule portion
								select @duration_in_minutes = (case  @frequency_interval_units when 'Minutes' then @frequency_interval
																when 'Hours' then @frequency_interval * 60
																when 'Days' then @frequency_interval * 60 * 24
																when 'NA' then 0
																else 0 end);
								-- multiply the previously derived duration minutes by the # of doses/durations
								set @duration_total_minutes = @duration * @duration_in_minutes;
								set @day_adder = 1;
							end;
						-------- Recurring Fixed Times
						else if @frequency_type in ('Daily', 'Weekly') and @frequency_interval = 0
							begin
								declare @thedate date, @freqtime time(0);
								WITH ordered AS (
								SELECT ROW_NUMBER() OVER (ORDER BY the_date,frequency_time) AS RowNumber,the_date,frequency_time
								FROM   [dbo].[frequency_schedules] as [fs]
										inner join [dbo].[frequency_interval_day_times] as [fjd] on [fs].[id] = [fjd].[frequency_schedule_id]
										inner join [dbo].[frequency_calendar] as [cal] on [fjd].[frequency_day_id] = [cal].[the_day_of_week]
																					   or [fjd].[frequency_day_id] = @DAILY_SCHEDULE
								WHERE  [fs].[id] = @frequency_schedule_id and cast(cast(the_date as varchar(10)) + ' ' + cast(frequency_time as varchar(8)) as datetime) >= cast(@schedule_period_begin as datetime))
								SELECT @thedate=the_date, @freqtime=frequency_time FROM ordered WHERE RowNumber=@duration;
								-- calculate the duration total minutes using the difference between the schedule period begin and the calculated period end
								select @duration_total_minutes = DATEDIFF(minute, cast(@schedule_period_begin as datetime),
																			      cast(convert(varchar(10), @thedate, 23) +  ' ' + convert(varchar(8), @freqtime, 108) as datetime));
								set @day_adder = 1;
							end;
						---------- Recurring Interval Times
						else if @frequency_type in ('Daily', 'Weekly') and @frequency_interval > 0
							begin
								declare @dailyendtime time(0), @dailydoses int, @remainingdosestoday int, @numdays int
								      , @remainingdoseslastday int, @current_datetime2 datetime2(0), @starthours int
									  , @endtime datetime;
								set @dailyendtime = DATEADD(minute, @interval_end_minutes, @interval_start_time);
								set @dailydoses = @interval_end_minutes/60/@frequency_interval + 1;
								-- setting @current_datetime2 based upon now or should it be based upon the scheduled begin time (@schedule_period_begin)?
								set @current_datetime2 = cast(switchoffset(sysutcdatetime(), @current_utc_offset) as datetime2(0));
								-- calculate the remaining doses for today
								if (DATEPART(hour,@current_datetime2) + (@duration * @frequency_interval)) > DATEPART(hour,@dailyendtime)
									begin
										-- the doses go beyond today 
										set @remainingdosestoday = DATEDIFF(hour, @current_datetime2,
																			cast(convert(varchar(10), @current_datetime2, 23) +  ' ' + convert(varchar(8), @dailyendtime, 108) as datetime))/ @frequency_interval;
									end;
									else
									begin
										-- the doses are today only
										set @remainingdosestoday = @duration;
									end;
								-- determine the remaining doses for today by comparing against the daily doses value
								set @remainingdosestoday = (case when @remainingdosestoday > @dailydoses then @dailydoses else @remainingdosestoday end);
								set @starthours = DATEDIFF(hour, '00:00:00', @interval_start_time);
								-- check if the doses are today only
								if @remainingdosestoday >= @duration
									begin
										-- calculate the end time (today)
										set @endtime = DATEADD(hour, DATEPART(hour,@current_datetime2) + (@remainingdosestoday * @frequency_interval), DATEADD(day, DATEDIFF(day, 0, @current_datetime2), 0));
									end;
									else
									begin
										-- end time is not today. calculate number of days
										set @numdays = (@duration - @remainingdosestoday)/@dailydoses;
										set @numdays = (case when ((@duration - @remainingdosestoday) % @dailydoses) != 0 then @numdays + 1 else @numdays end);
										set @remainingdoseslastday = (@duration - @remainingdosestoday) % @dailydoses;
										-- calculate the ending day (at '00:00:00')
										set @endtime = DATEADD(day, DATEDIFF(day, 0, @current_datetime2), @numdays);
										-- add the hours to the ending day
										if @remainingdoseslastday > 0
											begin
												-- does not end at interval end time so calculate actual end time
												set @endtime = DATEADD(hour, @starthours + ((@remainingdoseslastday - 1) * @frequency_interval), @endtime);
											end;
											else
											begin
												-- ends at interval end time
												set @endtime = DATEADD(hour, @starthours + ((@dailydoses - 1) * @frequency_interval), @endtime);
											end;
									end;
								-- calculate duration total minutes by comparing the start datetime against the ending datetime
								set @duration_total_minutes = DATEDIFF(minute, @current_datetime2, @endtime);
								-- calculate the number of days that need to be added for the @future_days calculation - a fudge factor
								-- tested a variety of possible duration values and it appears to work in all cases
								-- can rewrite this code when time allows
								set @day_adder = (case when @numdays is null then 1 
													   when @duration <= @dailydoses then 2
													   when (@duration % @dailydoses) = 0 then 2
													   when @duration < (@remainingdosestoday + @dailydoses) then 1
													   when (@duration % @dailydoses) = @remainingdosestoday then 0
													   when (@duration % @dailydoses) > @remainingdosestoday then 2
													   else 1 end);
							end;
						-------- one time order
						else if @frequency_type in ('One Time', 'STAT', 'Continuous')
							begin
								set @duration_total_minutes = 1;
								set @day_adder = 1;
							end;
						-------- unknown frequency schedule scenario
						else
							begin
								set @duration_total_minutes = 0;
								set @day_adder = 1;
							end;
                    end;
                    else
                    begin
                        set @duration_total_minutes = @duration * @duration_in_minutes;
						set @day_adder = 1;
                    end;
                -- only set @future_days if there is a @duration_total_minutes with a non-null value
                if @duration_total_minutes is not null
                    begin
				        select @future_days = (case when @duration_total_minutes >= 60 * 24 then @duration_total_minutes / (60 * 24) + @day_adder
						        					else @day_adder end);
                    end;
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
				if @duration_total_minutes is not null
					begin
						set @temp_datetime_end = DATEADD(minute,@duration_total_minutes,@datetime_period_begin);
						set @temp_time_end = CONVERT(VARCHAR(8), @temp_datetime_end, 108);
						set @schedule_period_end = dateadd(day, @future_days - 1, cast(convert(char(8), @datetime_period_begin, 112) + ' ' + @temp_time_end as datetime));
						set @time_period_end_offset = @duration_total_minutes;
--						set @time_period_end_offset = @future_days * 1440;

						-- If the day part of @schedule_period_end
						-- is earlier than the day part of @temp_datetime_end,
						-- then the x number of doses took us into a future day.
						-- And we need to bump @schedule_period_end up by the number of
						-- days difference so that the end date/time is not in the past,
						-- and so that it is also not before the start date/time.
						-- Winston Murdock, Romel Ursua.  08/26/2021.
						declare @days_difference int = datediff(day, @schedule_period_end, @temp_datetime_end)
						if @days_difference > 0
						begin
							set @schedule_period_end = dateadd(day, @days_difference, @schedule_period_end)
						end --end if (date difference > 0?)
					end;
					else
					begin
						set @schedule_period_end = dateadd(day, @future_days - 1, cast(convert(char(8), @datetime_period_begin, 112) + ' 23:59:00' as datetime));
						set @time_period_end_offset = @future_days * 1440;
					end;
            end;

        if @schedule_period_end is null
            begin
--				set @date_period_end = cast(@schedule_period_end as date);
                set @date_period_end = dateadd(day, @future_days - 1, cast(@local_current_time as date));
				if @duration_total_minutes is null
					begin
						set @datetime_period_end = cast(convert(char(8), @date_period_end, 112) + ' 23:59:00' as datetime);
						set @time_period_end_offset = @future_days * 1440;
					end;
					else
					begin
						set @temp_datetime_end = DATEADD(minute, @duration_total_minutes, @datetime_period_begin);
						set @temp_time_end = CONVERT(VARCHAR(8), @temp_datetime_end, 108);
						set @datetime_period_end = cast(convert(char(8), @date_period_end, 112) +  ' ' + @temp_time_end as datetime);
						set @time_period_end_offset = @duration_total_minutes;
					end;
            end;
            else
            begin
                set @date_period_end = cast(@schedule_period_end as date);
                set @datetime_period_end = cast(@schedule_period_end as datetime);
				-- revisit the assigning of @time_period_end_offset
				if @duration_total_minutes is null
					begin
						set @time_period_end_offset = datediff(minute, @schedule_period_begin, @schedule_period_end);
					end;
					else
					begin
						set @time_period_end_offset = @duration_total_minutes;
					end;
            end;

		-- Hack to determine if minute rounding involved (between 59.5 and 0.499 secs)
		-- Currently only concerned with scenario where duration data provided and not a recurring fixed times type or recurring interval times type
		-- This could cause an extra administration to be returned
		if @duration_total_minutes is not null and (@frequency_type not in ('Daily', 'Weekly'))
			begin
				declare @seconds int, @milliseconds int;
				set @seconds = DATEPART(second, @datetime_period_begin);
				set @milliseconds = DATEPART(millisecond, @datetime_period_begin);
				if (@seconds = 59 and @milliseconds >= 500) or (@seconds = 0 and @milliseconds < 500)
					begin
						-- subtract 1 second to remove extra administration
						set @datetime_period_end = DATEADD(second, -1, @datetime_period_end);
					end;
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