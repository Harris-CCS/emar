print 'create procedure [dbo].[export_ibex_devices];'
drop procedure if exists [dbo].[export_ibex_devices];

set @template = N'
create or alter procedure [dbo].[export_ibex_devices]
as
    begin

    -- Rather than selecting the devices here.
	-- Pull them from the view I created (which the IDS view will reference).
	-- This allows us to have this filter logic in only one place.
	-- Winston Murdock, 01/28/2022.  PC-26949

    --    select [num]
    --         , rtrim(ltrim([site]))    [site]
    --         , rtrim(ltrim([status]))  [status]
    --         , rtrim(ltrim([type]))    [type]
    --         , rtrim(ltrim([address])) [address]
    --         , rtrim(ltrim([dname]))   [dname]
    --         , rtrim(ltrim([descrip])) [descrip]
    --         , rtrim(ltrim([tray]))    [tray]
    --         , rtrim(ltrim([ptype]))   [ptype]
    --    from   [<@export_database_name>].[dbo].[dvc]
    --    where  [type] in(''P'', ''I'', ''W'', ''D'')
    --           and [pfunction] = ''M'';

		SELECT *
		FROM [<@export_database_name>].[dbo].[ibex..emar_devices_to_pull_in_from_ibex]

    end;
';

set @sql_cmd = @template;
set @sql_cmd = replace(@sql_cmd, '<@export_database_name>', @export_database_name);

execute [dbo].[sp_executesql]
    @statement = @sql_cmd;
