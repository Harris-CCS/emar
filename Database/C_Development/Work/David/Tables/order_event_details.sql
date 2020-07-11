create table [dbo].[order_event_details]
    (
      [id]             [bigint] identity(1, 1) not null
    , [order_event_id] [bigint] not null
    , [prompt_id]      [int] not null
    , [prompt_text]    [varchar](200) not null
    , [entered_text]   [varchar](max) null
    , constraint [pk__order_event_details__id] primary key clustered([id] asc));
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

alter table [dbo].[order_event_details]
add constraint [fk__order_event_details__order_events] foreign key([order_event_id]) references [dbo].[order_events]([id]);
go

alter table [dbo].[order_event_details]
add constraint [fk__order_event_details__prompts] foreign key([prompt_id]) references [dbo].[prompts]([id]);
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
  , @level1name = N'order_event_details'
  , @level2type = N'CONSTRAINT'
  , @level2name = N'pk__order_event_details__id';
go

/***************
 Data Dictionary
    Table
***************/

execute [sys].[sp_addextendedproperty]
    @name = N'MS_Description'
  , @value = N'This table contains: order event details'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'order_event_details';
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
  , @level1name = N'order_event_details'
  , @level2type = N'COLUMN'
  , @level2name = N'id';
go

execute [sys].[sp_addextendedproperty]
    @name = N'MS_Description'
  , @value = N'order_event_id'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'order_event_details'
  , @level2type = N'COLUMN'
  , @level2name = N'order_event_id';
go

execute [sys].[sp_addextendedproperty]
    @name = N'MS_Description'
  , @value = N'prompt_id'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'order_event_details'
  , @level2type = N'COLUMN'
  , @level2name = N'prompt_id';
go

execute [sys].[sp_addextendedproperty]
    @name = N'MS_Description'
  , @value = N'prompt_text'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'order_event_details'
  , @level2type = N'COLUMN'
  , @level2name = N'prompt_text';
go

execute [sys].[sp_addextendedproperty]
    @name = N'MS_Description'
  , @value = N'entered_text'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'order_event_details'
  , @level2type = N'COLUMN'
  , @level2name = N'entered_text';
go