create table [dbo].[patient_indicators]
    (
      [id]               [int] identity(1, 1) not null
    , [patient_id]       [bigint] not null
    , [ordinal_position] [smallint] not null
    , [code]             [varchar](10) not null
    , [type]             [varchar](10) not null
    , [description]      [varchar](255) not null
    , [image_name]       [varchar](255) not null
    , constraint [pk__patient_indicators__id] primary key clustered([id] asc));
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
  , @level1name = N'patient_indicators'
  , @level2type = N'CONSTRAINT'
  , @level2name = N'pk__patient_indicators__id';
go

/*******
 Indexes
*******/

create nonclustered index [ix__patient_indicators__patient_id_site_id] on [dbo].[patient_indicators]
    ([patient_id] asc);
go

/***********
 Foreign Key
***********/

alter table [dbo].[patient_indicators]
add constraint [fk__patient_indicators__patients] foreign key([patient_id]) references [dbo].[patients]([id]);
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
  , @value = N'Default Index applied during design'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'patient_indicators'
  , @level2type = N'INDEX'
  , @level2name = N'ix__patient_indicators__patient_id_site_id';
go

/***************
 Data Dictionary
    Table
***************/

execute [sys].[sp_addextendedproperty]
    @name = N'MS_Description'
  , @value = N'This table contains indicators (icons) for informational display on the page header'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'patient_indicators';
go

/***************
 Data Dictionary
    Columns
***************/

execute [sys].[sp_addextendedproperty]
    @name = N'MS_Description'
  , @value = N'patient indicator auto number id'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'patient_indicators'
  , @level2type = N'COLUMN'
  , @level2name = N'id';
go

execute [sys].[sp_addextendedproperty]
    @name = N'MS_Description'
  , @value = N'Patient ID foreign key to patients table'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'patient_indicators'
  , @level2type = N'COLUMN'
  , @level2name = N'patient_id';
go

execute [sys].[sp_addextendedproperty]
    @name = N'MS_Description'
  , @value = N'Ordinal Position for Image Display'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'patient_indicators'
  , @level2type = N'COLUMN'
  , @level2name = 'ordinal_position';
go

execute [sys].[sp_addextendedproperty]
    @name = N'MS_Description'
  , @value = N'Custom image code'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'patient_indicators'
  , @level2type = N'COLUMN'
  , @level2name = N'code';
go

execute [sys].[sp_addextendedproperty]
    @name = N'MS_Description'
  , @value = N'Custom image type'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'patient_indicators'
  , @level2type = N'COLUMN'
  , @level2name = N'type';
go

execute [sys].[sp_addextendedproperty]
    @name = N'MS_Description'
  , @value = N'Custom image description'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'patient_indicators'
  , @level2type = N'COLUMN'
  , @level2name = N'description';
go

execute [sys].[sp_addextendedproperty]
    @name = N'MS_Description'
  , @value = N'Image File Name for display'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'patient_indicators'
  , @level2type = N'COLUMN'
  , @level2name = N'image_name';
go