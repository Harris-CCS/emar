create view [dbo].[get_code_share_site_view__order_instructions]
as
    select [s].[id]
         , [cs].[site_id]
    from   [sites] as [s]
           cross apply [get_code_share_site]
        ([s].[id], 'order_instructions') as [cs];

go
-- Data Dictionary
--    View

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'View to display to get code share site for order_instructions'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'VIEW'
  , @level1name = N'get_code_share_site_view__order_instructions';
go