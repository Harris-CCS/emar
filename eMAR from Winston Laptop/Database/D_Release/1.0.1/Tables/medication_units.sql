create table [dbo].[medication_units]
    (
        [id]         [int]         identity (1, 1) not null
      , [site_id]    [int]         not null
      , [code]       [varchar](50) not null
      , [name]       [varchar](50) not null
      , [print_name] [varchar](50) not null
      , [is_active]  [bit]         not null
      , [priority]   [int]         null
      , constraint [pk__medication_units__id] primary key clustered ([id] asc)
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

create unique nonclustered index [ix__medication_units__code_site_id] on [dbo].[medication_units] ([code] asc, [site_id] asc);
go

/***********
 Foreign Key
***********/

alter table [dbo].[medication_units]
add constraint [fk__medication_units__sites] foreign key ([site_id]) references [dbo].[sites] ([id]);
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
    @name       = N'MS_Description'
  , @value      = N'Primary Key Column'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'medication_units'
  , @level2type = N'CONSTRAINT'
  , @level2name = N'pk__medication_units__id';
go

/***************
 Data Dictionary
    Table
***************/

execute [sys].[sp_addextendedproperty]
    @name       = N'MS_Description'
  , @value      = N'This table contains: medication units'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'medication_units';
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
  , @level1name = N'medication_units'
  , @level2type = N'COLUMN'
  , @level2name = N'id';
go

execute [sys].[sp_addextendedproperty]
    @name       = N'MS_Description'
  , @value      = N'Hospital identifier foreign key to site table'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'medication_units'
  , @level2type = N'COLUMN'
  , @level2name = N'site_id';
go

execute [sys].[sp_addextendedproperty]
    @name       = N'MS_Description'
  , @value      = N'code or short name, combined with site to make a unique pair'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'medication_units'
  , @level2type = N'COLUMN'
  , @level2name = N'code';
go

execute [sys].[sp_addextendedproperty]
    @name       = N'MS_Description'
  , @value      = N'Descriptive name'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'medication_units'
  , @level2type = N'COLUMN'
  , @level2name = N'name';
go

execute [sys].[sp_addextendedproperty]
    @name       = N'MS_Description'
  , @value      = N'Name for Printing'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'medication_units'
  , @level2type = N'COLUMN'
  , @level2name = N'print_name';
go

execute [sys].[sp_addextendedproperty]
    @name       = N'MS_Description'
  , @value      = N'is_active 1=True 0=False'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'medication_units'
  , @level2type = N'COLUMN'
  , @level2name = N'is_active';
go

execute [sys].[sp_addextendedproperty]
    @name       = N'MS_Description'
  , @value      = N'Display sort order'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'medication_units'
  , @level2type = N'COLUMN'
  , @level2name = N'priority';
go