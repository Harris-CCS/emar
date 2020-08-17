create table [dbo].[frequency_schedules]
    (
      [id]                         [int] not null identity(1,1) 
    , [site_id]                    [int] NOT null
    , [name]                       [sysname] NOT null
    , [point_in_time]              [bit] NOT null
    , [frequency_type_id]          [int] NOT null
    , [frequency_type_recuring]    [int] NOT null
    , [frequency_interval]         [int] NOT null
    , [frequency_interval_unit_id] [int] NOT null
    , [interval_start_time]        [time](0) not null
    , [interval_end_minutes]       [smallint] not null
    , [notes]                      [varchar](1000) null
    , primary key clustered([id] asc));

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

alter table [dbo].[frequency_schedules]
add constraint [fk__frequency_schedules__sites] foreign key([site_id]) references [dbo].[sites]([id]);
go

alter table [dbo].[frequency_schedules]
add constraint [fk__frequency_schedules__frequency_types] foreign key([frequency_type_id]) references [dbo].[frequency_types]([id]);
go

alter table [dbo].[frequency_schedules]
add constraint [fk__frequency_schedules__frequency_interval_units] foreign key([frequency_interval_unit_id]) references [dbo].[frequency_interval_units]([id]);
go

/***************
 Data Dictionary
    Defaults
***************/
/***************
 Data Dictionary
    Indexes
***************/
/***************
 Data Dictionary
    Table
***************/

execute [sys].[sp_addextendedproperty]
    @name = N'MS_Description'
  , @value = N'Table contains the master frequency schedule record.'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'frequency_schedules';
go

/***************
 Data Dictionary
    Columns
***************/


