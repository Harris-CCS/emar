print 'create procedure [dbo].[export_ibex_site_code_shares];'
drop procedure if exists [dbo].[export_ibex_site_code_shares];

set @template = N'
create or alter procedure [dbo].[export_ibex_site_code_shares]
as
    begin

        select [source].[site] as    [source_site_id]
             , [source].[cs_site] as [target_site_id]
             , case
                   when [source].[cs_name] = ''med_route''
                       then ''medication_routes''
                   when [source].[cs_name] = ''med_unit''
                       then ''medication_units''
                   else ''******ERROR******''
               end as                [entity]
        from   [<@export_database_name>].[dbo].[code_share] as [source]
        where  [source].[cs_name] in(''med_route'', ''med_unit'')
        order by [entity]
               , [source_site_id]
               , [target_site_id];
    end;
';

set @sql_cmd = @template;
set @sql_cmd = replace(@sql_cmd, '<@export_database_name>', @export_database_name);

execute [dbo].[sp_executesql]
    @statement = @sql_cmd;