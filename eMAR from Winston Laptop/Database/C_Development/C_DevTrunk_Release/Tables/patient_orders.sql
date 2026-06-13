create table [dbo].[patient_orders]
    (
      [id]                            [bigint] identity(1, 1) not null
    , [patient_id]                    [bigint] not null
    , [add_user_id]                   [int] not null
    , [add_datetime]                  [datetimeoffset](7) not null
    , [order_physician_user_id]       [int] not null
    , [begin_datetime]                [datetimeoffset](7) not null
    , [end_datetime]                  [datetimeoffset](7) null
    , [dose]                          [decimal](12, 3) null
    , [medication_unit_id]            [int] null
    , [medication_route_id]           [int] null
    , [priority]                      [tinyint] not null
    , [frequency_schedule_id]         [int] null
    , [prn]                           [bit] not null
    , [point_in_time]                 [bit] not null
    , [order_status]                  [varchar](25) not null
    , [order_notes]                   [nvarchar](max) null
    , [medication_id]                 [int] not null
    , [antimicrobial_indication_id]   [int] null
    , [duration]                      [int] null
    , [duration_unit_id]              [int] null
    , [patient_problem_id]            [bigint] null
    , [antimicrobial_indication_text] [nvarchar](255) null
    , [pharmacy_verification_status]  [tinyint] null
    , [ndc]                           [varchar](11) null
    , [prn_indication]                [nvarchar](255) null
    , constraint [pk__patient_orders__id] primary key clustered([id] asc));
go

/********
 Defaults
********/
alter table [dbo].[patient_orders]
add constraint [df__patient_orders__pharmacy_verification_status] default ((0)) for [pharmacy_verification_status];
go
/**********
 Constraint
**********/

alter table [dbo].[patient_orders]
add constraint [cc__patient_orders__order_status] check(([order_status] = 'Deleted'
                                                         or [order_status] = 'Completed'
                                                         or [order_status] = 'Discontinued'
                                                         or [order_status] = 'PendingDiscontinue'
                                                         or [order_status] = 'OnHold'
                                                         or [order_status] = 'OnGoing'
                                                         or [order_status] = 'Cancelled'
                                                         or [order_status] = 'Pending')
                                                        and [order_status] is not null);

go

/*******
 Indexes
*******/

CREATE NONCLUSTERED INDEX [ix__patient_orders__patient_id] ON [dbo].[patient_orders]
    ([patient_id]);
GO

CREATE NONCLUSTERED INDEX [ix__patient_orders__begin_datetime_order_status] ON [dbo].[patient_orders]
    ([begin_datetime],[order_status])
    INCLUDE ([id],[patient_id],[medication_unit_id],[medication_route_id],[medication_id]);
GO

/***********
 Foreign Key
***********/

alter table [dbo].[patient_orders]
add constraint [fk__patient_orders__patients] foreign key([patient_id]) references [dbo].[patients]([id]);
go

alter table [dbo].[patient_orders]
add constraint [fk__patient_orders__users__add_user_id] foreign key([add_user_id]) references [dbo].[users]([id]);
go

alter table [dbo].[patient_orders]
add constraint [fk__patient_orders__users__order_physician_user_id] foreign key([order_physician_user_id]) references [dbo].[users]([id]);
go

alter table [dbo].[patient_orders]
add constraint [fk__patient_orders__medication_routes] foreign key([medication_route_id]) references [dbo].[medication_routes]([id]);
go

alter table [dbo].[patient_orders]
add constraint [fk__patient_orders__medication_units] foreign key([medication_unit_id]) references [dbo].[medication_units]([id]);
go

alter table [dbo].[patient_orders]
add constraint [fk__patient_orders__frequency_schedules] foreign key([frequency_schedule_id]) references [dbo].[frequency_schedules]([id]);
go

alter table [dbo].[patient_orders]
add constraint [fk__patient_orders__medications] foreign key([medication_id]) references [dbo].[medications]([id]);
go

alter table [dbo].[patient_orders]
add constraint [fk__patient_orders__duration_units] foreign key([duration_unit_id]) references [dbo].[duration_units]([id]);
go

alter table [dbo].[patient_orders]
add constraint [fk__patient_orders__patient_problems] foreign key([patient_problem_id]) references [dbo].[patient_problems]([id]);
go

alter table [dbo].[patient_orders]
add constraint [fk__patient_orders__antimicrobial_indications] foreign key([antimicrobial_indication_id]) references [dbo].[antimicrobial_indications]([id])
on delete set null;
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
  , @level1name = N'patient_orders'
  , @level2type = N'CONSTRAINT'
  , @level2name = N'pk__patient_orders__id';
go

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'Column Constraint to enforce a value set.'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'patient_orders'
  , @level2type = N'CONSTRAINT'
  , @level2name = N'cc__patient_orders__order_status';
go

/***************
 Data Dictionary
    Table
***************/

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'This table contains: patient orders'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'patient_orders';
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
  , @level1name = N'patient_orders'
  , @level2type = N'COLUMN'
  , @level2name = N'id';
go

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'Patient identifier, Foreign Key to patients table'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'patient_orders'
  , @level2type = N'COLUMN'
  , @level2name = N'patient_id';
go

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'Person identifier that created this record (Foreign Key to users table)'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'patient_orders'
  , @level2type = N'COLUMN'
  , @level2name = N'add_user_id';
go

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'add_datetime'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'patient_orders'
  , @level2type = N'COLUMN'
  , @level2name = N'add_datetime';
go

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'Ordering Physician identifier that ordered this record (Foreign Key to users table)'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'patient_orders'
  , @level2type = N'COLUMN'
  , @level2name = N'order_physician_user_id';
go

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'Medication Dose: numeric portion of dose/medication_unit_id pair'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'patient_orders'
  , @level2type = N'COLUMN'
  , @level2name = N'dose';
go

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'Route of administration; Foreign Key to medication_routes table'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'patient_orders'
  , @level2type = N'COLUMN'
  , @level2name = N'medication_route_id';
go

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'priority'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'patient_orders'
  , @level2type = N'COLUMN'
  , @level2name = N'priority';
go

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'Medication Unit: unit portion of dose/medication_unit_id pair'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'patient_orders'
  , @level2type = N'COLUMN'
  , @level2name = N'medication_unit_id';
go

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'Foreign Key to frequency_schedules table'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'patient_orders'
  , @level2type = N'COLUMN'
  , @level2name = N'frequency_schedule_id';
go

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'prn'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'patient_orders'
  , @level2type = N'COLUMN'
  , @level2name = N'prn';
go

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'point_in_time'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'patient_orders'
  , @level2type = N'COLUMN'
  , @level2name = N'point_in_time';
go

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'Possible values: Pending, Cancelled, OnGoing, OnHold, PendingDiscontinue, Discontinued, Completed, Deleted'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'patient_orders'
  , @level2type = N'COLUMN'
  , @level2name = N'order_status';
go

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'begin_datetime'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'patient_orders'
  , @level2type = N'COLUMN'
  , @level2name = N'begin_datetime';
go

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'end_datetime'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'patient_orders'
  , @level2type = N'COLUMN'
  , @level2name = N'end_datetime';
go

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'order_notes'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'patient_orders'
  , @level2type = N'COLUMN'
  , @level2name = N'order_notes';
go

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'Medications identifier, Foreign Key to medications table'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'patient_orders'
  , @level2type = N'COLUMN'
  , @level2name = N'medication_id';
go

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'antimicrobial_indication_id rational, Foreign Key to antimicrobial_indications table'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'patient_orders'
  , @level2type = N'COLUMN'
  , @level2name = N'antimicrobial_indication_id';
go

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'Length of duration based on duration_unit_id'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'patient_orders'
  , @level2type = N'COLUMN'
  , @level2name = N'duration';
go

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'Duration Unit identifier, Foreign Key to duration_unit table'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'patient_orders'
  , @level2type = N'COLUMN'
  , @level2name = N'duration_unit_id';
go

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'Patient Problem identifier, Foreign Key to patient_problems table'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'patient_orders'
  , @level2type = N'COLUMN'
  , @level2name = N'patient_problem_id';
go

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'antimicrobial indication text'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'patient_orders'
  , @level2type = N'COLUMN'
  , @level2name = N'antimicrobial_indication_text';
go

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'Possible values: NULL or 0 for ED order, 1 for requiring verification, 2 for verified'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'patient_orders'
  , @level2type = N'COLUMN'
  , @level2name = N'pharmacy_verification_status';
go

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'National Drug Code'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'patient_orders'
  , @level2type = N'COLUMN'
  , @level2name = N'ndc';
go

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'prn indication text'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'patient_orders'
  , @level2type = N'COLUMN'
  , @level2name = N'prn_indication';
go