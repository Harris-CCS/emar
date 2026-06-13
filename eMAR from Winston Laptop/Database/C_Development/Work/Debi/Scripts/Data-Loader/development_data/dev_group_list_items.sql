if '$(load_data)' in('sample', 'live')
    begin

        print 'Loading Table: development_data\dev_group_list_items.sql';

/*********************
** group_list_items **
*********************/
        -- assign frequency_schedule_id
        with FrequencyCount
             as (select count(*) as [cnt]
                 from   [dbo].[frequency_schedules]
                 where  [frequency_schedules].[site_id] = @dev_custom_data_site_id),
             FrequencyIndex
             as (select row_number() over(
                        order by [id]) - 1 as [idx]
                      , [frequency_schedules].[id]
                 from   [dbo].[frequency_schedules]
                 where  [frequency_schedules].[site_id] = @dev_custom_data_site_id),
             ListIndex
             as (select(row_number() over(
                        order by [id]) - 1) % [FrequencyCount].[cnt] as [idx]
                     , [group_list_items].[id]
                 from  [dbo].[group_list_items]
                       cross join [FrequencyCount]
                 where [group_list_items].[site_id] = @dev_custom_data_site_id)
             update [d] set
                 [frequency_schedule_id] = [f].[id]
             from   [dbo].[group_list_items] [d]
                    inner join [ListIndex] [l] on [d].[id] = [l].[id]
                    inner join [FrequencyIndex] [f] on [l].[idx] = [f].[idx];

        -- assign duration_in_minutes
        update [uql] set
            [duration_in_minutes] = (([medication_id] + [uql].[id]) % 89) + 53
        from   [dbo].[group_list_items] as [uql]
               inner join [dbo].[frequency_schedules] [fs] on [fs].[id] = [uql].[frequency_schedule_id]
        where  [uql].[site_id] = @dev_custom_data_site_id
               and [fs].[point_in_time] = 0
               and [duration_in_minutes] = 0;
        select @@rowcount as [update duration_in_minutes];

        -- assign department_code
        with SiteCounts
             as (select [group_list_items].[site_id]
                      , count(*) as [cnt]
                 from   [dbo].[group_list_items]
                 group by [group_list_items].[site_id]),
             src
             as (select [g].[id]
                      , case
                            when row_number() over(partition by [g].[site_id]
                                 order by [g].[site_id]
                                        , [medication_id]) > ([cnt].[cnt] / 2.0)
                                then 'Main ED'
                            else 'Fast Track'
                        end as [new_department_code]
                 from   [dbo].[group_list_items] as [g]
                        join [SiteCounts] as [cnt] on [g].[site_id] = [cnt].[site_id])
             update [g] set
                 [g].[department_code] = [new_department_code]
             from   [dbo].[group_list_items] [g]
                    join [src] on [g].[id] = [src].[id];

    end;