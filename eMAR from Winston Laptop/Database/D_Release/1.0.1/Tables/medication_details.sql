create table [dbo].[medication_details]
    (
      [id]                  [int] identity(1, 1) not null
    , [medication_id]       [int] not null
    , [drug_id]             [varchar](32) not null
    , [brand_name]          [nvarchar](255) not null
    , [active_list]         [nvarchar](max) not null
    , [dose]                [decimal](11, 2) null
    , [medication_unit_id]  [int] null
    ,[is_active]           bit not null
                                constraint [pk__medication_details__id] primary key clustered ([id] asc));
go

/********
 Defaults
********/
/*******
 Indexes
*******/

create index [ix__medication_details__medication_id] on [dbo].[medication_details]
    ([medication_id] asc) 
      include
    ([drug_id]);

go

/***********
 Foreign Key
***********/

alter table [dbo].[medication_details]
add constraint [fk__medication_details__medications] foreign key([medication_id]) references [dbo].[medications]([id]);
go

alter table [medication_details]
add constraint [fk__medication_details__medication_units] foreign key([medication_unit_id]) references [medication_units]([id]);
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
  , @level1name = N'medication_details'
  , @level2type = N'CONSTRAINT'
  , @level2name = N'pk__medication_details__id';
go

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'Index to get the drug_id needed to join into the fdb tables'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'medication_details'
  , @level2type = N'INDEX'
  , @level2name = N'ix__medication_details__medication_id';
go

/***************
 Data Dictionary
    Table
***************/

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'This table contains: details ifnormation for the medications'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'medication_details';
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
  , @level1name = N'medication_details'
  , @level2type = N'COLUMN'
  , @level2name = N'id';
go

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'Medications identifier, Foreign Key to medications table'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'medication_details'
  , @level2type = N'COLUMN'
  , @level2name = N'medication_id';
go

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'External Vendor Drug Database Identifier
    FDB: MEDID (MED Medication ID (Stable ID))
    Multum: dnum
These 2 columns will be carried as a set drug_id,brand_name
drug_id and brand_name are vendor specific concepts.
'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'medication_details'
  , @level2type = N'COLUMN'
  , @level2name = N'drug_id';
go

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'brand_name'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'medication_details'
  , @level2type = N'COLUMN'
  , @level2name = N'brand_name';
go

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'Medication Dose: numeric portion of dose/medication_unit_id pair'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'medication_details'
  , @level2type = N'COLUMN'
  , @level2name = N'dose';
go

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'Medication Unit: unit portion of dose/medication_unit_id pair'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'medication_details'
  , @level2type = N'COLUMN'
  , @level2name = N'medication_unit_id';
go


go