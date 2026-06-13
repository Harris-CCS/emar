create table [dbo].[template_response_rules]
    (
      [id]           [bigint] identity(1, 1) not null
    , [site_id]      [int] not null
    , [prompt_id]    [int] not null
    , [prompt_value] [varchar](50) null
    , [code_type]    [varchar](10) not null
    , [code_value]   [varchar](50) not null
    , constraint [pk__template_response_rules__id] primary key nonclustered([id]));
go

/********
 Defaults
********/
/*****************
 Unique constraint
*****************/

alter table [dbo].[template_response_rules]
    add constraint [uc__template_response_rules__site_id_prompt_id_prompt_value]
    unique clustered([site_id] asc, [prompt_id] asc, [prompt_value] asc);
go

/*******
 Indexes
*******/
/***********
 Foreign Key
***********/

alter table [dbo].[template_response_rules]
add constraint [fk__template_response_rules__sites] foreign key([site_id]) references [dbo].[sites]([id]);
go

alter table [dbo].[template_response_rules]
add constraint [fk__template_response_rules__prompts] foreign key([prompt_id]) references [dbo].[sites]([id]);
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
  , @level1name = N'template_response_rules'
  , @level2type = N'CONSTRAINT'
  , @level2name = N'pk__template_response_rules__id';
go

/***************
 Data Dictionary
    Table
***************/

execute [sys].[sp_addextendedproperty]
    @name = N'MS_Description'
  , @value = N'This table contains the configuration of template prompt responses and values to transmit to external systems'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'template_response_rules';
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
  , @level1name = N'template_response_rules'
  , @level2type = N'COLUMN'
  , @level2name = N'id';
go

execute [sys].[sp_addextendedproperty]
    @name = N'MS_Description'
  , @value = N'Hospital identifier foreign key to site table'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'template_response_rules'
  , @level2type = N'COLUMN'
  , @level2name = N'site_id';
go

execute [sys].[sp_addextendedproperty]
    @name = N'MS_Description'
  , @value = N'Prompt identifier foreign key to prompts table'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'template_response_rules'
  , @level2type = N'COLUMN'
  , @level2name = N'prompt_id';
go

execute [sys].[sp_addextendedproperty]
    @name = N'MS_Description'
  , @value = N'The value returned from the template prompt to match an external value'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'template_response_rules'
  , @level2type = N'COLUMN'
  , @level2name = N'prompt_value';
go

execute [sys].[sp_addextendedproperty]
    @name = N'MS_Description'
  , @value = N'External vendor code type'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'template_response_rules'
  , @level2type = N'COLUMN'
  , @level2name = N'code_type';
go

execute [sys].[sp_addextendedproperty]
    @name = N'MS_Description'
  , @value = N'External vendor code value to transmit'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'template_response_rules'
  , @level2type = N'COLUMN'
  , @level2name = N'code_value';
go