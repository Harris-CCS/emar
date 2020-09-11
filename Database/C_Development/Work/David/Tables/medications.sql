create table [dbo].[medications]
    (
      [id]           [int] identity(1, 1) not null
    , [site_id]      [int] not null
    , [drug_id]      [varchar](32) not null
    , [display_name] [nvarchar](255) not null
    , [drug_vendor]  [char](1) not null
    , constraint [pk__medications__id] primary key clustered([id] asc));
go

/********
 Defaults
********/
/*******
 Indexes
*******/

alter table [dbo].[medications]
add constraint [uc__medications__sites] unique([display_name], [site_id], [drug_id], [drug_vendor]);
go

/***********
 Foreign Key
***********/

alter table [dbo].[medications]
add constraint [fk__medications__sites] foreign key([site_id]) references [dbo].[sites]([id]);
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
  , @level1name = N'medications'
  , @level2type = N'CONSTRAINT'
  , @level2name = N'pk__medications__id';
go

/***************
 Data Dictionary
    Table
***************/

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'This table contains: patient orders'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'medications';
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
  , @level1name = N'medications'
  , @level2type = N'COLUMN'
  , @level2name = N'id';
go

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'Medication / Combo Medication name'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'medications'
  , @level2type = N'COLUMN'
  , @level2name = 'display_name';
go