print 'create procedure [dbo].[export_ibex_site_formulary];'
drop procedure if exists [dbo].[export_ibex_site_formulary];

set @template = N'
create or alter procedure [dbo].[export_ibex_site_formulary]
as
    begin

        select [formulary].[site]
             , rtrim(ltrim([formulary].[ndc])) as                [ndc]
             , isnull(cast([ndc].[medid] as varchar(25)), '''') as [drug_id]
             , isnull(rtrim(ltrim([brand])), '''') as              [brand_name]
             , isnull(rtrim(ltrim([aliencode])), '''') as          [hospital_drug_code]
             , rtrim(ltrim([svc])) as                            [service_code]
             , case
                   when [inpat] = ''Y''
                       then 1
                   else 0
               end as                                            [is_inpatient]
             , case
                   when [outpat] = ''Y''
                       then 1
                   else 0
               end as                                            [is_outpatient]
             , case
                   when [pyxis] = ''Y''
                       then 1
                   else 0
               end as                                            [is_pyxis]
             , [formulary].[dateadd]
        from   [<@export_database_name>].[dbo].[frm] as [formulary]
               left join [<@export_database_name>].[dbo].[fdb_ndc_info] as [ndc] on [ndc].[ndc] = [formulary].[ndc]
        order by [formulary].[ndc]
               , [formulary].[site];
    end;
';

set @sql_cmd = @template;
set @sql_cmd = replace(@sql_cmd, '<@export_database_name>', @export_database_name);

exec [dbo].[sp_executesql]
    @statement = @sql_cmd;
