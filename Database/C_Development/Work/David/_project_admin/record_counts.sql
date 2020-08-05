use emar_bacpac;

select quotename(schema_name([so].schema_id)) + '.' + quotename([so].[name]) as [TableName]
     , sum([spt].[Rows]) as                                                     [RowCount]
from   [sys].[objects] as [so]
       inner join [sys].[partitions] as [spt] on [so].object_id = [spt].object_id
where  [so].[type] = 'U'
       and [so].[is_ms_shipped] = 0x0
       and [index_id] < 2 -- 0:Heap, 1:Clustered
       and [so].[name] not in('__RefactorLog', 'sysdiagrams')
       and [spt].[Rows] > 0
group by [so].schema_id
       , [so].[name]
order by [TableName];


use emar_bacpac;

select * from SchemaDictionary where path not in('dbo.LoadLevels','tool.ScriptDiagram','dbo.load_levels','dbo.__RefactorLog.OperationKey')
and cast([path] as varchar(500)) like '%ordinal%'
order by 3,2,1


select * from SchemaDictionary where path not in('dbo.LoadLevels','tool.ScriptDiagram','dbo.load_levels','dbo.__RefactorLog.OperationKey')
and cast([path] as varchar(500)) like '%site_id%'
order by 2,1,3

    select [col].[TABLE_CATALOG]
         , [col].[TABLE_SCHEMA]
         , [col].[TABLE_NAME]
         , [col].[COLUMN_NAME]
         , [col].[DATA_TYPE]
    from   [INFORMATION_SCHEMA].[COLUMNS] as [col]
    where  ([COLUMN_NAME] like '%ordin%')and left(DATA_TYPE,1) not in ('n','b')
    order by 1
           , 2
           , 3
           , 4;



select 'rename: ['+ISNULL([emar_live].TABLE_NAME,'')+'].['+ISNULL([emar_live].COLUMN_NAME,'')+'  >>>  ['+ISNULL([emar].TABLE_NAME,'')+'].['+ISNULL([emar].COLUMN_NAME,'')+']'
from   emar.[INFORMATION_SCHEMA].[COLUMNS] as [emar]
full outer join emar_live.[INFORMATION_SCHEMA].[COLUMNS] as [emar_live] on [emar_live].TABLE_NAME=[emar].TABLE_NAME and [emar_live].ORDINAL_POSITION=[emar].ORDINAL_POSITION
WHERE ISNULL([emar].COLUMN_NAME,'')<>ISNULL([emar_live].COLUMN_NAME,'') and ISNULL([emar_live].COLUMN_NAME,'')>''
ORDER BY 1
