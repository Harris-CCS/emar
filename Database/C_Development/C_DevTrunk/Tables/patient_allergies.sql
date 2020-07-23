create table [dbo].[patient_allergies]
    (
      [id]                  [bigint] identity(1, 1) not null
    , [patient_id]          [bigint] null
    , [class]               [varchar](32) null
    , [category]            [varchar](32) null
    , [internal_drug_id]    [varchar](32) null
    , [ndc]                 [varchar](32) null
    , [drug_id]             [varchar](32) null
    , [name]                [nvarchar](255) null
    , [alternate_name]      [nvarchar](255) null
    , [dose]                [decimal](11, 2) null
    , [dose_unit]           [varchar](20) null
    , [medication_route_id] [int] null
    , [allergy_drug_id]     [varchar](32) null
    , [is_active]           [bit] not null
    , [comment]             [varchar](255) null
    , [schedule]            [varchar](40) null
    , [reaction]            [varchar](80) null
    , [severity]            [varchar](80) null
    , [parent_drug_id]      [varchar](32) null
    , [parent_drug_name]    [nvarchar](255) null
    , [add_user_id]         [int] not null
    , [add_datetime]        [datetimeoffset](7) null
    , [change_user_id]      [int] not null
    , [change_datetime]     [datetimeoffset](7) null
    , constraint [pk__patient_allergies__id] primary key clustered([id] asc));
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

alter table [dbo].[patient_allergies]
add constraint [fk__patients__patient_allergies] foreign key([patient_id]) references [dbo].[patients]([id]);
go

alter table [dbo].[patient_allergies]
add constraint [fk__users__patient_allergies__add_user_id] foreign key([add_user_id]) references [dbo].[users]([id]);
go

alter table [dbo].[patient_allergies]
add constraint [fk__users__patient_allergies__change_user_id] foreign key([change_user_id]) references [dbo].[users]([id]);
go

alter table [dbo].[patient_allergies]
add constraint [fk__patient_allergies__medication_routes] foreign key([medication_route_id]) references [dbo].[medication_routes]([id]);
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
  , @value = N'Primary Key Column'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'patient_allergies'
  , @level2type = N'CONSTRAINT'
  , @level2name = N'pk__patient_allergies__id';
go

/***************
 Data Dictionary
    Table
***************/

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'Table stores patient allergies'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'patient_allergies';
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
  , @level1name = N'patient_allergies'
  , @level2type = N'COLUMN'
  , @level2name = N'id';
go

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'Patient identifier, FKEY to patients table'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'patient_allergies'
  , @level2type = N'COLUMN'
  , @level2name = N'patient_id';
go

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'Drug class proprietary to the installed drug database
    FDB: Allergy cross-sensitivity
 Multum: Allergy class'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'patient_allergies'
  , @level2type = N'COLUMN'
  , @level2name = N'class';
go

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'Drug category proprietary to the installed drug database
    FDB: Allergy group
 Multum: Allergy category'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'patient_allergies'
  , @level2type = N'COLUMN'
  , @level2name = N'category';
go

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'Formulation ID proprietary to the installed drug database
    FDB: Routed Generic ID (RHICL), HICL, HIC
    Multum: dnum'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'patient_allergies'
  , @level2type = N'COLUMN'
  , @level2name = N'internal_drug_id';
go

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'Description of allergy'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'patient_allergies'
  , @level2type = N'COLUMN'
  , @level2name = N'name';
go

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'Alternate Name'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'patient_allergies'
  , @level2type = N'COLUMN'
  , @level2name = N'alternate_name';
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
  , @level1name = N'patient_allergies'
  , @level2type = N'COLUMN'
  , @level2name = N'drug_id';
go

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'Drug NDC (National Drug Code)'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'patient_allergies'
  , @level2type = N'COLUMN'
  , @level2name = N'ndc';
go

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'Allergy drug ID proprietary to the installed drug database
    FDB: HICL, HIC
 Multum: dnum'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'patient_allergies'
  , @level2type = N'COLUMN'
  , @level2name = N'allergy_drug_id';
go

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'is_active 1=True 0=False'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'patient_allergies'
  , @level2type = N'COLUMN'
  , @level2name = N'is_active';
go

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'Comment area for additional comments on allergy'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'patient_allergies'
  , @level2type = N'COLUMN'
  , @level2name = N'comment';
go

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'Medication Unit: unit portion of dose/dose_unit pair'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'patient_allergies'
  , @level2type = N'COLUMN'
  , @level2name = N'dose_unit';
go

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'Medication route'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'patient_allergies'
  , @level2type = N'COLUMN'
  , @level2name = N'medication_route_id';
go

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'Medication schedule'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'patient_allergies'
  , @level2type = N'COLUMN'
  , @level2name = N'schedule';
go

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'Medication Dose: numeric portion of dose/dose_unit pair'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'patient_allergies'
  , @level2type = N'COLUMN'
  , @level2name = N'dose';
go

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'Allergy reaction'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'patient_allergies'
  , @level2type = N'COLUMN'
  , @level2name = N'reaction';
go

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'Allergy severity'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'patient_allergies'
  , @level2type = N'COLUMN'
  , @level2name = N'severity';
go

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'Parent drug id'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'patient_allergies'
  , @level2type = N'COLUMN'
  , @level2name = N'parent_drug_id';
go

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'Parent drug name'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'patient_allergies'
  , @level2type = N'COLUMN'
  , @level2name = N'parent_drug_name';
go

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'Person Idendifier that created this record (Foreign Key to users table)'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'patient_allergies'
  , @level2type = N'COLUMN'
  , @level2name = N'add_user_id';
go

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'Date and time when record added YYYYMMDDHHMM'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'patient_allergies'
  , @level2type = N'COLUMN'
  , @level2name = N'add_datetime';
go

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'Person Idendifier that last changed this record (Foreign Key to users table)'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'patient_allergies'
  , @level2type = N'COLUMN'
  , @level2name = N'change_user_id';
go

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'Date and time when record changed YYYYMMDDHHMM'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'patient_allergies'
  , @level2type = N'COLUMN'
  , @level2name = N'change_datetime';
go