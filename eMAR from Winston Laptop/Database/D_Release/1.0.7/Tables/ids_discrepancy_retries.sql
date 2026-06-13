create table ids_discrepancy_retries
(
	Id int identity(1,1) NOT NULL,
	-- columns returned by the function
	ExternalPatientId varchar(40) NOT NULL,
	EmarPatientId bigint NOT NULL,
	Discrepancies varchar(500) NOT NULL,
	-- tracking fields
	FirstRetryTime datetime NOT NULL default (GETDATE()),
	LatestRetryTime datetime NOT NULL default (GETDATE()),
	RetryCount tinyint NOT NULL default (1),
	-- PK
	constraint PK_ids_discrepancy_retries primary key (ExternalPatientId, EmarPatientId, Discrepancies)
)
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
execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'Primary Key Column'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'ids_discrepancy_retries'
  , @level2type = N'CONSTRAINT'
  , @level2name = N'PK_ids_discrepancy_retries';
go

/***************
 Data Dictionary
    Table
***************/

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'This table contains a list of ids discrepancy retries'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'ids_discrepancy_retries';
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
  , @level1name = N'ids_discrepancy_retries'
  , @level2type = N'COLUMN'
  , @level2name = N'Id';
go

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'External patient id'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'ids_discrepancy_retries'
  , @level2type = N'COLUMN'
  , @level2name = N'ExternalPatientId';
go

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'External patient id'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'ids_discrepancy_retries'
  , @level2type = N'COLUMN'
  , @level2name = N'EmarPatientId';
go

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'Discrepancies'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'ids_discrepancy_retries'
  , @level2type = N'COLUMN'
  , @level2name = N'Discrepancies';
go

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'First retry time'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'ids_discrepancy_retries'
  , @level2type = N'COLUMN'
  , @level2name = N'FirstRetryTime';
go

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'Latest retry time'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'ids_discrepancy_retries'
  , @level2type = N'COLUMN'
  , @level2name = N'LatestRetryTime';
go

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'Retry count'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'ids_discrepancy_retries'
  , @level2type = N'COLUMN'
  , @level2name = N'RetryCount';
go