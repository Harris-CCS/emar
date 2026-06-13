CREATE TABLE [dbo].[future_administrations_reschedule]
(
	[id] INT identity(1, 1) NOT NULL, 
    [patient_order_id] BIGINT NOT NULL, 
    [time_offset_minutes] INT NOT NULL,
    constraint [pk__future_administrations_reschedule__id] primary key clustered([id] asc)
);
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

create nonclustered index [ix__future_administrations_reschedule__patient_order_id] on [dbo].[future_administrations_reschedule]
    ([patient_order_id] asc);
go

/***********
 Foreign Key
***********/

alter table [dbo].[future_administrations_reschedule]
add constraint [fk__future_administration_reschedule__patient_orders] foreign key ([patient_order_id]) references [dbo].[patient_orders] ([id]);
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
  , @value = N'Primary Key Constraint'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'future_administrations_reschedule'
  , @level2type = N'CONSTRAINT'
  , @level2name = N'pk__future_administrations_reschedule__id';
go

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'Default Index taken from emar future_administrations_reschedule'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'future_administrations_reschedule'
  , @level2type = N'Index'
  , @level2name = N'ix__future_administrations_reschedule__patient_order_id';
go

/***************
 Data Dictionary
    Table
***************/

execute [sys].[sp_addextendedproperty]
    @name       = N'MS_Description'
  , @value      = N'This table contains future administration rescheduling data'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'future_administrations_reschedule';
go

/***************
 Data Dictionary
    Columns
***************/

execute [sys].[sp_addextendedproperty]
    @name       = N'MS_Description'
  , @value      = N'Auto increment table ID'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'future_administrations_reschedule'
  , @level2type = N'COLUMN'
  , @level2name = N'id';
go

execute [sys].[sp_addextendedproperty]
    @name       = N'MS_Description'
  , @value      = N'patient order id'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'future_administrations_reschedule'
  , @level2type = N'COLUMN'
  , @level2name = N'patient_order_id';
go

execute [sys].[sp_addextendedproperty]
    @name       = N'MS_Description'
  , @value      = N'time offset in minutes where a positive values represents moving forward in the future and a negative value represents moving backward in the past'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'future_administrations_reschedule'
  , @level2type = N'COLUMN'
  , @level2name = N'time_offset_minutes';
go
