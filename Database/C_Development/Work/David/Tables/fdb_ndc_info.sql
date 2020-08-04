create table [dbo].[fdb_ndc_info]
    (
      [ndc]           [varchar](11) not null
    , [base_ndc]      [varchar](11) null
    , [repackaged]    [int] not null
    , [medid]         [numeric](8, 0) not null
    , [packaging]     [varchar](26) null
    , [strength]      [varchar](91) null
    , [days_obsolete] [int] null
    , [GCN_SEQNO]     [numeric](6, 0) null
    , [HICL_SEQNO]    [numeric](6, 0) null
    , [ROUTED_GEN_ID] [numeric](8, 0) null);
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

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'FDB: RMIID1_MED.GCN_SEQNO
 Clinical Formulation ID
 a six-character numeric column that represents a drug formulation identifier that groups together drug products by the following criteria and is stored in the following columns:
  - Ingredient List Identifier (HICL_SEQNO)—(formerly called the Hierarchical Ingredient Code List Sequence Number) represents the list or set of ingredients in a drug formulation. The HICL_SEQNO includes active ingredients.
  - Route of Administration (GCRT)—The Route of Administration Code represents a common or representative site or method by which the drug is administered, such as oral, injection, or topical.
  - Dosage Form (GCDF)—The Dosage Form Code represents a dosage form of the clinical formulation, such as tablet or capsule.
  - Strength of Drug (STR)—The Drug Strength Description describes the drug potency in metric units.
 A unique Clinical Formulation ID (GCN_SEQNO) is assigned to each different combination of ingredient(s), strength, dosage form, and route of administration for a drug formulation. The Clinical Formulation ID (GCN_SEQNO) aggregates drug products that share like ingredient sets, route of administration, dosage form, and strength of drug but are marketed by multiple manufacturers.'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'fdb_ndc_info'
  , @level2type = N'COLUMN'
  , @level2name = N'GCN_SEQNO';
go

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'FDB: RGCNSEQ4_GCNSEQNO_MSTR.HICL_SEQNO
 Ingredient List Identifier (formerly the Hierarchical Ingredient Code List Sequence Number)
 a six-character numeric column that identifies a unique combination of active ingredients, irrespective of the manufacturer, package size, dosage form, route of administration, or strength. For example, HICL_SEQNO 000222 identifies the following set of active ingredients:
  - Guaifenesin
  - Dextromethorphan HBr
  - Pseudoephedrine HCl
 The HICL_SEQNO is associated to one (or many) Clinical Formulation ID (GCN_SEQNO) to identify the active ingredients of the clinical formulation.'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'fdb_ndc_info'
  , @level2type = N'COLUMN'
  , @level2name = N'HICL_SEQNO';
go

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'FDB: RRTGNGC0_RTD_GEN_GCNSEQNO_LNK.ROUTED_GEN_ID
 Routed Generic Identifier
 an eight-character numeric column that identifies a combination of the product ingredient set and route of administration. It is a numeric identifier that is used for the navigational purposes of directly accessing screening functions from less specific clinical concepts than clinical formulations and product identifiers.
 One ROUTED_GEN_ID is linked to one-to-many Clinical Formulation IDs (GCN_SEQNO) and zero-to-many National Drug Codes (NDC).'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'fdb_ndc_info'
  , @level2type = N'COLUMN'
  , @level2name = N'ROUTED_GEN_ID';
go