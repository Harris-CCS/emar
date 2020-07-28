create table [dbo].[fdb_allergy_name]
    (
      [MEDID]          [numeric](8, 0) not null
    , [med_name]       [varchar](70) null
    , [MED_NAME_ID]    [numeric](8, 0) null
    , [PC_MED_NAME_ID] [varchar](9) null
    , [HICL_SEQNO]     [numeric](6, 0) null
    , [PC_HICL_SEQNO]  [varchar](7) null
    , [allergy_name]   [varchar](70) null);
go
/********
 Defaults
********/
/*******
 Indexes
*******/

create clustered index [ClusteredIndex-20140611-084822] on [dbo].[fdb_allergy_name]
    ([MEDID] asc);
go

create nonclustered index [NonClusteredIndex-20140611-102020] on [dbo].[fdb_allergy_name]
    ([MED_NAME_ID] asc);
go

create nonclustered index [NonClusteredIndex-20140611-103242] on [dbo].[fdb_allergy_name]
    ([med_name] asc);
go

create nonclustered index [NonClusteredIndex-20140611-103253] on [dbo].[fdb_allergy_name]
    ([allergy_name] asc);
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
  , @value = N'Default Index taken from ibex fdb_allergy_name'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'fdb_allergy_name'
  , @level2type = N'Index'
  , @level2name = N'ClusteredIndex-20140611-084822';
go

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'Default Index taken from ibex fdb_allergy_name'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'fdb_allergy_name'
  , @level2type = N'Index'
  , @level2name = N'NonClusteredIndex-20140611-102020';
go

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'Default Index taken from ibex fdb_allergy_name'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'fdb_allergy_name'
  , @level2type = N'Index'
  , @level2name = N'NonClusteredIndex-20140611-103242';
go

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'Default Index taken from ibex fdb_allergy_name'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'fdb_allergy_name'
  , @level2type = N'Index'
  , @level2name = N'NonClusteredIndex-20140611-103253';
go

/***************
 Data Dictionary
    Table
***************/

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'FDB_ALLERGY_NAME - FDB Code/Name Information Associated with Allergies
This table is for FDB only and contains pre-selected and formatted names/codes associated with Allergies.'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'fdb_allergy_name';
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
  , @level1name = N'fdb_allergy_name'
  , @level2type = N'COLUMN'
  , @level2name = N'MEDID';
go

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'The lowest level drug name for performing a drug search'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'fdb_allergy_name'
  , @level2type = N'COLUMN'
  , @level2name = N'med_name';
go

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'The Medication Name ID associated with the Med Name'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'fdb_allergy_name'
  , @level2type = N'COLUMN'
  , @level2name = N'MED_NAME_ID';
go

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'The string coded version of the MED_NAME_ID (prefixed with an ''G'')'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'fdb_allergy_name'
  , @level2type = N'COLUMN'
  , @level2name = N'PC_MED_NAME_ID';
go

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'The numeric storage of code for the Hierarchical Ingredient Concept List'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'fdb_allergy_name'
  , @level2type = N'COLUMN'
  , @level2name = N'HICL_SEQNO';
go

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'The string coded version of the HICL_SEQNO (prefixed with an ''L'')'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'fdb_allergy_name'
  , @level2type = N'COLUMN'
  , @level2name = N'PC_HICL_SEQNO';
go

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'The appropriate Brand Name that provides a unique allergy profile'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'fdb_allergy_name'
  , @level2type = N'COLUMN'
  , @level2name = N'allergy_name';
go