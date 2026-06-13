create table [dbo].[patient_home_medications]
    (
      [id]                  [bigint] identity(1, 1) not null
    , [patient_id]          [bigint] not null
    , [class]               [varchar](32) null
    , [category]            [varchar](32) null
    , [internal_drug_id]    [varchar](32) null
    , [name]                [nvarchar](255) null
    , [alternate_name]      [nvarchar](255) null
    , [dose]                [decimal](12, 3) null
    , [medication_unit_id]  [int] null
    , [medication_route_id] [int] null
    , [medication_drug_id]  [varchar](32) null
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
    , [action_status]       [char](1) null
    , [medication_id]       [int] null
    , [last_taken_note]     [nvarchar](100) null
    , [match]              [nvarchar](255) null
    , [internal_key]       [varchar](500) null
    , constraint [pk__patient_home_medications__id] primary key clustered([id] asc));
go

/********
 Defaults
********/
/*******
 Indexes
*******/
create nonclustered index [IDX_patient_home_medications__patient_id] on dbo.patient_home_medications([patient_id]) on [PRIMARY]
go
/***********
 Foreign Key
***********/

alter table [dbo].[patient_home_medications]
add constraint [fk__patients__patient_home_medications] foreign key([patient_id]) references [dbo].[patients]([id]);
go

alter table [dbo].[patient_home_medications]
add constraint [fk__users__patient_home_medications__add_user_id] foreign key([add_user_id]) references [dbo].[users]([id]);
go

alter table [dbo].[patient_home_medications]
add constraint [fk__users__patient_home_medications__change_user_id] foreign key([change_user_id]) references [dbo].[users]([id]);
go

alter table [dbo].[patient_home_medications]
add constraint [fk__patient_home_medications__medication_units] foreign key([medication_unit_id]) references [dbo].[medication_units]([id]);
go

alter table [dbo].[patient_home_medications]
add constraint [fk__patient_home_medications__medication_routes] foreign key([medication_route_id]) references [dbo].[medication_routes]([id]);
go

alter table [dbo].[patient_home_medications]
add constraint [fk__patient_home_medications__medications] foreign key([medication_id]) references [dbo].[medications]([id]);
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
  , @level1name = N'patient_home_medications'
  , @level2type = N'CONSTRAINT'
  , @level2name = N'pk__patient_home_medications__id';
go

/***************
 Data Dictionary
    Table
***************/

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'Table stores patient home medications'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'patient_home_medications';
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
  , @level1name = N'patient_home_medications'
  , @level2type = N'COLUMN'
  , @level2name = N'id';
go

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'Patient identifier, Foreign Key to patients table'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'patient_home_medications'
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
  , @level1name = N'patient_home_medications'
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
  , @level1name = N'patient_home_medications'
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
  , @level1name = N'patient_home_medications'
  , @level2type = N'COLUMN'
  , @level2name = N'internal_drug_id';
go

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'Description of medication'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'patient_home_medications'
  , @level2type = N'COLUMN'
  , @level2name = N'name';
go

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'Alternate Name'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'patient_home_medications'
  , @level2type = N'COLUMN'
  , @level2name = N'alternate_name';
go

go

go

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'Medication drug ID proprietary to the installed drug database
    FDB: HICL, HIC
 Multum: dnum'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'patient_home_medications'
  , @level2type = N'COLUMN'
  , @level2name = N'medication_drug_id';
go

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'is_active 1=True 0=False'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'patient_home_medications'
  , @level2type = N'COLUMN'
  , @level2name = N'is_active';
go

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'Comment area for additional comments on medication'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'patient_home_medications'
  , @level2type = N'COLUMN'
  , @level2name = N'comment';
go

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'Medication Unit: unit portion of dose/medication_unit_id pair'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'patient_home_medications'
  , @level2type = N'COLUMN'
  , @level2name = N'medication_unit_id';
go

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'Route of administration; Foreign Key to medication_routes table'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'patient_home_medications'
  , @level2type = N'COLUMN'
  , @level2name = N'medication_route_id';
go

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'Medication schedule'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'patient_home_medications'
  , @level2type = N'COLUMN'
  , @level2name = N'schedule';
go

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'Medication Dose: numeric portion of dose/medication_unit_id pair'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'patient_home_medications'
  , @level2type = N'COLUMN'
  , @level2name = N'dose';
go

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'Medication reaction'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'patient_home_medications'
  , @level2type = N'COLUMN'
  , @level2name = N'reaction';
go

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'Medication severity'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'patient_home_medications'
  , @level2type = N'COLUMN'
  , @level2name = N'severity';
go

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'Parent drug id'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'patient_home_medications'
  , @level2type = N'COLUMN'
  , @level2name = N'parent_drug_id';
go

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'Parent drug name'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'patient_home_medications'
  , @level2type = N'COLUMN'
  , @level2name = N'parent_drug_name';
go

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'Person identifier that created this record (Foreign Key to users table)'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'patient_home_medications'
  , @level2type = N'COLUMN'
  , @level2name = N'add_user_id';
go

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'Date and time when record added YYYYMMDDHHMM'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'patient_home_medications'
  , @level2type = N'COLUMN'
  , @level2name = N'add_datetime';
go

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'Person identifier that last changed this record (Foreign Key to users table)'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'patient_home_medications'
  , @level2type = N'COLUMN'
  , @level2name = N'change_user_id';
go

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'Date and time when record changed YYYYMMDDHHMM'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'patient_home_medications'
  , @level2type = N'COLUMN'
  , @level2name = N'change_datetime';
go

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'Action taken status. V = Viewed, R = Rejected, C = Confirmed, U=Unconfirmed'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'patient_home_medications'
  , @level2type = N'COLUMN'
  , @level2name = N'action_status';
go

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'Medications identifier, Foreign Key to medications table'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'patient_home_medications'
  , @level2type = N'COLUMN'
  , @level2name = N'medication_id';
go

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'Free Text note column, for medication last taken'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'patient_home_medications'
  , @level2type = N'COLUMN'
  , @level2name = N'last_taken_note';
go

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'Type of match'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'patient_home_medications'
  , @level2type = N'COLUMN'
  , @level2name = N'match';
go

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'Key used and its value'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'patient_home_medications'
  , @level2type = N'COLUMN'
  , @level2name = N'internal_key';
go