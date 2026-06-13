print 'Loading Table: prompt_choices';

declare
    @prompt_choices table
        (
            [prompt_id]         [int]           null
          , [sequence]          [smallint]      not null
          , [choice_text]       [nvarchar](200) not null
          , [prompt_group_id]   [int]           null
          , [prompt_sequence]   [int]           null
          , [prompt_group_name] [varchar](50)   not null
          , [prompt]            [nvarchar](200) not null
        );

insert into @prompt_choices
(
    [prompt_group_name]
  , [prompt]
  , [sequence]
  , [choice_text]
)
select
    [prompt_group_name]
  , [prompt]
  , [sequence]
  , [choice_text]
from (
values
('Medication', 'Site', 1, 'Left')
, ('Medication', 'Site', 2, 'Right')
, ('Medication', 'Site', 3, 'Bilaterally')

, ('OralMedication', 'Site', 1, 'P.O.')
, ('OralMedication', 'Site', 2, 'S.L.')
, ('OralMedication', 'Site', 3, 'Buccal')

, ('Emotional', 'Tolerated Procedure', 1, 'Well')
, ('Emotional', 'Tolerated Procedure', 2, 'With Difficulty')
, ('Emotional', 'Tolerated Procedure', 3, 'Uncooperative')
, ('Emotional', 'Additional Staff Required', 1, '1 additional staff')
, ('Emotional', 'Additional Staff Required', 2, '2 additional staff')
, ('Emotional', 'Additional Staff Required', 3, '3 additional staff')
, ('Emotional', 'Additional Staff Required', 4, '4 additional staff')
, ('Emotional', 'Reason', 1, 'Age')
, ('Emotional', 'Reason', 2, 'Combative')
, ('Emotional', 'Reason', 3, 'Confused')
, ('Emotional', 'Reason', 4, 'Distraction')
, ('Emotional', 'Reason', 5, 'Uncooperative')

, ('IntraMuscMedication', 'Site (non-antibiotic/immunization)', 1, 'Left deltoid')
, ('IntraMuscMedication', 'Site (non-antibiotic/immunization)', 2, 'Right deltoid')
, ('IntraMuscMedication', 'Site (non-antibiotic/immunization)', 3, 'Left buttock')
, ('IntraMuscMedication', 'Site (non-antibiotic/immunization)', 4, 'Right buttock')
, ('IntraMuscMedication', 'Site (non-antibiotic/immunization)', 5, 'Left hip')
, ('IntraMuscMedication', 'Site (non-antibiotic/immunization)', 6, 'Right hip')
, ('IntraMuscMedication', 'Site (non-antibiotic/immunization)', 7, 'Left thigh')
, ('IntraMuscMedication', 'Site (non-antibiotic/immunization)', 8, 'Right thigh')
, ('IntraMuscMedication', 'Site (non-antibiotic/immunization)', 9, 'Other IV sites - TODO')

, ('IntraMuscMedication', 'Site (antibiotic)', 1, 'Left deltoid')
, ('IntraMuscMedication', 'Site (antibiotic)', 2, 'Right deltoid')
, ('IntraMuscMedication', 'Site (antibiotic)', 3, 'Left buttock')
, ('IntraMuscMedication', 'Site (antibiotic)', 4, 'Right buttock')
, ('IntraMuscMedication', 'Site (antibiotic)', 5, 'Left hip')
, ('IntraMuscMedication', 'Site (antibiotic)', 6, 'Right hip')
, ('IntraMuscMedication', 'Site (antibiotic)', 7, 'Left thigh')
, ('IntraMuscMedication', 'Site (antibiotic)', 8, 'Right thigh')
, ('IntraMuscMedication', 'Site (antibiotic)', 9, 'Other IV sites - TODO')

, ('IntraMuscMedication', 'Site (immunization)', 1, 'Left deltoid')
, ('IntraMuscMedication', 'Site (immunization)', 2, 'Right deltoid')
, ('IntraMuscMedication', 'Site (immunization)', 3, 'Left buttock')
, ('IntraMuscMedication', 'Site (immunization)', 4, 'Right buttock')
, ('IntraMuscMedication', 'Site (immunization)', 5, 'Left hip')
, ('IntraMuscMedication', 'Site (immunization)', 6, 'Right hip')
, ('IntraMuscMedication', 'Site (immunization)', 7, 'Left thigh')
, ('IntraMuscMedication', 'Site (immunization)', 8, 'Right thigh')
, ('IntraMuscMedication', 'Site (immunization)', 9, 'Other IV sites - TODO')

, ('Assessment', 'O2 Stat', 1, '100%')
, ('Assessment', 'O2 Stat', 2, '99%')
, ('Assessment', 'O2 Stat', 3, '98%')
, ('Assessment', 'O2 Stat', 4, '97%')
, ('Assessment', 'O2 Stat', 5, '96%')
, ('Assessment', 'O2 Stat', 6, '95%')
, ('Assessment', 'O2 Stat', 7, '94%')
, ('Assessment', 'O2 Stat', 8, '93%')
, ('Assessment', 'O2 Stat', 9, '92%')
, ('Assessment', 'O2 Stat', 10, '91%')
, ('Assessment', 'O2 Stat', 11, '90%')
, ('Assessment', 'O2 Stat', 12, '89%')
, ('Assessment', 'O2 Stat', 13, '88%')
, ('Assessment', 'O2 Stat', 14, '87%')
, ('Assessment', 'O2 Stat', 15, '86%')
, ('Assessment', 'O2 Stat', 16, '85%')
, ('Assessment', 'O2 Stat', 17, '84%')
, ('Assessment', 'O2 Stat', 18, '83%')
, ('Assessment', 'O2 Stat', 19, '82%')
, ('Assessment', 'O2 Stat', 20, '81%')
, ('Assessment', 'O2 Stat', 21, '80%')
, ('Assessment', 'O2 Stat', 22, '<80%')

, ('Medication', 'All of the above', 0, 'Correct patient, time, route, dose and medication confirmed prior to administration')
, ('Medication', 'All of the above', 0, 'Patient advised of actions and side-effects prior to administration')
, ('Medication', 'All of the above', 0, 'Allergies confirmed and medications reviewed prior to administration')

, ('Safety', 'All of the above', 0, 'Patient in position of comfort')
, ('Safety', 'All of the above', 0, 'Side rails up')
, ('Safety', 'All of the above', 0, 'Cart in lowest position')
, ('Safety', 'All of the above', 0, 'Family at bedside')

, ('IntraMuscMedication', 'IM (Not an antibiotic or immunization)', 0, 'Site (non-antibiotic/immunization)')
, ('IntraMuscMedication', 'IM (Not an antibiotic or immunization)', 0, 'Amount given (non-antibiotic/immunization)')
, ('IntraMuscMedication', 'IM (Not an antibiotic or immunization)', 0, 'Combined with (non-antibiotic/immunization)')

, ('IntraMuscMedication', 'IM antibiotic', 0, 'Site (antibiotic)')
, ('IntraMuscMedication', 'IM antibiotic', 0, 'Amount given (antibiotic)')
, ('IntraMuscMedication', 'IM antibiotic', 0, 'Combined with (antibiotic)')

, ('IntraMuscMedication', 'IM immunization', 0, 'Site (immunization)')
, ('IntraMuscMedication', 'IM immunization', 0, 'Amount given (immunization)')
, ('IntraMuscMedication', 'IM immunization', 0, 'Combined with (immunization)')

, ('Assessment', 'All of the above', 0, 'Correct patient, time, route, dose and medication confirmed prior to administration')
, ('Assessment', 'All of the above', 0, 'Patient advised of actions and side-effects prior to administration')
, ('Assessment', 'All of the above', 0, 'Allergies confirmed and medications reviewed prior to administration')

, ('OralMedication', 'All of the above', 0, 'Correct patient, time, route, dose and medication confirmed prior to administration')
, ('OralMedication', 'All of the above', 0, 'Patient advised of actions and side-effects prior to administration')
, ('OralMedication', 'All of the above', 0, 'Allergies confirmed and medications reviewed prior to administration')
) as [items]
([prompt_group_name], [prompt], [sequence], [choice_text]);

--Get Releated ID's for relational tables

update [target] set
    [prompt_group_id] = [source].[id]
from @prompt_choices [target]
    inner join [dbo].[prompt_groups] [source]
        on [source].[name] = [target].[prompt_group_name];

update [target] set
    [prompt_sequence] = [source].[sequence]
from @prompt_choices [target]
    inner join [dbo].[prompts] [source]
        on [source].[prompt] = [target].[prompt]
            and [source].[prompt_group_id] = [target].[prompt_group_id];

update [target] set
    [choice_text] = [source].[id]
from @prompt_choices [target]
    inner join [dbo].[prompts] [source]
        on [source].[prompt] = [target].[choice_text]
            and [source].[prompt_group_id] = [target].[prompt_group_id]
where [target].[sequence] = 0;

update [target] set
    [prompt_id] = [source].[id]
from @prompt_choices [target]
    inner join [dbo].[prompts] [source]
        on [source].[prompt] = [target].[prompt]
            and [source].[prompt_group_id] = [target].[prompt_group_id];

/***********************
*** [prompt_choices] ***
***********************/

merge into [dbo].[prompt_choices] [target]
using @prompt_choices [source]
on [target].[prompt_id] = [source].[prompt_id]
    and [target].[sequence] = [source].[sequence]
    and [target].[choice_text] = [source].[choice_text]
    when not matched by target then
        insert
        (
            [prompt_id]
          , [sequence]
          , [choice_text]
        )
        values
            ([prompt_id], [sequence], [choice_text])
    when not matched by source then
        delete;