create procedure [dbo].[frequency_calendar_build] 
      @start_date     date = '20100101'
    , @generate_years int  = 50
as

/*****************************************************************************************************
Populate Calendar Procedure built from details located at this site
https://www.mssqltips.com/sqlservertip/4054/creating-a-date-dimension-or-calendar-table-in-sql-server/
*****************************************************************************************************/
    declare 
        @cut_off_date date = dateadd(DAY, -1, dateadd(YEAR, 50, @start_date));

    with seq([n])
         as (select 0
             union all
             select [n] + 1
             from   [seq]
             where  [n] < datediff(DAY, @start_date, @cut_off_date)),
         d([d])
         as (select dateadd(DAY, [n], @start_date)
             from   [seq]),
         src
         as (select [the_date] = convert(date, [d])
                  , [TheDay] = datepart(DAY, [d])
                  , [the_day_name] = DATENAME([WEEKDAY], [d])
                  , [TheWeek] = datepart(WEEK, [d])
                  , [TheISOWeek] = datepart([ISO_WEEK], [d])
                  , [the_day_of_week] = datepart([WEEKDAY], [d])
                  , [TheMonth] = datepart(MONTH, [d])
                  , [TheMonthName] = DATENAME(MONTH, [d])
                  , [TheQuarter] = datepart(Quarter, [d])
                  , [TheYear] = datepart(YEAR, [d])
                  , [TheFirstOfMonth] = datefromparts(year([d]), month([d]), 1)
                  , [TheLastOfYear] = datefromparts(year([d]), 12, 31)
                  , [TheDayOfYear] = datepart([DAYOFYEAR], [d])
             from   [d]),
         dim
         as (select [the_date]
                  , [TheMonthName]
                  , [the_day_name]
                  , [TheMonth]
                  , [TheDay]
                  , [the_day_of_week]
                  , [TheDayOfWeekInMonth] = convert(tinyint, row_number() over(partition by [TheFirstOfMonth]
                                                                                          , [the_day_of_week]
                    order by [the_date]))
                  , [TheWeek]
             from   [src])
         select *
         from   [dim]
         order by [the_date] option(maxrecursion 0);
go

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'Procedure used to build a calendar table, to be used in schedule calculation / generation'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'PROCEDURE'
  , @level1name = N'frequency_calendar_build';
go