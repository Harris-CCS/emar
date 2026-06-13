create table [dbo].[prompt_choices]
    (
      [id]           [int] identity(1, 1) not null
    , [prompt_id]    [int] not null
    , [sequence]     [smallint] not null
    , [choice_text]  [nvarchar](200) not null
	, [chart_markup] [nvarchar](256) null
    , constraint [pk__prompt_choices__id] primary key clustered([id] asc));
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

alter table [dbo].[prompt_choices]
add constraint [fk__prompt_choices__prompts] foreign key([prompt_id]) references [dbo].[prompts]([id]);
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
  , @level1name = N'prompt_choices'
  , @level2type = N'CONSTRAINT'
  , @level2name = N'pk__prompt_choices__id';
go

/***************
 Data Dictionary
    Table
***************/

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'This table contains: prompt choices'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'prompt_choices';
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
  , @level1name = N'prompt_choices'
  , @level2type = N'COLUMN'
  , @level2name = N'id';
go

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'prompt_id'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'prompt_choices'
  , @level2type = N'COLUMN'
  , @level2name = N'prompt_id';
go

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'sequence'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'prompt_choices'
  , @level2type = N'COLUMN'
  , @level2name = N'sequence';
go

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'choice_text'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'prompt_choices'
  , @level2type = N'COLUMN'
  , @level2name = N'choice_text';
go

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'Defines the chart markup, clincal identifiers and billing codes'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'prompt_choices'
  , @level2type = N'COLUMN'
  , @level2name = N'chart_markup';
go
