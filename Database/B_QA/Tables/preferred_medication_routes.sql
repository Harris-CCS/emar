create table [dbo].[preferred_medication_routes]
    (
      [drug_id]               [varchar](32) not null
    , [medication_route_id] [int] not null
    , [site_id]             [int] not null);
go

/********
 Defaults
********/
/*****************
 Unique constraint
*****************/

alter table [dbo].[preferred_medication_routes]
add constraint [uc__preferred_medication_routes] unique clustered([medication_route_id] asc, [site_id] asc, [drug_id] asc);
go

/*******
 Indexes
*******/
/***********
 Foreign Key
***********/
alter table [dbo].[preferred_medication_routes]
add constraint [fk__preferred_medication_routes__sites] foreign key([site_id]) references [dbo].[sites]([id]);
go

alter table [dbo].[preferred_medication_routes]
add constraint [fk__preferred_medication_routes__medication_routes] foreign key([medication_route_id]) references [dbo].[medication_routes]([id]);
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
  , @value = N'Constraint to enforce preferred medication routes uniqueness'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'preferred_medication_routes'
  , @level2type = N'CONSTRAINT'
  , @level2name = N'uc__preferred_medication_routes';
go

/***************
 Data Dictionary
    Table
***************/

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'This table contains a preferred list of medication routes for a specific drug id'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'preferred_medication_routes';
go

/***************
 Data Dictionary
    Columns
***************/

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'External Vendor Drug Database Identifier
  FDB: MEDID (MED Medication ID (Stable ID))'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'preferred_medication_routes'
  , @level2type = N'COLUMN'
  , @level2name = N'drug_id';
go

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'Route of administration; Foreign Key to medication_routes table'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'preferred_medication_routes'
  , @level2type = N'COLUMN'
  , @level2name = N'medication_route_id';
go

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'Hospital identifier foriegn key to site table'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'preferred_medication_routes'
  , @level2type = N'COLUMN'
  , @level2name = N'site_id';
go