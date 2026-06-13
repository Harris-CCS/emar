create table [dbo].[notification_categories]
	(
      [id] [int] not null
	, [code] [varchar](20) not null
	, [description] [nvarchar](150) not null
	, [priority] [smallint] not null
	, [action_url] [varchar](255) null
    , constraint [pk__notification_categories__id] primary key clustered([id] asc)); 
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
/***************
 Data Dictionary
    Defaults
***************/
/***************
 Data Dictionary
    Indexes
***************/
/***************
 Data Dictionary
    Table
***************/

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'Notification categories - 
This table defines notification categories to associate with notifications.'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'notification_categories';
go

/***************
 Data Dictionary
    Columns
***************/

execute [sys].[sp_addextendedproperty] 
	@name=N'MS_Description'
  , @value=N'Category ID'
  , @level0type=N'SCHEMA'
  , @level0name=N'dbo'
  , @level1type=N'TABLE'
  , @level1name=N'notification_categories'
  , @level2type=N'COLUMN'
  , @level2name=N'id'
go

execute [sys].[sp_addextendedproperty]
	@name=N'MS_Description'
  , @value=N'Categpry code'
  , @level0type=N'SCHEMA'
  , @level0name=N'dbo'
  , @level1type=N'TABLE'
  , @level1name=N'notification_categories'
  , @level2type=N'COLUMN'
  , @level2name=N'code'
go

execute [sys].[sp_addextendedproperty]
	@name=N'MS_Description'
  , @value=N'Category description'
  , @level0type=N'SCHEMA'
  , @level0name=N'dbo'
  , @level1type=N'TABLE'
  , @level1name=N'notification_categories'
  , @level2type=N'COLUMN'
  , @level2name=N'description'
go

execute [sys].[sp_addextendedproperty]
	@name=N'MS_Description'
  , @value=N'Category display priority - for sorting'
  , @level0type=N'SCHEMA'
  , @level0name=N'dbo'
  , @level1type=N'TABLE'
  , @level1name=N'notification_categories'
  , @level2type=N'COLUMN'
  , @level2name=N'priority'
go

execute [sys].[sp_addextendedproperty]
	@name=N'MS_Description'
  , @value=N'Category action URL'
  , @level0type=N'SCHEMA'
  , @level0name=N'dbo'
  , @level1type=N'TABLE'
  , @level1name=N'notification_categories'
  , @level2type=N'COLUMN'
  , @level2name=N'action_url'
go
