CREATE TABLE [dbo].[pharmacy_notification_administrations]
(
	[id] BIGINT identity(1,1) NOT NULL, 
    [pharmacy_notification_id] BIGINT NOT NULL, 
    [order_administration_id] BIGINT NOT NULL
)
go

/********
 Defaults
********/
/*****************
 Unique constraint
*****************/

alter table [dbo].[pharmacy_notification_administrations]
add constraint [pk__pharmacy_notification_administrations__id] primary key clustered([id] asc);
go

/*******
 Indexes
*******/

/************
 Foreign Keys
************/

alter table [dbo].[pharmacy_notification_administrations]
add constraint [fk__pharmacy_notification_administrations__pharmacy_notifications] foreign key([pharmacy_notification_id]) references [dbo].[pharmacy_notifications]([id]);
go
alter table [dbo].[pharmacy_notification_administrations]
add constraint [fk__pharmacy_notification_administrations__order_administrations] foreign key([order_administration_id]) references [dbo].[order_administrations]([id]);
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
  , @level1name = N'pharmacy_notification_administrations'
  , @level2type = N'CONSTRAINT'
  , @level2name = N'pk__pharmacy_notification_administrations__id';
go

/***************
 Data Dictionary
    Table
***************/

execute [sys].[sp_addextendedproperty]
    @name = N'MS_Description'
  , @value = N'This table contains: pharmacy notification administrations'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'pharmacy_notification_administrations';
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
  , @level1name = N'pharmacy_notification_administrations'
  , @level2type = N'COLUMN'
  , @level2name = N'id';
go

execute [sys].[sp_addextendedproperty]
    @name = N'MS_Description'
  , @value = N'Pharmacy notification identifier foreign key to pharmacy_notifications table'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'pharmacy_notification_administrations'
  , @level2type = N'COLUMN'
  , @level2name = N'pharmacy_notification_id';
go

execute [sys].[sp_addextendedproperty]
    @name = N'MS_Description'
  , @value = N'Order administration identifier foreign key to order_administrations table'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'pharmacy_notification_administrations'
  , @level2type = N'COLUMN'
  , @level2name = N'order_administration_id';
go