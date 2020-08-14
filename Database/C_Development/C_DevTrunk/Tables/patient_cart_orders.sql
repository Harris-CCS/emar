create table [dbo].[patient_cart_orders]
    (
      [id]                      [bigint] identity(1, 1) not null
    , [patient_id]              [bigint] not null
    , [user_id]                 [int] not null
    , [add_datetime]            [datetimeoffset](7) not null
    , [ndc]                     [varchar](32) null
    , [drug_id]                 [varchar](32) not null
    , [brand_name]              [nvarchar](255) not null
    , [dose]                    [decimal](11, 2) null
    , [medication_unit_id]      [int] null
    , [medication_route_id]     [int] null
    , [priority]                [tinyint] not null
    , [frequency_schedule_id]   [int] null
    , [prn]                     [bit] not null
    , [point_in_time]           [bit] not null
    , [begin_datetime]          [datetimeoffset](7) not null
    , [end_datetime]            [datetimeoffset](7) null
    , [order_notes]             [nvarchar](max) null
    , [user_quick_list_item_id] [int] null
    , constraint [pk__patient_cart_orders__id] primary key clustered([id] asc));
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

alter table [dbo].[patient_cart_orders]
add constraint [fk__patient_cart_orders__patients] foreign key([patient_id]) references [dbo].[patients]([id]);
go

alter table [dbo].[patient_cart_orders]
add constraint [fk__patient_cart_orders__users] foreign key([user_id]) references [dbo].[users]([id]);
go

alter table [dbo].[patient_cart_orders]
add constraint [fk__patient_cart_orders__medication_routes] foreign key([medication_route_id]) references [dbo].[medication_routes]([id]);
go

alter table [dbo].[patient_cart_orders]
add constraint [fk__patient_cart_orders__medication_units] foreign key([medication_unit_id]) references [dbo].[medication_units]([id]);
go

alter table [dbo].[patient_cart_orders]
add constraint [fk__patient_cart_orders__user_quick_list_items] foreign key([user_quick_list_item_id]) references [dbo].[user_quick_list_items]([id]);
go

alter table [dbo].[patient_cart_orders]
add constraint [fk__patient_cart_orders__frequency_schedules] foreign key([frequency_schedule_id]) references [dbo].[frequency_schedules]([id]);
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
  , @level1name = N'patient_cart_orders'
  , @level2type = N'CONSTRAINT'
  , @level2name = N'pk__patient_cart_orders__id';
go

/***************
 Data Dictionary
    Table
***************/

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'This table contains: patient cart details'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'patient_cart_orders';
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
  , @level1name = N'patient_cart_orders'
  , @level2type = N'COLUMN'
  , @level2name = N'id';
go

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'Patient identifier, Foreign Key to patients table'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'patient_cart_orders'
  , @level2type = N'COLUMN'
  , @level2name = N'patient_id';
go

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'Person Idendifier that owns this cart record (Foreign Key to users table)'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'patient_cart_orders'
  , @level2type = N'COLUMN'
  , @level2name = N'user_id';
go

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'add_datetime'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'patient_cart_orders'
  , @level2type = N'COLUMN'
  , @level2name = N'add_datetime';
go

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'Drug NDC (National Drug Code)'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'patient_cart_orders'
  , @level2type = N'COLUMN'
  , @level2name = N'ndc';
go

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'External Vendor Drug Database Identifier
    FDB: MEDID (MED Medication ID (Stable ID))
    Multum: dnum
These 3 columns will be carried as a set ndc,drug_id,brand_name
while drug_id and brand_name are vendor specific concepts and can be derived from ndc number
this will aid in display and lookup performance.
'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'patient_cart_orders'
  , @level2type = N'COLUMN'
  , @level2name = N'drug_id';
go

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'brand_name'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'patient_cart_orders'
  , @level2type = N'COLUMN'
  , @level2name = N'brand_name';
go

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'Medication Dose: numeric portion of dose/medication_unit_id pair'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'patient_cart_orders'
  , @level2type = N'COLUMN'
  , @level2name = N'dose';
go

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'medication_route_id'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'patient_cart_orders'
  , @level2type = N'COLUMN'
  , @level2name = N'medication_route_id';
go

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'priority'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'patient_cart_orders'
  , @level2type = N'COLUMN'
  , @level2name = N'priority';
go

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'Medication Unit: unit portion of dose/medication_unit_id pair'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'patient_cart_orders'
  , @level2type = N'COLUMN'
  , @level2name = N'medication_unit_id';
go

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'Foreign Key to frequency_schedules table'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'patient_cart_orders'
  , @level2type = N'COLUMN'
  , @level2name = N'frequency_schedule_id';
go

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'prn'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'patient_cart_orders'
  , @level2type = N'COLUMN'
  , @level2name = N'prn';
go

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'point_in_time'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'patient_cart_orders'
  , @level2type = N'COLUMN'
  , @level2name = N'point_in_time';
go

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'begin_datetime'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'patient_cart_orders'
  , @level2type = N'COLUMN'
  , @level2name = N'begin_datetime';
go

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'end_datetime'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'patient_cart_orders'
  , @level2type = N'COLUMN'
  , @level2name = N'end_datetime';
go

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'order_notes'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'patient_cart_orders'
  , @level2type = N'COLUMN'
  , @level2name = N'order_notes';
go

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'user_quick_list_item_id: to be used for most used item calculation'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'patient_cart_orders'
  , @level2type = N'COLUMN'
  , @level2name = N'user_quick_list_item_id';
go