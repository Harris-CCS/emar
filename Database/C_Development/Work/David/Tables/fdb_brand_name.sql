create table [dbo].[fdb_brand_name]
    (
      [MEDID]            [numeric](8, 0) not null
    , [long_brand_name]  [varchar](70) null
    , [active]           [varchar](max) null
    , [MED_NAME_ID]      [numeric](8, 0) null
    , [PC_MED_NAME_ID]   [varchar](9) null
    , [ROUTED_GEN_ID]    [numeric](8, 0) null
    , [PC_ROUTED_GEN_ID] [varchar](9) null
    , [brand_name]       [varchar](70) null
    , [dea_schedule]     [varchar](1) not null
    , [rx_otc]           [varchar](1) null
    , [erx_search]       [int] not null
    , [MEDID_string] as convert([varchar](32), [MEDID]) persisted);
go

/********
 Defaults
********/
/*******
 Indexes
*******/

create clustered index [ClusteredIndex-20140611-085119] on [dbo].[fdb_brand_name]
    ([MEDID] asc);
go

create index [ix__fdb_brand_name__MEDID_string] on [dbo].[fdb_brand_name]
    ([MEDID_string] asc);
go

create nonclustered index [NonClusteredIndex-20140611-101716] on [dbo].[fdb_brand_name]
    ([brand_name] asc);
go

create nonclustered index [NonClusteredIndex-20140611-101732] on [dbo].[fdb_brand_name]
    ([PC_ROUTED_GEN_ID] asc);
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
  , @value = N'Default Index taken from ibex fdb_brand_name'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'fdb_brand_name'
  , @level2type = N'Index'
  , @level2name = N'ClusteredIndex-20140611-085119';
go

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'Default Index taken from ibex fdb_brand_name'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'fdb_brand_name'
  , @level2type = N'Index'
  , @level2name = N'NonClusteredIndex-20140611-101716';
go

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'Default Index taken from ibex fdb_brand_name'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'fdb_brand_name'
  , @level2type = N'Index'
  , @level2name = N'NonClusteredIndex-20140611-101732';
go

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'Index added for join performance on computed column MEDID_string'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'fdb_brand_name'
  , @level2type = N'Index'
  , @level2name = N'ix__fdb_brand_name__MEDID_string';
go

/***************
 Data Dictionary
    Table
***************/

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'FDB_BRAND_NAME - FDB Code/Name Information Associated with Drug Interaction Checking
This table is for FDB only and contains pre-selected and formatted names/codes associated with Medications.'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'fdb_brand_name';
go

/***************
 Data Dictionary
    Columns
***************/

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'The code identifying a unique Drug Name/Formulation (includes strength, dose-form and route)'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'fdb_brand_name'
  , @level2type = N'COLUMN'
  , @level2name = N'MEDID';
go

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'The complete name (med name and formulation information)'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'fdb_brand_name'
  , @level2type = N'COLUMN'
  , @level2name = N'long_brand_name';
go

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'The active ingredient'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'fdb_brand_name'
  , @level2type = N'COLUMN'
  , @level2name = N'active';
go

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'Medication Name ID associated with the Med Name'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'fdb_brand_name'
  , @level2type = N'COLUMN'
  , @level2name = N'MED_NAME_ID';
go

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'The string coded version of the MED_NAME_ID (prefixed with an ''G'')'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'fdb_brand_name'
  , @level2type = N'COLUMN'
  , @level2name = N'PC_MED_NAME_ID';
go

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'The Routed Generic ID'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'fdb_brand_name'
  , @level2type = N'COLUMN'
  , @level2name = N'ROUTED_GEN_ID';
go

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'The string coded version of the ROUTED_GEN_ID (prefixed with an ''R'')'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'fdb_brand_name'
  , @level2type = N'COLUMN'
  , @level2name = N'PC_ROUTED_GEN_ID';
go

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'The appropriate Brand Name that provides a unique Drug Interaction profile'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'fdb_brand_name'
  , @level2type = N'COLUMN'
  , @level2name = N'brand_name';
go

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'The Drug Enforcement Agency schedule identifying controlled substances'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'fdb_brand_name'
  , @level2type = N'COLUMN'
  , @level2name = N'dea_schedule';
go

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'Identify whether a drug is available as Over-The-Counter (''O''), Prescription Only (''R''), Both (''B'') or Unknown (empty)'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'fdb_brand_name'
  , @level2type = N'COLUMN'
  , @level2name = N'rx_otc';
go

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'Identify whether a drug can be searched via e-Prescribing (0 or 1)'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'fdb_brand_name'
  , @level2type = N'COLUMN'
  , @level2name = N'erx_search';
go

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'String representation of the MEDID column.'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'fdb_brand_name'
  , @level2type = N'COLUMN'
  , @level2name = N'MEDID_string';
go