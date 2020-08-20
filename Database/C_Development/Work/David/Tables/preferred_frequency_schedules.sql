create table [dbo].[preferred_frequency_schedules]
    (
      [drug_id]               [varchar](32) not null
    , [frequency_schedule_id] [int] null
    , [site_id]               [int] null);
go

/********
 Defaults
********/
/*****************
 Unique constraint
*****************/

alter table [dbo].[preferred_frequency_schedules]
add constraint [uc__preferred_frequency_schedules] unique clustered([frequency_schedule_id] asc, [site_id] asc, [drug_id] asc);
go

/*******
 Indexes
*******/
/***********
 Foreign Key
***********/

alter table [dbo].[preferred_frequency_schedules]
add constraint [fk__preferred_frequency_schedules__sites] foreign key([site_id]) references [dbo].[sites]([id]);
go

alter table [dbo].[preferred_frequency_schedules]
add constraint [fk__preferred_frequency_schedules__frequency_schedules] foreign key([frequency_schedule_id]) references [dbo].[frequency_schedules]([id]);
go

/***************
 Data Dictionary
    Defaults
***************/
/***************
 Data Dictionary
    Indexes
***************/

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'Constraint to enforce preferred frequency schedules uniqueness'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'preferred_frequency_schedules'
  , @level2type = N'CONSTRAINT'
  , @level2name = N'uc__preferred_frequency_schedules';
go

/***************
 Data Dictionary
    Table
***************/

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'This table contains a preferred list of frequency schedules for a specific drug id'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'preferred_frequency_schedules';
go

/***************
 Data Dictionary
    Columns
***************/

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'External Vendor Drug Database Identifier
  FDB: MEDID (MED Medication ID (Stable ID))'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'preferred_frequency_schedules'
  , @level2type = N'COLUMN'
  , @level2name = N'drug_id';
go

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'Site Name'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'preferred_frequency_schedules'
  , @level2type = N'COLUMN'
  , @level2name = N'frequency_schedule_id';
go

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'Hospital identifier foriegn key to site table'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'preferred_frequency_schedules'
  , @level2type = N'COLUMN'
  , @level2name = N'site_id';
go