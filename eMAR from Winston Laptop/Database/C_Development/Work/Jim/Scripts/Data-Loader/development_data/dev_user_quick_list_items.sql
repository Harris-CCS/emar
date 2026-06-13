if '$(load_data)' in('sample', 'live')
    begin

        print 'Loading Table: development_data\dev_user_quick_list_items.sql';

/************************************************
Building additional UQLI Items for IBEX.SITE = 36
and assign random frequencies
************************************************/

        ---~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        --- [user_quick_list_items]
        ---~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

        --generate [user_quick_list_items]
        with cte_ql
             as (select distinct
                        [dose]
                      , [medication_unit_id]
                      , [medication_route_id]
                      , [frequency_schedule_id]
                      , [order_notes]
                      , [usages_this_week]
                      , [weekly_usage_rolling_average]
                      , [medication_id]
                      , [duration_in_minutes]
                 from   [dbo].[user_quick_list_items] as [uql]
                 where  [site_id] = @dev_custom_data_site_id)
             insert into [dbo].[user_quick_list_items]
                 ([site_id]
                , [user_id]
                , [dose]
                , [medication_unit_id]
                , [medication_route_id]
                , [frequency_schedule_id]
                , [order_notes]
                , [usages_this_week]
                , [weekly_usage_rolling_average]
                , [medication_id]
                , [duration_in_minutes]
                 )
             select [users].[site_id]
                  , [users].[id] as [user_id]
                  , [cte_ql].[dose]
                  , [cte_ql].[medication_unit_id]
                  , [cte_ql].[medication_route_id]
                  , [cte_ql].[frequency_schedule_id]
                  , 'My Order Notes' ---[cte_ql].[order_notes]
                  , [cte_ql].[usages_this_week]
                  , [cte_ql].[weekly_usage_rolling_average]
                  , [cte_ql].[medication_id]
                  , [cte_ql].[duration_in_minutes]
             from   [cte_ql]
                    cross join [dbo].[users]
                    left join [dbo].[user_quick_list_items] as [uql] on [users].[site_id] = [uql].[site_id]
                                                                        and [uql].[medication_id] = [cte_ql].[medication_id]
                                                                        and [users].[id] = [uql].user_id
                                                                        and [uql].[frequency_schedule_id] = 0
             where  [users].[site_id] = @dev_custom_data_site_id
                    and [uql].user_id is null;
        select @@rowcount as [insert into dbo.user_quick_list_items];

        --assign [frequency_schedule_id] 
        update    [uqli] set
            [frequency_schedule_id] = [fs].[frequency_schedule_id]
        from
        (
            select row_number() over(
                   order by [s1].[id]) as [id]
                 , [s1].[id] as           [frequency_schedule_id]
                 , [s1].[site_id]
            from   [dbo].[frequency_schedules] as [s1]
            where  [s1].[site_id] = @dev_custom_data_site_id
        ) [fs]
        inner join
        (
            select row_number() over(
                   order by [s2].[id]) as [id]
                 , [s2].[id] as           [user_quick_list_item_id]
                 , [s2].user_id
                 , [s2].[site_id]
            from   [dbo].[user_quick_list_items] as [s2]
            where  [s2].[site_id] = @dev_custom_data_site_id
        ) [uql] on [fs].[id] = [uql].[id] % 124 + 1
        inner join [dbo].[user_quick_list_items] as [uqli] on [uqli].[id] = [uql].[user_quick_list_item_id]
        where [uqli].[frequency_schedule_id] = 0;
        select @@rowcount as [update frequency_schedule_id];

        --generate [user_quick_list_items]
        insert into [dbo].[user_quick_list_items]
            ([site_id]
           , [user_id]
           , [dose]
           , [medication_unit_id]
           , [medication_route_id]
           , [frequency_schedule_id]
           , [order_notes]
           , [usages_this_week]
           , [weekly_usage_rolling_average]
           , [medication_id]
           , [duration_in_minutes]
            )
        select [uql].[site_id]
             , [uql].[user_id]
             , [uql].[dose] * 2 as [dose]
             , [uql].[medication_unit_id]
             , [uql].[medication_route_id]
             , 0 as                [frequency_schedule_id]
             , [uql].[order_notes]
             , [uql].[usages_this_week]
             , [uql].[weekly_usage_rolling_average]
             , [uql].[medication_id]
             , [uql].[duration_in_minutes]
        from   [dbo].[user_quick_list_items] as [uql]
        where  [uql].[site_id] = @dev_custom_data_site_id
               and [uql].[dose] > 0;
        select @@rowcount as [insert into dbo.user_quick_list_items];

        --assign [frequency_schedule_id] 
        update    [uqli] set
            [frequency_schedule_id] = [fs].[frequency_schedule_id]
        from
        (
            select row_number() over(
                   order by [s1].[id]) as [id]
                 , [s1].[id] as           [frequency_schedule_id]
                 , [s1].[site_id]
            from   [dbo].[frequency_schedules] as [s1]
            where  [s1].[site_id] = @dev_custom_data_site_id
        ) [fs]
        inner join
        (
            select row_number() over(
                   order by [s2].[id]) as [id]
                 , [s2].[id] as           [user_quick_list_item_id]
                 , [s2].user_id
                 , [s2].[site_id]
            from   [dbo].[user_quick_list_items] as [s2]
            where  [s2].[site_id] = @dev_custom_data_site_id
        ) [uql] on [fs].[id] = [uql].[id] % 124 + 1
        inner join [dbo].[user_quick_list_items] as [uqli] on [uqli].[id] = [uql].[user_quick_list_item_id]
        where [uqli].[frequency_schedule_id] = 0;
        select @@rowcount as [update frequency_schedule_id];

        --assign [duration_in_minutes]
        update [uql] set
            [duration_in_minutes] = (([medication_id] + [uql].[id]) % 89) + 53
        from   [dbo].[user_quick_list_items] as [uql]
               inner join [dbo].[frequency_schedules] [fs] on [fs].[id] = [uql].[frequency_schedule_id]
        where  [uql].[site_id] = @dev_custom_data_site_id
               and [fs].[point_in_time] = 0
               and [duration_in_minutes] = 0;
        select @@rowcount as [update duration_in_minutes];

    end;