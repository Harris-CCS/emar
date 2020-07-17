create procedure [dbo].[export_ibex_user_quick_list_items]
as
    begin
        select distinct
               [source].[site]
             , [source].[usr]
             , [source].[ndc]
             , [ndc].[medid]
             , [source].[brand]
             , case
                   when isnumeric([source].[strength]) = 0
                        or [source].[strength] = '-'
                       then 0
                        else cast([source].[strength] as decimal(11, 2))
               end as              [dose]
             , [source].[unit]
             , [source].[route]
             , 0 as                [frequency]
             , [source].[notes] as [order_notes]
        from   [ibex].[dbo].[rxl] as [source]
               left join [ibex].[dbo].[fdb_ndc_info] as [ndc] on [source].[ndc] = [ndc].[ndc]
        order by [source].[brand]
               , [source].[usr]
               , [source].[site];
    end;
go

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'Procedure used to export ibex export_ibex_user_quick_list_items in emar format'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'PROCEDURE'
  , @level1name = N'export_ibex_user_quick_list_items';
go