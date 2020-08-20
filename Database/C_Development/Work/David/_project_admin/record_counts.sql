use master;

declare 
    @show_all bit = 1;

with cte_database_count
     as (select 'emar_bacpac' as               [database_name]
              , schema_name([so].schema_id) as [schema_name]
              , [so].[name] as                 [table_name]
              , sum([spt].[Rows]) as           [row_count]
         from     [emar_bacpac].[sys].[objects] as [so]
                  inner join [emar_bacpac].[sys].[partitions] as [spt] on [so].object_id = [spt].object_id
         where   [so].[type] = 'U'
                 and [so].[is_ms_shipped] = 0x0
                 and [index_id] < 2 -- 0:Heap, 1:Clustered
                 and [so].[name] not in('__RefactorLog', 'sysdiagrams')
         group by [so].schema_id
                , [so].[name]
         union all
         select 'emar_bacpac2' as              [database_name]
              , schema_name([so].schema_id) as [schema_name]
              , [so].[name] as                 [table_name]
              , sum([spt].[Rows]) as           [row_count]
         from     [emar_bacpac2].[sys].[objects] as [so]
                  inner join [emar_bacpac2].[sys].[partitions] as [spt] on [so].object_id = [spt].object_id
         where   [so].[type] = 'U'
                 and [so].[is_ms_shipped] = 0x0
                 and [index_id] < 2 -- 0:Heap, 1:Clustered
                 and [so].[name] not in('__RefactorLog', 'sysdiagrams')
         group by [so].schema_id
                , [so].[name]
         union all
         select 'emar_clean' as                [database_name]
              , schema_name([so].schema_id) as [schema_name]
              , [so].[name] as                 [table_name]
              , sum([spt].[Rows]) as           [row_count]
         from     [emar_clean].[sys].[objects] as [so]
                  inner join [emar_clean].[sys].[partitions] as [spt] on [so].object_id = [spt].object_id
         where   [so].[type] = 'U'
                 and [so].[is_ms_shipped] = 0x0
                 and [index_id] < 2 -- 0:Heap, 1:Clustered
                 and [so].[name] not in('__RefactorLog', 'sysdiagrams')
         group by [so].schema_id
                , [so].[name]
         union all
         select 'emar_dacpac_live' as          [database_name]
              , schema_name([so].schema_id) as [schema_name]
              , [so].[name] as                 [table_name]
              , sum([spt].[Rows]) as           [row_count]
         from     [emar_dacpac_live].[sys].[objects] as [so]
                  inner join [emar_dacpac_live].[sys].[partitions] as [spt] on [so].object_id = [spt].object_id
         where   [so].[type] = 'U'
                 and [so].[is_ms_shipped] = 0x0
                 and [index_id] < 2 -- 0:Heap, 1:Clustered
                 and [so].[name] not in('__RefactorLog', 'sysdiagrams')
         group by [so].schema_id
                , [so].[name]
         union all
         select 'emar_dacpac_sample' as        [database_name]
              , schema_name([so].schema_id) as [schema_name]
              , [so].[name] as                 [table_name]
              , sum([spt].[Rows]) as           [row_count]
         from   [emar_dacpac_sample].[sys].[objects] as [so]
                inner join [emar_dacpac_sample].[sys].[partitions] as [spt] on [so].object_id = [spt].object_id
         where  [so].[type] = 'U'
                and [so].[is_ms_shipped] = 0x0
                and [index_id] < 2 -- 0:Heap, 1:Clustered
                and [so].[name] not in('__RefactorLog', 'sysdiagrams')
         group by [so].schema_id
                , [so].[name])
     select [schema_name]
          , [table_name]
          , [emar_bacpac]
          , [emar_bacpac2]
          , [emar_clean]
          , [emar_dacpac_live]
          , [emar_dacpac_sample]
     from
     (
         select [database_name]
              , [schema_name]
              , [table_name]
              , [row_count]
         from   [cte_database_count]
     ) as [source] pivot(max([row_count]) for [database_name] in([emar_bacpac]
                                                               , [emar_bacpac2]
                                                               , [emar_clean]
                                                               , [emar_dacpac_live]
                                                               , [emar_dacpac_sample])) as [pivot_table]
     where  [emar_bacpac] <> [emar_bacpac2]
            or [emar_bacpac2] <> [emar_clean]
            or [emar_clean] <> [emar_dacpac_live]
            or [emar_dacpac_live] <> [emar_dacpac_sample]
            or [emar_dacpac_sample] <> [emar_bacpac]
            or @show_all = 1
     order by [schema_name]
            , [table_name];

/**************************************************************************************************************
use emar_bacpac;

select *
from   [SchemaDictionary]
where  path not in('dbo.LoadLevels', 'tool.ScriptDiagram', 'dbo.load_levels', 'dbo.__RefactorLog.OperationKey')
--       and cast([path] as varchar(500)) like '%key%'
       and cast([description] as varchar(500)) like '%key%'
order by 3
       , 2
       , 1;

select *
from   [SchemaDictionary]
where  path not in('dbo.LoadLevels', 'tool.ScriptDiagram', 'dbo.load_levels', 'dbo.__RefactorLog.OperationKey')
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
where  ([COLUMN_NAME] like '%drug_id%') --and left(DATA_TYPE,1) not in ('n','b')
order by 1
       , 2
       , 3
       , 4;
**************************************************************************************************************/


