create table [dbo].[users]
    (
      [id]                      [int] identity(1, 1) not null
    , [site_id]                 [int] not null
    , [type]                    [char](1) not null
    , [is_active]               [bit] not null
    , [initials_display]        [varchar](4) not null
    , [first_name]              [varchar](20) not null
    , [last_name]               [varchar](20) not null
    , [ordering_only_physician] [bit]
    , [name_display_preference] [bit]
    , [login_name]              [varchar](255) not null
    , [login_password]          [varchar](255) not null
    , [salt]                    [binary](16) not null
    , [last_login_time]         [datetimeoffset](7) null
    , [failed_login_attempts]   [int] not null
    , constraint [pk__users__id] primary key clustered([id] asc));
go

/********
 Defaults
********/

alter table [dbo].[users]
add constraint [df__users__ordering_only_physician] default('N') for [ordering_only_physician];
go

alter table [dbo].[users]
add constraint [df__users__name_display_preference] default('N') for [name_display_preference];
go

alter table [dbo].[users]
add constraint [df__users__failed_login_attempts] default((0)) for [failed_login_attempts];
go

/*******
 Indexes
*******/

create nonclustered index [ix_users__last_name_first_name_site_id] on [dbo].[users]
    ([last_name] asc, [first_name] asc, [site_id] asc);
go

create nonclustered index [ix_users__login_name_site_id] on [dbo].[users]
    ([login_name] asc, [site_id] asc);
go

/***********
 Foreign Key
***********/

alter table [dbo].[users]
add constraint [fk__users__sites] foreign key([site_id]) references [dbo].[sites]([id]);
go

/***************
 Data Dictionary
    Defaults
***************/

execute [sys].[sp_addextendedproperty]
    @name = N'MS_Description'
  , @value = N'default ordering_only_physician to N'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'users'
  , @level2type = N'CONSTRAINT'
  , @level2name = N'df__users__ordering_only_physician';
go

execute [sys].[sp_addextendedproperty]
    @name = N'MS_Description'
  , @value = N'default df__users__name_display_preference to N'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'users'
  , @level2type = N'CONSTRAINT'
  , @level2name = N'df__users__name_display_preference';
go

execute [sys].[sp_addextendedproperty]
    @name = N'MS_Description'
  , @value = N'default df__users__failed_login_attempts to 0'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'users'
  , @level2type = N'CONSTRAINT'
  , @level2name = N'df__users__failed_login_attempts';
go

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
  , @level1name = N'users'
  , @level2type = N'CONSTRAINT'
  , @level2name = N'pk__users__id';
go

execute [sys].[sp_addextendedproperty]
    @name = N'MS_Description'
  , @value = N'Default Index applied during design'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'users'
  , @level2type = N'INDEX'
  , @level2name = N'ix_users__last_name_first_name_site_id';
go

execute [sys].[sp_addextendedproperty]
    @name = N'MS_Description'
  , @value = N'Default Index applied during design'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'users'
  , @level2type = N'INDEX'
  , @level2name = N'ix_users__login_name_site_id';
go

/***************
 Data Dictionary
    Table
***************/

execute [sys].[sp_addextendedproperty]
    @name = N'MS_Description'
  , @value = N'This table contains username, password, and other user attributes'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'users';
go

/***************
 Data Dictionary
    Columns
***************/

execute [sys].[sp_addextendedproperty]
    @name = N'MS_Description'
  , @value = N'Person identifier auto number'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'users'
  , @level2type = N'COLUMN'
  , @level2name = N'id';
go

execute [sys].[sp_addextendedproperty]
    @name = N'MS_Description'
  , @value = N'Hospital identifier 1...255 for multi-site servers, FKEY to ORG site table'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'users'
  , @level2type = N'COLUMN'
  , @level2name = N'site_id';
go

execute [sys].[sp_addextendedproperty]
    @name = N'MS_Description'
  , @value = N'D=physician, N=nurse, S=associate, A=administrator'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'users'
  , @level2type = N'COLUMN'
  , @level2name = N'type';
go

execute [sys].[sp_addextendedproperty]
    @name = N'MS_Description'
  , @value = N'is_active 1=true 0=false'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'users'
  , @level2type = N'COLUMN'
  , @level2name = N'is_active';
go

execute [sys].[sp_addextendedproperty]
    @name = N'MS_Description'
  , @value = N'Initials frequently used for display'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'users'
  , @level2type = N'COLUMN'
  , @level2name = N'initials_display';
go

execute [sys].[sp_addextendedproperty]
    @name = N'MS_Description'
  , @value = N'First name'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'users'
  , @level2type = N'COLUMN'
  , @level2name = N'first_name';
go

execute [sys].[sp_addextendedproperty]
    @name = N'MS_Description'
  , @value = N'Last name'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'users'
  , @level2type = N'COLUMN'
  , @level2name = N'last_name';
go

execute [sys].[sp_addextendedproperty]
    @name = N'MS_Description'
  , @value = N'Y=ordering only physician 1=true 0=false'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'users'
  , @level2type = N'COLUMN'
  , @level2name = N'ordering_only_physician';
go

execute [sys].[sp_addextendedproperty]
    @name = N'MS_Description'
  , @value = N'Name Display Preference 1=true 0=false'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'users'
  , @level2type = N'COLUMN'
  , @level2name = N'name_display_preference';
go

execute [sys].[sp_addextendedproperty]
    @name = N'MS_Description'
  , @value = N'User logon name for authentication'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'users'
  , @level2type = N'COLUMN'
  , @level2name = N'login_name';
go

execute [sys].[sp_addextendedproperty]
    @name = N'MS_Description'
  , @value = N'Encrypted user password for authentication'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'users'
  , @level2type = N'COLUMN'
  , @level2name = N'login_password';
go

execute [sys].[sp_addextendedproperty]
    @name = N'MS_Description'
  , @value = N'Random generated string used to build a hash with the password'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'users'
  , @level2type = N'COLUMN'
  , @level2name = N'salt';
go

execute [sys].[sp_addextendedproperty]
    @name = N'MS_Description'
  , @value = N'Date the user last accessed authentication'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'users'
  , @level2type = N'COLUMN'
  , @level2name = N'last_login_time';
go

execute [sys].[sp_addextendedproperty]
    @name = N'MS_Description'
  , @value = N'Failed login attempts'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'users'
  , @level2type = N'COLUMN'
  , @level2name = N'failed_login_attempts';
go