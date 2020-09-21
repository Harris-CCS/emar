print 'create procedure [dbo].[export_ibex_override_reasons];'
drop procedure if exists [dbo].[export_ibex_override_reasons];

set @template = N'
create procedure [dbo].[export_ibex_override_reasons]
as
    begin
        select rtrim(ltrim([source].[site])) as [site_id]
             , 0 as                             [is_medication]
             , rtrim(ltrim([source].[name])) as [description]
        from     [<@export_database_name>].[dbo].[cde] as [source]
        where   [source].[type] = ''A''
        union all
        select rtrim(ltrim([source].[site])) as [site_id]
             , 1 as                             [is_medication]
             , rtrim(ltrim([source].[name])) as [description]
        from   [<@export_database_name>].[dbo].[cde] as [source]
        where  [source].[type] = ''M'';
    end;
';

set @sql_cmd = @template;
set @sql_cmd = replace(@sql_cmd, '<@export_database_name>', @export_database_name);

execute [dbo].[sp_executesql]
    @statement = @sql_cmd;