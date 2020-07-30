create table [dbo].[site_code_shares]
    (
      [id]             [int] identity(1, 1) not null
    , [source_site_id] [int] not null
    , [target_site_id] [int] not null
    , [entity]         [varchar](40) not null
    , constraint [pk__site_code_shares__id] primary key clustered([id] asc));
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

alter table [dbo].[site_code_shares]
add constraint [fk__site_code_shares__sites__source_site_id] foreign key([source_site_id]) references [dbo].[sites]([id]);
go

alter table [dbo].[site_code_shares]
add constraint [fk__site_code_shares__sites__target_site_id] foreign key([target_site_id]) references [dbo].[sites]([id]);
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
  , @level1name = N'site_code_shares'
  , @level2type = N'CONSTRAINT'
  , @level2name = N'pk__site_code_shares__id';
go

/***************
 Data Dictionary
    Table
***************/

execute [sys].[sp_addextendedproperty]
    @name = N'MS_Description'
  , @value = N'This table contains a list of site using code share'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'site_code_shares';
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
  , @level1name = N'site_code_shares'
  , @level2type = N'COLUMN'
  , @level2name = N'id';
go

execute [sys].[sp_addextendedproperty]
    @name = N'MS_Description'
  , @value = N'Hospital identifier foriegn key to site table'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'site_code_shares'
  , @level2type = N'COLUMN'
  , @level2name = N'source_site_id';
go

execute [sys].[sp_addextendedproperty]
    @name = N'MS_Description'
  , @value = N'Site to use for the code share, Hospital identifier foriegn key to site table'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'site_code_shares'
  , @level2type = N'COLUMN'
  , @level2name = N'target_site_id';
go

execute [sys].[sp_addextendedproperty]
    @name = N'MS_Description'
  , @value = N'Free Text Name of the code set being shared'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'site_code_shares'
  , @level2type = N'COLUMN'
  , @level2name = N'entity';
go