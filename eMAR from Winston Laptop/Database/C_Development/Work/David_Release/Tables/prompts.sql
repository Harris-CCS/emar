create table [dbo].[prompts]
    (
        [id]                          [int]           identity (1, 1) not null
      , [prompt_group_id]             [int]           not null
      , [sequence]                    [smallint]      not null
      , [prompt]                      [nvarchar](200) not null
      , [is_active]                   [bit]           not null
      , [prompt_type]                 [varchar](25)   not null
      , [prompt_default]              [varchar](100)  null
      , [required]                    [bit]           not null
      , [is_on_newline]               [bit]           not null
      , [placeholder_text]            [nvarchar](100) null
      , [display_child_prompts_value] [nvarchar](100) null
      , constraint [pk__prompts__id] primary key clustered ([id] asc)
    );
go

/********
 Defaults
********/

alter table [dbo].[prompts]
add constraint [df__prompts__is_active] default ((1)) for [is_active];
go

alter table [dbo].[prompts]
add constraint [df__prompts__required] default ((0)) for [required];
go

alter table [dbo].[prompts]
add constraint [df__prompts__is_on_newline] default ((1)) for [is_on_newline];
go

/*****************
 Unique constraint
*****************/

alter table [dbo].[prompts]
add constraint [uc__prompts__name] unique ([prompt_group_id], [prompt]);
go

alter table [dbo].[prompts]
add constraint [uc__prompts__prompt_group_id_sequence] unique ([prompt_group_id], [sequence]);
go

/*******
 Indexes
*******/
/***********
 Foreign Key
***********/

alter table [dbo].[prompts]
add constraint [fk__prompts__prompt_groups] foreign key ([prompt_group_id]) references [dbo].[prompt_groups] ([id]);
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
    @name       = N'MS_Description'
  , @value      = N'Primary Key Column'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'prompts'
  , @level2type = N'CONSTRAINT'
  , @level2name = N'pk__prompts__id';
go

/***************
 Data Dictionary
    Table
***************/

execute [sys].[sp_addextendedproperty]
    @name       = N'MS_Description'
  , @value      = N'This table contains: prompts'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'prompts';
go

/***************
 Data Dictionary
    Columns
***************/

execute [sys].[sp_addextendedproperty]
    @name       = N'MS_Description'
  , @value      = N'Auto increment table ID'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'prompts'
  , @level2type = N'COLUMN'
  , @level2name = N'id';
go

execute [sys].[sp_addextendedproperty]
    @name       = N'MS_Description'
  , @value      = N'prompt_group_id'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'prompts'
  , @level2type = N'COLUMN'
  , @level2name = N'prompt_group_id';
go

execute [sys].[sp_addextendedproperty]
    @name       = N'MS_Description'
  , @value      = N'sequence'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'prompts'
  , @level2type = N'COLUMN'
  , @level2name = N'sequence';
go

execute [sys].[sp_addextendedproperty]
    @name       = N'MS_Description'
  , @value      = N'prompt'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'prompts'
  , @level2type = N'COLUMN'
  , @level2name = N'prompt';
go

execute [sys].[sp_addextendedproperty]
    @name       = N'MS_Description'
  , @value      = N'is_active 1=True 0=False'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'prompts'
  , @level2type = N'COLUMN'
  , @level2name = N'is_active';
go

execute [sys].[sp_addextendedproperty]
    @name       = N'MS_Description'
  , @value      = N'default prompts__is_active to 1'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'prompts'
  , @level2type = N'CONSTRAINT'
  , @level2name = N'df__prompts__is_active';
go

execute [sys].[sp_addextendedproperty]
    @name       = N'MS_Description'
  , @value      = N'prompt_type'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'prompts'
  , @level2type = N'COLUMN'
  , @level2name = N'prompt_type';
go

execute [sys].[sp_addextendedproperty]
    @name       = N'MS_Description'
  , @value      = N'prompt_default'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'prompts'
  , @level2type = N'COLUMN'
  , @level2name = N'prompt_default';
go

execute [sys].[sp_addextendedproperty]
    @name       = N'MS_Description'
  , @value      = N'required'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'prompts'
  , @level2type = N'COLUMN'
  , @level2name = N'required';
go

execute [sys].[sp_addextendedproperty]
    @name       = N'MS_Description'
  , @value      = N'default prompts__required to 0'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'prompts'
  , @level2type = N'CONSTRAINT'
  , @level2name = N'df__prompts__required';
go

execute [sys].[sp_addextendedproperty]
    @name       = N'MS_Description'
  , @value      = N'Indicator to display prompt on a new line'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'prompts'
  , @level2type = N'COLUMN'
  , @level2name = N'is_on_newline';
go