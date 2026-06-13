CREATE TABLE [dbo].[pharmacy_notification_orders]
(
	[id] BIGINT identity(1,1) NOT NULL, 
    [pharmacy_notification_id] BIGINT NOT NULL, 
    [patient_order_id] BIGINT NOT NULL
)
go

/********
 Defaults
********/
/*****************
 Unique constraint
*****************/

alter table [dbo].[pharmacy_notification_orders]
add constraint [pk__pharmacy_notification_orders__id] primary key clustered([id] asc);
go

/*******
 Indexes
*******/

/************
 Foreign Keys
************/

alter table [dbo].[pharmacy_notification_orders]
add constraint [fk__pharmacy_notification_orders__pharmacy_notifications] foreign key([pharmacy_notification_id]) references [dbo].[pharmacy_notifications]([id]);
go
alter table [dbo].[pharmacy_notification_orders]
add constraint [fk__pharmacy_notification_orders__patient_orders] foreign key([patient_order_id]) references [dbo].[patient_orders]([id]);
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
  , @level1name = N'pharmacy_notification_orders'
  , @level2type = N'CONSTRAINT'
  , @level2name = N'pk__pharmacy_notification_orders__id';
go

/***************
 Data Dictionary
    Table
***************/

execute [sys].[sp_addextendedproperty]
    @name = N'MS_Description'
  , @value = N'This table contains: pharmacy notification orders'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'pharmacy_notification_orders';
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
  , @level1name = N'pharmacy_notification_orders'
  , @level2type = N'COLUMN'
  , @level2name = N'id';
go

execute [sys].[sp_addextendedproperty]
    @name = N'MS_Description'
  , @value = N'Pharmacy notification identifier foreign key to pharmacy_notifications table'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'pharmacy_notification_orders'
  , @level2type = N'COLUMN'
  , @level2name = N'pharmacy_notification_id';
go

execute [sys].[sp_addextendedproperty]
    @name = N'MS_Description'
  , @value = N'Patient order identifier foreign key to patient_orders table'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'pharmacy_notification_orders'
  , @level2type = N'COLUMN'
  , @level2name = N'patient_order_id';
go
