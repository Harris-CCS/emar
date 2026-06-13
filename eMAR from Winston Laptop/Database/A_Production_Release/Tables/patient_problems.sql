create table [dbo].[patient_problems]
    (
      [id]             [bigint] identity(1, 1) not null
    , [patient_id]     [bigint] not null
    , [code_set_name]  [varchar](25) null
    , [code_set_value] [varchar](25) null
    , [problem_name]   [varchar](255) NOT null
    , [diagnosis_type] [varchar](25) null
    , constraint [pk__patient_problems__id] primary key clustered([id] asc));
go

/********
 Defaults
********/
/*****************
 Unique constraint
*****************/

alter table [dbo].[patient_problems]
add constraint [uc__patient_problems__patient_id_code_set_name_code_set_value_problem_name_diagnosis_type] unique([patient_id], [code_set_name], [code_set_value], [problem_name], [diagnosis_type]);
go

/*******
 Indexes
*******/
/***********
 Foreign Key
***********/

alter table [dbo].[patient_problems]
add constraint [fk__users__patient_problems__patient_id] foreign key([patient_id]) references [dbo].[patients]([id]);
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
  , @level1name = N'patient_problems'
  , @level2type = N'CONSTRAINT'
  , @level2name = N'pk__patient_problems__id';
go

/***************
 Data Dictionary
    Table
***************/

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'This table contains: patient problems and diagnosis'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'patient_problems';
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
  , @level1name = N'patient_problems'
  , @level2type = N'COLUMN'
  , @level2name = N'id';
go

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'Patient identifier, Foreign Key to patients table'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'patient_problems'
  , @level2type = N'COLUMN'
  , @level2name = N'patient_id';
go

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'Name of Code Set (ICD-9,ICD-10,ICD-10-CA...)'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'patient_problems'
  , @level2type = N'COLUMN'
  , @level2name = N'code_set_name';
go

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'Code from defined "Code Set"'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'patient_problems'
  , @level2type = N'COLUMN'
  , @level2name = N'code_set_value';
go

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'When Code Set exists, this is the description from that code set; else it is treated as free form text'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'patient_problems'
  , @level2type = N'COLUMN'
  , @level2name = N'problem_name';
go

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'diagnosis_type Primary, Secondary, Admitting'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'patient_problems'
  , @level2type = N'COLUMN'
  , @level2name = N'diagnosis_type';
go
