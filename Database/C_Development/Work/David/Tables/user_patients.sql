create table [dbo].[user_patients]
    (
      [user_id]    [int] not null
    , [patient_id] BIGINT not null
    , [role_name]  [varchar](25) not null
                                 constraint [pk_user_patients] primary key clustered ([user_id] asc, [patient_id] asc, [role_name] asc));

go

/********
 Defaults
********/
/*****************
 Unique constraint
*****************/

alter table [dbo].[user_patients]
add constraint [uc__user_patients__patient_id__role_name] unique([patient_id], [role_name]);
go

/*******
 Indexes
*******/
/***********
 Foreign Key
***********/

alter table [dbo].[user_patients]
add constraint [fk__user_patients__users] foreign key([user_id]) references [dbo].[users]([id]);
go

alter table [dbo].[user_patients]
add constraint [fk__user_patients__patients] foreign key([patient_id]) references [dbo].[patients]([id]);
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

exec [sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'Table to hold User Patients'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'user_patients'
  , @level2type = null
  , @level2name = null;

/***************
 Data Dictionary
    Columns
***************/

go

exec [sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'User identifier, Foreign Key to users table'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'user_patients'
  , @level2type = N'COLUMN'
  , @level2name = N'user_id';
go

exec [sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'Patient identifier, Foreign Key to patients table'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'user_patients'
  , @level2type = N'COLUMN'
  , @level2name = N'patient_id';
go

exec [sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'Role Name for User Patient Assignment:
    DOCTOR1 = [pat].[firstdoctor]
    DOCTOR2 = [pat].[doctor]
    DOCTOR3 = [pat].[resident]
    DOCTOR4 = [pat].[drextender]
    NURSE1  = [pat].[primarynurse]
    NURSE2  = [pat].[extender]
'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'user_patients'
  , @level2type = N'COLUMN'
  , @level2name = N'role_name';
go