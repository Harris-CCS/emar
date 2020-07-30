create procedure [dbo].[export_ibex_site_code_shares]
as
    begin

        select [source].[site] as    [source_site_id]
             , [source].[cs_site] as [target_site_id]
             , case
                   when [source].[cs_name] = 'med_route'
                       then 'medication_routes'
                   when [source].[cs_name] = 'med_unit'
                       then 'medication_units'
                   else '******ERROR******'
               end as                [entity]
        from   [ibex].[dbo].[code_share] as [source]
        where  [source].[cs_name] in('med_route', 'med_unit')
        order by [entity]
               , [source_site_id]
               , [target_site_id];
    end;
go

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'Procedure used to export ibex site_code_share in emar format'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'PROCEDURE'
  , @level1name = N'export_ibex_site_code_shares';
go