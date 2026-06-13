print 'Loading Table: preferred_frequency_schedules';

drop table if exists [#medication_ids];

drop table if exists [#frequency_schedule_ids];

if '$(load_data)' = 'live'
   or '$(load_data)' = 'sample'
    begin

        declare 
            @max_med_group int;

        select [fs].[id] as                [frequency_schedule_id]
             , [fs].[site_id]
             , row_number() over(partition by [site_id]
                                            , ([id] % 3) + 1
               order by [site_id]
                      , [id]
                      , ([id] % 3) + 1) as [group_id]
        into [#frequency_schedule_ids]
        from   [frequency_schedules] as [fs]
        where  [site_id] > 0;

        select @max_med_group = max([group_id])
        from   [#frequency_schedule_ids];

        with cte_sites
             as (select distinct 
                        [site_id]
                 from   [frequency_schedules]
                 where  [site_id] > 0),
             cte_meds
             as (select [med].[id] as                        [medication_id]
                      , [src].[site_id]
                      , ([med].[id] % @max_med_group) + 1 as [group_id]
                 from   [cte_sites] as [src]
                        cross join [dbo].[medications] as [med])
             select *
             into [#medication_ids]
             from   [cte_meds];

        insert into [dbo].[preferred_frequency_schedules]
            ([medication_id]
           , [frequency_schedule_id]
           , [site_id]
            )
        select [d].[medication_id]
             , [fs].[frequency_schedule_id]
             , [d].[site_id]
        from   [#medication_ids] as [d]
               cross join [#frequency_schedule_ids] as [fs]
        where  [d].[site_id] = [fs].[site_id]
               and [d].[group_id] = [fs].[group_id];

/****************
        end table
****************/
    end;

drop table if exists [#medication_ids];

drop table if exists [#frequency_schedule_ids];