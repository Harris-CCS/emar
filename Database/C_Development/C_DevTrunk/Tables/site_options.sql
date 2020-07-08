create table [dbo].[site_options]
    (
      [id]           [int] identity(1, 1) not null
    , [site_id]      [int] not null
    , [option_id]    [int] not null
    , [option_value] [varchar](255) not null
    , constraint [pk__site_options__id] primary key clustered([id] asc));
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

create nonclustered index [site_options__option_id_site_id] on [site_options]
    ([option_id] asc, [site_id] asc)
      include
    ([option_value]);
go

/***********
 Foreign Key
***********/

alter table [dbo].[site_options]
add constraint [fk__site_options__options] foreign key([option_id]) references [dbo].[options]([id]);
go

alter table [dbo].[site_options]
add constraint [fk__site_options__sites] foreign key([site_id]) references [dbo].[sites]([id]);
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
  , @level1name = N'site_options'
  , @level2type = N'CONSTRAINT'
  , @level2name = N'pk__site_options__id';
go

execute [sys].[sp_addextendedproperty]
    @name = N'MS_Description'
  , @value = N'Default index created during design.'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'site_options'
  , @level2type = N'INDEX'
  , @level2name = N'site_options__option_id_site_id';
go

/***************
 Data Dictionary
    Table
***************/

execute [sys].[sp_addextendedproperty]
    @name = N'MS_Description'
  , @value = N'This table contains a list of site_options to be assigned to a site'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'site_options';
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
  , @level1name = N'site_options'
  , @level2type = N'COLUMN'
  , @level2name = N'id';
go

execute [sys].[sp_addextendedproperty]
    @name = N'MS_Description'
  , @value = N'Hospital identifier foriegn key to site table'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'site_options'
  , @level2type = N'COLUMN'
  , @level2name = N'site_id';
go

execute [sys].[sp_addextendedproperty]
    @name = N'MS_Description'
  , @value = N'Option ID reference to options table'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'site_options'
  , @level2type = N'COLUMN'
  , @level2name = N'option_id';
go

execute [sys].[sp_addextendedproperty]
    @name = N'MS_Description'
  , @value = N'Option Value'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'site_options'
  , @level2type = N'COLUMN'
  , @level2name = N'option_value';
go