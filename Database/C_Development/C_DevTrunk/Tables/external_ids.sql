create table [dbo].[external_ids]
(
    [internal_id] [bigint] not null
  , [site_id]     [int] not null
  , [vendor]      [varchar](50) not null
  , [entity]      [varchar](50) not null
  , [external_id] [varchar](50) not null
  , constraint [pk__external_ids] primary key clustered([internal_id] asc, [site_id] asc, [vendor] asc, [entity] asc, [external_id] asc)
);
go

/*********
 Defaults 
*********/

/*********
 Indexes  
*********/

create nonclustered index [ix__external_ids] on [dbo].[external_ids]
([external_id] asc, [site_id] asc, [vendor] asc, [entity] asc, [internal_id] asc
);
go

/***********
 Foriegn Key
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
  , @value = N'Multi-Part Primary Key linking internal to external ids'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'external_ids'
  , @level2type = N'CONSTRAINT'
  , @level2name = N'pk__external_ids';
go

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'Multi-Part Index linking external to internal ids'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'external_ids'
  , @level2type = N'INDEX'
  , @level2name = N'ix__external_ids';
go

/***************
 Data Dictionary
    Table
***************/
execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'Table used to link external vendor ID to internal database ID'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'external_ids';
go

/***************
 Data Dictionary
    Columns
***************/

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'Internal Database ID of Entity Record'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'external_ids'
  , @level2type = N'COLUMN'
  , @level2name = N'internal_id';
go

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'Site ID of database record'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'external_ids'
  , @level2type = N'COLUMN'
  , @level2name = N'site_id';
go

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'External Vendor Name / Code'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'external_ids'
  , @level2type = N'COLUMN'
  , @level2name = N'vendor';
go

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'Database Table Name for id linking'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'external_ids'
  , @level2type = N'COLUMN'
  , @level2name = N'entity';
go

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'External Vendor ID of Entity Record'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'external_ids'
  , @level2type = N'COLUMN'
  , @level2name = N'external_id';
go