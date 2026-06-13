create table [dbo].[print_history]
    (
      [id]                  [int] identity(1, 1) not null
    , [print_user_id]       [int] not null
    , [device_id]           [int] not null
    , [patient_id]          [bigint] not null
    , [description]         [nvarchar](500) not null
    , [document_type]       [varchar](25) not null
    , [file_name]           [nvarchar](500) not null
    , [file_format]         [varchar](10) not null
    , [page_count]          [int]
    , [print_datetime]      [datetimeoffset](7)
    , [expiration_datetime] [datetimeoffset](7));
go

/********
 Defaults
********/
/*****************
 Unique constraint
*****************/

alter table [dbo].[print_history]
add constraint [pk__print_history__id] primary key nonclustered([id] asc);
go

/*******
 Indexes
*******/

create nonclustered index [ix__print_history__patient_id_print_datetime] on [dbo].[print_history]
    ([patient_id] asc, [print_datetime] asc);
go

create clustered index [ix__print_history__print_datetime_patient_id] on [dbo].[print_history]
    ([print_datetime] asc, [patient_id] asc);
go

/***********
 Foreign Key
***********/

alter table [dbo].[print_history]
add constraint [fk__print_history__devices] foreign key([device_id]) references [dbo].[devices]([id]);
go

alter table [dbo].[print_history]
add constraint [fk__print_history__patients] foreign key([patient_id]) references [dbo].[patients]([id]);
go

alter table [dbo].[print_history]
add constraint [fk__print_history__users] foreign key([print_user_id]) references [dbo].[users]([id]);
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
  , @level1name = N'print_history'
  , @level2type = N'CONSTRAINT'
  , @level2name = N'pk__print_history__id';
go

/***************
 Data Dictionary
    Table
***************/

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'This table contains: print_history'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'print_history';
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
  , @level1name = N'print_history'
  , @level2type = N'COLUMN'
  , @level2name = N'id';
go

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'User identifier foreign key to users table'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'print_history'
  , @level2type = N'COLUMN'
  , @level2name = N'print_user_id';
go

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'Device identifier foreign key to devices table'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'print_history'
  , @level2type = N'COLUMN'
  , @level2name = N'device_id';
go

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'Patient identifier foreign key to patients table'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'print_history'
  , @level2type = N'COLUMN'
  , @level2name = N'patient_id';
go

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'Print job description'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'print_history'
  , @level2type = N'COLUMN'
  , @level2name = N'description';
go

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'Document Type: File or Print '
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'print_history'
  , @level2type = N'COLUMN'
  , @level2name = N'document_type';
go

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'File name'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'print_history'
  , @level2type = N'COLUMN'
  , @level2name = N'file_name';
go

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'File Extension'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'print_history'
  , @level2type = N'COLUMN'
  , @level2name = N'file_format';
go

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'count of pages'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'print_history'
  , @level2type = N'COLUMN'
  , @level2name = N'page_count';
go

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'datetime print job was submitted'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'print_history'
  , @level2type = N'COLUMN'
  , @level2name = N'print_datetime';
go

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'datetime document is considered expired'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'print_history'
  , @level2type = N'COLUMN'
  , @level2name = N'expiration_datetime';
go