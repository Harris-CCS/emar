create table [dbo].[order_interactions]
    (
      [id]                         [bigint] identity(1, 1) not null
    , [medication_interaction_id]  [bigint] not null
    , [patient_order_id]           [bigint] null
    , [patient_cart_order_id]      [bigint] null
    , [patient_home_medication_id] [bigint] null
    , [drug_num]                   [tinyint] not null
    , constraint [pk__order_interactions__id] primary key clustered([id] asc));
go

/********
 Defaults
********/
/*******
 Indexes
*******/

create unique index [IX_order_interactions_medication_interaction_id__drug_num] on [dbo].[order_interactions]
    ([medication_interaction_id], [drug_num]);
go

/***********
 Foreign Key
***********/

alter table [dbo].[order_interactions]
add constraint [fk__order_interactions__medication_interactions] foreign key([medication_interaction_id]) references [dbo].[medication_interactions]([id]);
go

alter table [dbo].[order_interactions]
add constraint [fk__order_interactions__patient_orders] foreign key([patient_order_id]) references [dbo].[patient_orders]([id]);
go

alter table [dbo].[order_interactions]
add constraint [fk__order_interactions__patient_cart_orders] foreign key([patient_cart_order_id]) references [dbo].[patient_cart_orders]([id]);
go

alter table [dbo].[order_interactions]
add constraint [fk__order_interactions__patient_home_medications] foreign key([patient_home_medication_id]) references [dbo].[patient_home_medications]([id]);
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
  , @level1name = N'order_interactions'
  , @level2type = N'CONSTRAINT'
  , @level2name = N'pk__order_interactions__id';
go

/***************
 Data Dictionary
    Table
***************/

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'This table contains: links to interactions with other medications'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'order_interactions';
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
  , @level1name = N'order_interactions'
  , @level2type = N'COLUMN'
  , @level2name = N'id';
go

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'Foreign Key to medication_interactions table'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'order_interactions'
  , @level2type = N'COLUMN'
  , @level2name = N'medication_interaction_id';
go

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'Foreign Key to patient_order_id table'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'order_interactions'
  , @level2type = N'COLUMN'
  , @level2name = N'patient_order_id';
go

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'Foreign Key to patient_cart_orders table'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'order_interactions'
  , @level2type = N'COLUMN'
  , @level2name = N'patient_cart_order_id';
go

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'Foreign Key to patient_home_medication_id table'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'order_interactions'
  , @level2type = N'COLUMN'
  , @level2name = N'patient_home_medication_id';
go

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'drug_num'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'order_interactions'
  , @level2type = N'COLUMN'
  , @level2name = N'drug_num';
go