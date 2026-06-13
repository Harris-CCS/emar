create view [dbo].[prompt_groups_view]

as

select
    [pg].[name]    [prompt_group_name]
  , [p].[sequence] [prompt_sequence]
  , [p].[prompt]   [prompt_name]
  , [p].[prompt_type]
  , case [pc].[sequence]
        when 0 then 'Child Prompt'
        else convert(varchar, pc.sequence)
    end            [prompt_choice_sequence]
  , case
        when [pc].[sequence] = 0
            and childprompt.id is null then 'ERROR: ChoiceText points to prompt that doesn''t exist, or is in the wrong prompt_group'
        when [pc].[sequence] = 0 then childprompt.prompt
        else [pc].[choice_text]
    end            'choice Text or Child Prompt'
  , [pg].[id]      [prompt_group_id]
  , [p].[id]       [prompt_id]
  , [pc].[id]      [prompt_choice_id]
from [dbo].[prompt_groups] [pg]
    left join [dbo].[prompts] [p]
        on [pg].[id] = [p].[prompt_group_id]
    left join [dbo].[prompt_choices] [pc]
        on [p].[id] = [pc].[prompt_id]
    left join [dbo].[prompts] childprompt
        on pc.sequence = 0
            and
                case
                    when isnumeric(pc.choice_text) = 0 then 0
                    else convert(int, pc.choice_text)
                end = childprompt.id
            and p.prompt_group_id = childprompt.prompt_group_id;

go

-- Data Dictionary
--    View

execute [sys].[sp_addextendedproperty]
    @name       = N'MS_Description'
  , @value      = N'View to display prompt_groups responses'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'VIEW'
  , @level1name = N'prompt_groups_view';
go