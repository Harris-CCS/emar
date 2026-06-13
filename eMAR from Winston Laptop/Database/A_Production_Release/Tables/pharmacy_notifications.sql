CREATE TABLE [dbo].[pharmacy_notifications]
(
	[id] BIGINT identity(1, 1) NOT NULL, 
    [patient_id] BIGINT NOT NULL, 
    [type] VARCHAR(20) NOT NULL, 
    [entered_datetime] DATETIMEOFFSET(7) NOT NULL, 
    [completed_datetime] DATETIMEOFFSET(7) NULL
)
go

/********
 Defaults
********/
/*****************
 Unique constraint
*****************/

alter table [dbo].[pharmacy_notifications]
add constraint [pk__pharmacy_notifications__id] primary key clustered([id] asc);
go

/*******
 Indexes
*******/

/************
 Foreign Keys
************/

alter table [dbo].[pharmacy_notifications]
add constraint [fk__pharmacy_notifications__patients] foreign key([patient_id]) references [dbo].[patients]([id]);
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
  , @value = N'Primary Key Column'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'pharmacy_notifications'
  , @level2type = N'CONSTRAINT'
  , @level2name = N'pk__pharmacy_notifications__id';
go

/***************
 Data Dictionary
    Table
***************/

execute [sys].[sp_addextendedproperty]
    @name = N'MS_Description'
  , @value = N'This table contains: pharmacy notifications'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'pharmacy_notifications';
go

/***************
 Data Dictionary
    Columns
***************/

execute [sys].[sp_addextendedproperty]
    @name = N'MS_Description'
  , @value = N'Auto increment table ID'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'pharmacy_notifications'
  , @level2type = N'COLUMN'
  , @level2name = N'id';
go

execute [sys].[sp_addextendedproperty]
    @name = N'MS_Description'
  , @value = N'Patient identifier foreign key to patients table'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'pharmacy_notifications'
  , @level2type = N'COLUMN'
  , @level2name = N'patient_id';
go

execute [sys].[sp_addextendedproperty]
    @name = N'MS_Description'
  , @value = N'Type of notification'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'pharmacy_notifications'
  , @level2type = N'COLUMN'
  , @level2name = N'type';
go

execute [sys].[sp_addextendedproperty]
    @name = N'MS_Description'
  , @value = N'Notification time entered'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'pharmacy_notifications'
  , @level2type = N'COLUMN'
  , @level2name = N'entered_datetime';
go

execute [sys].[sp_addextendedproperty]
    @name = N'MS_Description'
  , @value = N'Notification time completed'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'pharmacy_notifications'
  , @level2type = N'COLUMN'
  , @level2name = N'completed_datetime';
go