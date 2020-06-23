create table [dbo].[site_formularies]
(
    [id]                  bigint identity(1, 1) not null
  , [site_id]             int not null
  , [drug_id]             [varchar](10) not null
  , [formulation_drug_id] [varchar](32) null
  , [ndc]                 [varchar](32) null
  , [brand_name]          [varchar](255) not null
  , [site_drug_code]      [varchar](32) null
  , [service_code]        [varchar](32) null
  , [active_ingredient]   [varchar](255) null
  , [is_inpatient]        [bit] not null
  , [is_outpatient]       [bit] not null
  , [is_pyxis]            [bit] not null
  , constraint [pk__formlary__id] primary key clustered([id] asc)
);
go

/********
 Defaults
********/
/*****************
 Unique constraint
*****************/

execute [sys].[sp_addextendedproperty]
    @name = N'MS_Description'
  , @value = N'Primary Key Constraint'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'site_formularies'
  , @level2type = N'CONSTRAINT'
  , @level2name = N'pk__formlary__id';
go

/*******
 Indexes
*******/
/***********
 Foreign Key
***********/

alter table [dbo].[site_formularies]
add constraint [fk__site_formularies__sites] foreign key([site_id]) references [dbo].[sites]([id]);
go

/***************
 Data Dictionary
    Defaults
***************/
/***************
 Data Dictionary
    Indexes
***************/
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
  , @level1name = N'site_formularies';
go

/***************
 Data Dictionary
    Columns
***************/

execute [sys].[sp_addextendedproperty]
    @name = N'MS_Description'
  , @value = N'Identifier auto number'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'site_formularies'
  , @level2type = N'COLUMN'
  , @level2name = N'id';
go

exec [sys].[sp_addextendedproperty]
    @name = N'MS_Description'
  , @value = N'Hospital identifier 1...255 for multi-site servers, FKEY to ORG site table'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'site_formularies'
  , @level2type = N'COLUMN'
  , @level2name = N'site_id';
go

exec [sys].[sp_addextendedproperty]
    @name = N'MS_Description'
  , @value = N'Drug ID number'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'site_formularies'
  , @level2type = N'COLUMN'
  , @level2name = N'drug_id';
go

exec [sys].[sp_addextendedproperty]
    @name = N'MS_Description'
  , @value = N'Formulation Drug ID number'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'site_formularies'
  , @level2type = N'COLUMN'
  , @level2name = N'formulation_drug_id';
go

exec [sys].[sp_addextendedproperty]
    @name = N'MS_Description'
  , @value = N'National Drug Code'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'site_formularies'
  , @level2type = N'COLUMN'
  , @level2name = N'ndc';
go

exec [sys].[sp_addextendedproperty]
    @name = N'MS_Description'
  , @value = N'Brand name'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'site_formularies'
  , @level2type = N'COLUMN'
  , @level2name = N'brand_name';
go

exec [sys].[sp_addextendedproperty]
    @name = N'MS_Description'
  , @value = N'Site specific code for drug'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'site_formularies'
  , @level2type = N'COLUMN'
  , @level2name = N'site_drug_code';
go

exec [sys].[sp_addextendedproperty]
    @name = N'MS_Description'
  , @value = N'Site billing code for Procedure, supplies, and other reportable services '
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'site_formularies'
  , @level2type = N'COLUMN'
  , @level2name = N'service_code';
go

exec [sys].[sp_addextendedproperty]
    @name = N'MS_Description'
  , @value = N'Active ingredient(s)'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'site_formularies'
  , @level2type = N'COLUMN'
  , @level2name = N'active_ingredient';
go

exec [sys].[sp_addextendedproperty]
    @name = N'MS_Description'
  , @value = N'Flag indicating that formulary item is on In-patient formulary. 1=True 0=False'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'site_formularies'
  , @level2type = N'COLUMN'
  , @level2name = N'is_inpatient';
go

exec [sys].[sp_addextendedproperty]
    @name = N'MS_Description'
  , @value = N'Flag indicating that formulary item is on Out-patient formulary. 1=True 0=False'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'site_formularies'
  , @level2type = N'COLUMN'
  , @level2name = N'is_outpatient';
go

exec [sys].[sp_addextendedproperty]
    @name = N'MS_Description'
  , @value = N'Flag indicating that formulary item is on the medication dispensing machine. 1=True 0=False'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'site_formularies'
  , @level2type = N'COLUMN'
  , @level2name = N'is_pyxis';
go