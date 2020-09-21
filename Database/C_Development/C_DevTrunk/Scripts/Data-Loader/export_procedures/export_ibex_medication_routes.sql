print 'create procedure [dbo].[export_ibex_medication_routes];';

drop procedure if exists [dbo].[export_ibex_medication_routes];

set @template = N'
create procedure [export_ibex_medication_routes]
as
    begin

        select distinct
               rtrim(ltrim([a].[site])) [site_id]
             , rtrim(ltrim([a].[name])) [name]
        from   [<@export_database_name>].[dbo].[idx] as [a]
        where  [type] in(''AC'')
        order by [site_id]
               , [name];
    end;
';

set @sql_cmd = @template;

set @sql_cmd = replace(@sql_cmd, '<@export_database_name>', @export_database_name);

execute [dbo].[sp_executesql]
    @statement = @sql_cmd;