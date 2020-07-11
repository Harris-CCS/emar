create table [dbo].[order_administration_notes]
    (
      [id]                      [bigint] identity(1, 1) not null
    , [order_administration_id] [bigint] not null
    , [note_sequence]           [smallint] not null
    , [note_type_id]            [int] null
    , [note_id]                 [int] null
    , [note_text]               [nvarchar](max) null
    , [add_user_id]             [int] not null
    , [add_datetime]            [datetimeoffset](7) not null
    , constraint [pk__order_administration_notes__id] primary key clustered([id] asc));
go

/********
 Defaults
********/
/*******
 Indexes
*******/
/***********
 Foreign Key
***********/

alter table [dbo].[order_administration_notes]
add constraint [fk__order_administration_notes__order_administrations] foreign key([order_administration_id]) references [dbo].[order_administrations]([id]);
go

alter table [dbo].[order_administration_notes]
add constraint [fk__order_administration_notes__users] foreign key([add_user_id]) references [dbo].[users]([id]);
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
  , @level1name = N'order_administration_notes'
  , @level2type = N'CONSTRAINT'
  , @level2name = N'pk__order_administration_notes__id';
go

/***************
 Data Dictionary
    Table
***************/

execute [sys].[sp_addextendedproperty]
    @name = N'MS_Description'
  , @value = N'This table contains: order administration notes'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'order_administration_notes';
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
  , @level1name = N'order_administration_notes'
  , @level2type = N'COLUMN'
  , @level2name = N'id';
go

execute [sys].[sp_addextendedproperty]
    @name = N'MS_Description'
  , @value = N'order_administration_id'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'order_administration_notes'
  , @level2type = N'COLUMN'
  , @level2name = N'order_administration_id';
go

execute [sys].[sp_addextendedproperty]
    @name = N'MS_Description'
  , @value = N'note_sequence'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'order_administration_notes'
  , @level2type = N'COLUMN'
  , @level2name = N'note_sequence';
go

execute [sys].[sp_addextendedproperty]
    @name = N'MS_Description'
  , @value = N'note_type_id'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'order_administration_notes'
  , @level2type = N'COLUMN'
  , @level2name = N'note_type_id';
go

execute [sys].[sp_addextendedproperty]
    @name = N'MS_Description'
  , @value = N'note_id'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'order_administration_notes'
  , @level2type = N'COLUMN'
  , @level2name = N'note_id';
go

execute [sys].[sp_addextendedproperty]
    @name = N'MS_Description'
  , @value = N'note_text'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'order_administration_notes'
  , @level2type = N'COLUMN'
  , @level2name = N'note_text';
go

execute [sys].[sp_addextendedproperty]
    @name = N'MS_Description'
  , @value = N'add_user_id'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'order_administration_notes'
  , @level2type = N'COLUMN'
  , @level2name = N'add_user_id';
go

execute [sys].[sp_addextendedproperty]
    @name = N'MS_Description'
  , @value = N'add_datetime'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'order_administration_notes'
  , @level2type = N'COLUMN'
  , @level2name = N'add_datetime';
go