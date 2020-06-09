create table [dbo].[users]
(
    [user_id]                 [int] identity(1, 1) not null
  , [site]                    [int] not null
  , [external_identifier]     [varchar](25) null
  , [type]                    [char](1) not null
  , [status]                  [char](1) not null
  , [initials_display]        [varchar](4) not null
  , [first_name]              [varchar](20) not null
  , [last_name]               [varchar](20) not null
  , [ordering_only_physician] char(1)
  , [name_display_preference] char(1)
  , [login_name]              [varchar](255) not null
  , [login_password]          [varchar](255) not null
  , [salt]                    [binary](16) not null
  , [last_login_time]         [datetimeoffset](7) null
  , [failed_login_attempts]   [int] not null
  , constraint [pk_users] primary key clustered([user_id] asc)
);
go

alter table [dbo].[users]
add constraint [df_users__ordering_only_physician] default('N') for [ordering_only_physician];
go

alter table [dbo].[users]
add constraint [df_users__name_display_preference] default('N') for [name_display_preference];
go

alter table [dbo].[users]
add constraint [df_users__failed_login_attempts] default((0)) for [failed_login_attempts];
go

create nonclustered index [ix_users__last_name_first_name_site] on [dbo].[users]([last_name] asc, [first_name] asc, [site] asc);
go

create nonclustered index [ix_users__login_name_site] on [dbo].[users]([login_name] asc, [site] asc);
go

exec [sys].[sp_addextendedproperty]
    @name = N'MS_Description'
  , @value = N'Primary Key Column'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'users'
  , @level2type = N'CONSTRAINT'
  , @level2name = N'pk_users'
go

exec [sys].[sp_addextendedproperty]
    @name = N'MS_Description'
  , @value = N'Default Index applied during design'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'users'
  , @level2type = N'INDEX'
  , @level2name = N'ix_users__last_name_first_name_site'
go

exec [sys].[sp_addextendedproperty]
    @name = N'MS_Description'
  , @value = N'Default Index applied during design'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'users'
  , @level2type = N'INDEX'
  , @level2name = N'ix_users__login_name_site'
go

exec [sys].[sp_addextendedproperty]
    @name = N'MS_Description'
  , @value = N'This table contains username, password, and other user attributes'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'users';
go

exec [sys].[sp_addextendedproperty]
    @name = N'MS_Description'
  , @value = N'Person identifier auto number'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'users'
  , @level2type = N'COLUMN'
  , @level2name = N'user_id';
go

exec [sys].[sp_addextendedproperty]
    @name = N'MS_Description'
  , @value = N'Hospital identifier 1...255 for multi-site servers, FKEY to ORG site table'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'users'
  , @level2type = N'COLUMN'
  , @level2name = N'site';
go

exec [sys].[sp_addextendedproperty]
    @name = N'MS_Description'
  , @value = N'identifier id used by external systems'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'users'
  , @level2type = N'COLUMN'
  , @level2name = N'external_identifier';
go

exec [sys].[sp_addextendedproperty]
    @name = N'MS_Description'
  , @value = N'D=physician, N=nurse, S=associate, A=administrator'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'users'
  , @level2type = N'COLUMN'
  , @level2name = N'type';
go

exec [sys].[sp_addextendedproperty]
    @name = N'MS_Description'
  , @value = N'A=active, I=inactive logical delete'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'users'
  , @level2type = N'COLUMN'
  , @level2name = N'status';
go

exec [sys].[sp_addextendedproperty]
    @name = N'MS_Description'
  , @value = N'Initials frequently used for display'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'users'
  , @level2type = N'COLUMN'
  , @level2name = N'initials_display';
go

exec [sys].[sp_addextendedproperty]
    @name = N'MS_Description'
  , @value = N'First name'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'users'
  , @level2type = N'COLUMN'
  , @level2name = N'first_name';
go

exec [sys].[sp_addextendedproperty]
    @name = N'MS_Description'
  , @value = N'Last name'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'users'
  , @level2type = N'COLUMN'
  , @level2name = N'last_name';
go

exec [sys].[sp_addextendedproperty]
    @name = N'MS_Description'
  , @value = N'Y=ordering only physician'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'users'
  , @level2type = N'COLUMN'
  , @level2name = N'ordering_only_physician';
go

exec [sys].[sp_addextendedproperty]
    @name = N'MS_Description'
  , @value = N'Name Display Preference'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'users'
  , @level2type = N'COLUMN'
  , @level2name = N'name_display_preference';
go

exec [sys].[sp_addextendedproperty]
    @name = N'MS_Description'
  , @value = N'User logon name for authentication'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'users'
  , @level2type = N'COLUMN'
  , @level2name = N'login_name';
go

exec [sys].[sp_addextendedproperty]
    @name = N'MS_Description'
  , @value = N'Encrypted user password for authentication'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'users'
  , @level2type = N'COLUMN'
  , @level2name = N'login_password';
go

exec [sys].[sp_addextendedproperty]
    @name = N'MS_Description'
  , @value = N'Random generated string used to build a hash with the password'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'users'
  , @level2type = N'COLUMN'
  , @level2name = N'salt';
go

exec [sys].[sp_addextendedproperty]
    @name = N'MS_Description'
  , @value = N'Date the user last accessed authentication'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'users'
  , @level2type = N'COLUMN'
  , @level2name = N'last_login_time';
go

exec [sys].[sp_addextendedproperty]
    @name = N'MS_Description'
  , @value = N'Failed login attempts'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'users'
  , @level2type = N'COLUMN'
  , @level2name = N'failed_login_attempts';
go
