create procedure [dbo].[export_ibex_antimicrobial_indication_items]
as
    begin

        select [source].[site] as    [site_id]
             , [source].[sub_cat] as [sub_category]
        from   [ibex].[dbo].[medication_indication_list] as [source]
        order by [source].[sub_cat]
               , [source].[site];
    end;
go

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'Procedure used to export ibex antimicrobial_indication_items in emar format'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'PROCEDURE'
  , @level1name = N'export_ibex_antimicrobial_indication_items';
go