create view [dbo].[preferred_medication_routes_export_view]

as

with cte_pivot
    as (
                 select
                     [site_id]
                   , [medication_id]
                   , [button01]
                   , [button02]
                   , [button03]
                   , [button04]
                   , [button05]
                   , [button06]
                   , [button07]
                   , [button08]
                   , [button09]
                   , [button10]
                 from (
                          select
                              case
                                  when [m].[site_id] = -1 then [pmr].[site_id]
                                  else [m].[site_id]
                              end                                                                                                                                                                     [site_id]
                            , [pmr].[medication_id]
                            , 'button' + right('00' + cast(row_number() over (partition by [pmr].[site_id], [pmr].[medication_id] order by [pmr].[site_id], [pmr].[medication_id]) as varchar(3)), 2) column_number
                            , [mr].[name]                                                                                                                                                             [medication_route_name]
                          from [dbo].[preferred_medication_routes] [pmr]
                              inner join [dbo].[medications] [m]
                                  on [pmr].[medication_id] = [m].[id]
                              inner join [dbo].[medication_routes] [mr]
                                  on [pmr].[medication_route_id] = [mr].[id]
                 ) [source_table]
                 pivot (
                 max([medication_route_name])
                 for [column_number] in ([button01], [button02], [button03], [button04], [button05], [button06], [button07], [button08], [button09], [button10])
                 ) [pivot_table]
        )
select
    [s].[name]                   site_name
  , [m].[drug_id]
  , [m].[display_name]
  , isnull([pvt].[button01], '') as [button01]
  , isnull([pvt].[button02], '') as [button02]
  , isnull([pvt].[button03], '') as [button03]
  , isnull([pvt].[button04], '') as [button04]
  , isnull([pvt].[button05], '') as [button05]
  , isnull([pvt].[button06], '') as [button06]
  , isnull([pvt].[button07], '') as [button07]
  , isnull([pvt].[button08], '') as [button08]
  , isnull([pvt].[button09], '') as [button09]
  , isnull([pvt].[button10], '') as [button10]
from cte_pivot [pvt]
    inner join [dbo].[sites] [s]
        on [s].[id] = [pvt].[site_id]
    inner join [dbo].[medications] [m]
        on [m].[id] = [pvt].[medication_id]

go

-- Data Dictionary
--    View

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'View used to generate text to be applied into the Excel Workbook for the purposes of import / edit'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'VIEW'
  , @level1name = N'preferred_medication_routes_export_view';
go