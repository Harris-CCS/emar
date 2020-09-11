create table [dbo].[user_quick_list_items]
    (
      [id]                           [int] identity(1, 1) not null
    , [site_id]                      [int] not null
    , [user_id]                      [int] not null
    , [dose]                         [decimal](11, 2) null
    , [medication_unit_id]           [int] null
    , [medication_route_id]          [int] null
    , [frequency_schedule_id]        [int] null
    , [order_notes]                  [nvarchar](max) null
    , [usages_this_week]             [int] null
    , [weekly_usage_rolling_average] [decimal](9, 3) not null
    , [medication_id]                [int] not null
    , [duration_in_minutes]          [int] not null
    , constraint [pk__user_quick_list_items__id] primary key clustered([id] asc));
go

/********
 Defaults
********/

alter table [dbo].[user_quick_list_items]
add constraint [df__user_quick_list_items__usages_this_week] default((0)) for [usages_this_week];
go

alter table [dbo].[user_quick_list_items]
add constraint [df__user_quick_list_items__weekly_usage_rolling_average] default((-1)) for [weekly_usage_rolling_average];
go

alter table [dbo].[user_quick_list_items]
add constraint [df__user_quick_list_items__duration_in_minutes] default((0)) for [duration_in_minutes];
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

alter table [dbo].[user_quick_list_items]
add constraint [fk__user_quick_list_items__user] foreign key([user_id]) references [dbo].[users]([id]);
go

alter table [dbo].[user_quick_list_items]
add constraint [fk__user_quick_list_items__sites] foreign key([site_id]) references [dbo].[sites]([id]);
go

alter table [dbo].[user_quick_list_items]
add constraint [fk__user_quick_list_items__medication_units] foreign key([medication_unit_id]) references [dbo].[medication_units]([id]);
go

alter table [dbo].[user_quick_list_items]
add constraint [fk__user_quick_list_items__medication_routes] foreign key([medication_route_id]) references [dbo].[medication_routes]([id]);
go

alter table [dbo].[user_quick_list_items]
add constraint [fk__user_quick_list_items__frequency_schedules] foreign key([frequency_schedule_id]) references [dbo].[frequency_schedules]([id]);
go

alter table [dbo].[user_quick_list_items]
add constraint [fk__user_quick_list_items__medications] foreign key([medication_id]) references [dbo].[medications]([id]);
go

/***************
 Data Dictionary
    Defaults
***************/

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'default usages_this_week to 0'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'usages_this_week'
  , @level2type = N'CONSTRAINT'
  , @level2name = N'df__user_quick_list_items__usages_this_week';
go

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'default weekly_usage_rolling_average to -1'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'weekly_usage_rolling_average'
  , @level2type = N'CONSTRAINT'
  , @level2name = N'df__user_quick_list_items__weekly_usage_rolling_average';
go

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
  , @level1name = N'user_quick_list_items'
  , @level2type = N'CONSTRAINT'
  , @level2name = N'pk__user_quick_list_items__id';
go

/***************
 Data Dictionary
    Table
***************/

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'This table contains medications preferred list for the department preferred list tab'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'user_quick_list_items';
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
  , @level1name = N'user_quick_list_items'
  , @level2type = N'COLUMN'
  , @level2name = N'id';
go

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'Hospital identifier foriegn key to site table'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'user_quick_list_items'
  , @level2type = N'COLUMN'
  , @level2name = N'site_id';
go

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'Person Idendifier that owns this medication list record (Foreign Key to users table)'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'user_quick_list_items'
  , @level2type = N'COLUMN'
  , @level2name = N'user_id';
go

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'Route of administration; Foreign Key to medication_routes table'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'user_quick_list_items'
  , @level2type = N'COLUMN'
  , @level2name = N'medication_route_id';
go

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'Medication Dose: numeric portion of dose/medication_unit_id pair'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'user_quick_list_items'
  , @level2type = N'COLUMN'
  , @level2name = N'dose';
go

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'Medication Unit: unit portion of dose/medication_unit_id pair'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'user_quick_list_items'
  , @level2type = N'COLUMN'
  , @level2name = N'medication_unit_id';
go

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'Foreign Key to frequency_schedules table'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'user_quick_list_items'
  , @level2type = N'COLUMN'
  , @level2name = N'frequency_schedule_id';
go

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'order_notes'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'user_quick_list_items'
  , @level2type = N'COLUMN'
  , @level2name = N'order_notes';
go

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'Number of times this quick list items has been used this week'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'user_quick_list_items'
  , @level2type = N'COLUMN'
  , @level2name = N'usages_this_week';
go

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'Rolling Avreage of weekly usage used to set a most used priority sort order'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'user_quick_list_items'
  , @level2type = N'COLUMN'
  , @level2name = N'weekly_usage_rolling_average';
go

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'Medications identifier, Foreign Key to medications table'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'user_quick_list_items'
  , @level2type = N'COLUMN'
  , @level2name = N'medication_id';
go