print 'create procedure [dbo].[export_ibex_user_settings];'
drop procedure if exists [dbo].[export_ibex_user_settings];

set @template = N'
create or alter procedure [dbo].[export_ibex_user_settings]
as
    begin

        select
               [source].[site]
             , [source].[num]
             , ''MEDICATION_SERVICES''
             , case
                   when substring([grid], 76, 1) in(''R'', ''W'')
                       then substring([grid], 76, 1)
                   else ''E''
               end
        from   [<@export_database_name>].[dbo].[drs] as [source]
        where [source].[site] > 0
        union
        select
            [source].[site]
          , [source].[num]
          , ''LAST_USED_PRINTER''
          , cast([devices].[site] as varchar(15)) + ''|'' + cast([devices].[num] as varchar(15))
        from [<@export_database_name>].[dbo].[drs] as [source]
            inner join [<@export_database_name>].[dbo].[dvc] as [devices]
                on [source].[medprn] = [devices].[num]
        where [source].[site] > 0
              and [devices].[type] in (''P'', ''I'', ''W'', ''D'')
              and [devices].[pfunction] = ''M''
        order by [source].[site]
               , [source].[num];
    end;
';

set @sql_cmd = @template;
set @sql_cmd = replace(@sql_cmd, '<@export_database_name>', @export_database_name);

exec [dbo].[sp_executesql]
    @statement = @sql_cmd;