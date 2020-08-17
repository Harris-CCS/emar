create table [dbo].[medication_interactions]
    (
      [id]                       [bigint] identity(1, 1) not null
    , [interaction_drug_1]                   [varchar](50) not null
    , [interaction_drug_2]                   [varchar](50) not null
    , [severity]                 [tinyint] not null
    , [override_reason_id]       [int] null
    , [override_reason_user_id]  [int] null
    , [override_reason_datetime] [datetimeoffset](7) null
    , constraint [pk__medication_interactions__id] primary key clustered([id] asc));
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

alter table [dbo].[medication_interactions]
add constraint [fk__medication_interactions__override_reasons] foreign key([override_reason_id]) references [dbo].[override_reasons]([id]);
go

alter table [dbo].[medication_interactions]
add constraint [fk__medication_interactions__users] foreign key([override_reason_user_id]) references [dbo].[users]([id]);
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
  , @level1name = N'medication_interactions'
  , @level2type = N'CONSTRAINT'
  , @level2name = N'pk__medication_interactions__id';
go

/***************
 Data Dictionary
    Table
***************/

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'This table contains: patient orders'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'medication_interactions';
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
  , @level1name = N'medication_interactions'
  , @level2type = N'COLUMN'
  , @level2name = N'id';
go

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'interaction drug pair item 1'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'medication_interactions'
  , @level2type = N'COLUMN'
  , @level2name = N'interaction_drug_1';
go

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'interaction drug pair item 2'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'medication_interactions'
  , @level2type = N'COLUMN'
  , @level2name = N'interaction_drug_2';
go

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'reaction severity'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'medication_interactions'
  , @level2type = N'COLUMN'
  , @level2name = N'severity';
go

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'Foreign Key to override_reasons table'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'medication_interactions'
  , @level2type = N'COLUMN'
  , @level2name = N'override_reason_id';
go

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'Foreign Key to users table'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'medication_interactions'
  , @level2type = N'COLUMN'
  , @level2name = N'override_reason_user_id';
go

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'override reason datetime'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'medication_interactions'
  , @level2type = N'COLUMN'
  , @level2name = N'override_reason_datetime';
go