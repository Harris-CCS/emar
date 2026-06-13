create table [dbo].[devices]
    (
      [id]               [int] identity(1, 1) not null
    , [site_id]          [int] not null
    , [address]          [nvarchar](50) null
    , [description]      [nvarchar](50) not null
    , [is_active]        [bit] not null
    , [print_queue_name] [varchar](80) null
    , [tray]             [char](1) null
    , [device_type]      [char](1) not null
    , [pcl_type]         [char](1) null);
go
/********
 Defaults
********/
/*****************
 Unique constraint
*****************/

alter table [dbo].[devices]
add constraint [pk__devices__id] primary key clustered([id] asc);
go

/*******
 Indexes
*******/

create unique nonclustered index [ix__devices__site_id__description] on [dbo].[devices]
    ([site_id] asc, [description] asc) 
go

/***********
 Foreign Key
***********/

alter table [dbo].[devices]
add constraint [fk__devices__sites] foreign key([site_id]) references [dbo].[sites]([id]);
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
  , @level1name = N'devices'
  , @level2type = N'CONSTRAINT'
  , @level2name = N'pk__devices__id';
go

/***************
 Data Dictionary
    Table
***************/

execute [sys].[sp_addextendedproperty]
    @name = N'MS_Description'
  , @value = N'This table contains: devices'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'devices';
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
  , @level1name = N'devices'
  , @level2type = N'COLUMN'
  , @level2name = N'id';
go

execute [sys].[sp_addextendedproperty]
    @name = N'MS_Description'
  , @value = N'Hospital identifier foreign key to site table'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'devices'
  , @level2type = N'COLUMN'
  , @level2name = N'site_id';
go

execute [sys].[sp_addextendedproperty]
    @name = N'MS_Description'
  , @value = N'Address as dotted IP or network share'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'devices'
  , @level2type = N'COLUMN'
  , @level2name = N'address';
go

execute [sys].[sp_addextendedproperty]
    @name = N'MS_Description'
  , @value = N'Device description'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'devices'
  , @level2type = N'COLUMN'
  , @level2name = N'description';
go

execute [sys].[sp_addextendedproperty]
    @name = N'MS_Description'
  , @value = N'is_active 1=True 0=False'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'devices'
  , @level2type = N'COLUMN'
  , @level2name = N'is_active';
go

execute [sys].[sp_addextendedproperty]
    @name = N'MS_Description'
  , @value = N'Print queue name'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'devices'
  , @level2type = N'COLUMN'
  , @level2name = N'print_queue_name';
go

execute [sys].[sp_addextendedproperty]
    @name = N'MS_Description'
  , @value = N'Values: D=default, M=main, P=multi-purpose, C=cassette 1, S=Cassette 2'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'devices'
  , @level2type = N'COLUMN'
  , @level2name = N'tray';
go

execute [sys].[sp_addextendedproperty]
    @name = N'MS_Description'
  , @value = N'
  I = IP printer, 
  P = server printer, 
  W = windows shared printer, 
  A = avery 5160 labels, 
  E = eltron labels, 
  L = wrist badge,
  Z = zebra labels, 
  C = camera, 
  S = scanner (Axis),
  D = PDF Delivery O=Scanner (Network)'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'devices'
  , @level2type = N'COLUMN'
  , @level2name = N'device_type';
go

execute [sys].[sp_addextendedproperty]
    @name = N'MS_Description'
  , @value = N'P=postscript, C=PCL'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'devices'
  , @level2type = N'COLUMN'
  , @level2name = N'pcl_type';
go