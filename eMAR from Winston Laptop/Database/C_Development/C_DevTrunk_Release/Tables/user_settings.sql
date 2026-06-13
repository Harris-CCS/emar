create table [dbo].[user_settings]
(
    [id]               [int] identity(1, 1) not null
  , [site_id]          [int] not null
  , [user_id]          [int] not null
  , [setting_id]    [int] not null
  , [setting_value] [varchar](255) not null
  , constraint [pk__user_settings__id] primary key clustered([id] asc)
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

create nonclustered index [ix__user_settings__user_id_site_id_setting_id] on [user_settings]
([user_id] asc, [site_id] asc, [setting_id] asc
)
    include([setting_value]);
go

/***********
 Foreign Key
***********/

alter table [dbo].[user_settings]
add constraint [fk__user_settings__settings] foreign key([setting_id]) references [dbo].[settings]([id]);
go

alter table [dbo].[user_settings]
add constraint [fk__user_settings__users] foreign key([user_id]) references [dbo].[users]([id]);
go

alter table [dbo].[user_settings]
add constraint [fk__user_settings__sites] foreign key([site_id]) references [dbo].[sites]([id]);
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
  , @level1name = N'user_settings'
  , @level2type = N'CONSTRAINT'
  , @level2name = N'pk__user_settings__id';
go

execute [sys].[sp_addextendedproperty]
    @name = N'MS_Description'
  , @value = N'Default index created during design.'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'user_settings'
  , @level2type = N'INDEX'
  , @level2name = N'ix__user_settings__user_id_site_id_setting_id';
go

/***************
 Data Dictionary
    Table
***************/

execute [sys].[sp_addextendedproperty]
    @name = N'MS_Description'
  , @value = N'This table contains a list of user_settings to be assigned to a user'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'user_settings';
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
  , @level1name = N'user_settings'
  , @level2type = N'COLUMN'
  , @level2name = N'id';
go

execute [sys].[sp_addextendedproperty]
    @name = N'MS_Description'
  , @value = N'Hospital identifier foreign key to site table'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'user_settings'
  , @level2type = N'COLUMN'
  , @level2name = N'site_id';
go

execute [sys].[sp_addextendedproperty]
    @name = N'MS_Description'
  , @value = N'Person identifier that this setting record applies to (Foreign Key to users table)'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'user_settings'
  , @level2type = N'COLUMN'
  , @level2name = N'user_id';
go

execute [sys].[sp_addextendedproperty]
    @name = N'MS_Description'
  , @value = N'Setting ID reference to settings table'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'user_settings'
  , @level2type = N'COLUMN'
  , @level2name = N'setting_id';
go

execute [sys].[sp_addextendedproperty]
    @name = N'MS_Description'
  , @value = N'Setting Value'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'user_settings'
  , @level2type = N'COLUMN'
  , @level2name = N'setting_value';
go
