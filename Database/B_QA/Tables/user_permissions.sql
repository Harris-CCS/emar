create table [dbo].[user_permissions]
(
    [id]               [int] identity(1, 1) not null
  , [site_id]          [int] not null
  , [user_id]          [int] not null
  , [permission_id]    [int] not null
  , [permission_value] [varchar](255) not null
  , constraint [pk__user_permissions__id] primary key clustered([id] asc)
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

create nonclustered index [user_permissions__user_id_site_id] on [user_permissions]
([user_id] asc, [site_id] asc
)
    include([permission_value]);
go

/***********
 Foreign Key
***********/

alter table [dbo].[user_permissions]
add constraint [fk__user_permissions__permissions] foreign key([permission_id]) references [dbo].[permissions]([id]);
go

alter table [dbo].[user_permissions]
add constraint [fk__user_permissions__users] foreign key([user_id]) references [dbo].[users]([id]);
go

alter table [dbo].[user_permissions]
add constraint [fk__user_permissions__sites] foreign key([site_id]) references [dbo].[sites]([id]);
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
  , @level1name = N'user_permissions'
  , @level2type = N'CONSTRAINT'
  , @level2name = N'pk__user_permissions__id';
go

execute [sys].[sp_addextendedproperty]
    @name = N'MS_Description'
  , @value = N'Default index created during design.'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'user_permissions'
  , @level2type = N'INDEX'
  , @level2name = N'user_permissions__user_id_site_id';
go

/***************
 Data Dictionary
    Table
***************/

execute [sys].[sp_addextendedproperty]
    @name = N'MS_Description'
  , @value = N'This table contains a list of user_permissions to be assigned to a user'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'user_permissions';
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
  , @level1name = N'user_permissions'
  , @level2type = N'COLUMN'
  , @level2name = N'id';
go

execute [sys].[sp_addextendedproperty]
    @name = N'MS_Description'
  , @value = N'Hospital identifier foriegn key to site table'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'user_permissions'
  , @level2type = N'COLUMN'
  , @level2name = N'site_id';
go

execute [sys].[sp_addextendedproperty]
    @name = N'MS_Description'
  , @value = N'User ID reference to users table'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'user_permissions'
  , @level2type = N'COLUMN'
  , @level2name = N'user_id';
go

execute [sys].[sp_addextendedproperty]
    @name = N'MS_Description'
  , @value = N'Permission ID reference to permissions table'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'user_permissions'
  , @level2type = N'COLUMN'
  , @level2name = N'permission_id';
go

execute [sys].[sp_addextendedproperty]
    @name = N'MS_Description'
  , @value = N'Permission Value R=Read W=Write X=No Access'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'user_permissions'
  , @level2type = N'COLUMN'
  , @level2name = N'permission_value';
go
