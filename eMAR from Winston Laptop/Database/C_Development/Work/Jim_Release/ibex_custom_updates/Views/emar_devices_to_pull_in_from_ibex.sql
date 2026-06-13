print 'create view [ibex].[dbo].[emar_devices_to_pull_in_from_ibex];';

set @template = N'
create or alter view [dbo].[emar_devices_to_pull_in_from_ibex]

as
	select [num]
			, rtrim(ltrim([site]))    [site]
			, rtrim(ltrim([status]))  [status]
			, rtrim(ltrim([type]))    [type]
			, rtrim(ltrim([address])) [address]
			, rtrim(ltrim([dname]))   [dname]
			, rtrim(ltrim([descrip])) [descrip]
			, rtrim(ltrim([tray]))    [tray]
			, rtrim(ltrim([ptype]))   [ptype]
	from   [ibex].[dbo].[dvc]
	where  [type] in(''P'', ''I'', ''W'', ''D'')
			and [pfunction] = ''M''
';

set @sql_cmd = @template;

execute [ibex].[dbo].[sp_executesql] 
    @statement = @sql_cmd;
