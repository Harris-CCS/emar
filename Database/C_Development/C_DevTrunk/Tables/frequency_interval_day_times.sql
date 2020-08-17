create table [dbo].[frequency_interval_day_times]
    (
      [frequency_schedule_id] [int] not null
    , [frequency_day_id]      [tinyint] not null
    , [frequency_time]        [time](0) null);
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

alter table [dbo].[frequency_interval_day_times]
add constraint [fk__frequency_interval_day_times__frequency_schedules] foreign key([frequency_schedule_id]) references [dbo].[frequency_schedules]([id]);
go

alter table [dbo].[frequency_interval_day_times]
add constraint [fk__frequency_interval_day_times__frequency_days] foreign key([frequency_day_id]) references [dbo].[frequency_days]([id]);
go

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
  , @value = N'Table contains the specific days and times or a frequency to generate a schedule record for.'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'frequency_interval_day_times';
go

/***************
 Data Dictionary
    Columns
***************/

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'frequency_schedule_id'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'frequency_interval_day_times'
  , @level2type = N'COLUMN'
  , @level2name = N'frequency_schedule_id';
go

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'frequency_day_id'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'frequency_interval_day_times'
  , @level2type = N'COLUMN'
  , @level2name = N'frequency_day_id';
go

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'frequency_time'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'frequency_interval_day_times'
  , @level2type = N'COLUMN'
  , @level2name = N'frequency_time';
go

