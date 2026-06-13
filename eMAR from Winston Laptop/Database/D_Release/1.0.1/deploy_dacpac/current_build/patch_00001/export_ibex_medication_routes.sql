print 'create procedure [dbo].[export_ibex_medication_routes];';

drop procedure if exists [dbo].[export_ibex_medication_routes];

set @template = N'
create procedure [export_ibex_medication_routes]
as
    begin

        select
            [source].[idx_id]
          , rtrim(ltrim([source].[site]))  [site]
          , rtrim(ltrim([source].[name]))  [name]
          , rtrim(ltrim([source].[misc2])) [misc2]
        from [ibex].[dbo].[idx] as [source]
        where [source].[type] in (''AC'')
        order by [site]
               , [name];

    end;
';

set @sql_cmd = @template;

set @sql_cmd = replace(@sql_cmd, '<@export_database_name>', @export_database_name);

execute [dbo].[sp_executesql]
    @statement = @sql_cmd;