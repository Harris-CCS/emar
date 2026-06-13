CREATE TABLE [dbo].[medications_high_risk]
(
        [id]               [int] identity(1, 1) not null
      , [long_brand_name]  [nvarchar](255) not null
	  , [active]           [nvarchar](255) not null
	  , [routed_gen_id]    [numeric](8, 0) not null
	  , [pc_routed_gen_id] [varchar](9) not null
	  , [route]            [varchar](40) not null
      , [medication_id]    [int] not null
    ,constraint pk__medications_high_risk__id primary key clustered ([id] asc)
);
go

/********
 Defaults
********/
/*****************
 Unique constraint
*****************/
execute [sys].[sp_addextendedproperty]
    @name = N'MS_Description'
  , @value = N'Primary Key Column'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'medications_high_risk'
  , @level2type = N'CONSTRAINT'
  , @level2name = N'pk__medications_high_risk__id';
go

/*******
 Indexes
*******/
create index [ix__medications_high_risk__medication_id] on [dbo].[medications_high_risk]
    ([medication_id] asc);
go
/***********
 Foreign Key
***********/
alter table [dbo].[medications_high_risk]
add constraint [fk__medications_high_risk__medications] foreign key([medication_id]) references [dbo].[medications]([id]);
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
  , @value = N'Index to get the medication_id needed to join into the fdb tables'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'medications_high_risk'
  , @level2type = N'INDEX'
  , @level2name = N'ix__medications_high_risk__medication_id';
go

/***************
 Data Dictionary
    Table
***************/

execute [sys].[sp_addextendedproperty]
    @name = N'MS_Description'
  , @value = N'This table contains: high risk medications'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'medications_high_risk';
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
  , @level1name = N'medications_high_risk'
  , @level2type = N'COLUMN'
  , @level2name = N'id';
go

execute [sys].[sp_addextendedproperty]
    @name = N'MS_Description'
  , @value = N'The complete name (med name and formulation information)'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'medications_high_risk'
  , @level2type = N'COLUMN'
  , @level2name = N'long_brand_name';
go

execute [sys].[sp_addextendedproperty]
    @name = N'MS_Description'
  , @value = N'The active ingredient'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'medications_high_risk'
  , @level2type = N'COLUMN'
  , @level2name = N'active';
go

execute [sys].[sp_addextendedproperty]
    @name = N'MS_Description'
  , @value = N'The Routed Generic ID'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'medications_high_risk'
  , @level2type = N'COLUMN'
  , @level2name = N'routed_gen_id';
go

execute [sys].[sp_addextendedproperty]
    @name = N'MS_Description'
  , @value = N'The string coded version of the routed_gen_id (prefixed with an ''R'')'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'medications_high_risk'
  , @level2type = N'COLUMN'
  , @level2name = N'pc_routed_gen_id';
go

execute [sys].[sp_addextendedproperty]
    @name = N'MS_Description'
  , @value = N'Medication route'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'medications_high_risk'
  , @level2type = N'COLUMN'
  , @level2name = N'route';
go

execute [sys].[sp_addextendedproperty]
    @name = N'MS_Description'
  , @value = N'Medications identifier, Foreign Key to medications table'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'medications_high_risk'
  , @level2type = N'COLUMN'
  , @level2name = N'medication_id';
go
