create table [dbo].[medication_details]
    (
      [id]                      [int] identity(1, 1) not null
    , [medication_id]           [int] not null
    , [ndc]                     [varchar](32) null
    , [drug_id]                 [varchar](32) null
    , [brand_name]              [nvarchar](255) null
    , [dose]                    [decimal](11, 2) null
    , [medication_unit_id]      [int] null
    , [medication_route_id]     [int] null
    , constraint [pk__medication_details__id] primary key clustered([id] asc));
go

/********
 Defaults
********/
/*******
 Indexes
*******/
/***********
 Foreign Key
***********/

alter table [dbo].[medication_details]
add constraint [fk__medication_details__medications] foreign key([medication_id]) references [dbo].[medications]([id]);
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
  , @level1name = N'medication_details'
  , @level2type = N'CONSTRAINT'
  , @level2name = N'pk__medication_details__id';
go

/***************
 Data Dictionary
    Table
***************/

execute [sys].[sp_addextendedproperty]
    @name = N'MS_Description'
  , @value = N'This table contains: details ifnormation for the medications'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'medication_details';
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
  , @level1name = N'medication_details'
  , @level2type = N'COLUMN'
  , @level2name = N'id';
go

execute [sys].[sp_addextendedproperty]
    @name = N'MS_Description'
  , @value = N'Medications identifier, Foreign Key to medications table'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'medication_details'
  , @level2type = N'COLUMN'
  , @level2name = N'medication_id';
go

execute [sys].[sp_addextendedproperty]
    @name = N'MS_Description'
  , @value = N'National Drug Code that identifies the brand, formulation and packaging of a drug'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'medication_details'
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
  , @level1name = N'medication_details'
  , @level2type = N'COLUMN'
  , @level2name = N'drug_id';
go

execute [sys].[sp_addextendedproperty]
    @name = N'MS_Description'
  , @value = N'brand_name'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'medication_details'
  , @level2type = N'COLUMN'
  , @level2name = N'brand_name';
go

execute [sys].[sp_addextendedproperty]
    @name = N'MS_Description'
  , @value = N'Medication Dose: numeric portion of dose/medication_unit_id pair'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'medication_details'
  , @level2type = N'COLUMN'
  , @level2name = N'dose';
go

execute [sys].[sp_addextendedproperty]
    @name = N'MS_Description'
  , @value = N'Medication Unit: unit portion of dose/medication_unit_id pair'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'medication_details'
  , @level2type = N'COLUMN'
  , @level2name = N'medication_unit_id';
go

execute [sys].[sp_addextendedproperty]
    @name = N'MS_Description'
  , @value = N'Route of administration; Foreign Key to medication_routes table'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'medication_details'
  , @level2type = N'COLUMN'
  , @level2name = N'medication_route_id';
go
