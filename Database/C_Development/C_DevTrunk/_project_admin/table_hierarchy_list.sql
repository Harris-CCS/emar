use emar_clean;
go

/********************************************
Steps to add new table
SEE: READ_ME.TXT
********************************************/
set nocount on;

drop table if exists [#table_order];

drop table if exists [#table_exclude];

create table [#table_exclude]
    (
      [table_name] sysname);

insert into [#table_exclude]
values
    ('__RefactorLog'),
    ('sysdiagrams'),
    ('external_ids'),
    ('table_scope_documentation');

create table [#table_order]
    (
      [schema_name]     sysname
    , [table_name]      sysname
    , [category]        sysname
);
-- Input Tables That have completed export scripts here
insert into [#table_order] values
    --~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
    ('dbo','fdb_allergy_name','global_data'),
    ('dbo','fdb_brand_name','global_data'),
    ('dbo','fdb_ndc_info','global_data'),
    ('dbo','options','global_data'),
    ('dbo','frequency_calendar','global_data'),
    ('dbo','frequency_days','global_data'),
    ('dbo','frequency_interval_units','global_data'),
    ('dbo','frequency_minutes','global_data'),
    ('dbo','frequency_types','global_data'),
    --~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
    ('dbo','patient_allergies','phi_data'),
    ('dbo','patient_home_medications','phi_data'),
    ('dbo','patient_indicators','phi_data'),
    ('dbo','patient_orders','phi_data'),
    ('dbo','patients','phi_data'),
    --~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
    ('dbo','antimicrobial_indication_items','site_data'),
    ('dbo','antimicrobial_indications','site_data'),
    ('dbo','frequency_schedules','site_data'),
    ('dbo','frequency_interval_day_times','site_data'),
    ('dbo','group_list_items','site_data'),
    ('dbo','medication_routes','site_data'),
    ('dbo','medication_units','site_data'),
    ('dbo','override_reasons','site_data'),
    ('dbo','site_code_shares','site_data'),
    ('dbo','site_formulary','site_data'),
    ('dbo','site_formulary_match','site_data'),
    ('dbo','site_options','site_data'),
    ('dbo','sites','site_data'),
    --~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
    ('dbo','user_quick_list_items','user_data'),
    ('dbo','users','user_data');
    --~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

go
/********************************************************************************************************************************************
-- https://blog.sqlauthority.com/2015/04/16/sql-server-walking-the-table-hierarchy-in-microsoft-sql-server-database-notes-from-the-field-076/
-- ==========================================================================
-- Description: Get the load levels by tracing foreign keys in the database.
-- License: Creative Commons (Free / Public Domain)
-- Rights: This work (Linchpin People LLC Database Load Levels Function,
-- by W. Kevin Hazzard), identified by Linchpin People LLC, is
-- free of known copyright restrictions.
-- Warranties: This code comes with no implicit or explicit warranties.
-- Linchpin People LLC and W. Kevin Hazzard are not responsible
-- for the use of this work or its derivatives.
-- ==========================================================================
********************************************************************************************************************************************/

create or alter function [dbo].[load_levels]() returns @results table
    (
      [schema_name] sysname
    , [table_name]  sysname
    , [load_level]  int) as begin
                            with [key_info]
                                 as (select [parent_object_id] as     [from_table_id]
                                          , [referenced_object_id] as [to_table_id]
                                     from   [sys].[foreign_keys]
                                     where  [parent_object_id] <> [referenced_object_id]
                                            and [is_disabled] = 0),
                                 [level_info]
                                 as (select -- anchor part
                                     [st].[object_id] as [to_table_id]
                                   , 0 as                [load_level]
                                     from   [sys].[tables] as [st]
                                            left outer join [key_info] as [ki] on [st].[object_id] = [ki].[from_table_id]
                                     where [ki].[from_table_id] is null
                                     union all
                                     select -- recursive part
                                     [ki].[from_table_id]
                                   , [li].[load_level] + 1
                                     from [key_info] as [ki]
                                          inner join [level_info] as [li] on [ki].[to_table_id] = [li].[to_table_id])
                                 insert into @results
                                 select object_schema_name([to_table_id]) as [schema_name]
                                      , object_name([to_table_id]) as        [table_name]
                                      , max([load_level]) as                 [load_level]
                                 from   [level_info]
                                 group by [to_table_id];
                                     return;
                                 end;

go
/*
--comments for delete order
print '--comments for delete order'
select 'LVL: '+right('000'+cast(load_level as varchar(3)),3)+' SEQ: '+right('000'+cast(row_number() over(partition by [load_level]
          order by [table_name])  as varchar(3)),3)+' TBL: '+[schema_name]+'.'+[table_name]+''
from
(
    select *
    from     [dbo].[load_levels]()
    where   [table_name] not in('__RefactorLog', 'sysdiagrams', 'external_ids')
    union
    select 'dbo'
         , 'external_ids'
         , 99
) as [lst]
order by [load_level] desc
       , [schema_name]
       , [table_name];

set nocount on;
select    '    delete [' + [schema_name] + '].[' + [table_name] + '];'
from
(
    select *
    from     [dbo].[load_levels]()
    where   [table_name] not in('__RefactorLog', 'sysdiagrams', 'external_ids')
    union
    select 'dbo'
         , 'external_ids'
         , 99
) as [lst]
order by [load_level] desc
       , [schema_name]
       , [table_name];

select    '    dbcc checkident(''[' + [schema_name] + '].[' + [table_name] + ']'',reseed,1) with no_infomsgs;'
from
(
    select *
    from     [dbo].[load_levels]()
    where   [table_name] not in('__RefactorLog', 'sysdiagrams', 'external_ids')
    --union
    --select 'dbo'
    --     , 'external_ids'
    --     , 99
) as [lst]
order by [load_level] desc
       , [schema_name]
       , [table_name];
*/

--comments for insert order list
select 'LVL: '+right('000'+cast(load_level as varchar(3)),3)+' SEQ: '+right('000'+cast(row_number() over(partition by [load_level]
          order by [table_name])  as varchar(3)),3)+' TBL: '+[schema_name]+'.'+[table_name]+''
as [comments for Script.PostDeployment.sql]
from
(
    select *
    from     [dbo].[load_levels]()
    where   [table_name] not in(select table_name From [#table_exclude])
    --union
    --select 'dbo'
    --     , 'external_ids'
    --     , 99
) as [lst]
order by [load_level] asc
       , [schema_name]
       , [table_name];


--script run order
select ':r ..\Scripts\Data-Loader\'+[to].[category]+'\'+[lst].[table_name]+'.sql'
as [commands for Script.PostDeployment.sql]
from
(
    select *
    from     [dbo].[load_levels]()
    where   [table_name] not in(select table_name From [#table_exclude])
--    union
--    select 'dbo'
--         , 'external_ids'
--         , 99
) as [lst]
inner join [#table_order] [to] on [to].[schema_name]=[lst].[schema_name] and [to].table_name=[lst].table_name
where [lst].table_name in(select table_name from [#table_order])
and [lst].table_name not in('frequency_interval_day_times')--"frequency_interval_day_times" combined with frequency_schedules.sql
order by [lst].[load_level]
       , [lst].[schema_name]
       , [lst].[table_name];

-- drop procedures (needed because cannot generate bacpac where external db references exist in the project)
select '    drop procedure if exists [dbo].[export_ibex_'+[table_name]+'];'
as [commands for Script.PostDeployment.sql]
from
(
    select *
    from     [dbo].[load_levels]()
    where   [table_name] not in(select table_name From [#table_exclude])
) as [lst]
where table_name in(select table_name from [#table_order])
and table_name not in('site_options','options')--"not In Tables" are loaded from static scripts
and [lst].table_name not like 'frequency%'--"frequency_interval_day_times" combined with frequency_schedules.sql
order by [schema_name]
       , [table_name];

-- tables inserts used for edlete data order: delete_emar_data.sql
select 'insert into [#table_order] values('+cast(load_level as varchar(3))+','+cast(row_number() over(partition by [load_level] order by [table_name])  as varchar(3))+','''+[schema_name]+''','''+[table_name]+''',0);'
as [commands for delete_emar_data.sql]
from
(
    select *
    from     [dbo].[load_levels]()
    where   [table_name] not in(select table_name From [#table_exclude])
    union
    select 'dbo'
         , 'external_ids'
         , 99
) as [lst]
order by [load_level] asc
       , [schema_name]
       , [table_name];

--- export bcp data into sample_data folder
select 'call :ek "execute emar_clean.dbo.export_ibex_'+[table_name]+'"'+space(30-len([table_name]))+';'+[table_name]+''+space(30-len([table_name]))+';"|~"'
as [commands for 09_bcp.cmd]
from
(
    select *
    from     [dbo].[load_levels]()
    where   [table_name] not in(select table_name From [#table_exclude])
) as [lst]
where table_name in(select table_name from [#table_order])
and table_name not in('site_options','options')--"not In Tables" are loaded from static scripts
and [lst].table_name not like 'frequency%'--"frequency_interval_day_times" combined with frequency_schedules.sql
order by [schema_name]
       , [table_name];

go
drop function if exists [dbo].[load_levels]
go
