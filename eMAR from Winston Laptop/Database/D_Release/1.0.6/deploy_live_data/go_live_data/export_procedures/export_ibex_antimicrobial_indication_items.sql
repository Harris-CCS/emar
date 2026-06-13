print 'create procedure [dbo].[export_ibex_antimicrobial_indication_items];'
drop procedure if exists [dbo].[export_ibex_antimicrobial_indication_items];

set @template = N'
create or alter procedure [dbo].[export_ibex_antimicrobial_indication_items]
as
    begin

        select [source].[id]
             , [source].[site] as    [site_id]
             , [source].[sub_cat] as [sub_category]
        from   [<@export_database_name>].[dbo].[medication_indication_list] as [source]
        order by [source].[sub_cat]
               , [source].[site];
    end;
';

set @sql_cmd = @template;
set @sql_cmd = replace(@sql_cmd, '<@export_database_name>', @export_database_name);

exec [dbo].[sp_executesql] 
    @statement = @sql_cmd;