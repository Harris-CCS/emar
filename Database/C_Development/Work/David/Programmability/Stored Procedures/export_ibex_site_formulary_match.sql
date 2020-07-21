create procedure [dbo].[export_ibex_site_formulary_match]
as
    begin

        select [formulary].[site]
             , rtrim(ltrim([formulary].[ndc])) as                [ndc]
             , isnull(cast([ndc].[medid] as varchar(25)), '') as [drug_id]
             , isnull(rtrim(ltrim([brand].[brand_name])), '') as [brand_name]
             , [formulary].[inpat] as                            [inpatient_match]
             , [formulary].[outpat] as                           [outpatient_match]
             , [formulary].[pyxis] as                            [pyxis_match]
        from   [ibex].[dbo].[formulary_match] as [formulary]
               left join [ibex].[dbo].[fdb_ndc_info] as [ndc] on [ndc].[ndc] = [formulary].[ndc]
               left join [ibex].[dbo].[fdb_brand_name] as [brand] on [brand].[MEDID] = [ndc].[medid]
        order by [formulary].[ndc], [formulary].[site];
    end;
go

execute [sys].[sp_addextendedproperty]
    @name = N'MS_Description'
  , @value = N'Procedure used to export ibex site_formulary_match in emar format'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'PROCEDURE'
  , @level1name = N'export_ibex_site_formulary_match';
go