use emar;

drop table if exists [#frequency_schedules_import_template];

drop table if exists [#frequency_schedules];

drop table if exists [#frequency_interval_day_times_target];

drop table if exists [#frequency_interval_day_times_source];

create table [#frequency_interval_day_times_target]
    (
      [frequency_schedule_id] [int] null
    , [frequency_day_id]      [tinyint] null
    , [frequency_time]        [time](0) null);

create table [#frequency_interval_day_times_source]
    (
      [frequency_schedule_id] [int] null
    , [frequency_day_id]      [tinyint] null
    , [frequency_time]        [time](0) null);

create table [#frequency_schedules]
    (
      [id]                         [int] null
    , [site_id]                    [int] null
    , [name]                       [sysname] null
    , [point_in_time]              [bit] null
    , [frequency_type_id]          [int] null
    , [frequency_type_recurring]   [int] null
    , [frequency_interval]         [int] null
    , [frequency_interval_unit_id] [int] null
    , [interval_start_time]        [time](0) null
    , [interval_end_minutes]       [smallint] null
    , [notes]                      [nvarchar](1000) null
    , [is_active]                  [bit] null);

select cast(null as int) as [frequency_schedule_id]
     , cast(null as int) as [site_id]
     , cast(null as int) as [frequency_interval_unit_id]
     , cast(null as int) as [frequency_type_id]
     , [source].[site_name]
     , [source].[frequency_name]
     , [source].[point_in_time]
     , [source].[frequency_type_description]
     , [source].[frequency_type_recurring]
     , [source].[frequency_interval_name]
     , [source].[frequency_interval]
     , [source].[interval_start_time]
     , [source].[interval_end_minutes]
     , [source].[notes]
     , [source].[is_active]
     , [source].[interval_01]
     , [source].[time_01]
     , [source].[interval_02]
     , [source].[time_02]
     , [source].[interval_03]
     , [source].[time_03]
     , [source].[interval_04]
     , [source].[time_04]
     , [source].[interval_05]
     , [source].[time_05]
     , [source].[interval_06]
     , [source].[time_06]
     , [source].[interval_07]
     , [source].[time_07]
     , [source].[interval_08]
     , [source].[time_08]
     , [source].[interval_09]
     , [source].[time_09]
     , [source].[interval_10]
     , [source].[time_10]
     , [source].[interval_11]
     , [source].[time_11]
     , [source].[interval_12]
     , [source].[time_12]
     , [source].[interval_13]
     , [source].[time_13]
     , [source].[interval_14]
     , [source].[time_14]
     , [source].[interval_15]
     , [source].[time_15]
into [#frequency_schedules_import_template]
from   [dbo].[frequency_schedules_import_template] as [source];

/*********************************/
---testing
--select * from [#frequency_schedules_import_template] where [frequency_name] like 'Every 4 hours while awake  --  %';
--select * from [frequency_schedules] where [name] like 'Every 4 hours while awake  --  %' and site_id=16

--update [frequency_schedules_import_template] set [time_01]='12:00:00' where [frequency_name]='Continuous PAH';
--set rowcount 0
--update [frequency_schedules_import_template] set    
--    --[frequency_name] = 'Every 4 hours while awake  --  (4)'
--    [is_active]=1
--where  [frequency_name] = 'Every 4 hours while awake  --  (4)';

--delete [frequency_schedules_import_template] 
--where  [frequency_name] = 'Every 4 hours while awake  --  (2)';


---testing
/*********************************/

update [target] set [site_id]=[source].[id] 
from [#frequency_schedules_import_template] as [target] 
     inner join [dbo].[sites] as [source] on [source].[name]=[target].[site_name];

update [target] set    
    [frequency_schedule_id] = [source].[id]
from   [#frequency_schedules_import_template] as [target]
       inner join [dbo].[frequency_schedules] as [source] on [source].[name] = [target].[frequency_name]
                                                             and [source].[site_id] = [target].[site_id];

update [target] set    
    [frequency_type_id] = [source].[id]
from   [#frequency_schedules_import_template] as [target]
       inner join [dbo].[frequency_types] as [source] on [source].[name] = [target].[frequency_type_description];

update [target] set    
    [frequency_interval_unit_id] = [source].[id]
from   [#frequency_schedules_import_template] as [target]
       inner join [dbo].[frequency_interval_units] as [source] on [source].[name] = [target].[frequency_interval_name];

insert into [#frequency_schedules]
    ([id]
   , [site_id]
   , [name]
   , [point_in_time]
   , [frequency_type_id]
   , [frequency_type_recurring]
   , [frequency_interval]
   , [frequency_interval_unit_id]
   , [interval_start_time]
   , [interval_end_minutes]
   , [notes]
   , [is_active]
    )
select [source].[frequency_schedule_id]
     , [source].[site_id]
     , [source].[frequency_name]
     , [source].[point_in_time]
     , [source].[frequency_type_id]
     , [source].[frequency_type_recurring]
     , [source].[frequency_interval]
     , [source].[frequency_interval_unit_id]
     , [source].[interval_start_time]
     , [source].[interval_end_minutes]
     , [source].[notes]
     , [source].[is_active]
from   [#frequency_schedules_import_template] as [source];

begin transaction;

declare 
    @frequency_schedules_i int = 0
  , @frequency_schedules_u int = 0
  , @frequency_schedules_d int = 0
  , @frequency_interval_day_times_i int = 0
  , @frequency_interval_day_times_u int = 0
  , @frequency_interval_day_times_d int = 0

update [target] set    
    [site_id] = [source].[site_id]
  , [name] = [source].[name]
  , [point_in_time] = [source].[point_in_time]
  , [frequency_type_id] = [source].[frequency_type_id]
  , [frequency_type_recurring] = [source].[frequency_type_recurring]
  , [frequency_interval] = [source].[frequency_interval]
  , [frequency_interval_unit_id] = [source].[frequency_interval_unit_id]
  , [interval_start_time] = [source].[interval_start_time]
  , [interval_end_minutes] = [source].[interval_end_minutes]
  , [notes] = [source].[notes]
  , [is_active] = [source].[is_active]
from   [#frequency_schedules] as [source]
       inner join [dbo].[frequency_schedules] as [target] on [source].[id] = [target].[id]
  where [target].[site_id] <> [source].[site_id]
     or [target].[name] <> [source].[name]
     or [target].[point_in_time] <> [source].[point_in_time]
     or [target].[frequency_type_id] <> [source].[frequency_type_id]
     or [target].[frequency_type_recurring] <> [source].[frequency_type_recurring]
     or [target].[frequency_interval] <> [source].[frequency_interval]
     or [target].[frequency_interval_unit_id] <> [source].[frequency_interval_unit_id]
     or [target].[interval_start_time] <> [source].[interval_start_time]
     or [target].[interval_end_minutes] <> [source].[interval_end_minutes]
     or [target].[notes] <> [source].[notes]
     or [target].[is_active] <> [source].[is_active];

set @frequency_schedules_u=@@rowcount


;

insert into [dbo].[frequency_schedules]
    ([site_id]
   , [name]
   , [point_in_time]
   , [frequency_type_id]
   , [frequency_type_recurring]
   , [frequency_interval]
   , [frequency_interval_unit_id]
   , [interval_start_time]
   , [interval_end_minutes]
   , [notes]
   , [is_active]
    )
select [source].[site_id]
     , [source].[name]
     , [source].[point_in_time]
     , [source].[frequency_type_id]
     , [source].[frequency_type_recurring]
     , [source].[frequency_interval]
     , [source].[frequency_interval_unit_id]
     , [source].[interval_start_time]
     , [source].[interval_end_minutes]
     , [source].[notes]
     , [source].[is_active]
from   [#frequency_schedules] as [source]
where  [id] is null;
set @frequency_schedules_i=@@rowcount;

update [target] set    
    [id] = [source].[id]
from   [#frequency_schedules] as [target]
       inner join [dbo].[frequency_schedules] as [source] on [source].[name] = [target].[name]
                                                             and [source].[site_id] = [target].[site_id];

update [target] set    
    [frequency_schedule_id] = [source].[id]
from   [#frequency_schedules_import_template] as [target]
       inner join [dbo].[frequency_schedules] as [source] on [source].[name] = [target].[frequency_name]
                                                             and [source].[site_id] = [target].[site_id];

update [target] set    
    [is_active] = 0
from   [dbo].[frequency_schedules] as [target]
       left join [#frequency_schedules] as [source] on [target].[name] = [source].[name]
                                                       and [source].[site_id] = [target].[site_id]
where  [target].[site_id] in
(
    select distinct 
           [site_id]
    from [#frequency_schedules]
)
       and [source].[site_id] is null
       and [target].[is_active] <> 0;
set @frequency_schedules_d=@@rowcount;


insert into [#frequency_interval_day_times_source]
select 
[source].[frequency_schedule_id]
,[fd].[id] [frequency_day_id]
--,[source].[interval_01] [frequency_day]
,[source].[time_01] [frequency_time]
from(
select frequency_schedule_id,cast(null as int) [frequency_day_id],interval_01,time_01 from [#frequency_schedules_import_template] where interval_01 is not null union
select frequency_schedule_id,cast(null as int) [frequency_day_id],interval_02,time_02 from [#frequency_schedules_import_template] where interval_02 is not null union
select frequency_schedule_id,cast(null as int) [frequency_day_id],interval_03,time_03 from [#frequency_schedules_import_template] where interval_03 is not null union
select frequency_schedule_id,cast(null as int) [frequency_day_id],interval_04,time_04 from [#frequency_schedules_import_template] where interval_04 is not null union
select frequency_schedule_id,cast(null as int) [frequency_day_id],interval_05,time_05 from [#frequency_schedules_import_template] where interval_05 is not null union
select frequency_schedule_id,cast(null as int) [frequency_day_id],interval_06,time_06 from [#frequency_schedules_import_template] where interval_06 is not null union
select frequency_schedule_id,cast(null as int) [frequency_day_id],interval_07,time_07 from [#frequency_schedules_import_template] where interval_07 is not null union
select frequency_schedule_id,cast(null as int) [frequency_day_id],interval_08,time_08 from [#frequency_schedules_import_template] where interval_08 is not null union
select frequency_schedule_id,cast(null as int) [frequency_day_id],interval_09,time_09 from [#frequency_schedules_import_template] where interval_09 is not null union
select frequency_schedule_id,cast(null as int) [frequency_day_id],interval_10,time_10 from [#frequency_schedules_import_template] where interval_10 is not null union
select frequency_schedule_id,cast(null as int) [frequency_day_id],interval_11,time_11 from [#frequency_schedules_import_template] where interval_11 is not null union
select frequency_schedule_id,cast(null as int) [frequency_day_id],interval_12,time_12 from [#frequency_schedules_import_template] where interval_12 is not null union
select frequency_schedule_id,cast(null as int) [frequency_day_id],interval_13,time_13 from [#frequency_schedules_import_template] where interval_13 is not null union
select frequency_schedule_id,cast(null as int) [frequency_day_id],interval_14,time_14 from [#frequency_schedules_import_template] where interval_14 is not null union
select frequency_schedule_id,cast(null as int) [frequency_day_id],interval_15,time_15 from [#frequency_schedules_import_template] where interval_15 is not null
) [source]
inner join dbo.frequency_days [fd] on [fd].[name]=[source].[interval_01]

insert into [#frequency_interval_day_times_target]
select [frequency_schedule_id]
     , [frequency_day_id]
     , [frequency_time]
from     [#frequency_schedules] as [fs]
         inner join [dbo].[frequency_interval_day_times] as [fidt] on [fidt].[frequency_schedule_id] = [fs].[id]
union
select [frequency_schedule_id]
     , [frequency_day_id]
     , [frequency_time]
from   [#frequency_interval_day_times_source];

delete [target]
from [#frequency_interval_day_times_target] as [ref]
     inner join [frequency_interval_day_times] as [target] on [ref].[frequency_schedule_id] = [target].[frequency_schedule_id]
                                                              and [ref].[frequency_day_id] = [target].[frequency_day_id]
                                                              and isnull([ref].[frequency_time], '00:00:00') = isnull([target].[frequency_time], '00:00:00')
     left join [#frequency_interval_day_times_source] as [source] on [ref].[frequency_schedule_id] = [source].[frequency_schedule_id]
                                                                     and [ref].[frequency_day_id] = [source].[frequency_day_id]
                                                                     and isnull([ref].[frequency_time], '00:00:00') = isnull([source].[frequency_time], '00:00:00')
where  [source].[frequency_schedule_id] is null;
set @frequency_interval_day_times_d=@@rowcount;

insert into [dbo].[frequency_interval_day_times]
select [source].[frequency_schedule_id]
     , [source].[frequency_day_id]
     , [source].[frequency_time]
from   [#frequency_interval_day_times_target] as [ref]
       inner join [frequency_interval_day_times] as [target] on [ref].[frequency_schedule_id] = [target].[frequency_schedule_id]
                                                                and [ref].[frequency_day_id] = [target].[frequency_day_id]
                                                                and isnull([ref].[frequency_time], '00:00:00') = isnull([target].[frequency_time], '00:00:00')
       right join [#frequency_interval_day_times_source] as [source] on [ref].[frequency_schedule_id] = [source].[frequency_schedule_id]
                                                                        and [ref].[frequency_day_id] = [source].[frequency_day_id]
                                                                        and isnull([ref].[frequency_time], '00:00:00') = isnull([source].[frequency_time], '00:00:00')
where  [target].[frequency_schedule_id] is null;
set @frequency_interval_day_times_i=@@rowcount;



select 'frequency_schedules' [Table]
  , [insert] = @frequency_schedules_i         
  , [update] = @frequency_schedules_u         
  , [logical delete] = @frequency_schedules_d         

select 'frequency_interval_day_times' [Table]
  , [insert] = @frequency_interval_day_times_i
  , [update] = @frequency_interval_day_times_u
  , [delete] = @frequency_interval_day_times_d

go
commit transaction
