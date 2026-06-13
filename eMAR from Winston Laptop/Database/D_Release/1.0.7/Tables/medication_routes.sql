create table [dbo].[medication_routes]
    (
        [id]        [int]          identity (1, 1) not null
      , [site_id]   [int]          not null
      , [name]      [nvarchar](50) not null
      , [priority]  [int]          null
      , [is_active] [bit]          not null
      , [code]      [varchar](50)  not null
      , [type]      [varchar](25)  null
      , constraint [pk__medication_routes__id] primary key clustered ([id] asc)
    );

go

/********
 Defaults
********/

alter table [dbo].[medication_routes] add constraint [df__medication_routes__is_active]
default ((1)) for [is_active];
go

/*****************
 Unique constraint
*****************/
/*******
 Indexes
*******/

create unique nonclustered index [ix__medication_routes__code_site_id] on [dbo].[medication_routes] ([code] asc, [site_id] asc);
go

/***********
 Foreign Key
***********/

alter table [dbo].[medication_routes]
add constraint [fk__medication_routes__sites] foreign key ([site_id]) references [dbo].[sites] ([id]);
go

/***************
 Data Dictionary
    Defaults
***************/

execute [sys].[sp_addextendedproperty]
    @name       = N'MS_Description'
  , @value      = N'default is_active to 1'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'medication_routes'
  , @level2type = N'CONSTRAINT'
  , @level2name = N'df__medication_routes__is_active';
go

/***************
 Data Dictionary
    Indexes
***************/

execute [sys].[sp_addextendedproperty]
    @name       = N'MS_Description'
  , @value      = N'Primary Key Column'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'medication_routes'
  , @level2type = N'CONSTRAINT'
  , @level2name = N'pk__medication_routes__id';
go

/***************
 Data Dictionary
    Table
***************/

execute [sys].[sp_addextendedproperty]
    @name       = N'MS_Description'
  , @value      = N'This table contains: medication routes'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'medication_routes';
go

/***************
 Data Dictionary
    Columns
***************/

execute [sys].[sp_addextendedproperty]
    @name       = N'MS_Description'
  , @value      = N'Auto increment table ID'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'medication_routes'
  , @level2type = N'COLUMN'
  , @level2name = N'id';
go

execute [sys].[sp_addextendedproperty]
    @name       = N'MS_Description'
  , @value      = N'Hospital identifier foreign key to site table'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'medication_routes'
  , @level2type = N'COLUMN'
  , @level2name = N'site_id';
go

execute [sys].[sp_addextendedproperty]
    @name       = N'MS_Description'
  , @value      = N'name'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'medication_routes'
  , @level2type = N'COLUMN'
  , @level2name = N'name';
go

execute [sys].[sp_addextendedproperty]
    @name       = N'MS_Description'
  , @value      = N'Display sort order'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'medication_routes'
  , @level2type = N'COLUMN'
  , @level2name = N'priority';
go

execute [sys].[sp_addextendedproperty]
    @name       = N'MS_Description'
  , @value      = N'is_active 1=True 0=False'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'medication_routes'
  , @level2type = N'COLUMN'
  , @level2name = N'is_active';
go

execute [sys].[sp_addextendedproperty]
    @name       = N'MS_Description'
  , @value      = N'code or short name, combined with site to make a unique pair'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'medication_routes'
  , @level2type = N'COLUMN'
  , @level2name = N'code';
go

execute [sys].[sp_addextendedproperty]
    @name       = N'MS_Description'
  , @value      = N'type of medication route'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'medication_routes'
  , @level2type = N'COLUMN'
  , @level2name = N'type';
go