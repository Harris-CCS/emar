create table [dbo].[fdb_ndc_info]
    (
      [ndc]           [varchar](11) not null
    , [base_ndc]      [varchar](11) null
    , [repackaged]    [int] not null
    , [medid]         [numeric](8, 0) not null
    , [packaging]     [varchar](26) null
    , [strength]      [varchar](91) null
    , [days_obsolete] [int] null);
go
/********
 Defaults
********/
/*******
 Indexes
*******/

create clustered index [ndc-base_ndc] on [dbo].[fdb_ndc_info]
    ([ndc] asc, [base_ndc] asc);
go

create nonclustered index [ndc] on [dbo].[fdb_ndc_info]
    ([ndc] asc);
go

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
  , @value = N'Default Index taken from ibex fdb_ndc_info'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'fdb_ndc_info'
  , @level2type = N'Index'
  , @level2name = N'ndc-base_ndc';
go

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'Default Index taken from ibex fdb_ndc_info'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'fdb_ndc_info'
  , @level2type = N'Index'
  , @level2name = N'ndc';
go

/***************
 Data Dictionary
    Table
***************/

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'FDB_NDC_INFO - FDB Information Related to a Distinct NDC
This table contains information specific to individual NDC codes to improve performance.'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'fdb_ndc_info';
go

/***************
 Data Dictionary
    Columns
***************/

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'National Drug Code that identifies the brand, formulation and packaging of a drug'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'fdb_ndc_info'
  , @level2type = N'COLUMN'
  , @level2name = N'ndc';
go

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'A representative NDC (active or least obsolete) that will be '
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'fdb_ndc_info'
  , @level2type = N'COLUMN'
  , @level2name = N'base_ndc';
go

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'The repackaged status (0 or 1)'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'fdb_ndc_info'
  , @level2type = N'COLUMN'
  , @level2name = N'repackaged';
go

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'FDB code that uniquely identifies a brand/formulation'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'fdb_ndc_info'
  , @level2type = N'COLUMN'
  , @level2name = N'medid';
go

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'The amount of medication in a pre-packaged item (IV bag, syringe, etc.)'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'fdb_ndc_info'
  , @level2type = N'COLUMN'
  , @level2name = N'packaging';
go

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'The medication strength including packaging information'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'fdb_ndc_info'
  , @level2type = N'COLUMN'
  , @level2name = N'strength';
go

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'Number of days past the obsolete date identified in the database'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'fdb_ndc_info'
  , @level2type = N'COLUMN'
  , @level2name = N'days_obsolete';
go