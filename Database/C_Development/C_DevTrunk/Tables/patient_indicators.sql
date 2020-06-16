create table [dbo].[patient_indicators]
(
    [id]          [int] identity(1, 1) not null
  , [site_id]     [int] not null
  , [patient_id]  [bigint] not null
  , [position]    [smallint] not null
  , [code]        [varchar](10) not null
  , [admreq]      [varchar](1) not null
  , [triaged]     [varchar](1) not null
  , [type]        [varchar](10) not null
  , [description] [varchar](255) not null
  , [image_name]  [varchar](255) not null
  , constraint [pk__patient_indicators__id] primary key clustered([id] asc)
);
go

/*********
 Defaults 
*********/

/*******
 Indexes
*******/

create nonclustered index [ix__patient_indicators__patient_id_site_id] on [dbo].[patient_indicators]
([patient_id] asc, [site_id] asc
);
go

/***********
 Foriegn Key
***********/

alter table [dbo].[patient_indicators]
with check
add constraint [fk__patient_indicators__patients] foreign key([patient_id]) references [dbo].[patients]([id]);
go

alter table [dbo].[patient_indicators] check constraint [fk__patient_indicators__patients];
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
  , @level1name = N'patient_indicators'
  , @level2type = N'CONSTRAINT'
  , @level2name = N'pk__patient_indicators__id';
go

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
  , @value = N'This table contains username, password, and other user attributes'
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
  , @value = N'Hospital identifier 1...255 for multi-site servers, FKEY to ORG site table'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'patient_indicators'
  , @level2type = N'COLUMN'
  , @level2name = N'site_id';
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
  , @level2name = N'position';
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
  , @value = N'Admission Request Entry. Y=Yes, N=No'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'patient_indicators'
  , @level2type = N'COLUMN'
  , @level2name = N'admreq';
go

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'Triaged. Y=Yes, N=No'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'patient_indicators'
  , @level2type = N'COLUMN'
  , @level2name = N'triaged';
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