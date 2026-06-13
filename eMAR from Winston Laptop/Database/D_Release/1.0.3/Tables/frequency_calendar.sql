create table [dbo].[frequency_calendar]
    (
      [the_date]                 [date] not null
    , [the_month_name]           [nvarchar](30) not null
    , [the_day_name]             [nvarchar](30) not null
    , [the_month]                [tinyint] not null
    , [the_day]                  [tinyint] not null
    , [the_day_of_week]          [tinyint] not null
    , [the_day_of_week_in_month] [tinyint] not null
    , [the_week]                 [tinyint] not null
    , constraint [pk__frequency_calendar__the_date] primary key clustered([the_date] asc));
go

/********
 Defaults
********/
/*****************
 Unique constraint
*****************/
/*******
 Indexes
*******/
/***********
 Foreign Key
***********/
/***************
 Data Dictionary
    Defaults
***************/
/***************
 Data Dictionary
    Indexes
***************/
/***************
 Data Dictionary
    Table
***************/

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'Table contains Generated Data with Calendar Date for every day for 50 Years. This tables is used to generate the frequency schedules.'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'frequency_calendar';
go

/***************
 Data Dictionary
    Columns
***************/

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'Primary Key'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'frequency_calendar'
  , @level2type = N'COLUMN'
  , @level2name = N'the_date';
go

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'Full Text Name of calendar Month'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'frequency_calendar'
  , @level2type = N'COLUMN'
  , @level2name = N'the_month_name';
go

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'Full Text Name of the day of week'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'frequency_calendar'
  , @level2type = N'COLUMN'
  , @level2name = N'the_day_name';
go

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'Numeric Value of the month 1 - 12'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'frequency_calendar'
  , @level2type = N'COLUMN'
  , @level2name = N'the_month';
go

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'Numeric Day of the month 1 - 31'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'frequency_calendar'
  , @level2type = N'COLUMN'
  , @level2name = N'the_day';
go

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'Numeric Day of the week 1 - 7'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'frequency_calendar'
  , @level2type = N'COLUMN'
  , @level2name = N'the_day_of_week';
go

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'Numeric day of week in the month 1 - 5. Example 3 = 3rd Tuesday of the Month'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'frequency_calendar'
  , @level2type = N'COLUMN'
  , @level2name = N'the_day_of_week_in_month';
go

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'Numeric Week of the year 1 - 53'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'frequency_calendar'
  , @level2type = N'COLUMN'
  , @level2name = N'the_week';
go