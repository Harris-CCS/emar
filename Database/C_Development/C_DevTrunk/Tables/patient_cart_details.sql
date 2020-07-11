create table [dbo].[patient_cart_details]
    (
      [id]                  [bigint] identity(1, 1) not null
    , [patient_cart_id]     [bigint] not null
    , [add_datetime]        [datetimeoffset](7) not null
    , [ndc]                 [varchar](32) null
    , [drug_id]             [varchar](32) not null
    , [brand_name]          [nvarchar](255) not null
    , [dose]                [decimal](11, 2) null
    , [dose_unit]           [varchar](20) null
    , [medication_route_id] [int] null
    , [priority]            [tinyint] not null
    , [frequency_id]        [int] null
    , [prn]                 [bit] not null
    , [point_in_time]       [bit] not null
    , [order_status]        [varchar](10) not null
    , [begin_datetime]      [datetimeoffset](7) not null
    , [end_datetime]        [datetimeoffset](7) null
    , [order_notes]         [nvarchar](max) null
    , constraint [pk__patient_cart_details__id] primary key clustered([id] asc));
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

alter table [dbo].[patient_cart_details]
add constraint [fk__patient_cart_details__medication_routes] foreign key([medication_route_id]) references [dbo].[medication_routes]([id]);
go

alter table [dbo].[patient_cart_details]
add constraint [fk__patient_cart_details__patient_carts] foreign key([patient_cart_id]) references [dbo].[patient_carts]([id]);
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
  , @level1name = N'patient_cart_details'
  , @level2type = N'CONSTRAINT'
  , @level2name = N'pk__patient_cart_details__id';
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
  , @level1name = N'patient_cart_details';
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
  , @level1name = N'patient_cart_details'
  , @level2type = N'COLUMN'
  , @level2name = N'id';
go

execute [sys].[sp_addextendedproperty]
    @name = N'MS_Description'
  , @value = N'patient_cart_id'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'patient_cart_details'
  , @level2type = N'COLUMN'
  , @level2name = N'patient_cart_id';
go

execute [sys].[sp_addextendedproperty]
    @name = N'MS_Description'
  , @value = N'add_datetime'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'patient_cart_details'
  , @level2type = N'COLUMN'
  , @level2name = N'add_datetime';
go

execute [sys].[sp_addextendedproperty]
    @name = N'MS_Description'
  , @value = N'ndc'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'patient_cart_details'
  , @level2type = N'COLUMN'
  , @level2name = N'ndc';
go

execute [sys].[sp_addextendedproperty]
    @name = N'MS_Description'
  , @value = N'drug_id'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'patient_cart_details'
  , @level2type = N'COLUMN'
  , @level2name = N'drug_id';
go

execute [sys].[sp_addextendedproperty]
    @name = N'MS_Description'
  , @value = N'brand_name'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'patient_cart_details'
  , @level2type = N'COLUMN'
  , @level2name = N'brand_name';
go

execute [sys].[sp_addextendedproperty]
    @name = N'MS_Description'
  , @value = N'Medication Dose: numeric portion of dose/dose_unit pair'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'patient_cart_details'
  , @level2type = N'COLUMN'
  , @level2name = N'dose';
go

execute [sys].[sp_addextendedproperty]
    @name = N'MS_Description'
  , @value = N'medication_route_id'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'patient_cart_details'
  , @level2type = N'COLUMN'
  , @level2name = N'medication_route_id';
go

execute [sys].[sp_addextendedproperty]
    @name = N'MS_Description'
  , @value = N'priority'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'patient_cart_details'
  , @level2type = N'COLUMN'
  , @level2name = N'priority';
go

execute [sys].[sp_addextendedproperty]
    @name = N'MS_Description'
  , @value = N'Medication Unit: unit portion of dose/dose_unit pair'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'patient_cart_details'
  , @level2type = N'COLUMN'
  , @level2name = N'dose_unit';
go

execute [sys].[sp_addextendedproperty]
    @name = N'MS_Description'
  , @value = N'frequency_id'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'patient_cart_details'
  , @level2type = N'COLUMN'
  , @level2name = N'frequency_id';
go

execute [sys].[sp_addextendedproperty]
    @name = N'MS_Description'
  , @value = N'prn'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'patient_cart_details'
  , @level2type = N'COLUMN'
  , @level2name = N'prn';
go

execute [sys].[sp_addextendedproperty]
    @name = N'MS_Description'
  , @value = N'point_in_time'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'patient_cart_details'
  , @level2type = N'COLUMN'
  , @level2name = N'point_in_time';
go

execute [sys].[sp_addextendedproperty]
    @name = N'MS_Description'
  , @value = N'order_status'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'patient_cart_details'
  , @level2type = N'COLUMN'
  , @level2name = N'order_status';
go

execute [sys].[sp_addextendedproperty]
    @name = N'MS_Description'
  , @value = N'begin_datetime'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'patient_cart_details'
  , @level2type = N'COLUMN'
  , @level2name = N'begin_datetime';
go

execute [sys].[sp_addextendedproperty]
    @name = N'MS_Description'
  , @value = N'end_datetime'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'patient_cart_details'
  , @level2type = N'COLUMN'
  , @level2name = N'end_datetime';
go

execute [sys].[sp_addextendedproperty]
    @name = N'MS_Description'
  , @value = N'order_notes'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'patient_cart_details'
  , @level2type = N'COLUMN'
  , @level2name = N'order_notes';
go