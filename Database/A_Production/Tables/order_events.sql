create table [dbo].[order_events]
    (
      [id]                      [bigint] identity(1, 1) not null
    , [patient_order_id]        [bigint] not null
    , [order_administration_id] [bigint] null
    , [event_datetime]          [datetimeoffset](7) not null
    , [add_user_id]             [int] not null
    , [add_datetime]            [datetimeoffset](7) not null
    , [action_id]               [int] not null
    , constraint [pk__order_events__id] primary key clustered([id] asc));
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

alter table [dbo].[order_events]
add constraint [fk__order_events__actions] foreign key([action_id]) references [dbo].[actions]([id]);
go

alter table [dbo].[order_events]
add constraint [fk__order_events__order_administrations] foreign key([order_administration_id]) references [dbo].[order_administrations]([id]);
go

alter table [dbo].[order_events]
add constraint [fk__order_events__patient_orders] foreign key([patient_order_id]) references [dbo].[patient_orders]([id]);
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
  , @level1name = N'order_events'
  , @level2type = N'CONSTRAINT'
  , @level2name = N'pk__order_events__id';
go

/***************
 Data Dictionary
    Table
***************/

execute [sys].[sp_addextendedproperty]
    @name = N'MS_Description'
  , @value = N'This table contains: order events'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'order_events';
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
  , @level1name = N'order_events'
  , @level2type = N'COLUMN'
  , @level2name = N'id';
go

execute [sys].[sp_addextendedproperty]
    @name = N'MS_Description'
  , @value = N'patient_order_id'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'order_events'
  , @level2type = N'COLUMN'
  , @level2name = N'patient_order_id';
go

execute [sys].[sp_addextendedproperty]
    @name = N'MS_Description'
  , @value = N'order_administration_id'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'order_events'
  , @level2type = N'COLUMN'
  , @level2name = N'order_administration_id';
go

execute [sys].[sp_addextendedproperty]
    @name = N'MS_Description'
  , @value = N'event_datetime'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'order_events'
  , @level2type = N'COLUMN'
  , @level2name = N'event_datetime';
go

execute [sys].[sp_addextendedproperty]
    @name = N'MS_Description'
  , @value = N'add_user_id'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'order_events'
  , @level2type = N'COLUMN'
  , @level2name = N'add_user_id';
go

execute [sys].[sp_addextendedproperty]
    @name = N'MS_Description'
  , @value = N'add_datetime'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'order_events'
  , @level2type = N'COLUMN'
  , @level2name = N'add_datetime';
go

execute [sys].[sp_addextendedproperty]
    @name = N'MS_Description'
  , @value = N'action_id'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'order_events'
  , @level2type = N'COLUMN'
  , @level2name = N'action_id';
go