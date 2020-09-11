create procedure [export_ibex_medication_routes]
as
    begin

        select distinct
               rtrim(ltrim([a].[site])) [site_id]
             , rtrim(ltrim([a].[name])) [name]
        from   [ibex].[dbo].[idx] as [a]
        where  [type] in('AC')
        order by [site_id]
               , [name];
    end;
go

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'Procedure used to export ibex medication_routes in emar format'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'PROCEDURE'
  , @level1name = N'export_ibex_medication_routes';
go