create table [dbo].[action_route_templates]
    (
      [id]                  [int] identity(1, 1) not null
    , [action_id]           [int] not null
    , [medication_route_id] [int] null
    , [template_id]         [int] not null
    , [site_id]             [int] null
    , constraint [pk__action_route_templates__id] primary key clustered([id] asc));
go

/********
 Defaults
********/
/*****************
 Unique constraint
*****************/

alter table [dbo].[action_route_templates]
add constraint [uc__action_route_templates__action_id_site_id_medication_route_id] unique([action_id], [site_id], [medication_route_id]);
go

/*******
 Indexes
*******/
/***********
 Foreign Key
***********/

alter table [dbo].[action_route_templates]
add constraint [fk__action_route_templates__actions] foreign key([action_id]) references [dbo].[actions]([id]);
go

alter table [dbo].[action_route_templates]
add constraint [fk__action_route_templates__medication_routes] foreign key([medication_route_id]) references [dbo].[medication_routes]([id]);
go

alter table [dbo].[action_route_templates]
add constraint [fk__action_route_templates__templates] foreign key([template_id]) references [dbo].[templates]([id]);
go

alter table [dbo].[action_route_templates]
add constraint [fk__action_route_templates__sites] foreign key([site_id]) references [dbo].[sites]([id]);
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
  , @level1name = N'action_route_templates'
  , @level2type = N'CONSTRAINT'
  , @level2name = N'pk__action_route_templates__id';
go

/***************
 Data Dictionary
    Table
***************/

execute [sys].[sp_addextendedproperty]
    @name = N'MS_Description'
  , @value = N'This table contains: action route template'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'action_route_templates';
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
  , @level1name = N'action_route_templates'
  , @level2type = N'COLUMN'
  , @level2name = N'id';
go

execute [sys].[sp_addextendedproperty]
    @name = N'MS_Description'
  , @value = N'action_id'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'action_route_templates'
  , @level2type = N'COLUMN'
  , @level2name = N'action_id';
go

execute [sys].[sp_addextendedproperty]
    @name = N'MS_Description'
  , @value = N'Route of administration; Foreign Key to medication_routes table'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'action_route_templates'
  , @level2type = N'COLUMN'
  , @level2name = N'medication_route_id';
go

execute [sys].[sp_addextendedproperty]
    @name = N'MS_Description'
  , @value = N'template_id'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'action_route_templates'
  , @level2type = N'COLUMN'
  , @level2name = N'template_id';
go

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'Hospital identifier foriegn key to site table'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'action_route_templates'
  , @level2type = N'COLUMN'
  , @level2name = N'site_id';
go