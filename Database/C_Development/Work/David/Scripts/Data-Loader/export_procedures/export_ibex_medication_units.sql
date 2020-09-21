print 'create procedure [dbo].[export_ibex_medication_units];';

drop procedure if exists [dbo].[export_ibex_medication_units];

set @template = N'
create or alter procedure [dbo].[export_ibex_medication_units]
as
    begin

        select [source].[site] as [site_id]
             , ltrim(rtrim([source].[id])) as   [code]
             , ltrim(rtrim([source].[name])) as [name]
             , ltrim(rtrim([source].[misc])) as [print_name]
             , case
                   when ltrim(rtrim([source].[status])) = ''A''
                       then 1
                   else 0
               end as             [is_active]
        from   [<@export_database_name>].[dbo].[idx] as [source]
        where  [source].[type] = ''BE''
        order by [source].[id]
               , [source].[name]
               , [source].[site];
    end;
';

set @sql_cmd = @template;

set @sql_cmd = replace(@sql_cmd, '<@export_database_name>', @export_database_name);

execute [dbo].[sp_executesql]
    @statement = @sql_cmd;