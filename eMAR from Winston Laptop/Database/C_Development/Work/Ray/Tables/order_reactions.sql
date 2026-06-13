create table [dbo].[order_reactions]
    (
      [id]                       bigint identity(1, 1) not null
    , [patient_allergy_id]       bigint not null
    , [patient_order_id]         bigint null
    , [patient_cart_order_id]    bigint null
    , [override_reason_id]       int null
    , [override_reason_user_id]  int null
    , [override_reason_datetime] datetimeoffset(7) null
    , constraint [pk__order_reactions__id] primary key clustered([id] asc));
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

alter table [dbo].[order_reactions]
add constraint [fk__order_reactions__override_reasons] foreign key([override_reason_id]) references [dbo].[override_reasons]([id]);

go

alter table [dbo].[order_reactions]
with check add constraint [fk__order_reactions__patient_allergies] foreign key([patient_allergy_id]) references [dbo].[patient_allergies]([id]) on delete cascade;

go

alter table [dbo].[order_reactions]
add constraint [fk__order_reactions__patient_cart_orders] foreign key([patient_cart_order_id]) references [dbo].[patient_cart_orders]([id]);

go

alter table [dbo].[order_reactions]
add constraint [fk__order_reactions__patient_orders] foreign key([patient_order_id]) references [dbo].[patient_orders]([id]);

go

alter table [dbo].[order_reactions]
add constraint [fk__order_reactions__users] foreign key([override_reason_user_id]) references [dbo].[users]([id]);

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
  , @level1name = N'order_reactions'
  , @level2type = N'CONSTRAINT'
  , @level2name = N'pk__order_reactions__id';
go

/***************
 Data Dictionary
    Table
***************/

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'This table contains: links to interactions with allergies'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'order_reactions';
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
  , @level1name = N'order_reactions'
  , @level2type = N'COLUMN'
  , @level2name = N'id';
go

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'Foreign Key to patient_allergies table'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'order_reactions'
  , @level2type = N'COLUMN'
  , @level2name = N'patient_allergy_id';
go

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'Foreign Key to patient_orders table'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'order_reactions'
  , @level2type = N'COLUMN'
  , @level2name = N'patient_order_id';
go

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'Foreign Key to patient_cart_orders table'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'order_reactions'
  , @level2type = N'COLUMN'
  , @level2name = N'patient_cart_order_id';
go

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'Foreign Key to override_reasons table'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'order_reactions'
  , @level2type = N'COLUMN'
  , @level2name = N'override_reason_id';
go

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'Override Reason User: Foreign Key to users table'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'order_reactions'
  , @level2type = N'COLUMN'
  , @level2name = N'override_reason_user_id';
go

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'Time the User Overrode the Reaction Reason'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'order_reactions'
  , @level2type = N'COLUMN'
  , @level2name = N'override_reason_datetime';
go