print 'create procedure [dbo].[export_ibex_medication_routes];';

drop procedure if exists [dbo].[export_ibex_medication_routes];

set @template = N'
create procedure [export_ibex_medication_routes]
as
    begin

        select
            [idx_id]
          , rtrim(ltrim([site]))   [site]
          , rtrim(ltrim([name]))   [name]
          , rtrim(ltrim([misc2]))  [misc2]
          , rtrim(ltrim([misc3]))  [misc3]
          , rtrim(ltrim([status])) [status]
          , rtrim(ltrim([id]))     [id]
        from [<@export_database_name>].[dbo].[idx] as [source]
        where [source].[type] in (''AC'')
        order by [site]
               , [name];

    end;
';

set @sql_cmd = @template;

set @sql_cmd = replace(@sql_cmd, '<@export_database_name>', @export_database_name);

execute [dbo].[sp_executesql]
    @statement = @sql_cmd;