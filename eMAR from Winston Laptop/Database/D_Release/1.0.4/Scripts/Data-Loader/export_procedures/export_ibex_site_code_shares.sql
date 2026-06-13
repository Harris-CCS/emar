print 'create procedure [dbo].[export_ibex_site_code_shares];'
drop procedure if exists [dbo].[export_ibex_site_code_shares];

set @template = N'
create or alter procedure [dbo].[export_ibex_site_code_shares]
as
    begin

        select
            [site]    [source_site]
          , [cs_site] [target_site]
          , [cs_name] [entity]
        from [<@export_database_name>].[dbo].[code_share] as [target];

    end;
';

set @sql_cmd = @template;
set @sql_cmd = replace(@sql_cmd, '<@export_database_name>', @export_database_name);

execute [dbo].[sp_executesql]
    @statement = @sql_cmd;