create view [dbo].[frequency_schedules_export_view]
as with cte_intervals
        as (select row_number() over(partition by [frequency_schedule_id]
                   order by [frequency_schedule_id]
                          , [frequency_day_id]
                          , [frequency_time]) as           [interval_id]
                 , [frequency_schedule_id]
                 , [frequency_day_id]
                 , cast([frequency_time] as varchar(8)) as [frequency_time]
            from   [frequency_interval_day_times])
        select     [site].[name] as                                  [site_name]
                 , [fs].[name] as                                    [frequency_name]
                 , case [fs].[point_in_time]
                       when 1
                           then 'True'
                       else 'False'
                   end as                                            [point_in_time]
                 , [fs].[frequency_type_description]
                 , [fs].[frequency_type_recurring]
                 , [fi].[name] as                                    [frequency_interval_name]
                 , [fs].[frequency_interval]
                 , cast([fs].[interval_start_time] as varchar(8)) as [interval_start_time]
                 , [fs].[interval_end_minutes]
                 , [fs].[notes]
                 , case [fs].[is_active]
                       when 1
                           then 'True'
                       else 'False'
                   end as                                            [is_active]
                 , isnull([ds1].[frequency_day_id], '') as           [interval_01]
                 , isnull([ds1].[frequency_time], '') as             [time_01]
                 , isnull([ds2].[frequency_day_id], '') as           [interval_02]
                 , isnull([ds2].[frequency_time], '') as             [time_02]
                 , isnull([ds3].[frequency_day_id], '') as           [interval_03]
                 , isnull([ds3].[frequency_time], '') as             [time_03]
                 , isnull([ds4].[frequency_day_id], '') as           [interval_04]
                 , isnull([ds4].[frequency_time], '') as             [time_04]
                 , isnull([ds5].[frequency_day_id], '') as           [interval_05]
                 , isnull([ds5].[frequency_time], '') as             [time_05]
                 , isnull([ds6].[frequency_day_id], '') as           [interval_06]
                 , isnull([ds6].[frequency_time], '') as             [time_06]
                 , isnull([ds7].[frequency_day_id], '') as           [interval_07]
                 , isnull([ds7].[frequency_time], '') as             [time_07]
                 , isnull([ds8].[frequency_day_id], '') as           [interval_08]
                 , isnull([ds8].[frequency_time], '') as             [time_08]
                 , isnull([ds9].[frequency_day_id], '') as           [interval_09]
                 , isnull([ds9].[frequency_time], '') as             [time_09]
                 , isnull([ds10].[frequency_day_id], '') as          [interval_10]
                 , isnull([ds10].[frequency_time], '') as            [time_10]
                 , isnull([ds11].[frequency_day_id], '') as          [interval_11]
                 , isnull([ds11].[frequency_time], '') as            [time_11]
                 , isnull([ds12].[frequency_day_id], '') as          [interval_12]
                 , isnull([ds12].[frequency_time], '') as            [time_12]
                 , isnull([ds13].[frequency_day_id], '') as          [interval_13]
                 , isnull([ds13].[frequency_time], '') as            [time_13]
                 , isnull([ds14].[frequency_day_id], '') as          [interval_14]
                 , isnull([ds14].[frequency_time], '') as            [time_14]
                 , isnull([ds15].[frequency_day_id], '') as          [interval_15]
                 , isnull([ds15].[frequency_time], '') as            [time_15]
        from       [frequency_schedules_view] as [fs]
                   inner join [sites] as [site] on [site].[id] = [fs].[site_id]
                   inner join [frequency_interval_units] as [fi] on [fi].[id] = [fs].[frequency_interval_unit_id]
                   outer apply(select [fd].[name] [frequency_day_id]
                                    , [ci].[frequency_time]
                               from [cte_intervals] as [ci]
                                    inner join [dbo].[frequency_days] as [fd] on [fd].[id] = [ci].[frequency_day_id]
                               where [interval_id] = 1
                                     and [ci].[frequency_schedule_id] = [fs].[frequency_schedule_id]) as [ds1]
                   outer apply(select [fd].[name] [frequency_day_id]
                                    , [ci].[frequency_time]
                               from [cte_intervals] as [ci]
                                    inner join [dbo].[frequency_days] as [fd] on [fd].[id] = [ci].[frequency_day_id]
                               where [interval_id] = 2
                                     and [ci].[frequency_schedule_id] = [fs].[frequency_schedule_id]) as [ds2]
                   outer apply(select [fd].[name] [frequency_day_id]
                                    , [ci].[frequency_time]
                               from [cte_intervals] as [ci]
                                    inner join [dbo].[frequency_days] as [fd] on [fd].[id] = [ci].[frequency_day_id]
                               where [interval_id] = 3
                                     and [ci].[frequency_schedule_id] = [fs].[frequency_schedule_id]) as [ds3]
                   outer apply(select [fd].[name] [frequency_day_id]
                                    , [ci].[frequency_time]
                               from [cte_intervals] as [ci]
                                    inner join [dbo].[frequency_days] as [fd] on [fd].[id] = [ci].[frequency_day_id]
                               where [interval_id] = 4
                                     and [ci].[frequency_schedule_id] = [fs].[frequency_schedule_id]) as [ds4]
                   outer apply(select [fd].[name] [frequency_day_id]
                                    , [ci].[frequency_time]
                               from [cte_intervals] as [ci]
                                    inner join [dbo].[frequency_days] as [fd] on [fd].[id] = [ci].[frequency_day_id]
                               where [interval_id] = 5
                                     and [ci].[frequency_schedule_id] = [fs].[frequency_schedule_id]) as [ds5]
                   outer apply(select [fd].[name] [frequency_day_id]
                                    , [ci].[frequency_time]
                               from [cte_intervals] as [ci]
                                    inner join [dbo].[frequency_days] as [fd] on [fd].[id] = [ci].[frequency_day_id]
                               where [interval_id] = 6
                                     and [ci].[frequency_schedule_id] = [fs].[frequency_schedule_id]) as [ds6]
                   outer apply(select [fd].[name] [frequency_day_id]
                                    , [ci].[frequency_time]
                               from [cte_intervals] as [ci]
                                    inner join [dbo].[frequency_days] as [fd] on [fd].[id] = [ci].[frequency_day_id]
                               where [interval_id] = 7
                                     and [ci].[frequency_schedule_id] = [fs].[frequency_schedule_id]) as [ds7]
                   outer apply(select [fd].[name] [frequency_day_id]
                                    , [ci].[frequency_time]
                               from [cte_intervals] as [ci]
                                    inner join [dbo].[frequency_days] as [fd] on [fd].[id] = [ci].[frequency_day_id]
                               where [interval_id] = 8
                                     and [ci].[frequency_schedule_id] = [fs].[frequency_schedule_id]) as [ds8]
                   outer apply(select [fd].[name] [frequency_day_id]
                                    , [ci].[frequency_time]
                               from [cte_intervals] as [ci]
                                    inner join [dbo].[frequency_days] as [fd] on [fd].[id] = [ci].[frequency_day_id]
                               where [interval_id] = 9
                                     and [ci].[frequency_schedule_id] = [fs].[frequency_schedule_id]) as [ds9]
                   outer apply(select [fd].[name] [frequency_day_id]
                                    , [ci].[frequency_time]
                               from [cte_intervals] as [ci]
                                    inner join [dbo].[frequency_days] as [fd] on [fd].[id] = [ci].[frequency_day_id]
                               where [interval_id] = 10
                                     and [ci].[frequency_schedule_id] = [fs].[frequency_schedule_id]) as [ds10]
                   outer apply(select [fd].[name] [frequency_day_id]
                                    , [ci].[frequency_time]
                               from [cte_intervals] as [ci]
                                    inner join [dbo].[frequency_days] as [fd] on [fd].[id] = [ci].[frequency_day_id]
                               where [interval_id] = 11
                                     and [ci].[frequency_schedule_id] = [fs].[frequency_schedule_id]) as [ds11]
                   outer apply(select [fd].[name] [frequency_day_id]
                                    , [ci].[frequency_time]
                               from [cte_intervals] as [ci]
                                    inner join [dbo].[frequency_days] as [fd] on [fd].[id] = [ci].[frequency_day_id]
                               where [interval_id] = 12
                                     and [ci].[frequency_schedule_id] = [fs].[frequency_schedule_id]) as [ds12]
                   outer apply(select [fd].[name] [frequency_day_id]
                                    , [ci].[frequency_time]
                               from [cte_intervals] as [ci]
                                    inner join [dbo].[frequency_days] as [fd] on [fd].[id] = [ci].[frequency_day_id]
                               where [interval_id] = 13
                                     and [ci].[frequency_schedule_id] = [fs].[frequency_schedule_id]) as [ds13]
                   outer apply(select [fd].[name] [frequency_day_id]
                                    , [ci].[frequency_time]
                               from [cte_intervals] as [ci]
                                    inner join [dbo].[frequency_days] as [fd] on [fd].[id] = [ci].[frequency_day_id]
                               where [interval_id] = 14
                                     and [ci].[frequency_schedule_id] = [fs].[frequency_schedule_id]) as [ds14]
                   outer apply(select [fd].[name] [frequency_day_id]
                                    , [ci].[frequency_time]
                               from [cte_intervals] as [ci]
                                    inner join [dbo].[frequency_days] as [fd] on [fd].[id] = [ci].[frequency_day_id]
                               where [interval_id] = 15
                                     and [ci].[frequency_schedule_id] = [fs].[frequency_schedule_id]) as [ds15]
        where [site].[id] > 0;
go
-- Data Dictionary
--    View

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'View used to generate text to be applied into the Excel Workbook for the purposes of import / edit'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'VIEW'
  , @level1name = N'frequency_schedules_export_view';
go
