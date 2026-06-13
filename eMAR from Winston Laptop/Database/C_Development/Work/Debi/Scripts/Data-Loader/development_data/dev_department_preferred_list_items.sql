if '$(load_data)' in('sample', 'live')
    begin

        print 'Loading Table: development_data\dev_department_preferred_list_items.sql';



/************************************
** department_preferred_list_items **
************************************/

        -- generate department_preferred_list_items
        with SiteCounts
             as (select [site_id]
                      , count(*) as [cnt]
                 from   [dbo].[user_quick_list_items]
                 group by [site_id]
                        )
             insert into [dbo].[department_preferred_list_items]
             (site_id,department_code,medication_id,dose,medication_unit_id,medication_route_id,frequency_schedule_id,order_notes)
             select distinct [q].[site_id]
                  , [department_code] = case
                                            when row_number() over(partition by [q].[site_id]
                                                 order by [q].[site_id]
                                                        , [medication_id]) % 2.0 =0
                                                then 'Main ED'
                                            else 'Fast Track'
                                        end
                  , isnull([medication_id], 999999)
                  , [dose]
                  , [medication_unit_id]
                  , [medication_route_id]
                  , [frequency_schedule_id]
                  , [order_notes]
             from   [dbo].[user_quick_list_items] as [q]
                    inner join [SiteCounts] as [cnt] on [q].[site_id] = [cnt].[site_id]
             order by [q].[site_id]
                    , isnull([medication_id], 999999);


        declare 
            @scriptB_dept_list_site_id int =
        (
            select [sites].[id]
            from   [dbo].[sites]
            where  [sites].[name] = 'Automation Test_Multum'
        );

        declare 
            @scriptB_frequency_site_id int = @dev_custom_data_site_id;

        with FrequencyCount
             as (select count(*) as [cnt]
                 from   [dbo].[frequency_schedules]
                 where  [frequency_schedules].[site_id] = @scriptB_frequency_site_id),
             FrequencyIndex
             as (select row_number() over(
                        order by [id]) - 1 as [idx]
                      , [frequency_schedules].[id]
                 from   [dbo].[frequency_schedules]
                 where  [frequency_schedules].[site_id] = @scriptB_frequency_site_id),
             ListIndex
             as (select(row_number() over(
                        order by [id]) - 1) % [FrequencyCount].[cnt] as [idx]
                     , [department_preferred_list_items].[id]
                 from  [dbo].[department_preferred_list_items]
                       cross join [FrequencyCount]
                 where [department_preferred_list_items].[site_id] = @scriptB_dept_list_site_id)
             update [d] set    
                 [frequency_schedule_id] = [f].[id]
             from   [dbo].[department_preferred_list_items] [d]
                    inner join [ListIndex] [l] on [d].[id] = [l].[id]
                    inner join [FrequencyIndex] [f] on [l].[idx] = [f].[idx];

        ---~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        --- [department_preferred_list_items] 
        ---~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        -- assign [frequency_schedule_id] 
        update    [dpli] set       
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
                 , [s2].[id] as           [department_preferred_list_item_id]
                 , [s2].[site_id]
            from   [dbo].[department_preferred_list_items] as [s2]
            where  [s2].[site_id] = @dev_custom_data_site_id
        ) [dpl] on [fs].[id] = [dpl].[id] % 124 + 1
        inner join [dbo].[department_preferred_list_items] as [dpli] on [dpli].[id] = [dpl].[department_preferred_list_item_id]
        where [dpli].[frequency_schedule_id] = 0;

        select @@rowcount as [update frequency_schedule_id];

        -- generate additional department_preferred_list_items
        insert into [dbo].[department_preferred_list_items]
            ([site_id]
           , [department_code]
           , [dose]
           , [medication_unit_id]
           , [medication_route_id]
           , [frequency_schedule_id]
           , [order_notes]
           , [medication_id]
           , [duration_in_minutes]
            )
        select [site_id]
             , [department_code]
             , 2 * [dose] as [dose]
             , [medication_unit_id]
             , [medication_route_id]
             , 0 as          [frequency_schedule_id]
             , [order_notes]
             , [medication_id]
             , [duration_in_minutes]
        from   [dbo].[department_preferred_list_items] as [uql]
        --where  [uql].[site_id] = @dev_custom_data_site_id
        where  [uql].[site_id] = @dev_custom_data_site_id
               and [uql].[dose] > 0;

        select @@rowcount as [insert into dbo.department_preferred_list_items];

        -- assign [frequency_schedule_id] 
        update    [dpli] set       
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
                 , [s2].[id] as           [department_preferred_list_item_id]
                 , [s2].[site_id]
            from   [dbo].[department_preferred_list_items] as [s2]
            where  [s2].[site_id] = @dev_custom_data_site_id
        ) [dpl] on [fs].[id] = [dpl].[id] % 124 + 1
        inner join [dbo].[department_preferred_list_items] as [dpli] on [dpli].[id] = [dpl].[department_preferred_list_item_id]
        where [dpli].[frequency_schedule_id] = 0;

        select @@rowcount as [update frequency_schedule_id];

        -- generate additional department_preferred_list_items
        insert into [dbo].[department_preferred_list_items]
            ([site_id]
           , [department_code]
           , [dose]
           , [medication_unit_id]
           , [medication_route_id]
           , [frequency_schedule_id]
           , [order_notes]
           , [medication_id]
           , [duration_in_minutes]
            )
        select [site_id]
             , [department_code]
             , 1.5 * [dose] as [new_dose]
             , [medication_unit_id]
             , [medication_route_id]
             , 0 as            [frequency_schedule_id]
             , [order_notes]
             , [medication_id]
             , [duration_in_minutes]
        from   [dbo].[department_preferred_list_items] as [uql]
        --where  [uql].[site_id] = @dev_custom_data_site_id
        where  [uql].[site_id] = @dev_custom_data_site_id
               and [uql].[dose] > 1
               and [uql].[dose] < 10000;

        select @@rowcount as [insert into dbo.department_preferred_list_items];

        -- assign [frequency_schedule_id] 
        update    [dpli] set       
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
                 , [s2].[id] as           [department_preferred_list_item_id]
                 , [s2].[site_id]
            from   [dbo].[department_preferred_list_items] as [s2]
            where  [s2].[site_id] = @dev_custom_data_site_id
        ) [dpl] on [fs].[id] = [dpl].[id] % 124 + 1
        inner join [dbo].[department_preferred_list_items] as [dpli] on [dpli].[id] = [dpl].[department_preferred_list_item_id]
        where [dpli].[frequency_schedule_id] = 0;

        select @@rowcount as [update frequency_schedule_id];

    end;