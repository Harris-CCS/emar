create procedure [dbo].[export_ibex_override_reasons]
as
    begin
        select rtrim(ltrim([source].[site])) as [site_id]
             , 0 as                             [is_medication]
             , rtrim(ltrim([source].[name])) as [description]
        from     [ibex].[dbo].[cde] as [source]
        where   [source].[type] = 'A'
        union all
        select rtrim(ltrim([source].[site])) as [site_id]
             , 1 as                             [is_medication]
             , rtrim(ltrim([source].[name])) as [description]
        from   [ibex].[dbo].[cde] as [source]
        where  [source].[type] = 'M';
    end;
go

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'Procedure used to export ibex override_reasons in emar format'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'PROCEDURE'
  , @level1name = N'export_ibex_override_reasons';
go