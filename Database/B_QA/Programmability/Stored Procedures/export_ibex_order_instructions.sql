create procedure [dbo].[export_ibex_order_instructions]
as
    begin
        select [idx].[site] as               [site_id]
             , ltrim(rtrim([idx].[name])) as [description]
             , 1 as                          [is_active]
        from   [ibex].[dbo].[idx] as [idx]
        where  [idx].[type] = 'SO'
               and [idx].status = 'Y'
        order by 2;
    end;
go

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'Procedure used to export ibex order_instructions in emar format'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'PROCEDURE'
  , @level1name = N'export_ibex_order_instructions';
go