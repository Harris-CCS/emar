create view [dbo].[templates_prompt_groups_view]

as

select distinct
    [a].[name]   as [action_name]
  , [t].[name]   [template_name]
  , [pg].[name]  [prompt_group_name]
  , tpg.sequence as [prompt_group_sequence]
  , [t].[id]     [template_id]
  , [a].[id]     [action_id]
  , [pg].[id]    [prompt_group_id]
from [dbo].[templates] [t]
    left join [dbo].[action_route_templates] [art]
        on [art].[template_id] = [t].[id]
    left join [dbo].[actions] [a]
        on [art].[action_id] = [a].[id]
    left join [dbo].[template_prompt_groups] [tpg]
        on [tpg].[template_id] = [t].[id]
    left join [dbo].[prompt_groups] [pg]
        on [pg].[id] = [tpg].[prompt_group_id];

go

-- Data Dictionary
--    View

execute [sys].[sp_addextendedproperty]
    @name       = N'MS_Description'
  , @value      = N'View to display templates with prompt_groups'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'VIEW'
  , @level1name = N'templates_prompt_groups_view';
go