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

--This SP looks through all ongoing, pending, and onhold orders and
-- genereates future administration events if they are a daily,
-- interval, or weekly frequency.
-- 
-- It uses site option 3 (SCHEDULE_FUTURE_ITEMS) to see how many
-- days out we want to schedule for.
-- 
-- Example.
-- If an order was placed on 02/01, and if that option is set to
-- seven days for this site, then we will have administrations
-- going through 02/08.
-- When this job runs on 02/02, it will create administrations
-- through 02/09.  When it runs on 02/03, it will create
-- administrations through 02/10.  And so on.
-- 
-- It is run by a SQL job at 2 AM every day.
--
-- I think David Mehegan initialy wrote this (possibly in
-- conjunction with Bradley Marshall).
-- Winston Murdock made a massive amount of updates to it
-- the week of 02/07/2022 - 02/11/2022 to fix some issues
-- seen on Emerus Prod.
-- 
-- Summary of the changes...
--
-- 1) If an order's end_datetime is null, we always create future
--		administrations.  There was a change to the API to only
--		set end_datetime when the user selects one in the UI.
--
-- 2) Don't attempt to create future administrations for orders
--		that have a duration set.  There is a known issue in the
--		API where we don't set the end_datetime for an order
--		with durations.  We'll fix that later.  And it was
--		faster to make this change here.
--
-- 3) When calling get_frequency_schedule_items to get the list
--		of administrations within a time range, we pass in the
--		site id so that we correctly pull the time zone from
--		the patient's site and not from the frequency (which
--		handles the case when a mountain time site code shares
--		frequencies from a central time site, leading to
--		administrations being one hour offset from where they
--		should be).
--
-- 4) Since we pass in the time of the last administration as
--		start time in number 3, we will get a duplicate
--		administration in the list.  Remove that first one.
--
-- 5) Added a period_begin_cursor variable so we reset
--		period_begin each time through the cursor.
--		This prevents the value from the previous
--		iteration from affecting this iteration.
--
-- 6) If we have a last scheduled date, then we set
--		period_begin to that value (commented out the
--		previous check to see if we need to set it or not).
--
-- 7) Insetead of inserting right into order_administrations,
--		insert into a table variable.  Then do a select distinct *
--		from that table to insert into order_administrations.
--		We had to comment out the last ilne of the where for the
--		initial insert into order_administrations because we were
--		omitting the first future administrations sometimes.
--		This led to a cartesian product for some orders, which
--		led to selecting into the temp table and then selecting
--		distinct from the temp table when inserting into
--		order_administrations.

    begin

        set nocount on;

		-- Variables declarations.
        declare 
            @period_begin          datetime2(0)
          , @period_end            datetime2(0)
          , @SCHEDULE_FUTURE_ITEMS varchar(25)
          , @last_scheduled_date   datetime2(0)
          , @frequency_schedule_id int
          , @patient_order_id      int
		  , @site_id			   int
		  ;

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
            , [order_status]            [varchar](10)
			, [duration]				[int] null
			, [duration_unit_id]		[int] null
			);

		-- Holds the results from the call to get_frequency_schedule_items.
		-- Should/could have its first at the exact same time as the last
		-- administration for the patient.  We will delete that one entry
		-- if that is the case.
        declare 
            @tmp_schedule table
            (
              [point_in_time]                     bit
            , [administration_scheduled_datetime] datetimeoffset(7)
            , [stop_scheduled_datetime]           datetimeoffset(7));

		-- Only pull in orders with one of these statuses.
        declare 
            @OrderStatus table
            (
              [valid_status] varchar(10));

        insert into @OrderStatus([valid_status])
        values('OnHold'),('OnGoing'),('Pending');

		-- How many days into the future we're using (eMAR option 3).
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

		-- Common table expression.
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
						 , [po].[duration]
						 , [po].[duration_unit_id]
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
                 )
				 -- Only pull in active patients.
				 -- Winston Murdock, 09/01/2021.  EMAR-1190 (not sure of ticket number...)
				 AND [p].[is_active] = 1
				 )

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
				, [duration]
				, [duration_unit_id]
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
					 , [gen].[duration]
					 , [gen].[duration_unit_id]
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
             --where [gen].[order_start_datetime] <= [gen].[period_end]
             --      and [gen].[order_end_datetime] >= @period_begin;
			-- Only include orders with...
			-- 1) A begin time before the end of the period we're creating future administrations for.
			-- 2) 
			--		A) An end date after the beginning of the period we're creating future administrations for
			--		B) A null end date
			-- 3) No duration specified (we already created all of the administrations for these orders when they were added to the cart).
			-- Winston Murdock, 02/09/2022.  PC-26986
			where
				[gen].[order_start_datetime] <= [gen].[period_end]
				and
				(
					[gen].[order_end_datetime] >= @period_begin
					or
					[gen].[order_end_datetime] is null
				)
				and
				[gen].[duration] is null
				and
				[gen].[duration_unit_id] is null
				;

		-- If we're only running this for one order, then
		-- delete all other orders from @order_administrations.
        if @override_patient_order_id is not null
            begin
                delete @order_administrations
                where  [patient_order_id] <> @override_patient_order_id;
            end;

		-- Reset the period begin outside each iteration of the loop.
		-- This prevents us from accidentally having the previous
		-- iteration's value for this iteration.
		declare @period_begin_cursor datetime2(0)
		set @period_begin_cursor = @period_begin;

		-- Added the patient's site_id into the cursor so that we
		-- have it to pass along to get_frequency_schedule_items.
		-- This way, we aren't one hour offset on any administrations
		-- for mountain time sites that code share frequencies from
		-- central time sites.
		-- Winston Murdock, 02/09/2022.  PC-26986
        declare csr cursor local fast_forward
        for select distinct [pending_orders].[frequency_schedule_id]
                          , [pending_orders].[patient_order_id]
                          , [pending_orders].[period_end]
                          , [pending_orders].[last_scheduled_date]
						  , [pending_orders].[site_id]
            from @order_administrations as [pending_orders]
            order by [patient_order_id];
        open csr;

        fetch next from csr into 
            @frequency_schedule_id
          , @patient_order_id
          , @period_end
          , @last_scheduled_date
		  , @site_id;

        while @@FETCH_STATUS = 0
            begin
				--Reset @period_begin to the initial value.
				SET @period_begin = @period_begin_cursor

				-- Ensure this doesn't have rows from the previous iteration.
                delete @tmp_schedule;

				-- If we have a last scheduled date, then use that for period begin.
				-- Else, period begin will be today at midnight.
                if @last_scheduled_date is not null
                    begin
						-- If we have a last scheduled date, then always
						-- use that for period begin.
						-- I'm not sure of the reason for the commented out logic below,
						-- but I've commented it out to ensure we always set this properly.
						-- If we ever don't have a last scheduled date, then period begin
						-- will be midnight today.
						-- Winston Murdock, 02/10/2022.
                        --if not exists
                        --    (
                        --    select null
                        --    from @order_administrations as [pending_orders]
                        --    where [patient_order_id] = @patient_order_id
                        --      and existing_scheduled_date is null
                        --    )
                        --    begin
                                set @period_begin = @last_scheduled_date;
                        --    end;
                    end;

                if @is_debug = 1
                    begin
                        select @frequency_schedule_id as [@frequency_schedule_id]
                             , @patient_order_id as      [@patient_order_id     ]
                             , @period_begin as          [@period_begin         ]
                             , @period_end as            [@period_end           ]
                             , @last_scheduled_date as   [@last_scheduled_date  ];
                    end;

				-- In case we bump the frequency from 14 to 7 but have already made days 8-14 for an order...
				if (@period_begin < @period_end)
				BEGIN
					insert into @tmp_schedule
						([point_in_time]
					   , [administration_scheduled_datetime]
					   , [stop_scheduled_datetime]
						)
					-- Add the patient_order_id parameter.
					-- Since we're skipping the optional duration parameters,
					-- I have to specify the parameter name for each one.
					-- Winston Murdock, 04/12/PC-27077
					--execute [dbo].[get_frequency_schedule_items] 
					--	@frequency_schedule_id
					--  , @period_begin
					--  , @period_end
					--  , @site_id;
					execute [dbo].[get_frequency_schedule_items] 
						@frequency_schedule_id = @frequency_schedule_id
					  , @schedule_period_begin = @period_begin
					  , @schedule_period_end = @period_end
					  , @patient_site_id = @site_id
					  , @patient_order_id = @patient_order_id;

					-- Remove the earliest administration if the timestamp for it is the exact same as
					-- another administration for this patient_order
					--Get the time for the first administration in the temp table.
					DECLARE @temp_time datetimeoffset(7)
					SELECT TOP 1 @temp_Time = administration_scheduled_datetime FROM @tmp_schedule ORDER by administration_scheduled_datetime
					IF EXISTS (SELECT 1 FROM order_administrations WHERE patient_order_id = @patient_order_id AND administration_scheduled_datetime = @temp_time)
					BEGIN
					DELETE FROM @tmp_schedule WHERE administration_scheduled_datetime = @temp_time
					END
				END

				-- Temp table to hold the return data momentarily.
				-- We had to comment out the last line of the
				-- where clause to get all of the future administrations.
				-- But that sometimes gives us duplicates.
				-- So I insert into here, then I select distinct when
				-- inserting into order_administrations.
				-- Winston Murdock, 02/10/2022.
                declare @temp_return table
				(
					patient_order_id varchar(100),
					point_in_time varchar(100),
					on_hold varchar(100),
					missed_dose varchar(100),
					administration_scheduled_datetime varchar(100),
					[administration_system_datetime] varchar(100) null,
                    [administering_user_id] varchar(100) null,
                    [administration_datetime] varchar(100) null,
                    [stop_scheduled_datetime] varchar(100) null,
                    [stop_input_datetime] varchar(100) null,
                    [stop_user_id] varchar(100) null,
                    [stop_datetime] varchar(100) null,
                    [acknowledge_user_id] varchar(100) null,
                    [acknowledge_datetime] varchar(100) null
				)
				-- Empty the table so it doesn't have any rows
				-- from a previous iteration through the loop.
				delete from @temp_return

				insert into @temp_return
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
					   -- Commented out to prevent sometimes skipping the first administration.
					   -- We insert into the temp table and then do select distinct * from that
					   -- for inserting into order_administrations.
                       --and cast([schedule].[administration_scheduled_datetime] as date) = [pending_orders].[proposed_scheduled_date];

                --if @is_debug = 1
                --    begin
                --        select [point_in_time]
                --             , [administration_scheduled_datetime]
                --             , [stop_scheduled_datetime]
                --        from   @tmp_schedule;
                --    end;

				-- Select distinct from the temp table and insert into order_administrations.
				-- Winston Murdock, 02/10/2022.
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
				SELECT DISTINCT *
				FROM @temp_return
				ORDER BY administration_scheduled_datetime

                set @last_scheduled_date = null;

                fetch next from csr into 
                    @frequency_schedule_id
                  , @patient_order_id
                  , @period_end
                  , @last_scheduled_date
				  , @site_id;
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