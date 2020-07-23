create table [dbo].[cart_order_administrations]
    (
      [id]                                [bigint] identity(1, 1) not null
    , [patient_cart_order_id]             [bigint] not null
    , [point_in_time]                     [bit] not null
    , [administration_scheduled_datetime] [datetimeoffset](7) not null
    , [stop_scheduled_datetime]           [datetimeoffset](7) null
    , constraint [pk__cart_order_administrations__id] primary key clustered([id] asc));
go

/********
 Defaults
********/
/*******
 Indexes
*******/
/***********
 Foreign Key
***********/

alter table [dbo].[cart_order_administrations]
add constraint [fk__cart_order_administrations__patient_cart_orders] foreign key([patient_cart_order_id]) references [dbo].[patient_cart_orders]([id]);
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
  , @level1name = N'cart_order_administrations'
  , @level2type = N'CONSTRAINT'
  , @level2name = N'pk__cart_order_administrations__id';
go

/***************
 Data Dictionary
    Table
***************/

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'This table contains: order administrations'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'cart_order_administrations';
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
  , @level1name = N'cart_order_administrations'
  , @level2type = N'COLUMN'
  , @level2name = N'id';
go

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'patient_cart_order_id'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'cart_order_administrations'
  , @level2type = N'COLUMN'
  , @level2name = N'patient_cart_order_id';
go

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'point_in_time'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'cart_order_administrations'
  , @level2type = N'COLUMN'
  , @level2name = N'point_in_time';
go

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'administration_scheduled_datetime'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'cart_order_administrations'
  , @level2type = N'COLUMN'
  , @level2name = N'administration_scheduled_datetime';
go

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'stop_scheduled_datetime'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'cart_order_administrations'
  , @level2type = N'COLUMN'
  , @level2name = N'stop_scheduled_datetime';
go