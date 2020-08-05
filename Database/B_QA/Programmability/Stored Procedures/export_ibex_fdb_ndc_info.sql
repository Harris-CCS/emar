create procedure [dbo].[export_ibex_fdb_ndc_info]
as
    begin

        select rtrim(ltrim([source].[ndc])) as       [ndc]
             , rtrim(ltrim([source].[base_ndc])) as  [base_ndc]
             , [source].[repackaged]
             , [source].[medid]
             , rtrim(ltrim([source].[packaging])) as [packaging]
             , rtrim(ltrim([source].[strength])) as  [strength]
             , [source].[days_obsolete]
        from   [ibex].[dbo].[fdb_ndc_info] as [source]
        order by [source].[ndc];
    end;
go

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'Procedure used to export ibex fdb_ndc_info in emar format'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'PROCEDURE'
  , @level1name = N'export_ibex_fdb_ndc_info';
go
