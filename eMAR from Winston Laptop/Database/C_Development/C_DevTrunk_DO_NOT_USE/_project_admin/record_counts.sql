use master;

declare 
    @show_all tinyint = 0;

/**************************************************
  @show_all = 0   Show Tables with different counts
  @show_all = 1   Show Tables all counts
  @show_all = 2   Show Tables count  = 0
  @show_all = 3   Show Tables count <> 0
**************************************************/

with cte_database_count
     as (select 'emar_dev_base' as             [database_name]
              , schema_name([so].schema_id) as [schema_name]
              , [so].[name] as                 [table_name]
              , sum([spt].[Rows]) as           [row_count]
         from     [emar_dev_base].[sys].[objects] as [so]
                  inner join [emar_dev_base].[sys].[partitions] as [spt] on [so].object_id = [spt].object_id
         where   [so].[type] = 'U'
                 and [so].[is_ms_shipped] = 0x0
                 and [index_id] < 2 -- 0:Heap, 1:Clustered
                 and [so].[name] not in('__RefactorLog', 'sysdiagrams')
         group by [so].schema_id
                , [so].[name]
         union all
         select 'emar_dev_load2' as            [database_name]
              , schema_name([so].schema_id) as [schema_name]
              , [so].[name] as                 [table_name]
              , sum([spt].[Rows]) as           [row_count]
         from     [emar_dev_load2].[sys].[objects] as [so]
                  inner join [emar_dev_load2].[sys].[partitions] as [spt] on [so].object_id = [spt].object_id
         where   [so].[type] = 'U'
                 and [so].[is_ms_shipped] = 0x0
                 and [index_id] < 2 -- 0:Heap, 1:Clustered
                 and [so].[name] not in('__RefactorLog', 'sysdiagrams')
         group by [so].schema_id
                , [so].[name]
         union all
         select 'emar_dev_load' as             [database_name]
              , schema_name([so].schema_id) as [schema_name]
              , [so].[name] as                 [table_name]
              , sum([spt].[Rows]) as           [row_count]
         from     [emar_dev_load].[sys].[objects] as [so]
                  inner join [emar_dev_load].[sys].[partitions] as [spt] on [so].object_id = [spt].object_id
         where   [so].[type] = 'U'
                 and [so].[is_ms_shipped] = 0x0
                 and [index_id] < 2 -- 0:Heap, 1:Clustered
                 and [so].[name] not in('__RefactorLog', 'sysdiagrams')
         group by [so].schema_id
                , [so].[name]
         union all
         select 'emar_dev_load_live' as        [database_name]
              , schema_name([so].schema_id) as [schema_name]
              , [so].[name] as                 [table_name]
              , sum([spt].[Rows]) as           [row_count]
         from     [emar_dev_load_live].[sys].[objects] as [so]
                  inner join [emar_dev_load_live].[sys].[partitions] as [spt] on [so].object_id = [spt].object_id
         where   [so].[type] = 'U'
                 and [so].[is_ms_shipped] = 0x0
                 and [index_id] < 2 -- 0:Heap, 1:Clustered
                 and [so].[name] not in('__RefactorLog', 'sysdiagrams')
         group by [so].schema_id
                , [so].[name]
         union all
         select 'emar_dev_load_sample' as      [database_name]
              , schema_name([so].schema_id) as [schema_name]
              , [so].[name] as                 [table_name]
              , sum([spt].[Rows]) as           [row_count]
         from     [emar_dev_load_sample].[sys].[objects] as [so]
                  inner join [emar_dev_load_sample].[sys].[partitions] as [spt] on [so].object_id = [spt].object_id
         where   [so].[type] = 'U'
                 and [so].[is_ms_shipped] = 0x0
                 and [index_id] < 2 -- 0:Heap, 1:Clustered
                 and [so].[name] not in('__RefactorLog', 'sysdiagrams')
         group by [so].schema_id
                , [so].[name]
         union all
         select 'emar_dev_load_final' as       [database_name]
              , schema_name([so].schema_id) as [schema_name]
              , [so].[name] as                 [table_name]
              , sum([spt].[Rows]) as           [row_count]
         from   [emar_dev_load_final].[sys].[objects] as [so]
                inner join [emar_dev_load_final].[sys].[partitions] as [spt] on [so].object_id = [spt].object_id
         where  [so].[type] = 'U'
                and [so].[is_ms_shipped] = 0x0
                and [index_id] < 2 -- 0:Heap, 1:Clustered
                and [so].[name] not in('__RefactorLog', 'sysdiagrams')
         group by [so].schema_id
                , [so].[name])
     select [schema_name]
          , [table_name]
          , [emar_dev_load]
          , [emar_dev_base]
          , [emar_dev_load2]
          , [emar_dev_load_sample]
          , [emar_dev_load_live]
          , [emar_dev_load_final]
     from   (select [database_name]
                  , [schema_name]
                  , [table_name]
                  , [row_count]
             from [cte_database_count]) as [source] pivot(max([row_count]) for [database_name] in([emar_dev_load]
                                                                                                , [emar_dev_base]
                                                                                                , [emar_dev_load2]
                                                                                                , [emar_dev_load_sample]
                                                                                                , [emar_dev_load_live]
                                                                                                , [emar_dev_load_final])) as [pivot_table]
     where  [emar_dev_base] <> [emar_dev_load2]
            or [emar_dev_load2] <> [emar_dev_load]
            or [emar_dev_load] <> [emar_dev_load_live]
            or [emar_dev_load_live] <> [emar_dev_load_final]
            or [emar_dev_load_final] <> [emar_dev_load_sample]
            or [emar_dev_load_sample] <> [emar_dev_base]
            or @show_all = 1
            or (@show_all = 2
                and [emar_dev_base] = 0)
            or (@show_all = 3
                and [emar_dev_base] <> 0)
     order by [schema_name]
            , [table_name];

/****************************************************************************************************************
use emar_dev_base;

select *, right(Path,len(Path)-charindex('.',Path,charindex('.',Path)+1))
from   [SchemaDictionary]
where  path not in('dbo.LoadLevels', 'tool.ScriptDiagram', 'dbo.load_levels', 'dbo.__RefactorLog.OperationKey')
--       and cast([path] as varchar(500)) like '%patient_id%'
--       and cast([description] as varchar(500)) like '%patient_id%'
order by 4,3
       , 2
       , 1;

select *
from   [SchemaDictionary]
--where  path not in('dbo.LoadLevels', 'tool.ScriptDiagram', 'dbo.load_levels', 'dbo.__RefactorLog.OperationKey')
--       and cast([path] as varchar(500)) like '%drug_id%'
order by 2
       , 1
       , 3;
    
select [col].[TABLE_CATALOG]
     , [col].[TABLE_SCHEMA]
     , [col].[TABLE_NAME]
     , [col].[COLUMN_NAME]
     , [col].[DATA_TYPE]
     , [col].[CHARACTER_MAXIMUM_LENGTH]
from   [INFORMATION_SCHEMA].[COLUMNS] as [col]
where  ([COLUMN_NAME] like 'medication_id') --and left(DATA_TYPE,1) not in ('n','b')
order by 1
       , 2
       , 3
       , 4;

select [col].[TABLE_CATALOG]
     , [col].[TABLE_SCHEMA]
     , [col].[TABLE_NAME]
     , [col].[COLUMN_NAME]
     , [col].[DATA_TYPE]
     , [col].[CHARACTER_MAXIMUM_LENGTH]
from   [INFORMATION_SCHEMA].[COLUMNS] as [col]
where  ([COLUMN_NAME] like '%prompt_group%') --and left(DATA_TYPE,1) not in ('n','b')
order by 1
       , 2
       , 3
       , 4;

select [col].[TABLE_CATALOG]
     , [col].[TABLE_SCHEMA]
     , [col].[TABLE_NAME]
     , [col].[COLUMN_NAME]
     , [col].[DATA_TYPE]
     , [col].[CHARACTER_MAXIMUM_LENGTH]
from   emar.[INFORMATION_SCHEMA].[COLUMNS] as [col]
where  ([COLUMN_NAME] like '%duration_in_minutes%') --and left(DATA_TYPE,1) not in ('n','b')
order by 1
       , 2
       , 3
       , 4;
****************************************************************************************************************/
/**************************************************************************************************
use master;

         select 'emar' as               [database_name]
              , schema_name([so].schema_id) as [schema_name]
              , [so].[name] as                 [table_name]
              , sum([spt].[Rows]) as           [row_count]
         from     [emar].[sys].[objects] as [so]
                  inner join [emar].[sys].[partitions] as [spt] on [so].object_id = [spt].object_id
         where   [so].[type] = 'U'
                 and [so].[is_ms_shipped] = 0x0
                 and [index_id] < 2 -- 0:Heap, 1:Clustered
                 and [so].[name] not in('__RefactorLog', 'sysdiagrams')
         group by [so].schema_id
                , [so].[name]

         select 'emar' as               [database_name]
              , schema_name([so].schema_id) as [schema_name]
              , [so].[name] as                 [table_name]
              , sum([spt].[Rows]) as           [row_count]
         from     [sys].[objects] as [so]
                  inner join [sys].[partitions] as [spt] on [so].object_id = [spt].object_id
         where   [so].[type] = 'U'
                 and [so].[is_ms_shipped] = 0x0
                 and [index_id] < 2 -- 0:Heap, 1:Clustered
                 and [so].[name] not in('__RefactorLog', 'sysdiagrams')
                 and [spt].[Rows] > 0
         group by [so].schema_id
                , [so].[name]

**************************************************************************************************/

select * from [emar_dev_load_final].[dbo].[emar_version] [ev]