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
			   -- Instead of hardcoding central standard time,
			   -- map from the PulseCheck time zone to normal/eMAR time zones.
			   -- Winston Murdock, 04/24/2023.  PC-27916
             --, ''Central Standard Time''
			 ,  [dbo].[map_ibex_time_zone_name_to_emar_time_zone_name]([tz].[timezone])
        from   [<@export_database_name>].[dbo].[org] as [source]
		-- Left outer join so that we get sites that don''t have a timezone listed in PulseCheck.
		-- Those that don''t have an entry in the timezones table will result in "central standard time."
		-- This matches the current behavior of merely hardcoding "central standard time."
		left outer join [<@export_database_name>].[dbo].[timezones] as [tz] on [source].[site_timezone]=[tz].[num]
        order by [source].[name]
               , [source].[site];
    end;
';

set @sql_cmd = @template;
set @sql_cmd = replace(@sql_cmd, '<@export_database_name>', @export_database_name);

exec [dbo].[sp_executesql]
    @statement = @sql_cmd;
