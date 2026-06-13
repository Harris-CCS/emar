print 'create procedure [dbo].[export_ibex_sites];'
drop procedure if exists [dbo].[export_ibex_sites];

set @template = N'
create or alter procedure [dbo].[export_ibex_sites]
as
    begin

        select [source].[site]
             , ltrim(rtrim([source].[name]))
             , case
                   when [source].[status] = ''A''
                       then 1
                        else 0
               end
             , ''Central Standard Time''
        from   [<@export_database_name>].[dbo].[org] as [source]
        order by [source].[name]
               , [source].[site];
    end;
';

set @sql_cmd = @template;
set @sql_cmd = replace(@sql_cmd, '<@export_database_name>', @export_database_name);

exec [dbo].[sp_executesql]
    @statement = @sql_cmd;
