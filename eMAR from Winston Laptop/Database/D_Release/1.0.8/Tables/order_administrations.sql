create table [dbo].[order_administrations]
    (
      [id]                                [bigint] identity(1, 1) not null
    , [patient_order_id]                  [bigint] not null
    , [point_in_time]                     [bit] not null
    , [on_hold]                           [bit] not null
    , [missed_dose]                       [bit] not null
    , [administration_scheduled_datetime] [datetimeoffset](7) not null
    , [administration_system_datetime]    [datetimeoffset](7) null
    , [administering_user_id]             [int] null
    , [administration_datetime]           [datetimeoffset](7) null
    , [stop_scheduled_datetime]           [datetimeoffset](7) null
    , [stop_input_datetime]               [datetimeoffset](7) null
    , [stop_user_id]                      [int] null
    , [stop_datetime]                     [datetimeoffset](7) null
    , [acknowledge_user_id]               [int] null
    , [acknowledge_datetime]              [datetimeoffset](7) null
    , constraint [pk__order_administrations__id] primary key clustered([id] asc));
go

/********
 Defaults
********/
/*******
 Indexes
*******/

create nonclustered index [ix__order_administrations__patient_order_id__administration_datetime__administration_scheduled_datetime] on [dbo].[order_administrations]
    ([patient_order_id] asc, [administration_datetime] asc, [administration_scheduled_datetime] asc)
      include
    ([id]);
go

CREATE NONCLUSTERED INDEX [ix__order_administrations__patient_order_id] ON [dbo].[order_administrations]
(
	[patient_order_id] ASC
)
INCLUDE([id],[point_in_time],[on_hold],[missed_dose],[administration_scheduled_datetime],[administration_system_datetime],[administering_user_id],[administration_datetime],[stop_scheduled_datetime],[stop_input_datetime],[stop_user_id],[stop_datetime],[acknowledge_user_id],[acknowledge_datetime]);
GO
/***********
 Foreign Key
***********/

alter table [dbo].[order_administrations]
add constraint [fk__order_administrations__patient_orders] foreign key([patient_order_id]) references [dbo].[patient_orders]([id]);
go

alter table [dbo].[order_administrations]
add constraint [fk__order_administrations__patient_orders__administering_user_id] foreign key([administering_user_id]) references [dbo].[users]([id]);
go

alter table [dbo].[order_administrations]
add constraint [fk__order_administrations__patient_orders__acknowledge_user_id] foreign key([acknowledge_user_id]) references [dbo].[users]([id]);
go

alter table [dbo].[order_administrations]
add constraint [fk__order_administrations__patient_orders__stop_user_id] foreign key([stop_user_id]) references [dbo].[users]([id]);
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
  , @value = N'Primary Key Constraint'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'order_administrations'
  , @level2type = N'CONSTRAINT'
  , @level2name = N'pk__order_administrations__id';
go

/***************
 Data Dictionary
    Table
***************/

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'This table contains: order administrations'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'order_administrations';
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
  , @level1name = N'order_administrations'
  , @level2type = N'COLUMN'
  , @level2name = N'id';
go

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'Foreign Key to patient_order_id table'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'order_administrations'
  , @level2type = N'COLUMN'
  , @level2name = N'patient_order_id';
go

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'point_in_time'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'order_administrations'
  , @level2type = N'COLUMN'
  , @level2name = N'point_in_time';
go

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'on_hold'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'order_administrations'
  , @level2type = N'COLUMN'
  , @level2name = N'on_hold';
go

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'missed_dose'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'order_administrations'
  , @level2type = N'COLUMN'
  , @level2name = N'missed_dose';
go

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'administration_scheduled_datetime'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'order_administrations'
  , @level2type = N'COLUMN'
  , @level2name = N'administration_scheduled_datetime';
go

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'administration_input_datetime'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'order_administrations'
  , @level2type = N'COLUMN'
  , @level2name = 'administration_system_datetime';
go

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'Person Identifier that administered this record (Foreign Key to users table)'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'order_administrations'
  , @level2type = N'COLUMN'
  , @level2name = N'administering_user_id';
go

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'administration_datetime'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'order_administrations'
  , @level2type = N'COLUMN'
  , @level2name = N'administration_datetime';
go

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'stop_scheduled_datetime'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'order_administrations'
  , @level2type = N'COLUMN'
  , @level2name = N'stop_scheduled_datetime';
go

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'stop_input_datetime'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'order_administrations'
  , @level2type = N'COLUMN'
  , @level2name = N'stop_input_datetime';
go

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'Person identifier that cancelled this record (Foreign Key to users table)'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'order_administrations'
  , @level2type = N'COLUMN'
  , @level2name = N'stop_user_id';
go

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'stop_datetime'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'order_administrations'
  , @level2type = N'COLUMN'
  , @level2name = N'stop_datetime';
go

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'Person identifier that acknowledged this record (Foreign Key to users table)'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'order_administrations'
  , @level2type = N'COLUMN'
  , @level2name = N'acknowledge_user_id';
go

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'acknowledge_datetime'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'order_administrations'
  , @level2type = N'COLUMN'
  , @level2name = N'acknowledge_datetime';
go