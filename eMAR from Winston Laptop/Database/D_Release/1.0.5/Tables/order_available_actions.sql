create table [dbo].[order_available_actions]
    (
      [id]                  int not null identity(1, 1)
    , [site_id]             [int] not null
    , [order_status]        [varchar](25) not null
    , [available_action_id] [int] not null
    , [is_pit]              [bit] null
    , [is_prn_only]         [bit] not null);
go

/********
 Defaults
********/
/*****************
 Unique constraint
*****************/

alter table [dbo].[order_available_actions]
add constraint [cc__order_availalbe_actions__order_status] check([order_status] = 'PendingDiscontinue'
                                                                 or [order_status] = 'Pending'
                                                                 or [order_status] = 'OnHold'
                                                                 or [order_status] = 'OnGoing'
                                                                 or [order_status] = 'Discontinued'
                                                                 or [order_status] = 'Deleted'
                                                                 or [order_status] = 'Completed'
                                                                 or [order_status] = 'Cancelled'
                                                                 or [order_status] is not null);

go

/*******
 Indexes
*******/

alter table [dbo].[order_available_actions]
add constraint [pk__order_available_actions__id] primary key nonclustered([id]);

go

alter table [dbo].[order_available_actions]
add constraint [uc__order_available_actions__site_id_order_status_available_action] unique clustered([site_id], [order_status], [available_action_id]);

go

/***********
 Foreign Key
***********/

alter table [dbo].[order_available_actions]
add constraint [fk__order_available_actions__sites] foreign key([site_id]) references [dbo].[sites]([id]);
go

alter table [dbo].[order_available_actions]
add constraint [fk__order_available_actions__actions] foreign key([available_action_id]) references [dbo].[actions]([id]);
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
  , @level1name = N'order_available_actions'
  , @level2type = N'CONSTRAINT'
  , @level2name = N'pk__order_available_actions__id';
go

/***************
 Data Dictionary
    Table
***************/

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'This table contains: available actions based on order status'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'order_available_actions';
go

/***************
 Data Dictionary
    Columns
***************/

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'Hospital identifier foreign key to site table'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'order_available_actions'
  , @level2type = N'COLUMN'
  , @level2name = N'site_id';
go

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'allowable order status'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'order_available_actions'
  , @level2type = N'COLUMN'
  , @level2name = N'order_status';
go

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'Action identifier foreign key to actions table'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'order_available_actions'
  , @level2type = N'COLUMN'
  , @level2name = 'available_action_id';
go

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'Is this action for Point in Time orders?'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'order_available_actions'
  , @level2type = N'COLUMN'
  , @level2name = 'is_pit';
go

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'Is this action for PRN only orders?'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'order_available_actions'
  , @level2type = N'COLUMN'
  , @level2name = 'is_prn_only';
go