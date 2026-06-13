create table [dbo].[external_update_queue]
    (
      [id]                     [bigint] identity(1, 1) not null
    , [patient_id]             [bigint] not null
    , [patient_order_id]       [bigint] null
    , [order_administraton_id] [bigint] null
    , [code_type]              [varchar](10) not null
    , [code_value]             [xml] not null
    , [event_datetime]         [datetimeoffset](7) null
    , [inprocess_datetime]     [datetimeoffset](7) null
    , [complete_datetime]      [datetimeoffset](7) null
    , constraint [pk__external_update_queue__id] primary key clustered([id]));
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

alter table [dbo].[external_update_queue]
add constraint [fk__external_update_queue__patient_id] foreign key([patient_id]) references [dbo].[patients]([id]);
go

alter table [dbo].[external_update_queue]
add constraint [fk__external_update_queue__patient_order_id] foreign key([patient_order_id]) references [dbo].[patient_orders]([id]);
go

alter table [dbo].[external_update_queue]
add constraint [fk__external_update_queue__order_administraton_id] foreign key([order_administraton_id]) references [dbo].[order_administrations]([id]);
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
  , @level1name = N'external_update_queue'
  , @level2type = N'CONSTRAINT'
  , @level2name = N'pk__external_update_queue__id';
go

/***************
 Data Dictionary
    Table
***************/

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'This table contains template update values to transmit to external system'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'external_update_queue';
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
  , @level1name = N'external_update_queue'
  , @level2type = N'COLUMN'
  , @level2name = N'id';
go

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'Patient identifier, Foreign Key to patients table'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'external_update_queue'
  , @level2type = N'COLUMN'
  , @level2name = N'patient_id';
go

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'External vendor code type'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'external_update_queue'
  , @level2type = N'COLUMN'
  , @level2name = N'code_type';
go

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'External vendor code value to transmit'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'external_update_queue'
  , @level2type = N'COLUMN'
  , @level2name = N'code_value';
go

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'event_datetime'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'external_update_queue'
  , @level2type = N'COLUMN'
  , @level2name = N'event_datetime';
go

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'inprocess_datetime'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'external_update_queue'
  , @level2type = N'COLUMN'
  , @level2name = N'inprocess_datetime';
go

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'complete_datetime'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'external_update_queue'
  , @level2type = N'COLUMN'
  , @level2name = N'complete_datetime';
go