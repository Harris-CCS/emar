create view [dbo].[templates_view]

as

select
    [a].[name]  as [action_name]
  , [mr].[name] as [medication_route_name]
  , [t].[name]  [template_name]
  , [s].[name]  as [site_name]
  , [t].[id]    [template_id]
  , [a].[id]    [action_id]
  , [mr].[id]   [medication_route_id]
  , [s].[id]    [site_id]
from [dbo].[templates] [t]
    left join [dbo].[action_route_templates] [art]
        on [art].[template_id] = [t].[id]
    left join [dbo].[actions] [a]
        on [art].[action_id] = [a].[id]
    left join [dbo].[medication_routes] [mr]
        on [mr].[id] = [art].[medication_route_id]
    left join [dbo].[sites] [s]
        on [s].[id] = [art].[site_id];

go

-- Data Dictionary
--    View

execute [sys].[sp_addextendedproperty]
    @name       = N'MS_Description'
  , @value      = N'View to display template options'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'VIEW'
  , @level1name = N'templates_view';
go


