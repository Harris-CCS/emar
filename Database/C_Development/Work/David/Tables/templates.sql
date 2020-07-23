create table [dbo].[templates]
    (
      [id]        [int] identity(1, 1) not null
    , [name]      [nvarchar](20) not null
    , [is_active] [bit] not null
    , [title]     [varchar](50) not null
    , [site_id]   [int] not null
    , constraint pk__templates__id primary key clustered([id] asc));
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
/***********
 Foreign Key
***********/
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
  , @level1name = N'order_event_details'
  , @level2type = N'CONSTRAINT'
  , @level2name = N'pk__templates__id';
go

/***************
 Data Dictionary
    Table
***************/
/***************
 Data Dictionary
    Columns
***************/

alter table [dbo].[templates]
add constraint [df__templates__is_active] default((1)) for [is_active];
go

execute [sys].[sp_addextendedproperty]
    @name = N'MS_Description'
  , @value = N'Auto increment table ID'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'templates'
  , @level2type = N'COLUMN'
  , @level2name = N'id';
go

execute [sys].[sp_addextendedproperty]
    @name = N'MS_Description'
  , @value = N'name'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'templates'
  , @level2type = N'COLUMN'
  , @level2name = N'name';
go

execute [sys].[sp_addextendedproperty]
    @name = N'MS_Description'
  , @value = N'is_active 1=True 0=False'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'templates'
  , @level2type = N'COLUMN'
  , @level2name = N'is_active';
go

execute [sys].[sp_addextendedproperty]
    @name = N'MS_Description'
  , @value = N'default templates__is_active to 1'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'templates'
  , @level2type = N'CONSTRAINT'
  , @level2name = N'df__templates__is_active';
go

execute [sys].[sp_addextendedproperty]
    @name = N'MS_Description'
  , @value = N'title'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'templates'
  , @level2type = N'COLUMN'
  , @level2name = N'title';
go

execute [sys].[sp_addextendedproperty]
    @name = N'MS_Description'
  , @value = N'site_id'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'templates'
  , @level2type = N'COLUMN'
  , @level2name = N'site_id';
go

execute [sys].[sp_addextendedproperty]
    @name = N'MS_Description'
  , @value = N'This table contains: templates'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'templates';
go