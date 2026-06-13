create table [dbo].[patient_allergies]
    (
      [id]                 [bigint] identity(1, 1) not null
    , [patient_id]         [bigint] not null
    , [class]              [varchar](32) null
    , [category]           [varchar](32) null
    , [internal_drug_id]   [varchar](32) null
    , [name]               [nvarchar](255) null
    , [alternate_name]     [nvarchar](255) null
    , [allergy_drug_id]    [varchar](32) null
    , [is_active]          [bit] not null
    , [comment]            [varchar](255) null
    , [schedule]           [varchar](40) null
    , [reaction]           [varchar](80) null
    , [severity]           [varchar](80) null
    , [source]             [varchar](80) null
    , [parent_drug_id]     [varchar](32) null
    , [parent_drug_name]   [nvarchar](255) null
    , [add_user_id]        [int] not null
    , [add_datetime]       [datetimeoffset](7) null
    , [change_user_id]     [int] not null
    , [change_datetime]    [datetimeoffset](7) null
    , [action_status]      [char](1) null
    , [information_source] [varchar](25) null
    , [person_number]      [varchar](25) null
    , [account_number]     [varchar](25) null
    , [medication_id]      [int] null
    , [match]              [nvarchar](255) null
    , [internal_key]       [varchar](500) null
    constraint [pk__patient_allergies__id] primary key clustered([id] asc));
go

/********
 Defaults
********/
/*******
 Indexes
*******/
create nonclustered index [IDX_patient_allergies__patient_id] on dbo.patient_allergies([patient_id]) on [PRIMARY]
go
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
add constraint [fk__patient_allergies__medications] foreign key([medication_id]) references [dbo].[medications]([id]);
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
  , @value = N'Patient identifier, Foreign Key to patients table'
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

go

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
  , @value = N'Allergy source'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'patient_allergies'
  , @level2type = N'COLUMN'
  , @level2name = N'source';
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
  , @value = N'Person identifier that created this record (Foreign Key to users table)'
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
  , @value = N'Person identifier that last changed this record (Foreign Key to users table)'
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

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'Action taken status. V = Viewed, R = Rejected, C = Confirmed, U=Unconfirmed'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'patient_allergies'
  , @level2type = N'COLUMN'
  , @level2name = N'action_status';
go

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'Information Source - ''PC'' = PulseCheck, ''HIE'' = HIE/CCD, ''ADT'' = Interface'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'patient_allergies'
  , @level2type = N'COLUMN'
  , @level2name = N'information_source';
go

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'Primary Use: Master patient index HL7 2.4 super number of the patient from ADT interface'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'patient_allergies'
  , @level2type = N'COLUMN'
  , @level2name = N'person_number';
go

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'Primary Use: Account Number HL7 2.4 super number of the patient account from ADT interface'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'patient_allergies'
  , @level2type = N'COLUMN'
  , @level2name = N'account_number';
go

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'Medications identifier, Foreign Key to medications table'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'patient_allergies'
  , @level2type = N'COLUMN'
  , @level2name = N'medication_id';
go

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'Type of match'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'patient_allergies'
  , @level2type = N'COLUMN'
  , @level2name = N'match';
go

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'Key used and its value'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'patient_allergies'
  , @level2type = N'COLUMN'
  , @level2name = N'internal_key';
go