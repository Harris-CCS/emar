print 'create procedure [dbo].[export_ibex_override_reasons];'
drop procedure if exists [dbo].[export_ibex_override_reasons];

set @template = N'
create procedure [dbo].[export_ibex_override_reasons]
as
    begin
        select 
               [num]
             , rtrim(ltrim([source].[site])) as [site]
             , rtrim(ltrim([type])) as [type]
             , rtrim(ltrim([source].[name])) as [description]
             , rtrim(ltrim([source].[status])) as [status]
        from     [<@export_database_name>].[dbo].[cde] as [source]
        where   [source].[type] in (''A'',''M'');
    end;
';

set @sql_cmd = @template;
set @sql_cmd = replace(@sql_cmd, '<@export_database_name>', @export_database_name);

execute [dbo].[sp_executesql]
    @statement = @sql_cmd;