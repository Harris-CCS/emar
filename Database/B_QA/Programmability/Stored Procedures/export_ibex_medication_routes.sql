create procedure [export_ibex_medication_routes]
as
    begin

        select distinct
               [a].[site]
             , [a].[name]
        from   [ibex].[dbo].[idx] as [a]
        where  [type] in('AC')
        order by [a].[name]
               , [a].[site];
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