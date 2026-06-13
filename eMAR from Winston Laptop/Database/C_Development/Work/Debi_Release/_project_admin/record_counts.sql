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
     as (select 'emar_prod_base' as            [database_name]
              , schema_name([so].schema_id) as [schema_name]
              , [so].[name] as                 [table_name]
              , sum([spt].[Rows]) as           [row_count]
         from     [emar_prod_base].[sys].[objects] as [so]
                  inner join [emar_prod_base].[sys].[partitions] as [spt] on [so].object_id = [spt].object_id
         where   [so].[type] = 'U'
                 and [so].[is_ms_shipped] = 0x0
                 and [index_id] < 2 -- 0:Heap, 1:Clustered
                 and [so].[name] not in('__RefactorLog', 'sysdiagrams')
         group by [so].schema_id
                , [so].[name]
         union all
         select 'emar_prod_load' as            [database_name]
              , schema_name([so].schema_id) as [schema_name]
              , [so].[name] as                 [table_name]
              , sum([spt].[Rows]) as           [row_count]
         from     [emar_prod_load].[sys].[objects] as [so]
                  inner join [emar_prod_load].[sys].[partitions] as [spt] on [so].object_id = [spt].object_id
         where   [so].[type] = 'U'
                 and [so].[is_ms_shipped] = 0x0
                 and [index_id] < 2 -- 0:Heap, 1:Clustered
                 and [so].[name] not in('__RefactorLog', 'sysdiagrams')
         group by [so].schema_id
                , [so].[name]
         union all
         select 'emar_prod_final' as           [database_name]
              , schema_name([so].schema_id) as [schema_name]
              , [so].[name] as                 [table_name]
              , sum([spt].[Rows]) as           [row_count]
         from   [emar_prod_final].[sys].[objects] as [so]
                inner join [emar_prod_final].[sys].[partitions] as [spt] on [so].object_id = [spt].object_id
         where  [so].[type] = 'U'
                and [so].[is_ms_shipped] = 0x0
                and [index_id] < 2 -- 0:Heap, 1:Clustered
                and [so].[name] not in('__RefactorLog', 'sysdiagrams')
         group by [so].schema_id
                , [so].[name])
     select [schema_name]
          , [table_name]
          , [emar_prod_base]
          , [emar_prod_final]
          , [emar_prod_load]
     from   (select [database_name]
                  , [schema_name]
                  , [table_name]
                  , [row_count]
             from [cte_database_count]) as [source] pivot(max([row_count]) for [database_name] in([emar_prod_base]
                                                                                                , [emar_prod_final]
                                                                                                , [emar_prod_load])) as [pivot_table]
     where  [emar_prod_base] <> [emar_prod_load]
            or [emar_prod_load] <> [emar_prod_final]
            or [emar_prod_final] <> [emar_prod_base]
            or @show_all = 1
            or (@show_all = 2
                and [emar_prod_base] = 0)
            or (@show_all = 3
                and [emar_prod_base] <> 0)
     order by [schema_name]
            , [table_name];

/****************************************************************************************************************
use emar_bacpac;

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
where  ([COLUMN_NAME] like '%drug_id%') --and left(DATA_TYPE,1) not in ('n','b')
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
**************************************************************************************************/
