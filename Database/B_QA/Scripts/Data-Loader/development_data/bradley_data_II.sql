if '$(load_data)' in('sample', 'live')
    begin

        with src
             as (select *
                 from   (values
                     ('Ear', 1, 'Ear Give Template', -1)) as [t]([name], [is_active], [title], [site_id]))
             insert into [dbo].[templates]
             select *
             from   [src];

        with src
             as (select *
                 from   (values
                     ('Medication', 'MEDICATION'),
                     ('Emotional', ''),
                     ('Safety', 'SAFETY INTERVENTIONS')) as [t]([group_name], [title]))
             insert into [dbo].[prompt_groups]
             select *
             from   [src];

        with src
             as (select [t].[id] as [template_id]
                      , [sequence]
                      , [g].[id] as [prompt_group_id]
                      , [d].[required]
                 from   (values
                     ('Ear', 1, 'Medication', 0),
                     ('Ear', 2, 'Emotional', 0),
                     ('Ear', 3, 'Safety', 0)) as [d]([template_name], [sequence], [group_name], [required])
                        inner join [dbo].[templates] as [t] on [d].[template_name] = [t].[name]
                        inner join [dbo].[prompt_groups] as [g] on [d].[group_name] = [g].[name])
             insert into [dbo].[template_prompt_groups]
             select *
             from   [src];

        with src
             as (select [g].[id] as [prompt_group_id]
                      , [sequence]
                      , [prompt]
                      , [is_active]
                      , [prompt_type]
                      , [prompt_default]
                      , [required]
                 from   (values
                     ('Medication', 1, 'Verbal order read back and verified', 1, 'CheckBox', null, 0),
                     ('Medication', 2, 'Amount Given', 1, 'FreeText', null, 1),
                     ('Medication', 3, 'Administration of this medication is documented elsewhere in chart', 1, 'CheckBox', null, 0),
                     ('Medication', 4, 'Site', 1, 'DropDownListBox', null, 1),
                     ('Medication', 5, 'Correct patient, time, route, dose and medication confirmed prior to administration', 1, 'CheckBox', null, 0),
                     ('Medication', 6, 'Patient advised of actions and side-effects prior to administration', 1, 'CheckBox', null, 0),
                     ('Medication', 7, 'Allergies confirmed and medications reviewed prior to administration', 1, 'CheckBox', null, 0),
                     ('Medication', 8, 'All of the above', 1, 'CheckBox', null, 0),
                     ('Emotional', 1, 'Emotional support needed and given ', 1, 'CheckBox', null, 0),
                     ('Emotional', 2, 'Tolerated Procedure', 1, 'DropDownListBox', null, 0),
                     ('Emotional', 3, 'Additional Staff Required', 1, 'DropDownListBox', null, 0),
                     ('Emotional', 4, 'Reason', 1, 'DropDownListBox', null, 0),
                     ('Emotional', 5, 'Administered by', 1, 'FreeText', null, 1),
                     ('Safety', 1, 'Patient in position of comfort', 1, 'CheckBox', null, 0),
                     ('Safety', 2, 'Side rails up', 1, 'CheckBox', null, 0),
                     ('Safety', 3, 'Cart in lowest position', 1, 'CheckBox', null, 0),
                     ('Safety', 4, 'Family at bedside', 1, 'CheckBox', null, 0),
                     ('Safety', 5, 'All of the above', 1, 'CheckBox', null, 0),
                     ('Safety', 6, 'Friend at beside', 1, 'CheckBox', null, 0),
                     ('Safety', 7, 'Call light in reach', 1, 'CheckBox', null, 0),
                     ('Safety', 8, 'Other:', 1, 'MultiLineFreeText', null, 0)) as [p]([prompt_group_name], [sequence], [prompt], [is_active], [prompt_type], [prompt_default], [required])
                        inner join [dbo].[prompt_groups] as [g] on [p].[prompt_group_name] = [g].[name])
             insert into [dbo].[prompts]
             select *
             from   [src];

        with src
             as (select [p].[id]
                      , [c].[sequence]
                      , [c].[choice_text]
                 from   (values
                     ('Medication', 'Site', 1, 'Left'),
                     ('Medication', 'Site', 2, 'Right'),
                     ('Medication', 'Site', 3, 'Bilaterally'),
                     ('Medication', 'All of the above', 0, '5'),
                     ('Medication', 'All of the above', 0, '6'),
                     ('Medication', 'All of the above', 0, '7'),
                     ('Emotional', 'Tolerated Procedure', 1, 'Well'),
                     ('Emotional', 'Tolerated Procedure', 2, 'With Difficulty'),
                     ('Emotional', 'Tolerated Procedure', 3, 'Uncooperative'),
                     ('Emotional', 'Additional Staff Required', 1, '1 additional staff'),
                     ('Emotional', 'Additional Staff Required', 2, '2 additional staff'),
                     ('Emotional', 'Additional Staff Required', 3, '3 additional staff'),
                     ('Emotional', 'Additional Staff Required', 4, '4 additional staff'),
                     ('Emotional', 'Reason', 1, 'Age'),
                     ('Emotional', 'Reason', 2, 'Combative'),
                     ('Emotional', 'Reason', 3, 'Confused'),
                     ('Emotional', 'Reason', 4, 'Distraction'),
                     ('Emotional', 'Reason', 5, 'Uncooperative'),
                     ('Safety', 'All of the above', 0, '15'),
                     ('Safety', 'All of the above', 0, '16'),
                     ('Safety', 'All of the above', 0, '17'),
                     ('Safety', 'All of the above', 0, '18')) as [c]([group_name], [prompt], [sequence], [choice_text])
                        inner join [dbo].[prompt_groups] as [g] on [c].[group_name] = [g].[name]
                        inner join [dbo].[prompts] as [p] on [g].[id] = [p].[prompt_group_id]
                                                             and [c].[prompt] = [p].[prompt])
             insert into [dbo].[prompt_choices]
             select *
             from   [src];
    end;