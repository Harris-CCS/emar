create table [dbo].[template_prompt_groups]
    (
      [id]              [int] identity(1, 1) not null
    , [template_id]     [int] not null
    , [sequence]        [tinyint] not null
    , [prompt_group_id] [int] not null
    , [required]        [bit] not null
    , constraint [pk__template_prompt_groups__id] primary key clustered([id] asc));
go

/********
 Defaults
********/
/*****************
 Unique constraint
*****************/

alter table [dbo].[template_prompt_groups]
add constraint [uc__template_prompt_groups__template_id_sequence] unique([template_id], [sequence]);
go

/*******
 Indexes
*******/
/***********
 Foreign Key
***********/

alter table [dbo].[template_prompt_groups]
add constraint [fk__template_prompt_groups__prompt_groups] foreign key([prompt_group_id]) references [dbo].[prompt_groups]([id]);
go

alter table [dbo].[template_prompt_groups]
add constraint [fk__template_prompt_groups__templates] foreign key([template_id]) references [dbo].[templates]([id]);
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
  , @level1name = N'template_prompt_groups'
  , @level2type = N'CONSTRAINT'
  , @level2name = N'pk__template_prompt_groups__id';
go

/***************
 Data Dictionary
    Table
***************/

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'This table contains: template prompt groups'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'template_prompt_groups';
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
  , @level1name = N'template_prompt_groups'
  , @level2type = N'COLUMN'
  , @level2name = N'id';
go

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'template_id'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'template_prompt_groups'
  , @level2type = N'COLUMN'
  , @level2name = N'template_id';
go

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'sequence'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'template_prompt_groups'
  , @level2type = N'COLUMN'
  , @level2name = N'sequence';
go

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'prompt_group_id'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'template_prompt_groups'
  , @level2type = N'COLUMN'
  , @level2name = N'prompt_group_id';
go

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'required'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'template_prompt_groups'
  , @level2type = N'COLUMN'
  , @level2name = N'required';
go