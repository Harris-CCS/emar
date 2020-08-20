create table [dbo].[preferred_medication_doses]
    (
      [drug_id]               [varchar](32) not null
    , [dose]               [decimal](11, 2) null
    , [medication_unit_id] [int] null
    , [site_id]            [int] null);
go

/********
 Defaults
********/
/*****************
 Unique constraint
*****************/

alter table [dbo].[preferred_medication_doses]
add constraint [uc__preferred_medication_doses] unique clustered([medication_unit_id] asc, [dose] asc, [site_id] asc, [drug_id] asc);
go

/*******
 Indexes
*******/
/***********
 Foreign Key
***********/
alter table [dbo].[preferred_medication_doses]
add constraint [fk__preferred_medication_doses__sites] foreign key([site_id]) references [dbo].[sites]([id]);
go

alter table [dbo].[preferred_medication_doses]
add constraint [fk__preferred_medication_doses__medication_units] foreign key([medication_unit_id]) references [dbo].[medication_units]([id]);
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
  , @value = N'Constraint to enforce preferred medication doses uniqueness'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'sites'
  , @level2type = N'CONSTRAINT'
  , @level2name = N'uc__preferred_medication_doses';
go

/***************
 Data Dictionary
    Table
***************/

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'This table contains a preferred list of medication doses for a specific drug id'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'preferred_medication_doses';
go

/***************
 Data Dictionary
    Columns
***************/

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'External Vendor Drug Database Identifier
  FDB: MEDID (MED Medication ID (Stable ID))'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'preferred_medication_doses'
  , @level2type = N'COLUMN'
  , @level2name = N'drug_id';
go

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'Medication Dose: numeric portion of dose/medication_unit_id pair'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'preferred_medication_doses'
  , @level2type = N'COLUMN'
  , @level2name = N'dose';
go

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'Medication Unit: unit portion of dose/medication_unit_id pair'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'preferred_medication_doses'
  , @level2type = N'COLUMN'
  , @level2name = N'medication_unit_id';
go

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'Hospital identifier foriegn key to site table'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'preferred_medication_doses'
  , @level2type = N'COLUMN'
  , @level2name = N'site_id';
go