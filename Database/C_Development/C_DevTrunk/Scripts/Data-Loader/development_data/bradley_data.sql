if '$(load_data)' in('sample', 'live')
    begin

        print 'Loading Table: bradley_data';

        update [dbo].[patients] set    
            [first_name] = '   ' + [first_name] + '   '
          , [middle_name] = '   ' + [middle_name] + '   '
          , [last_name] = '   ' + [last_name] + '   '
          , [name_suffix] = '   ' + 'MD' + '   '
        where  [first_name] = 'Lillian'
               and [last_name] = 'Infobutton';

        update [dbo].[patients] set    
            [middle_name] = 'A.'
        where  [first_name] = 'Chester'
               and [last_name] = 'Arthur';

        update [dbo].[users] set    
            [first_name] = 'Heather'
          , [last_name] = 'Abebe'
          , [middle_name] = 'L'
          , [name_suffix] = 'P.A.C.'
        from   [dbo].[get_internal_id]('pulsecheck', 'users', 10087) [internal_id]
               inner join [dbo].[users] [users] on [internal_id].[id] = [users].[id];

        declare 
            @scriptB_user_Id int =
        (
            select [internal_id]
            from   [dbo].[external_ids]
            where  [external_id] = '10147'
                   and [vendor] = 'pulsecheck'
                   and [entity] = 'users'
        );

        update [dbo].[user_quick_list_items] set    
            [weekly_usage_rolling_average] = -1
        where  [user_id] = @scriptB_user_Id;

        set rowcount 100;
        update [dbo].[user_quick_list_items] set    
            [weekly_usage_rolling_average] = 1.0
        where  [user_id] = @scriptB_user_Id;

        set rowcount 80;
        update [dbo].[user_quick_list_items] set    
            [weekly_usage_rolling_average]+=1.0
        where  [user_id] = @scriptB_user_Id;

        set rowcount 60;
        update [dbo].[user_quick_list_items] set    
            [weekly_usage_rolling_average]+=1.0
        where  [user_id] = @scriptB_user_Id;

        set rowcount 30;
        update [dbo].[user_quick_list_items] set    
            [weekly_usage_rolling_average]+=1.0
        where  [user_id] = @scriptB_user_Id;

        set rowcount 20;
        update [dbo].[user_quick_list_items] set    
            [weekly_usage_rolling_average]+=1.0
        where  [user_id] = @scriptB_user_Id;

        set rowcount 0;

        with SiteCounts
             as (select [site_id]
                      , count(*) as [cnt]
                 from   [dbo].[user_quick_list_items]
                 group by [site_id]
                        )
             insert into [dbo].[department_preferred_list_items]
             select distinct [q].[site_id]
                  , [department_code] = case
                                            when row_number() over(partition by [q].[site_id]
                                                 order by [q].[site_id]
                                                        , [brand_name]) % 2.0 =0
                                                then 'Main ED'
                                            else 'Fast Track'
                                        end
                  , [ndc]
                  , isnull([drug_id], 999999)
                  , [brand_name]
                  , [dose]
                  , [medication_unit_id]
                  , [medication_route_id]
                  , [frequency_schedule_id]
                  , [order_notes]
             from   [dbo].[user_quick_list_items] as [q]
                    inner join [SiteCounts] as [cnt] on [q].[site_id] = [cnt].[site_id]
             order by [q].[site_id]
                    , [q].[brand_name];

        --~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        --~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

/************************************
** department_preferred_list_items **
************************************/
        declare 
            @scriptB_dept_list_site_id int =
        (
            select [sites].[id]
            from   [dbo].[sites]
            where  [sites].[name] = 'Automation Test_Multum'
        );
        declare 
            @scriptB_frequency_site_id int = 16;

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

/*********************
** group_list_items **
*********************/
        declare 
            @scriptB_site_id int =
        (
            select [sites].[id]
            from   [dbo].[sites]
            where  [sites].[name] = 'FDB (36)'
        );

        with FrequencyCount
             as (select count(*) as [cnt]
                 from   [dbo].[frequency_schedules]
                 where  [frequency_schedules].[site_id] = @scriptB_site_id),
             FrequencyIndex
             as (select row_number() over(
                        order by [id]) - 1 as [idx]
                      , [frequency_schedules].[id]
                 from   [dbo].[frequency_schedules]
                 where  [frequency_schedules].[site_id] = @scriptB_site_id),
             ListIndex
             as (select(row_number() over(
                        order by [id]) - 1) % [FrequencyCount].[cnt] as [idx]
                     , [group_list_items].[id]
                 from  [dbo].[group_list_items]
                       cross join [FrequencyCount]
                 where [group_list_items].[site_id] = @scriptB_site_id)
             update [d] set    
                 [frequency_schedule_id] = [f].[id]
             from   [dbo].[group_list_items] [d]
                    inner join [ListIndex] [l] on [d].[id] = [l].[id]
                    inner join [FrequencyIndex] [f] on [l].[idx] = [f].[idx];

/**************************
** user_quick_list_items **
**************************/

        set @scriptB_site_id =
        (
            select [site_id]
            from   [dbo].[users]
            where  [id] = @scriptB_user_Id
        );

        with FrequencyCount
             as (select count(*) as [cnt]
                 from   [dbo].[frequency_schedules]
                 where  [frequency_schedules].[site_id] = @scriptB_site_id),
             FrequencyIndex
             as (select row_number() over(
                        order by [id]) - 1 as         [idx]
                      , [frequency_schedules].[id] as [frequency_id]
                 from   [dbo].[frequency_schedules]
                 where  [frequency_schedules].[site_id] = @scriptB_site_id),
             ListIndex
             as (select(row_number() over(
                        order by [id]) - 1) % [FrequencyCount].[cnt] as [idx]
                     , [id] as                                          [list_item_id]
                 from  [dbo].[user_quick_list_items]
                       cross join [FrequencyCount]
                 where [user_id] = @scriptB_user_Id)
             update [d] set    
                 [frequency_schedule_id] = [f].[frequency_id]
             from   [dbo].[user_quick_list_items] [d]
                    inner join [ListIndex] [l] on [d].[id] = [l].[list_item_id]
                    inner join [FrequencyIndex] [f] on [l].[idx] = [f].[idx];
    end;