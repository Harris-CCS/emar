create table [dbo].[order_administration_available_actions]
    (
      [id]                    int identity(1, 1) not null
    , [site_id]               int not null
    , [order_status]          varchar(25) not null
    , [administration_status] varchar(20) not null
    , [point_in_time]         bit null
    , [available_action_id]   int not null
    , constraint [pk__order_administration_available_actions__id] primary key clustered([id] asc));
go

/********
 Defaults
********/
/*****************
 Unique constraint
*****************/

alter table [dbo].[order_administration_available_actions]
add constraint [uc__order_administration_available_actions__site_id_order_status_administration_status_point_in_time_available_action] unique([site_id] asc, [order_status] asc, [administration_status] asc, [point_in_time] asc, [available_action_id] asc);
go

alter table [dbo].[order_administration_available_actions]
add constraint [cc__order_administration_available_actions__order_status] check([order_status] = 'PendingDiscontinue'
                                                                                or [order_status] = 'Pending'
                                                                                or [order_status] = 'OnHold'
                                                                                or [order_status] = 'OnGoing'
                                                                                or [order_status] = 'Discontinued'
                                                                                or [order_status] = 'Deleted'
                                                                                or [order_status] = 'Completed'
                                                                                or [order_status] = 'Cancelled'
                                                                                or [order_status] is not null);
go

alter table [dbo].[order_administration_available_actions]
add constraint [cc__order_administration_available_actions__administration_status] check([administration_status] = 'Pending'
                                                                                         or [administration_status] = 'OnHold'
                                                                                         or [administration_status] = 'OnGoing'
                                                                                         or [administration_status] = 'Missed'
                                                                                         or [administration_status] = 'Late'
                                                                                         or [administration_status] = 'Given'
                                                                                         or [administration_status] is not null);
go

/*******
 Indexes
*******/
/***********
 Foreign Key
***********/

alter table [dbo].[order_administration_available_actions]
add constraint [fk__order_administration_available_actions__sites] foreign key([site_id]) references [dbo].[sites]([id]);
go

alter table [dbo].[order_administration_available_actions]
add constraint [fk__order_administration_available_actions__actions] foreign key([available_action_id]) references [dbo].[actions]([id]);
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
  , @level1name = N'order_administration_available_actions'
  , @level2type = N'CONSTRAINT'
  , @level2name = N'pk__order_administration_available_actions__id';
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
  , @level1name = N'order_administration_available_actions';
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
  , @level1name = N'order_administration_available_actions'
  , @level2type = N'COLUMN'
  , @level2name = N'id';
go

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'Hospital identifier foreign key to site table'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'order_administration_available_actions'
  , @level2type = N'COLUMN'
  , @level2name = N'site_id';
go

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'Allowable order status'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'order_administration_available_actions'
  , @level2type = N'COLUMN'
  , @level2name = N'order_status';
go

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'Allowable administration status'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'order_administration_available_actions'
  , @level2type = N'COLUMN'
  , @level2name = N'administration_status';
go

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'Is this a point_in_time order'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'order_administration_available_actions'
  , @level2type = N'COLUMN'
  , @level2name = N'point_in_time';
go

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'Action identifier foreign key to actions table'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'order_administration_available_actions'
  , @level2type = N'COLUMN'
  , @level2name = 'available_action_id';
go