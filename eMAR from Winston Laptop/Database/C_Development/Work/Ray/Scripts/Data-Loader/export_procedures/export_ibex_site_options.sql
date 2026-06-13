print 'create procedure [dbo].[export_ibex_site_options];'
drop procedure if exists [dbo].[export_ibex_site_options];

set @template = N'
create or alter procedure [dbo].[export_ibex_site_options]
as
    begin

        select [site]
             , ''SHOW_DOSE_FORM'' as [option_id]
             , case
                   when substring([opts], 1, 1) = ''Y''
                       then ''Y''
                   else ''N''
               end
        from     [<@export_database_name>].[dbo].[org]
        union
        select [site]
             , ''SHOW_STRENGTH'' as [option_id]
             , case
                   when substring([opts], 2, 1) = ''Y''
                       then ''Y''
                   else ''N''
               end
        from     [<@export_database_name>].[dbo].[org]
        union
        select [site]
             , ''POPUP_ON_GIVE'' as [option_id]
             , case
                   when substring([opts], 237, 1) = ''Y''
                       then ''Y''
                   else ''N''
               end
        from   [<@export_database_name>].[dbo].[org]
		union
        select [site]
             , ''MEDINPAT'' as [option_id]
             , case
                   when medinpat = ''Y''
                       then ''Y''
                   else ''N''
               end
        from   [<@export_database_name>].[dbo].[org]
		union
        select [site]
             , ''MEDOUTPAT'' as [option_id]
             , case
                   when medoutpat = ''Y''
                       then ''Y''
                   else ''N''
               end
        from   [<@export_database_name>].[dbo].[org]
		union
        select [site]
             , ''MEDPYXIS'' as [option_id]
             , case
                   when medpyxis = ''Y''
                       then ''Y''
                   else ''N''
               end
        from   [<@export_database_name>].[dbo].[org]
		union
        select [site]
             , ''MEDEXACTMATCH'' as [option_id]
             , case
                   when medexactmatch = ''Y''
                       then ''Y''
                   else ''N''
               end
        from   [<@export_database_name>].[dbo].[org]
		union
        select [site]
             , ''RXALERT'' as [option_id]
             , case rxalert
                   when ''0''
                       then ''0''
                   when ''5''
                       then ''5''
                   when ''6''
                       then ''6''
                   else ''0''
               end
        from   [<@export_database_name>].[dbo].[org];
    end;
';

set @sql_cmd = @template;
set @sql_cmd = replace(@sql_cmd, '<@export_database_name>', @export_database_name);

exec [dbo].[sp_executesql]
    @statement = @sql_cmd;


