create table [dbo].[override_reasons]
    (
      [id]            [int] identity(1, 1) not null
    , [site_id]       [int] not null
    , [is_medication] [bit] not null
    , [description]   [varchar](80) not null
    , constraint [pk__override_reasons__id] primary key clustered([id] asc));
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

alter table [dbo].[override_reasons]
add constraint [fk__override_reasons__sites] foreign key([site_id]) references [dbo].[sites]([id]);
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
  , @level1name = N'override_reasons'
  , @level2type = N'CONSTRAINT'
  , @level2name = N'pk__override_reasons__id';
go

/***************
 Data Dictionary
    Table
***************/

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'This table contains a list of override reasons'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'override_reasons';
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
  , @level1name = N'override_reasons'
  , @level2type = N'COLUMN'
  , @level2name = N'id';
go

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'Hospital identifier foreign key to site table'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'override_reasons'
  , @level2type = N'COLUMN'
  , @level2name = N'site_id';
go

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'is_medication 1=true 0=false (allergy)'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'override_reasons'
  , @level2type = N'COLUMN'
  , @level2name = N'is_medication';
go

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'description'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'override_reasons'
  , @level2type = N'COLUMN'
  , @level2name = N'description';
go