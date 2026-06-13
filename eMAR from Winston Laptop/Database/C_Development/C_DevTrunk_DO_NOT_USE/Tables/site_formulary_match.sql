create table [dbo].[site_formulary_match]
    (
      [id]               [bigint] identity(1, 1) not null
    , [site_id]          [int] not null
    , [inpatient_match]  [tinyint] not null
    , [outpatient_match] [tinyint] not null
    , [pyxis_match]      [tinyint] not null
    , [medication_id]    [int] not null
    , constraint [pk__site_formulary_match__id] primary key clustered([id] asc));
go

/********
 Defaults
********/
/*****************
 Unique constraint
*****************/
/*******
 Indexes
*******/
/***********
 Foreign Key
***********/

alter table [dbo].[site_formulary_match]
add constraint [fk__site_formulary_match__sites] foreign key([site_id]) references [dbo].[sites]([id]);
go

alter table [dbo].[site_formulary_match]
add constraint [fk__site_formulary_match__medications] foreign key([medication_id]) references [dbo].[medications]([id]);
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
  , @level1name = N'site_formulary_match'
  , @level2type = N'CONSTRAINT'
  , @level2name = N'pk__site_formulary_match__id';
go

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
  , @level1name = N'site_formulary_match';
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
  , @level1name = N'site_formulary_match'
  , @level2type = N'COLUMN'
  , @level2name = N'id';
go

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'Hospital identifier foreign key to site table'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'site_formulary_match'
  , @level2type = N'COLUMN'
  , @level2name = N'site_id';
go

go

go

go

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'Flag indicating matching criteria
    0 = Non match ,
    1 = Partial match,
    2 = Equivalent match,
    3 = Exact match,
    4 = Exact ndc match
  '
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'site_formulary_match'
  , @level2type = N'COLUMN'
  , @level2name = N'inpatient_match';
go

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'Flag indicating matching criteria
    0 = Non match ,
    1 = Partial match,
    2 = Equivalent match,
    3 = Exact match,
    4 = Exact ndc match
  '
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'site_formulary_match'
  , @level2type = N'COLUMN'
  , @level2name = N'outpatient_match';
go

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'Flag indicating matching criteria
    0 = Non match ,
    1 = Partial match,
    2 = Equivalent match,
    3 = Exact match,
    4 = Exact ndc match
  '
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'site_formulary_match'
  , @level2type = N'COLUMN'
  , @level2name = N'pyxis_match';
go

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'Medications identifier, Foreign Key to medications table'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'site_formulary_match'
  , @level2type = N'COLUMN'
  , @level2name = N'medication_id';
go