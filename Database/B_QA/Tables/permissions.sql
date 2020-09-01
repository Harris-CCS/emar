create table [dbo].[permissions]
(
    [id]          [int] identity(1, 1) not null
  , [name]        [nvarchar](40) not null
  , [description] [nvarchar](255) not null
  , constraint [pk__permissions__id] primary key clustered([id] asc)
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
  , @value = N'Primary Key Constraint'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'permissions'
  , @level2type = N'CONSTRAINT'
  , @level2name = N'pk__permissions__id';
go

/***************
 Data Dictionary
    Table
***************/

execute [sys].[sp_addextendedproperty]
    @name = N'MS_Description'
  , @value = N'This table contains a list of permissions to be assigned to a user'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'permissions';
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
  , @level1name = N'permissions'
  , @level2type = N'COLUMN'
  , @level2name = N'id';
go

execute [sys].[sp_addextendedproperty]
    @name = N'MS_Description'
  , @value = N'Permission Name'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'permissions'
  , @level2type = N'COLUMN'
  , @level2name = N'name';
go

execute [sys].[sp_addextendedproperty]
    @name = N'MS_Description'
  , @value = N'Description including possible valuse.'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'permissions'
  , @level2type = N'COLUMN'
  , @level2name = N'description';
go