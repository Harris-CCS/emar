create table [dbo].[antimicrobial_indication_items]
    (
      [id]           [int] identity(1, 1) not null
    , [site_id]      [int] not null
    , [sub_category] [int] not null
    , constraint [pk__antimicrobial_indication_items__id] primary key clustered([id] asc));
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

create nonclustered index [ix__antimicrobial_indication_items_site_id__covered] on [dbo].[antimicrobial_indication_items]
    ([site_id] asc) 
      include
    ([id], [sub_category]);
go

create unique nonclustered index [ix__antimicrobial_indication_items_SubcatSite_covered] on [dbo].[antimicrobial_indication_items]
    ([sub_category] asc, [site_id] asc) 
      include
    ([id]);
go

/***********
 Foreign Key
***********/

alter table [dbo].[antimicrobial_indication_items]
add constraint [fk__antimicrobial_indication_items__sites] foreign key([site_id]) references [dbo].[sites]([id]);
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
  , @value = N'Index for MED_INDICATION_LIST table'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'antimicrobial_indication_items'
  , @level2type = N'INDEX'
  , @level2name = N'ix__antimicrobial_indication_items_site_id__covered';
go

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'Unique index for MED_INDICATION_LIST table'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'antimicrobial_indication_items'
  , @level2type = N'INDEX'
  , @level2name = N'ix__antimicrobial_indication_items_SubcatSite_covered';
go

/***************
 Data Dictionary
    Table
***************/

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'MEDICATION_INDICATION_LIST - Medication Indication List 
This table contains the sub categories that are maintained for antimicrobial stewardship purposes.'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'antimicrobial_indication_items';
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
  , @level1name = N'antimicrobial_indication_items'
  , @level2type = N'COLUMN'
  , @level2name = N'id';
go

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'Hospital identifier foreign key to site table'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'antimicrobial_indication_items'
  , @level2type = N'COLUMN'
  , @level2name = N'site_id';
go

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'sub_cat'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'antimicrobial_indication_items'
  , @level2type = N'COLUMN'
  , @level2name = 'sub_category';
go