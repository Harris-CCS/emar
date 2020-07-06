create table [dbo].[user_quick_list]
    (
      [id]           [int] identity(1, 1) not null
    , [site_id]      [int] not null
    , [user_id]      [int] not null
    , [is_active]    [bit] not null
    , [ndc]          [varchar](32) null
    , [drug_id]      [varchar](32) not null
    , [brand_name]   [varchar](255) not null
    , [route]        [varchar](20) null
    , [dose]         [varchar](40) null
    , [unit]         [varchar](40) null
    , [frequency_id] [int] null
    , constraint [pk__user_quick_list__id] primary key clustered([id] asc));
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

alter table [dbo].[user_quick_list]
add constraint [fk__user_quick_list__user] foreign key([site_id]) references [dbo].[users]([id]);
go

alter table [dbo].[user_quick_list]
add constraint [fk__user_quick_list__sites] foreign key([site_id]) references [dbo].[sites]([id]);
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
  , @level1name = N'user_quick_list'
  , @level2type = N'CONSTRAINT'
  , @level2name = N'pk__user_quick_list__id';
go

/***************
 Data Dictionary
    Table
***************/

execute [sys].[sp_addextendedproperty]
    @name = N'MS_Description'
  , @value = N'This table contains medications preferred list for the department preferred list tab'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'user_quick_list';
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
  , @level1name = N'user_quick_list'
  , @level2type = N'COLUMN'
  , @level2name = N'id';
go

execute [sys].[sp_addextendedproperty]
    @name = N'MS_Description'
  , @value = N'Hospital identifier foriegn key to site table'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'user_quick_list'
  , @level2type = N'COLUMN'
  , @level2name = N'site_id';
go

execute [sys].[sp_addextendedproperty]
    @name = N'MS_Description'
  , @value = N'Person identifier, FKEY to users table'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'user_quick_list'
  , @level2type = N'COLUMN'
  , @level2name = N'user_id';
go

execute [sys].[sp_addextendedproperty]
    @name = N'MS_Description'
  , @value = N'is_active 1=true 0=false'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'user_quick_list'
  , @level2type = N'COLUMN'
  , @level2name = N'is_active';
go

execute [sys].[sp_addextendedproperty]
    @name = N'MS_Description'
  , @value = N'Drug NDC (National Drug Code)'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'user_quick_list'
  , @level2type = N'COLUMN'
  , @level2name = N'ndc';
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
  , @level1name = N'user_quick_list'
  , @level2type = N'COLUMN'
  , @level2name = N'drug_id';
go

execute [sys].[sp_addextendedproperty]
    @name = N'MS_Description'
  , @value = N'Brand name'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'user_quick_list'
  , @level2type = N'COLUMN'
  , @level2name = N'brand_name';
go

execute [sys].[sp_addextendedproperty]
    @name = N'MS_Description'
  , @value = N'Route of administration'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'user_quick_list'
  , @level2type = N'COLUMN'
  , @level2name = N'route';
go

execute [sys].[sp_addextendedproperty]
    @name = N'MS_Description'
  , @value = N'Storage for dose with medication'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'user_quick_list'
  , @level2type = N'COLUMN'
  , @level2name = N'dose';
go

execute [sys].[sp_addextendedproperty]
    @name = N'MS_Description'
  , @value = N'Unit the drug is to be dispensed, for example capsule (cap) '
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'user_quick_list'
  , @level2type = N'COLUMN'
  , @level2name = N'unit';
go

execute [sys].[sp_addextendedproperty]
    @name = N'MS_Description'
  , @value = N'Foreign Key to Frequencies table'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'user_quick_list'
  , @level2type = N'COLUMN'
  , @level2name = N'frequency_id';
go
