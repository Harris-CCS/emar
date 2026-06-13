create table [dbo].[preferred_medication_doses]
    (
      [id]                    int identity(1, 1) not null
    , [medication_id]         [int] not null
    , [dose]               [decimal](11, 2) not null
    , [medication_unit_id] [int] not null
    , [site_id]            [int] not null);
go

/********
 Defaults
********/
/*****************
 Unique constraint
*****************/

alter table [dbo].[preferred_medication_doses]
add constraint [pk__preferred_medication_doses] primary key nonclustered([id] asc);
go

alter table [dbo].[preferred_medication_doses]
add constraint [uc__preferred_medication_doses] unique clustered([medication_id] asc, [site_id] asc, [medication_unit_id] asc, [dose] asc);
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

alter table [dbo].[preferred_medication_doses]
add constraint [fk__preferred_medication_doses__medications] foreign key([medication_id]) references [dbo].[medications]([id]);
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
  , @value = N'Primary Key to enforce preferred medication doses uniqueness'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'sites'
  , @level2type = N'CONSTRAINT'
  , @level2name = N'pk__preferred_medication_doses';
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
  , @value = N'Medications identifier, Foreign Key to medications table'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'preferred_medication_doses'
  , @level2type = N'COLUMN'
  , @level2name = 'medication_id';
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
  , @value = N'Hospital identifier foreign key to site table'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'preferred_medication_doses'
  , @level2type = N'COLUMN'
  , @level2name = N'site_id';
go