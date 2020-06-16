create view [dbo].[SchemaDictionary]
as
    with cteObjects
         as (select --objects and columns
             case
                 when [so].[parent_object_id] > 0
                     then object_schema_name([so].[parent_object_id]) + '.' + object_name([so].[parent_object_id]) + '.' + [so].[name]
                 else object_schema_name([so].[object_id]) + '.' + [so].[name]
             end as                        Path
           , 'schema' + case
                            when [so].[parent_object_id] > 0
                                then '/table'
                            else ''
                        end + '/' + case
                                        when [so].[type] in('tf', 'fn', 'if', 'fs', 'ft')
                                            then 'function'
                                        when [so].[type] in('p', 'pc', 'rf', 'x')
                                            then 'procedure'
                                        when [so].[type] in('u', 'it')
                                            then 'table'
                                        when [so].[type] = 'sq'
                                            then 'queue'
                                        else lower([so].[type_desc])
                                    end as [Type]
           , [ep1].[value]
             from [sys].[objects] as [so]
                  left join [sys].[extended_properties] as [ep1] on [ep1].[major_id] = [so].[object_id]
                                                                    and [ep1].[class] = 1
                                                                    and [ep1].[minor_id] = 0
             where [so].[is_ms_shipped] = 0
                   and isnull([ep1].[name], 'MS_description') = 'MS_description'
                   and [so].name not like '%sysdiagrams%'
                   and [so].[type_desc] in('DEFAULT_CONSTRAINT','SQL_INLINE_TABLE_VALUED_FUNCTION', 'SQL_SCALAR_FUNCTION', 'SQL_TABLE_VALUED_FUNCTION', 'SQL_STORED_PROCEDURE', 'USER_TABLE', 'SQL_TRIGGER', 'VIEW')),
         cteColumns
         as (select --objects and columns
             case
                 when [so].[parent_object_id] > 0
                     then object_schema_name([so].[parent_object_id]) + '.' + object_name([so].[parent_object_id]) + '.' + [so].[name]
                 else object_schema_name([so].[object_id]) + '.' + [so].[name]
             end + case
                       when [col].[column_id] > 0
                           then '.' + [col].[name]
                       else ''
                   end as                        Path
           , 'schema' + case
                            when [so].[parent_object_id] > 0
                                then '/table'
                            else ''
                        end + '/' + case
                                        when [so].[type] in('tf', 'fn', 'if', 'fs', 'ft')
                                            then 'function'
                                        when [so].[type] in('p', 'pc', 'rf', 'x')
                                            then 'procedure'
                                        when [so].[type] in('u', 'it')
                                            then 'table'
                                        when [so].[type] = 'sq'
                                            then 'queue'
                                        else lower([so].[type_desc])
                                    end + case
                                              when [col].[column_id] is null
                                                  then ''
                                              else '/column'
                                          end as [Type]
           , [ep2].[value]
             from [sys].[objects] as [so]
                  inner join [sys].[columns] as [col] on [so].[object_id] = [col].[object_id]
                  left join [sys].[extended_properties] as [ep2] on [ep2].[major_id] = [col].[object_id]
                                                                    and [ep2].[class] = 1
                                                                    and [ep2].[minor_id] = [col].[column_id]
             where [so].[is_ms_shipped] = 0
                   and isnull([ep2].[name], 'MS_description') = 'MS_description'
                   and [so].name not like '%sysdiagrams%'
                   and [so].[type_desc] = 'USER_TABLE')
         select [obj].[Path]
              , [obj].[Type]
              , isnull([obj].[value], '') as [Description]
         from [cteObjects] as [obj]
         union all
         select [col].[Path]
              , [col].[Type]
              , isnull([col].[value], '') as [Description]
         from [cteColumns] as [col];
go

-- Data Dictionary
--    View
execute [sys].[sp_addextendedproperty]
    @name = N'MS_Description'
  , @value = N'View to display data dictionary / schema descriptions'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'VIEW'
  , @level1name = N'SchemaDictionary';
go
