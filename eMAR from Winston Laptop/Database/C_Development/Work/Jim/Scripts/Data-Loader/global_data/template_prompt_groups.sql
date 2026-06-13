print 'Loading Table: template_prompt_groups';

declare
    @template_prompt_groups table
        (
            [template_id]       [int]         null
          , [sequence]          [tinyint]     not null
          , [prompt_group_id]   [int]         null
          , [required]          [bit]         not null
          , [template_name]     [varchar](25) not null
          , [prompt_group_name] [varchar](25) not null
        );

insert into @template_prompt_groups
(
    [template_name]
  , [sequence]
  , [prompt_group_name]
  , [required]
)
select
    [template_name]
  , [sequence]
  , [prompt_group_name]
  , [required]
from (
values
('Ear', 1, 'Medication', 0)
, ('Ear', 2, 'Emotional', 0)
, ('Ear', 3, 'Safety', 0)
, ('CancelOrder', 1, 'CancelReason', 1)
, ('CancelOrder', 2, 'Notes_At_Notify', 0)
, ('Reschedule', 1, 'RescheduleDetails', 0)
, ('Hold', 1, 'HoldAndMissedDose', 1)
, ('Hold', 2, 'Notes_At_Notify', 0)
, ('Delete', 1, 'Delete', 1)
, ('MissedDose', 1, 'HoldAndMissedDose', 1)
, ('MissedDose', 2, 'Notes_At_Notify', 0)
, ('Unhold', 1, 'Unhold', 1)
, ('Unhold', 2, 'Notes_At_Notify', 0)
, ('Discontinued', 1, 'Notes_At_Notify', 0)
, ('Intramuscular', 1, 'IntraMuscMedication', 0)
, ('Intramuscular', 2, 'Assessment', 0)
, ('Intramuscular', 3, 'Emotional', 0)
, ('Intramuscular', 4, 'Safety', 0)
, ('Intramuscular', 5, 'GenericGive', 0)
, ('Oral', 1, 'OralMedication', 0)
, ('Oral', 2, 'Emotional', 0)
, ('Oral', 3, 'Safety', 0)
, ('Oral', 4, 'GenericGive', 0)
) as [items]
([template_name], [sequence], [prompt_group_name], [required]);

--Get Releated ID's for relational tables

update [target] set
    [template_id] = [source].[id]
from @template_prompt_groups [target]
    inner join [dbo].[templates] [source]
        on [source].[name] = [target].[template_name];

update [target] set
    [prompt_group_id] = [source].[id]
from @template_prompt_groups [target]
    inner join [dbo].[prompt_groups] [source]
        on [source].[name] = [target].[prompt_group_name];

/*******************************
*** [template_prompt_groups] ***
*******************************/

merge into [dbo].[template_prompt_groups] [target]
using @template_prompt_groups [source]
on [target].[template_id] = [source].[template_id]
    and [target].[sequence] = [source].[sequence]
    when matched
        and ([target].[prompt_group_id] <> [source].[prompt_group_id]
            or [target].[required] <> [source].[required]) then
        update set
            [prompt_group_id] = [source].[prompt_group_id]
          , [required]        = [source].[required]
    when not matched then
        insert
        (
            [template_id]
          , [sequence]
          , [prompt_group_id]
          , [required]
        )
        values
            ([template_id], [sequence], [prompt_group_id], [required])
    when not matched by source then
        delete;