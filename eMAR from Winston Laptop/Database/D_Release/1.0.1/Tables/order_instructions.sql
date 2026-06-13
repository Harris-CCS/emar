create table [dbo].[order_instructions]
    (
        [id]          [int]           identity (1, 1) not null
      , [site_id]     [int]           not null
      , [description] [nvarchar](255) not null
      , [is_active]   [bit]           not null
      , [code]        [varchar](50)   not null
      , constraint [pk__order_instructions__id] primary key clustered ([id] asc)
    );

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

create unique nonclustered index [ix__order_instructions__code_site_id] on [dbo].[order_instructions] ([code] asc, [site_id] asc);
go

/***********
 Foreign Key
***********/
alter table [dbo].[order_instructions]
add constraint [fk__order_instructions__sites] foreign key ([site_id]) references [dbo].[sites] ([id]);
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
  , @level1name = N'order_instructions'
  , @level2type = N'CONSTRAINT'
  , @level2name = N'pk__order_instructions__id';
go

/***************
 Data Dictionary
    Table
***************/

execute [sys].[sp_addextendedproperty]
    @name       = N'MS_Description'
  , @value      = N'This table contains: order_instructions'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'order_instructions';
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
  , @level1name = N'order_instructions'
  , @level2type = N'COLUMN'
  , @level2name = N'id';
go

execute [sys].[sp_addextendedproperty]
    @name       = N'MS_Description'
  , @value      = N'description'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'order_instructions'
  , @level2type = N'COLUMN'
  , @level2name = N'description';
go

execute [sys].[sp_addextendedproperty]
    @name       = N'MS_Description'
  , @value      = N'Hospital identifier foreign key to site table'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'order_instructions'
  , @level2type = N'COLUMN'
  , @level2name = N'site_id';
go

execute [sys].[sp_addextendedproperty]
    @name       = N'MS_Description'
  , @value      = N'is_active 1=True 0=False'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'order_instructions'
  , @level2type = N'COLUMN'
  , @level2name = N'is_active';
go

execute [sys].[sp_addextendedproperty]
    @name       = N'MS_Description'
  , @value      = N'code or short name, combined with site to make a unique pair'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'order_instructions'
  , @level2type = N'COLUMN'
  , @level2name = N'code';
go
