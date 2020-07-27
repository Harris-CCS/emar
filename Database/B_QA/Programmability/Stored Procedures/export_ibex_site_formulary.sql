create procedure [dbo].[export_ibex_site_formulary]
as
    begin

        select [formulary].[site]
             , rtrim(ltrim([formulary].[ndc])) as                [ndc]
             , isnull(cast([ndc].[medid] as varchar(25)), '') as [drug_id]
             , isnull(rtrim(ltrim([brand])), '') as              [brand_name]
             , isnull(rtrim(ltrim([aliencode])), '') as          [hospital_drug_code]
             , rtrim(ltrim([svc])) as                            [service_code]
             , case
                   when [inpat] = 'Y'
                       then 1
                                      else 0
               end as                                            [is_inpatient]
             , case
                   when [outpat] = 'Y'
                       then 1
                      else 0
               end as                                            [is_outpatient]
             , case
                   when [pyxis] = 'Y'
                       then 1
                      else 0
               end as                                            [is_pyxis]
        from   [ibex].[dbo].[frm] as [formulary]
               left join [ibex].[dbo].[fdb_ndc_info] as [ndc] on [ndc].[ndc] = [formulary].[ndc]
        order by [formulary].[ndc], [formulary].[site];
    end;
go

execute [sys].[sp_addextendedproperty]
    @name = N'MS_Description'
  , @value = N'Procedure used to export ibex site_formulary in emar format'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'PROCEDURE'
  , @level1name = N'export_ibex_site_formulary';
go