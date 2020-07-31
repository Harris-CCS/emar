create procedure [dbo].[export_ibex_medication_units]
as
    begin

        select [source].[site] as [site_id]
             , ltrim(rtrim([source].[id])) as   [code]
             , ltrim(rtrim([source].[name])) as [name]
             , ltrim(rtrim([source].[misc])) as [print_name]
             , case
                   when ltrim(rtrim([source].[status])) = 'A'
                       then 1
                   else 0
               end as             [is_active]
        from   [ibex].[dbo].[idx] as [source]
        where  [source].[type] = 'BE'
        order by [source].[id]
               , [source].[name]
               , [source].[site];
    end;
go

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'Procedure used to export ibex medication_units in emar format'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'PROCEDURE'
  , @level1name = N'export_ibex_medication_units';
go
