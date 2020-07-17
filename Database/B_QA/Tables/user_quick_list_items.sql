create table [dbo].[user_quick_list_items]
    (
      [id]                           [int] identity(1, 1) not null
    , [site_id]                      [int] not null
    , [user_id]                      [int] not null
    , [ndc]                          [varchar](32) null
    , [drug_id]                      [varchar](32) null
    , [brand_name]                   [nvarchar](255) not null
    , [dose]                         [decimal](11, 2) null
    , [dose_unit]                    [varchar](20) null
    , [medication_route_id]          [int] null
    , [frequency_id]                 [int] null
    , [order_notes]                  [nvarchar](max) null
    , [usages_this_week]             [int] null
    , [weekly_usage_rolling_average] [decimal](9, 3) null
    , constraint [pk__user_quick_list_items__id] primary key clustered([id] asc));
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

alter table [dbo].[user_quick_list_items]
add constraint [fk__user_quick_list_items__user] foreign key([site_id]) references [dbo].[users]([id]);
go

alter table [dbo].[user_quick_list_items]
add constraint [fk__user_quick_list_items__sites] foreign key([site_id]) references [dbo].[sites]([id]);
go

alter table [dbo].[user_quick_list_items]
add constraint [fk__user_quick_list_items__medication_routes] foreign key([medication_route_id]) references [dbo].[medication_routes]([id]);
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
  , @level1name = N'user_quick_list_items'
  , @level2type = N'CONSTRAINT'
  , @level2name = N'pk__user_quick_list_items__id';
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
  , @level1name = N'user_quick_list_items';
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
  , @level1name = N'user_quick_list_items'
  , @level2type = N'COLUMN'
  , @level2name = N'id';
go

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'Hospital identifier foriegn key to site table'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'user_quick_list_items'
  , @level2type = N'COLUMN'
  , @level2name = N'site_id';
go

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'Person Idendifier that owns this medication list record (Foreign Key to users table)'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'user_quick_list_items'
  , @level2type = N'COLUMN'
  , @level2name = N'user_id';
go

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'Drug NDC (National Drug Code)'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'user_quick_list_items'
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
  , @level1name = N'user_quick_list_items'
  , @level2type = N'COLUMN'
  , @level2name = N'drug_id';
go

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'Brand name'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'user_quick_list_items'
  , @level2type = N'COLUMN'
  , @level2name = N'brand_name';
go

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'Route of administration'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'user_quick_list_items'
  , @level2type = N'COLUMN'
  , @level2name = N'medication_route_id';
go

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'Medication Dose: numeric portion of dose/dose_unit pair'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'user_quick_list_items'
  , @level2type = N'COLUMN'
  , @level2name = N'dose';
go

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'Medication Unit: unit portion of dose/dose_unit pair'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'user_quick_list_items'
  , @level2type = N'COLUMN'
  , @level2name = N'dose_unit';
go

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'Foreign Key to Frequencies table'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'user_quick_list_items'
  , @level2type = N'COLUMN'
  , @level2name = N'frequency_id';
go

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'order_notes'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'user_quick_list_items'
  , @level2type = N'COLUMN'
  , @level2name = N'order_notes';
go

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'Number of times this quick list items has been used this week'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'user_quick_list_items'
  , @level2type = N'COLUMN'
  , @level2name = N'usages_this_week';
go

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'Rolling Avreage of weekly usage used to set a most used priority sort order'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'user_quick_list_items'
  , @level2type = N'COLUMN'
  , @level2name = N'weekly_usage_rolling_average';
go