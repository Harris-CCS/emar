print 'create procedure [dbo].[export_ibex_antimicrobial_indications];'
drop procedure if exists [dbo].[export_ibex_antimicrobial_indications];

set @template = N'
create or alter procedure [dbo].[export_ibex_antimicrobial_indications]
as
    begin

        select [source].[site] as                      [site_id]
             , ltrim(rtrim([source].[code])) as        [code]
             , ltrim(rtrim([source].[description])) as [description]
             , case
                   when ltrim(rtrim([source].[status])) = ''A''
                       then 1
                   else 0
               end as                                  [is_active]
             , [source].[position] as                  [ordinal_position]
        from   [<@export_database_name>].[dbo].[medication_indication] as [source]
        order by [source].[description]
               , [source].[site];
    end;
';

set @sql_cmd = @template;
set @sql_cmd = replace(@sql_cmd, '<@export_database_name>', @export_database_name);

exec [dbo].[sp_executesql] 
    @statement = @sql_cmd;