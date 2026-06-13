create table [dbo].[department_preferred_list_items]
    (
        [id]                    [int]            identity (1, 1) not null
      , [site_id]               [int]            not null
      , [department_code]       [varchar](15)    null
      , [dose]                  [decimal](11, 2) null
      , [medication_unit_id]    [int]            null
      , [medication_route_id]   [int]            null
      , [frequency_schedule_id] [int]            null
      , [order_notes]           [nvarchar](max)  null
      , [medication_id]         [int]            not null
      , [duration_in_minutes]   [int]            not null
      , [duration]              [int]            null
      , [duration_unit_id]      [int]            null
      , [priority]              [tinyint]        null
      , [ndc]                   [varchar](11)    null
      , constraint [pk__department_preferred_list_items__id] primary key clustered ([id] asc)
    );

go

/********
 Defaults
********/

alter table [dbo].[department_preferred_list_items]
add constraint [df__department_preferred_list_items__duration_in_minutes] default ((0)) for [duration_in_minutes];
go

/*****************
 Unique constraint
*****************/
/*******
 Indexes
*******/
/***********
 Foreign Key
***********/

alter table [dbo].[department_preferred_list_items]
add constraint [fk__department_preferred_list_items__sites] foreign key ([site_id]) references [dbo].[sites] ([id]);
go

alter table [dbo].[department_preferred_list_items]
add constraint [fk__department_preferred_list_items__medication_units] foreign key ([medication_unit_id]) references [dbo].[medication_units] ([id]);
go

alter table [dbo].[department_preferred_list_items]
add constraint [fk__department_preferred_list_items__medication_routes] foreign key ([medication_route_id]) references [dbo].[medication_routes] ([id]);
go

alter table [dbo].[department_preferred_list_items]
add constraint [fk__department_preferred_list_items__frequency_schedules] foreign key ([frequency_schedule_id]) references [dbo].[frequency_schedules] ([id]);
go

alter table [dbo].[department_preferred_list_items]
add constraint [fk__department_preferred_list_items__medications] foreign key ([medication_id]) references [dbo].[medications] ([id]);
go

alter table [dbo].[department_preferred_list_items]
add constraint [fk__department_preferred_list_items__duration_units] foreign key ([duration_unit_id]) references [dbo].[duration_units] ([id]);
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
  , @level1name = N'department_preferred_list_items'
  , @level2type = N'CONSTRAINT'
  , @level2name = N'pk__department_preferred_list_items__id';
go

/***************
 Data Dictionary
    Table
***************/

execute [sys].[sp_addextendedproperty]
    @name       = N'MS_Description'
  , @value      = N'This table contains medications preferred list for the department preferred list tab'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'department_preferred_list_items';
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
  , @level1name = N'department_preferred_list_items'
  , @level2type = N'COLUMN'
  , @level2name = N'id';
go

execute [sys].[sp_addextendedproperty]
    @name       = N'MS_Description'
  , @value      = N'Hospital identifier foreign key to site table'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'department_preferred_list_items'
  , @level2type = N'COLUMN'
  , @level2name = N'site_id';
go

execute [sys].[sp_addextendedproperty]
    @name       = N'MS_Description'
  , @value      = N'Department location for grouping medications'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'department_preferred_list_items'
  , @level2type = N'COLUMN'
  , @level2name = N'department_code';
go

go

go

go

execute [sys].[sp_addextendedproperty]
    @name       = N'MS_Description'
  , @value      = N'Route of administration; Foreign Key to medication_routes table'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'department_preferred_list_items'
  , @level2type = N'COLUMN'
  , @level2name = N'medication_route_id';
go

execute [sys].[sp_addextendedproperty]
    @name       = N'MS_Description'
  , @value      = N'Medication Dose: numeric portion of dose/medication_unit_id pair'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'department_preferred_list_items'
  , @level2type = N'COLUMN'
  , @level2name = N'dose';
go

execute [sys].[sp_addextendedproperty]
    @name       = N'MS_Description'
  , @value      = N'Medication Unit: unit portion of dose/medication_unit_id pair'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'department_preferred_list_items'
  , @level2type = N'COLUMN'
  , @level2name = N'medication_unit_id';
go

execute [sys].[sp_addextendedproperty]
    @name       = N'MS_Description'
  , @value      = N'Foreign Key to frequency_schedules table'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'department_preferred_list_items'
  , @level2type = N'COLUMN'
  , @level2name = N'frequency_schedule_id';
go

execute [sys].[sp_addextendedproperty]
    @name       = N'MS_Description'
  , @value      = N'order_notes'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'department_preferred_list_items'
  , @level2type = N'COLUMN'
  , @level2name = N'order_notes';
go

execute [sys].[sp_addextendedproperty]
    @name       = N'MS_Description'
  , @value      = N'Medications identifier, Foreign Key to medications table'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'department_preferred_list_items'
  , @level2type = N'COLUMN'
  , @level2name = N'medication_id';
go

execute [sys].[sp_addextendedproperty]
    @name       = N'MS_Description'
  , @value      = N'Administration duration in minutes, used to calculate the administration stop time'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'department_preferred_list_items'
  , @level2type = N'COLUMN'
  , @level2name = N'duration_in_minutes';
go

execute [sys].[sp_addextendedproperty]
    @name       = N'MS_Description'
  , @value      = N'Length of duration based on duration_unit_id'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'department_preferred_list_items'
  , @level2type = N'COLUMN'
  , @level2name = 'duration';
go

execute [sys].[sp_addextendedproperty]
    @name       = N'MS_Description'
  , @value      = N'Duration Unit identifier, Foreign Key to duration_unit table'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'department_preferred_list_items'
  , @level2type = N'COLUMN'
  , @level2name = 'duration_unit_id';
go

execute [sys].[sp_addextendedproperty]
    @name       = N'MS_Description'
  , @value      = N'Display sort order'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'department_preferred_list_items'
  , @level2type = N'COLUMN'
  , @level2name = 'priority';
go

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'National Drug Code'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'department_preferred_list_items'
  , @level2type = N'COLUMN'
  , @level2name = N'ndc';
go