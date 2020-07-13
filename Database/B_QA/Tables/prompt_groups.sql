create table [dbo].[prompt_groups]
    (
      [id]         [int] identity(1, 1) not null
    , [group_name] [varchar](20) not null
    , [title]      [varchar](50) not null
    , [site_id]    [int] not null
    , constraint [pk__prompt_groups__id] primary key clustered([id] asc));
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
  , @level1name = N'prompt_groups'
  , @level2type = N'CONSTRAINT'
  , @level2name = N'pk__prompt_groups__id';
go

/***************
 Data Dictionary
    Table
***************/

execute [sys].[sp_addextendedproperty]
    @name = N'MS_Description'
  , @value = N'This table contains: prompt groups'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'prompt_groups';
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
  , @level1name = N'prompt_groups'
  , @level2type = N'COLUMN'
  , @level2name = N'id';
go

execute [sys].[sp_addextendedproperty]
    @name = N'MS_Description'
  , @value = N'group_name'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'prompt_groups'
  , @level2type = N'COLUMN'
  , @level2name = N'group_name';
go

execute [sys].[sp_addextendedproperty]
    @name = N'MS_Description'
  , @value = N'title'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'prompt_groups'
  , @level2type = N'COLUMN'
  , @level2name = N'title';
go

execute [sys].[sp_addextendedproperty]
    @name = N'MS_Description'
  , @value = N'site_id'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'prompt_groups'
  , @level2type = N'COLUMN'
  , @level2name = N'site_id';
go