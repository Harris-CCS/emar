create table [dbo].[site_formulary]
(
    [id]                  bigint identity(1, 1) not null
  , [site_id]             int not null
  , [ndc]                 [varchar](32) null
  , [drug_id]             [varchar](32) not null
  , [brand_name]          [varchar](255) not null
  , [hospital_drug_code]  [varchar](32) null
  , [service_code]        [varchar](32) null
  , [is_inpatient]        [bit] not null
  , [is_outpatient]       [bit] not null
  , [is_pyxis]            [bit] not null
  , constraint [pk__site_formulary__id] primary key clustered([id] asc)
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

alter table [dbo].[site_formulary]
add constraint [fk__site_formulary__sites] foreign key([site_id]) references [dbo].[sites]([id]);
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
  , @level1name = N'site_formulary'
  , @level2type = N'CONSTRAINT'
  , @level2name = N'pk__site_formulary__id';
go

/***************
 Data Dictionary
    Table
***************/

execute [sys].[sp_addextendedproperty]
    @name = N'MS_Description'
  , @value = N'This table stores information regarding the hospital drug formulary as indexed by a National Drug Code'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'site_formulary';
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
  , @level1name = N'site_formulary'
  , @level2type = N'COLUMN'
  , @level2name = N'id';
go

execute [sys].[sp_addextendedproperty]
    @name = N'MS_Description'
  , @value = N'Hospital identifier foriegn key to site table'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'site_formulary'
  , @level2type = N'COLUMN'
  , @level2name = N'site_id';
go

execute [sys].[sp_addextendedproperty]
    @name = N'MS_Description'
  , @value = N'Drug NDC (National Drug Code)'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'site_formulary'
  , @level2type = N'COLUMN'
  , @level2name = N'ndc';
go

execute [sys].[sp_addextendedproperty]
    @name = N'MS_Description'
  , @value = N'External Vendor Drug Database Identifier
    FDB: MEDID (MED Medication ID (Stable ID))
    Multum: dnum
These 3 columns will be carried as a set ndc,drug_id,brand_name
while drug_id and brand_name are vendor specific concepts and can be derived from ndc number
this will aid in display and lookup performance.
'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'site_formulary'
  , @level2type = N'COLUMN'
  , @level2name = N'drug_id';
go

execute [sys].[sp_addextendedproperty]
    @name = N'MS_Description'
  , @value = N'Brand name'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'site_formulary'
  , @level2type = N'COLUMN'
  , @level2name = N'brand_name';
go

execute [sys].[sp_addextendedproperty]
    @name = N'MS_Description'
  , @value = N'Hospital specific code for drug'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'site_formulary'
  , @level2type = N'COLUMN'
  , @level2name = N'hospital_drug_code';
go

execute [sys].[sp_addextendedproperty]
    @name = N'MS_Description'
  , @value = N'Site billing code for Procedure, supplies, and other reportable services '
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'site_formulary'
  , @level2type = N'COLUMN'
  , @level2name = N'service_code';
go

execute [sys].[sp_addextendedproperty]
    @name = N'MS_Description'
  , @value = N'Flag indicating that formulary item is on In-patient formulary. 1=True 0=False'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'site_formulary'
  , @level2type = N'COLUMN'
  , @level2name = N'is_inpatient';
go

execute [sys].[sp_addextendedproperty]
    @name = N'MS_Description'
  , @value = N'Flag indicating that formulary item is on Out-patient formulary. 1=True 0=False'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'site_formulary'
  , @level2type = N'COLUMN'
  , @level2name = N'is_outpatient';
go

execute [sys].[sp_addextendedproperty]
    @name = N'MS_Description'
  , @value = N'Flag indicating that formulary item is on the medication dispensing machine. 1=True 0=False'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'site_formulary'
  , @level2type = N'COLUMN'
  , @level2name = N'is_pyxis';
go
