print 'Loading Table: prompts';

declare
    @prompts table
        (
            [prompt_group_id]   [int]           null
          , [sequence]          [smallint]      not null
          , [prompt]            [nvarchar](200) not null
          , [is_active]         [bit]           not null
          , [prompt_type]       [varchar](25)   not null
          , [prompt_default]    [varchar](100)  null
          , [required]          [bit]           not null
          , [prompt_group_name] [varchar](25)   not null
        );

insert into @prompts
(
    [prompt_group_name]
  , [sequence]
  , [prompt]
  , [is_active]
  , [prompt_type]
  , [prompt_default]
  , [required]
)
select
    [prompt_group_name]
  , [sequence]
  , [prompt]
  , [is_active]
  , [prompt_type]
  , [prompt_default]
  , [required]
from (
values
    ('Medication', 1, 'Verbal order read back and verified', 1, 'CheckBox', null, 0)
  , ('Medication', 2, 'Amount Given', 1, 'FreeText', null, 1)
  , ('Medication', 3, 'Administration of this medication is documented elsewhere in chart', 1, 'CheckBox', null, 0)
  , ('Medication', 4, 'Site', 1, 'DropDownListBox', null, 1)
  , ('Medication', 5, 'Correct patient, time, route, dose and medication confirmed prior to administration', 1, 'CheckBox', null, 0)
  , ('Medication', 6, 'Patient advised of actions and side-effects prior to administration', 1, 'CheckBox', null, 0)
  , ('Medication', 7, 'Allergies confirmed and medications reviewed prior to administration', 1, 'CheckBox', null, 0)
  , ('Medication', 8, 'All of the above', 1, 'CheckBoxCheckChildren', null, 0)

  , ('OralMedication', 1, 'Verbal order read back and verified', 1, 'CheckBox', null, 0)
  , ('OralMedication', 2, 'Amount Given', 1, 'FreeText', null, 1)
  , ('OralMedication', 3, 'Administration of this medication is documented elsewhere in chart', 1, 'CheckBox', null, 0)
  , ('OralMedication', 4, 'Site', 1, 'DropDownListBox', null, 1)
  , ('OralMedication', 5, 'Medication crushed prior to administration', 1, 'CheckBox', null, 0)
  , ('OralMedication', 6, 'Mixed in', 1, 'FreeText', null, 0)
  , ('OralMedication', 7, 'Patient vomited during or soon after administration', 1, 'CheckBox', null, 0)
  , ('OralMedication', 8, 'Correct patient, time, route, dose and medication confirmed prior to administration', 1, 'CheckBox', null, 0)
  , ('OralMedication', 9, 'Patient advised of actions and side-effects prior to administration', 1, 'CheckBox', null, 0)
  , ('OralMedication', 10, 'Allergies confirmed and medications reviewed prior to administration', 1, 'CheckBox', null, 0)
  , ('OralMedication', 11, 'All of the above', 1, 'CheckBox', null, 0)

  , ('Emotional', 1, 'Emotional support needed and given', 1, 'CheckBox', null, 0)
  , ('Emotional', 2, 'Tolerated Procedure', 1, 'DropDownListBox', null, 0)
  , ('Emotional', 3, 'Additional Staff Required', 1, 'DropDownListBox', null, 0)
  , ('Emotional', 4, 'Reason', 1, 'DropDownListBox', null, 0)
  , ('Emotional', 5, 'Administered by', 1, 'FreeText', null, 0)
  , ('Safety', 1, 'Patient in position of comfort', 1, 'CheckBox', null, 0)
  , ('Safety', 2, 'Side rails up', 1, 'CheckBox', null, 0)
  , ('Safety', 3, 'Cart in lowest position', 1, 'CheckBox', null, 0)
  , ('Safety', 4, 'Family at bedside', 1, 'CheckBox', null, 0)
  , ('Safety', 5, 'All of the above', 1, 'CheckBoxCheckChildren', null, 0)
  , ('Safety', 6, 'Friend at beside', 1, 'CheckBox', null, 0)
  , ('Safety', 7, 'Call light in reach', 1, 'CheckBox', null, 0)
  , ('Safety', 8, 'Other:', 1, 'MultiLineFreeText', null, 0)
  , ('CancelReason', 1, 'Symptoms resolved', 1, 'CheckBox', null, 0)
  , ('CancelReason', 2, 'Patient refused', 1, 'CheckBox', null, 0)
  , ('CancelReason', 3, 'Change in medication plan', 1, 'CheckBox', null, 0)

  , ('Notes_At_Notify', 1, 'Notes', 1, 'MultiLineFreeText', null, 0)
  , ('Notes_At_Notify', 2, 'At', 1, 'DateTime', 'Now', 1)
  , ('Notes_At_Notify', 3, 'Notify', 1, 'Notify', null, 0)
  , ('RescheduleDetails', 1, 'Reschedule to', 1, 'DateTime', 'Now', 1)
  , ('RescheduleDetails', 2, 'All future administration times will be updated based on the previously entered frequency.', 1, 'Information', null, 0)

  , ('HoldAndMissedDose', 1, 'Vital signs out of range', 1, 'CheckBox', null, 0)
  , ('HoldAndMissedDose', 2, 'Vital signs stabilized', 1, 'CheckBox', null, 0)
  , ('HoldAndMissedDose', 3, 'Patient refused', 1, 'CheckBox', null, 0)
  , ('HoldAndMissedDose', 4, 'Pain controlled at present', 1, 'CheckBox', null, 0)
  , ('HoldAndMissedDose', 5, 'Symptoms controlled at present', 1, 'CheckBox', null, 0)
  , ('HoldAndMissedDose', 6, 'Awaiting order confirmation', 1, 'CheckBox', null, 0)
  , ('HoldAndMissedDose', 7, 'Catheter/tube placement can not be confirmed', 1, 'CheckBox', null, 0)
  , ('HoldAndMissedDose', 8, 'Administration route unavailable', 1, 'CheckBox', null, 0)
  , ('HoldAndMissedDose', 9, 'Attending physician aware', 1, 'CheckBox', null, 0)
  , ('HoldAndMissedDose', 10, 'Out of department', 1, 'CheckBox', null, 0)

  , ('Delete', 1, 'Are you sure you want to delete this order?', 1, 'Information', null, 0)
  , ('Unhold', 1, 'Vital signs improved', 1, 'CheckBox', null, 0)
  , ('Unhold', 2, 'Patient currently in department', 1, 'CheckBox', null, 0)
  , ('Unhold', 3, 'Patient consents', 1, 'CheckBox', null, 0)
  , ('Unhold', 4, 'Pain not controlled at present', 1, 'CheckBox', null, 0)
  , ('Unhold', 5, 'Received order confirmation', 1, 'CheckBox', null, 0)
  , ('Unhold', 6, 'Returned to department', 1, 'CheckBox', null, 0)

  , ('GenericGive', 1, 'Notes', 1, 'MultiLineFreeText', null, 0)
  , ('GenericGive', 2, 'Given At', 1, 'DateTime', 'Now', 1)
  , ('GenericGive', 3, 'Self Administered', 1, 'CheckBox', null, 0)
  , ('GenericGive', 4, 'Patient Supplied', 1, 'CheckBox', null, 0)
  , ('GenericGive', 5, 'Notify', 1, 'Notify', null, 0)

  , ('IntraMuscMedication', 1, 'Verbal order read back and verified', 1, 'CheckBox', null, 0)

  , ('IntraMuscMedication', 2, 'IM (Not an antibiotic or immunization)', 1, 'CheckBoxShowChildren', null, 0)
  , ('IntraMuscMedication', 3, 'Site (non-antibiotic/immunization)', 1, 'DropDownListBox', null, 1)
  , ('IntraMuscMedication', 4, 'Amount given (non-antibiotic/immunization)', 1, 'CheckBox', null, 0)
  , ('IntraMuscMedication', 5, 'Combined with (non-antibiotic/immunization)', 1, 'CheckBox', null, 0)

  , ('IntraMuscMedication', 6, 'IM antibiotic', 1, 'CheckBoxShowChildren', null, 0)
  , ('IntraMuscMedication', 7, 'Site (antibiotic)', 1, 'DropDownListBox', null, 1)
  , ('IntraMuscMedication', 8, 'Amount given (antibiotic)', 1, 'CheckBox', null, 0)
  , ('IntraMuscMedication', 9, 'Combined with (antibiotic)', 1, 'CheckBox', null, 0)

  , ('IntraMuscMedication', 10, 'IM immunization', 1, 'CheckBoxShowChildren', null, 0)
  , ('IntraMuscMedication', 11, 'Site (immunization)', 1, 'DropDownListBox', null, 1)
  , ('IntraMuscMedication', 12, 'Amount given (immunization)', 1, 'CheckBox', null, 0)
  , ('IntraMuscMedication', 13, 'Combined with (immunization)', 1, 'CheckBox', null, 0)

  , ('Assessment', 1, 'O2 Stat', 1, 'DropDownListBox', null, 1)
  , ('Assessment', 2, 'O2 Amount', 1, 'DropDownListBox', null, 1)
  , ('Assessment', 3, 'O2 Type', 1, 'DropDownListBox', null, 1)
  , ('Assessment', 4, 'Rhythm', 1, 'DropDownListBox', null, 1)
  , ('Assessment', 5, 'Ectopy', 1, 'DropDownListBox', null, 1)
  , ('Assessment', 6, 'St changes', 1, 'DropDownListBox', null, 1)
  , ('Assessment', 7, 'Correct patient, time, route, dose and medication confirmed prior to administration', 1, 'CheckBox', null, 0)
  , ('Assessment', 8, 'Patient advised of actions and side-effects prior to administration', 1, 'CheckBox', null, 0)
  , ('Assessment', 9, 'Allergies confirmed and medications reviewed prior to administration', 1, 'CheckBox', null, 0)
  , ('Assessment', 10, 'All of the above', 1, 'CheckBoxCheckChildren', null, 0)
) as [items]
([prompt_group_name], [sequence], [prompt], [is_active], [prompt_type], [prompt_default], [required]);

--Get Releated ID's for relational tables

update [target] set
    [prompt_group_id] = [source].[id]
from @prompts [target]
    inner join [dbo].[prompt_groups] [source]
        on [source].[name] = [target].[prompt_group_name];

/****************
*** [prompts] ***
****************/

merge into [dbo].[prompts] [target]
using @prompts [source]
on [target].[prompt_group_id] = [source].[prompt_group_id]
    and [target].[prompt] = [source].[prompt]
    when matched
        and ([target].[sequence] <> [source].[sequence]
            or [target].[is_active] <> [source].[is_active]
            or [target].[prompt_type] <> [source].[prompt_type]
            or [target].[prompt_default] <> [source].[prompt_default]
            or [target].[required] <> [source].[required]) then
        update set
            [sequence]       = [source].[sequence]
          , [is_active]      = [source].[is_active]
          , [prompt_type]    = [source].[prompt_type]
          , [prompt_default] = [source].[prompt_default]
          , [required]       = [source].[required]
    when not matched by target then
        insert
        (
            [prompt_group_id]
          , [sequence]
          , [prompt]
          , [is_active]
          , [prompt_type]
          , [prompt_default]
          , [required]
        )
        values
            ([prompt_group_id], [sequence], [prompt], [is_active], [prompt_type], [prompt_default], [required])
    when not matched by source then
        delete;