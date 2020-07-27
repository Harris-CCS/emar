create procedure [dbo].[export_ibex_sites]
as
    begin

        select [source].[site]
             , ltrim(rtrim([source].[name]))
             , case
                   when [source].[status] = 'A'
                       then 1
                        else 0
               end
             , 'Central Standard Time'
        from   [ibex].[dbo].[org] as [source]
        order by [source].[name]
               , [source].[site];
    end;
go

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'Procedure used to export ibex sites in emar format'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'PROCEDURE'
  , @level1name = N'export_ibex_sites';
go