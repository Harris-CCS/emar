create table [dbo].[antimicrobial_indications]
    (
      [id]               [int] identity(1, 1) not null
    , [site_id]          [int] not null
    , [code]             [varchar](20) not null
    , [description]      [nvarchar](255) not null
    , [is_active]        [bit] not null
    , [ordinal_position] [int] not null
    , constraint [pk__antimicrobial_indications__id] primary key clustered([id] asc));
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

create nonclustered index [ix__antimicrobial_indications__code_site_id] on [dbo].[antimicrobial_indications]
    ([code] asc, [site_id] asc) 
      include
    ([description]); 
go

create nonclustered index [ix__antimicrobial_indications__SiteStatusCode_Covered] on [dbo].[antimicrobial_indications]
    ([site_id] asc, [is_active] asc, [code] asc) 
      include
    ([id], [description], [ordinal_position]) with
    (pad_index = off, statistics_norecompute = off, sort_in_tempdb = off, drop_existing = off, online = off, allow_row_locks = on, allow_page_locks = on) on [PRIMARY];
go

create nonclustered index [ix__antimicrobial_indications__SiteStatusPositionDescription__Code] on [dbo].[antimicrobial_indications]
    ([site_id] asc, [is_active] asc, [ordinal_position] asc, [description] asc) 
      include
    ([code]) with
    (pad_index = off, statistics_norecompute = off, sort_in_tempdb = off, drop_existing = off, online = off, allow_row_locks = on, allow_page_locks = on) on [PRIMARY];
go

/***********
 Foreign Key
***********/

alter table [dbo].[antimicrobial_indications]
add constraint [fk__antimicrobial_indications__sites] foreign key([site_id]) references [dbo].[sites]([id]);
go

/***************
 Data Dictionary
    Defaults
***************/
/***************
 Data Dictionary
    Indexes
***************/

exec [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'Index for MED_INDICATION table'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'antimicrobial_indications'
  , @level2type = N'INDEX'
  , @level2name = N'ix__antimicrobial_indications__code_site_id';
go

exec [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'Index for MED_INDICATION table'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'antimicrobial_indications'
  , @level2type = N'INDEX'
  , @level2name = N'ix__antimicrobial_indications__SiteStatusCode_Covered';
go

exec [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'Index for MED_INDICATION table'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'antimicrobial_indications'
  , @level2type = N'INDEX'
  , @level2name = N'ix__antimicrobial_indications__SiteStatusPositionDescription__Code';
go

/***************
 Data Dictionary
    Table
***************/

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'ANTIMICROBIAL_INDICATIONS - Predefined indications for Medication
The table contains pre-defined indications for medications.'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'antimicrobial_indications';
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
  , @level1name = N'antimicrobial_indications'
  , @level2type = N'COLUMN'
  , @level2name = N'id';
go

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'Hospital identifier foriegn key to site table'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'antimicrobial_indications'
  , @level2type = N'COLUMN'
  , @level2name = N'site_id';
go

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'code'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'antimicrobial_indications'
  , @level2type = N'COLUMN'
  , @level2name = N'code';
go

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'description'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'antimicrobial_indications'
  , @level2type = N'COLUMN'
  , @level2name = N'description';
go

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'is_active 1=True 0=False'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'antimicrobial_indications'
  , @level2type = N'COLUMN'
  , @level2name = N'is_active';
go

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'Ordinal Position for Display'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'antimicrobial_indications'
  , @level2type = N'COLUMN'
  , @level2name = N'ordinal_position';
go