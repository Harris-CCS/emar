create table [dbo].[frequency_schedules]
    (
      [id]                         [int] not null identity(1, 1)
    , [site_id]                    [int] not null
    , [name]                       [sysname] not null
    , [point_in_time]              [bit] not null
    , [frequency_type_id]          [int] not null
    , [frequency_type_recurring]   [int] not null
    , [frequency_interval]         [int] not null
    , [frequency_interval_unit_id] [int] not null
    , [interval_start_time]        [time](0) not null
    , [interval_end_minutes]       [smallint] not null
    , [notes]                      [nvarchar](1000) null
    , [is_active]                  [bit] not null
    , constraint [pk__frequency_schedules__id] primary key clustered([id] asc));

go

/********
 Defaults
********/

alter table [dbo].[frequency_schedules]
add constraint [df__frequency_schedules__is_active] default((1)) for [is_active];
go

/*****************
 Unique constraint
*****************/

alter table [dbo].[frequency_schedules]
add constraint [uk__frequency_schedules__name_site_id] unique([name], [site_id]);
go

/*******
 Indexes
*******/
/***********
 Foreign Key
***********/

alter table [dbo].[frequency_schedules]
add constraint [fk__frequency_schedules__sites] foreign key([site_id]) references [dbo].[sites]([id]);
go

alter table [dbo].[frequency_schedules]
add constraint [fk__frequency_schedules__frequency_types] foreign key([frequency_type_id]) references [dbo].[frequency_types]([id]);
go

alter table [dbo].[frequency_schedules]
add constraint [fk__frequency_schedules__frequency_interval_units] foreign key([frequency_interval_unit_id]) references [dbo].[frequency_interval_units]([id]);
go

/***************
 Data Dictionary
    Defaults
***************/

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'default templates__is_active to 1'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'frequency_schedules'
  , @level2type = N'CONSTRAINT'
  , @level2name = N'df__frequency_schedules__is_active';
go

/***************
 Data Dictionary
    Indexes
***************/

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'Ensure that a Schedule Name is only allowed once per site. No duplicate names.'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'frequency_schedules'
  , @level2type = N'CONSTRAINT'
  , @level2name = N'uk__frequency_schedules__name_site_id';
go

/***************
 Data Dictionary
    Table
***************/

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'Table contains the master frequency schedule record.'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'frequency_schedules';
go

/***************
 Data Dictionary
    Columns
***************/

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'id'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'frequency_schedules'
  , @level2type = N'COLUMN'
  , @level2name = N'id';
go

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'Hospital identifier foreign key to site table'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'frequency_schedules'
  , @level2type = N'COLUMN'
  , @level2name = N'site_id';
go

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'name'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'frequency_schedules'
  , @level2type = N'COLUMN'
  , @level2name = N'name';
go

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'point_in_time'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'frequency_schedules'
  , @level2type = N'COLUMN'
  , @level2name = N'point_in_time';
go

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'frequency_type_id'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'frequency_schedules'
  , @level2type = N'COLUMN'
  , @level2name = N'frequency_type_id';
go

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'frequency_type_recurring'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'frequency_schedules'
  , @level2type = N'COLUMN'
  , @level2name = N'frequency_type_recurring';
go

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'frequency_interval'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'frequency_schedules'
  , @level2type = N'COLUMN'
  , @level2name = N'frequency_interval';
go

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'frequency_interval_unit_id'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'frequency_schedules'
  , @level2type = N'COLUMN'
  , @level2name = N'frequency_interval_unit_id';
go

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'interval_start_time'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'frequency_schedules'
  , @level2type = N'COLUMN'
  , @level2name = N'interval_start_time';
go

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'interval_end_minutes'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'frequency_schedules'
  , @level2type = N'COLUMN'
  , @level2name = N'interval_end_minutes';
go

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'notes'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'frequency_schedules'
  , @level2type = N'COLUMN'
  , @level2name = N'notes';
go

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'is_active 1=True 0=False'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'frequency_schedules'
  , @level2type = N'COLUMN'
  , @level2name = N'is_active';
go