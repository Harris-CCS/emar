create view [dbo].[frequency_schedules_view]
as
    select    [fs].[id] as   [frequency_schedule_id]
            , [fs].[site_id]
            , [fs].[name]
            , case
                  when [ft].[name] = 'PRN'
                      then 1
                  else 0
              end as         [prn]
            , [fs].[point_in_time]
            , case
                  when [ft].[name] = 'Continuous'
                      then 1
                  else 0
              end as         [continuous]
            , [fs].[frequency_type_id]
            , [ft].[name] as [frequency_type_description]
            , [fs].[frequency_type_recuring]
            , case
                  when [ft].[name] = 'STAT'
                      then 'Right Now'
                  when [ft].[name] = 'Interval'
                      then 'See interval description'
                  when [ft].[name] = 'Daily'
                      then 'Every ' + cast([fs].[frequency_type_recuring] as varchar(15)) + ' day(s)'
                  when [ft].[name] = 'Weekly'
                      then 'Every ' + cast([fs].[frequency_type_recuring] as varchar(15)) + ' week(s)'
                  else ''
              end as         [frequency_type_recuring_description]
            , [fs].[frequency_interval]
            , [fs].[frequency_interval_unit_id]
            , case
                  when [fi].[name] = 'Minutes'
                      then 'Every ' + cast([fs].[frequency_interval] as varchar(15)) + ' Minute(s)'
                  when [fi].[name] = 'Hours'
                      then 'Every ' + cast([fs].[frequency_interval] as varchar(15)) + ' Hour(s)'
                  when [fi].[name] = 'Days'
                      then 'Every ' + cast([fs].[frequency_interval] as varchar(15)) + ' Day(s)'
                  when [ft].[name] = 'PRN'
                      then 'As Needed'
                  when [ft].[name] = 'One Time'
                      then 'Scheduled Once'
                  when [ft].[name] = 'STAT'
                      then 'Right Now'
                  when [ft].[name] = 'Continuous'
                      then 'Continuous'
                  when [ft].[name] = 'Daily'
                      then stuff(
    (
        select ', ' + convert(char(5), [fidt].[frequency_time], 114)
        from    [dbo].[frequency_interval_day_times] as [fidt]
        where  [fidt].[frequency_schedule_id] = [fs].[id]
        order by [fidt].[frequency_day_id]
               , [fidt].[frequency_time] for xml path('')
    ), 1, 2, '')
                  when [ft].[name] = 'Weekly'
                      then stuff(
    (
        select ', ' + [fd].[name] + ':' + convert(char(5), [fidt2].[frequency_time], 114)
        from   [dbo].[frequency_interval_day_times] as [fidt2]
               inner join [dbo].[frequency_days] as [fd] on [fd].[id] = [fidt2].[frequency_day_id]
        where  [fidt2].[frequency_schedule_id] = [fs].[id]
        order by [fidt2].[frequency_day_id]
               , [fidt2].[frequency_time] for xml path('')
    ), 1, 2, '')
                  else ''
              end as         [frequency_interval_description]
            , [fs].[interval_start_time]
            , [fs].[interval_end_minutes]
            , [fs].[notes]
    from [dbo].[frequency_schedules] as [fs]
         inner join [dbo].[frequency_types] as [ft] on [ft].[id] = [fs].[frequency_type_id]
         inner join [dbo].[frequency_interval_units] as [fi] on [fi].[id] = [fs].[frequency_interval_unit_id];

go

-- Data Dictionary
--    View

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'View to display Frequency Schedules'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'VIEW'
  , @level1name = N'frequency_schedules_view';
go