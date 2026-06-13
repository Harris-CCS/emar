print 'create procedure [dbo].[export_ibex_site_formulary_match];'
drop procedure if exists [dbo].[export_ibex_site_formulary_match];

set @template = N'
create or alter procedure [dbo].[export_ibex_site_formulary_match]
as
    begin

        select [formulary].[formulary_match_id]
             , [formulary].[site]
             , rtrim(ltrim([formulary].[ndc])) as                [ndc]
             , isnull(cast([ndc].[medid] as varchar(25)), '''') as [drug_id]
             , isnull(rtrim(ltrim([brand].[brand_name])), '''') as [brand_name]
             , [formulary].[inpat] as                            [inpatient_match]
             , [formulary].[outpat] as                           [outpatient_match]
             , [formulary].[pyxis] as                            [pyxis_match]
        from   [<@export_database_name>].[dbo].[formulary_match] as [formulary]
               left join [<@export_database_name>].[dbo].[fdb_ndc_info] as [ndc] on [ndc].[ndc] = [formulary].[ndc]
               left join [<@export_database_name>].[dbo].[fdb_brand_name] as [brand] on [brand].[MEDID] = [ndc].[medid]
        order by [formulary].[ndc], [formulary].[site];
    end;
';

set @sql_cmd = @template;
set @sql_cmd = replace(@sql_cmd, '<@export_database_name>', @export_database_name);

exec [dbo].[sp_executesql]
    @statement = @sql_cmd;
