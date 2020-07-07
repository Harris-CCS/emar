create table [dbo].[patient_cart_details]
    (
      [id]                  [int] not null
    , [patient_cart_id]     [int] not null
    , [add_datetime]        [datetimeoffset](7) not null
    , [ndc]                 [varchar](32) null
    , [drug_id]             [varchar](32) not null
    , [brand_name]          [varchar](255) not null
    , [dose]                [varchar](40) null
    , [medication_route_id] [int] null
    , [priority]            [tinyint] not null
    , [unit]                [varchar](40) null
    , [frequency_id]        [int] null
    , [prn]                 [bit] not null
    , [point_in_time]       [bit] not null
    , [order_status]        [varchar](10) not null
    , [begin_datetime]      [datetimeoffset](7) not null
    , [end_datetime]        [datetimeoffset](7) null
    , [order_notes]         [varchar](max) null
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

exec [sys].[sp_addextendedproperty]
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

exec [sys].[sp_addextendedproperty]
    @name = N'MS_Description'
  , @value = N'Auto increment table ID'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'patient_cart_details'
  , @level2type = N'COLUMN'
  , @level2name = N'id';
go

exec [sys].[sp_addextendedproperty]
    @name = N'MS_Description'
  , @value = N'patient_cart_id'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'patient_cart_details'
  , @level2type = N'COLUMN'
  , @level2name = N'patient_cart_id';
go

exec [sys].[sp_addextendedproperty]
    @name = N'MS_Description'
  , @value = N'add_datetime'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'patient_cart_details'
  , @level2type = N'COLUMN'
  , @level2name = N'add_datetime';
go

exec [sys].[sp_addextendedproperty]
    @name = N'MS_Description'
  , @value = N'ndc'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'patient_cart_details'
  , @level2type = N'COLUMN'
  , @level2name = N'ndc';
go

exec [sys].[sp_addextendedproperty]
    @name = N'MS_Description'
  , @value = N'drug_id'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'patient_cart_details'
  , @level2type = N'COLUMN'
  , @level2name = N'drug_id';
go

exec [sys].[sp_addextendedproperty]
    @name = N'MS_Description'
  , @value = N'brand_name'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'patient_cart_details'
  , @level2type = N'COLUMN'
  , @level2name = N'brand_name';
go

exec [sys].[sp_addextendedproperty]
    @name = N'MS_Description'
  , @value = N'dose'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'patient_cart_details'
  , @level2type = N'COLUMN'
  , @level2name = N'dose';
go

exec [sys].[sp_addextendedproperty]
    @name = N'MS_Description'
  , @value = N'medication_route_id'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'patient_cart_details'
  , @level2type = N'COLUMN'
  , @level2name = N'medication_route_id';
go

exec [sys].[sp_addextendedproperty]
    @name = N'MS_Description'
  , @value = N'priority'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'patient_cart_details'
  , @level2type = N'COLUMN'
  , @level2name = N'priority';
go

exec [sys].[sp_addextendedproperty]
    @name = N'MS_Description'
  , @value = N'unit'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'patient_cart_details'
  , @level2type = N'COLUMN'
  , @level2name = N'unit';
go

exec [sys].[sp_addextendedproperty]
    @name = N'MS_Description'
  , @value = N'frequency_id'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'patient_cart_details'
  , @level2type = N'COLUMN'
  , @level2name = N'frequency_id';
go

exec [sys].[sp_addextendedproperty]
    @name = N'MS_Description'
  , @value = N'prn'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'patient_cart_details'
  , @level2type = N'COLUMN'
  , @level2name = N'prn';
go

exec [sys].[sp_addextendedproperty]
    @name = N'MS_Description'
  , @value = N'point_in_time'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'patient_cart_details'
  , @level2type = N'COLUMN'
  , @level2name = N'point_in_time';
go

exec [sys].[sp_addextendedproperty]
    @name = N'MS_Description'
  , @value = N'order_status'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'patient_cart_details'
  , @level2type = N'COLUMN'
  , @level2name = N'order_status';
go

exec [sys].[sp_addextendedproperty]
    @name = N'MS_Description'
  , @value = N'begin_datetime'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'patient_cart_details'
  , @level2type = N'COLUMN'
  , @level2name = N'begin_datetime';
go

exec [sys].[sp_addextendedproperty]
    @name = N'MS_Description'
  , @value = N'end_datetime'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'patient_cart_details'
  , @level2type = N'COLUMN'
  , @level2name = N'end_datetime';
go

exec [sys].[sp_addextendedproperty]
    @name = N'MS_Description'
  , @value = N'order_notes'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'patient_cart_details'
  , @level2type = N'COLUMN'
  , @level2name = N'order_notes';
go