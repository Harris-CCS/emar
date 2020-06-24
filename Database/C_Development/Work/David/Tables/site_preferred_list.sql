create table [dbo].[site_preferred_list]
    (
      [id]           [int] identity(1, 1) not null
    , [site_id]      [int] not null
    , [is_active]    [bit] not null
    , [ndc]          [varchar](32) not null
    , [brand_name]   [varchar](255) not null
    , [drug_id]      [varchar](10) not null
    , [route]        [varchar](20) null
    , [dose]         [varchar](40) null
    , [unit]         [varchar](40) null
    , [frequency_id] [int] null
    , constraint [pk__site_preferred_list__id] primary key clustered([id] asc));
go

/********
 Defaults
********/
/*****************
 Unique constraint
*****************/

execute [sys].[sp_addextendedproperty]
    @name = N'MS_Description'
  , @value = N'Primary Key Column'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'site_preferred_list'
  , @level2type = N'CONSTRAINT'
  , @level2name = N'pk__site_preferred_list__id';
go

/*******
 Indexes
*******/
/***********
 Foreign Key
***********/

alter table [dbo].[site_preferred_list]
add constraint [fk__site_preferred_list__sites] foreign key([site_id]) references [dbo].[sites]([id]);
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

execute [sys].[sp_addextendedproperty]
    @name = N'MS_Description'
  , @value = N'This table contains medications preferred list for the department preferred list tab'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'site_preferred_list';
go

/***************
 Data Dictionary
    Columns
***************/

execute [sys].[sp_addextendedproperty]
    @name = N'MS_Description'
  , @value = N'Identifier auto number'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'site_preferred_list'
  , @level2type = N'COLUMN'
  , @level2name = N'id';
go

execute [sys].[sp_addextendedproperty]
    @name = N'MS_Description'
  , @value = N'Hospital identifier 1...255 for multi-site servers, FKEY to ORG site table'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'site_preferred_list'
  , @level2type = N'COLUMN'
  , @level2name = N'site_id';
go

execute [sys].[sp_addextendedproperty]
    @name = N'MS_Description'
  , @value = N'is_active 1=true 0=false'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'site_preferred_list'
  , @level2type = N'COLUMN'
  , @level2name = N'is_active';
go

execute [sys].[sp_addextendedproperty]
    @name = N'MS_Description'
  , @value = N'NDC Code'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'site_preferred_list'
  , @level2type = N'COLUMN'
  , @level2name = N'ndc';
go

execute [sys].[sp_addextendedproperty]
    @name = N'MS_Description'
  , @value = N'Medication Name'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'site_preferred_list'
  , @level2type = N'COLUMN'
  , @level2name = N'brand_name';
go

execute [sys].[sp_addextendedproperty]
    @name = N'MS_Description'
  , @value = N'Drug ID: example fdb: GCN_SEQNO: Clinical Formulation ID (Stable ID)'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'site_preferred_list'
  , @level2type = N'COLUMN'
  , @level2name = N'drug_id';
go

execute [sys].[sp_addextendedproperty]
    @name = N'MS_Description'
  , @value = N'Storage for route with medication'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'site_preferred_list'
  , @level2type = N'COLUMN'
  , @level2name = N'route';
go

execute [sys].[sp_addextendedproperty]
    @name = N'MS_Description'
  , @value = N'Storage for dose with medication'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'site_preferred_list'
  , @level2type = N'COLUMN'
  , @level2name = N'dose';
go

execute [sys].[sp_addextendedproperty]
    @name = N'MS_Description'
  , @value = N'Storage for unit with medication'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'site_preferred_list'
  , @level2type = N'COLUMN'
  , @level2name = N'unit';
go

execute [sys].[sp_addextendedproperty]
    @name = N'MS_Description'
  , @value = N'Foreign Key to Frequencies table'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'site_preferred_list'
  , @level2type = N'COLUMN'
  , @level2name = N'frequency_id';
go