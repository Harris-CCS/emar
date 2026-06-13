/*****************************************************************************************/
/* Data largely derived from $/Presentation/C_Development/C_DevTrunk/emar/src/app/mockup */
/*****************************************************************************************/

/**** templates.sql ****/
print 'Loading Table: templates';

declare 
    @templates table
    (
      [id]                 [int] not null
    , [name]               [nvarchar](20) not null
    , [is_active]          [bit] not null
    , [title]              [varchar](50) not null
    , [save_button_text]   [nvarchar](25) null
    , [cancel_button_text] [nvarchar](25) null);

insert into @templates
    ([id]
   , [name]
   , [is_active]
   , [title]
   , [save_button_text]
   , [cancel_button_text]
    )
select [id]
     , [name]
     , [is_active]
     , [title]
     , [save_button_text]
     , [cancel_button_text]
from   (values
			(1, 'Ear', 1, 'Ear Give Template', 'Give', 'Cancel')
			,(2, 'CancelOrder', 1, 'Cancel Order', 'Save Cancel', 'Cancel')
			,(3, 'Reschedule', 1, 'Reschedule Order', 'Confirm Reschedule', 'Cancel')
			,(4, 'Hold', 1, 'Hold Template', 'Hold', 'Cancel')
			,(5, 'Delete', 1, 'Delete Template', 'Confirm Delete', 'Cancel')
			,(6, 'MissedDose', 1, 'Missed Dose Template', 'Missed Dose', 'Cancel')
			,(7, 'Unhold', 1, 'Unhold Template', 'Unhold', 'Cancel')
			,(8, 'Discontinued', 1, 'Discontinued Template', 'Discontinued', 'Cancel')
			,(9, 'Intramuscular', 1, 'Intramuscular Give Template', 'Give', 'Cancel')
			,(10, 'Oral', 1, 'Oral Give Template', 'Give', 'Cancel')
			,(11, 'Intravenous', 1, 'Intravenous Give Template', 'Give', 'Cancel')
			,(12, 'Nasal', 1, 'Nasal Give Template', 'Give', 'Cancel')
			,(13, 'Eye', 1, 'Eye Give Template', 'Give', 'Cancel')
			,(14, 'Enteral', 1, 'Enteral Give Template', 'Give', 'Cancel')
			,(15, 'Transdermal', 1, 'Transdermal Give Template', 'Give', 'Cancel')
			,(16, 'Intradermal', 1, 'Intradermal Give Template', 'Give', 'Cancel')
			,(17, 'Inhalation', 1, 'Inhalation Give Template', 'Give', 'Cancel')
			,(18, 'IntravenousInI', 1, 'IntravenousInI Give Template', 'Give', 'Cancel')
			,(19, 'Rectal', 1, 'Rectal Give Template', 'Give', 'Cancel')
			,(20, 'Subcutaneous', 1, 'Subcutaneous Give Template', 'Give', 'Cancel')
			,(21, 'Vaginal', 1, 'Vaginal Give Template', 'Give', 'Cancel')
			,(22, 'GenericGive', 1, 'Generic Give Template', 'Give', 'Cancel')
        ) as [items]
    ([id], [name], [is_active], [title], [save_button_text], [cancel_button_text]);

/******************
*** [templates] ***
******************/

merge into [dbo].[templates] [target]
using @templates [source]
on [target].[id] = [source].[id]
    when matched and([target].[name] <> [source].[name]
                     or [target].[is_active] <> [source].[is_active]
                     or [target].[title] <> [source].[title]
                     or [target].[save_button_text] <> [source].[save_button_text]
                     or [target].[cancel_button_text] <> [source].[cancel_button_text])
        then update set 
    [name] = [source].[name]
  , [is_active] = [source].[is_active]
  , [title] = [source].[title]
  , [save_button_text] = [source].[save_button_text]
  , [cancel_button_text] = [source].[cancel_button_text]
    when not matched
        then
      insert([id]
           , [name]
           , [is_active]
           , [title]
           , [save_button_text]
           , [cancel_button_text])
      values
    ([id], [name], [is_active], [title], [save_button_text], [cancel_button_text])
    when not matched by source
        then update set 
    [is_active] = 0;

	

/**** prompt_groups.sql ****/
print 'Loading Table: prompt_groups';

declare 
    @prompt_groups table
    (
      [id]    [int] not null
    , [name]  [varchar](20) not null
    , [title] [varchar](50) not null);

insert into @prompt_groups
    ([id]
   , [name]
   , [title]
    )
select [id]
     , [name]
     , [title]
from   (values
            (1, 'Medication', 'MEDICATION'),
            (2, 'Emotional', ''),
            (3, 'Safety', 'SAFETY INTERVENTIONS'),
            (4, 'CancelReason', 'Cancellation Reasons')
			,(7, 'RescheduleDetails', '')			
			,(9, 'Notes_At_Notify', '')
			,(10, 'HoldAndMissedDose', 'Reasons')
			,(11, 'Delete', 'Confirm Delete')
			,(12, 'DeleteGeneric', '')
			,(13, 'Unhold', 'Unhold Reasons')
			,(15, 'IntraMuscMedication', 'Medication')
			,(16, 'Assessment', 'Pre-Administration Assessment')
			,(17, 'GenericGive', '')
			,(18, 'OralMedication', 'MEDICATION')
       ) as [items]
       ([id], [name], [title]);

/**********************
*** [prompt_groups] ***
**********************/

merge into [dbo].[prompt_groups] [target]
using @prompt_groups [source]
on [target].[id] = [source].[id]
    when matched and([target].[name] <> [source].[name]
                     or [target].[title] <> [source].[title])
        then update set 
    [name] = [source].[name]
  , [title] = [source].[title]
    when not matched by target
        then
      insert([id]
           , [name]
           , [title])
      values
    ([id], [name], [title]);



	
/**** template_prompt_groups.sql ****/
print 'Loading Table: template_prompt_groups';

declare 
    @template_prompt_groups table
    (
      [template_id]       [int] null
    , [sequence]          [tinyint] not null
    , [prompt_group_id]   [int] null
    , [required]          [bit] not null
    , [template_name]     [varchar](25) not null
    , [prompt_group_name] [varchar](25) not null);

insert into @template_prompt_groups
    ([template_name]
   , [sequence]
   , [prompt_group_name]
   , [required]
    )
select [template_name]
     , [sequence]
     , [prompt_group_name]
     , [required]
from   (values
            ('Ear', 1, 'Medication', 0)
            ,('Ear', 2, 'Emotional', 0)
            ,('Ear', 3, 'Safety', 0)
			,('CancelOrder', 1, 'CancelReason', 1)
			,('CancelOrder', 2, 'Notes_At_Notify', 0)
			,('Reschedule', 1, 'RescheduleDetails', 0)
			,('Hold', 1, 'HoldAndMissedDose', 1)
			,('Hold', 2, 'Notes_At_Notify', 0)
			,('Delete', 1, 'Delete', 1)
			,('MissedDose', 1, 'HoldAndMissedDose', 1)
			,('MissedDose', 2, 'Notes_At_Notify', 0)
			,('Unhold', 1, 'Unhold', 1)
			,('Unhold', 2, 'Notes_At_Notify', 0)
			,('Discontinued', 1, 'Notes_At_Notify', 0)
			,('Intramuscular', 1, 'IntraMuscMedication', 0)
			,('Intramuscular', 2, 'Assessment', 0)
			,('Intramuscular', 3, 'Emotional', 0)
			,('Intramuscular', 4, 'Safety', 0)
			,('Intramuscular', 5, 'GenericGive', 0)
            ,('Oral', 1, 'OralMedication', 0)
			,('Oral', 2, 'Emotional', 0)
			,('Oral', 3, 'Safety', 0)
			,('Oral', 4, 'GenericGive', 0)
       ) as [items]
       ([template_name], [sequence], [prompt_group_name], [required]);

--Get Releated ID's for relational tables

update [target] set    
    [template_id] = [source].[id]
from   @template_prompt_groups [target]
       inner join [dbo].[templates] [source] on [source].[name] = [target].[template_name];

update [target] set    
    [prompt_group_id] = [source].[id]
from   @template_prompt_groups [target]
       inner join [dbo].[prompt_groups] [source] on [source].[name] = [target].[prompt_group_name];

/*******************************
*** [template_prompt_groups] ***
*******************************/

merge into [dbo].[template_prompt_groups] [target]
using @template_prompt_groups [source]
on [target].[template_id] = [source].[template_id]
   and [target].[sequence] = [source].[sequence]
    when matched and([target].[prompt_group_id] <> [source].[prompt_group_id]
                     or [target].[required] <> [source].[required])
        then update set 
    [prompt_group_id] = [source].[prompt_group_id]
  , [required] = [source].[required]
    when not matched
        then
      insert([template_id]
           , [sequence]
           , [prompt_group_id]
           , [required])
      values
    ([template_id], [sequence], [prompt_group_id], [required])
    when not matched by source
        then delete;




/**** prompts.sql ****/
print 'Loading Table: prompts';

declare 
    @prompts table
    (
      [prompt_group_id]   [int] null
    , [sequence]          [smallint] not null
    , [prompt]            [nvarchar](200) not null
    , [is_active]         [bit] not null
    , [prompt_type]       [varchar](25) not null
    , [prompt_default]    [varchar](100) null
    , [required]          [bit] not null
    , [prompt_group_name] [varchar](25) not null);

insert into @prompts
    ([prompt_group_name]
   , [sequence]
   , [prompt]
   , [is_active]
   , [prompt_type]
   , [prompt_default]
   , [required]
    )
select [prompt_group_name]
     , [sequence]
     , [prompt]
     , [is_active]
     , [prompt_type]
     , [prompt_default]
     , [required]
from   (values
          	('Medication', 1, 'Verbal order read back and verified', 1, 'CheckBox', NULL, 0),
			('Medication', 2, 'Amount Given', 1, 'FreeText', NULL, 1),
			('Medication', 3, 'Administration of this medication is documented elsewhere in chart', 1, 'CheckBox', NULL, 0),
			('Medication', 4, 'Site', 1, 'DropDownListBox', NULL, 1),
			('Medication', 5, 'Correct patient, time, route, dose and medication confirmed prior to administration', 1, 'CheckBox', NULL, 0),
			('Medication', 6, 'Patient advised of actions and side-effects prior to administration', 1, 'CheckBox', NULL, 0),
			('Medication', 7, 'Allergies confirmed and medications reviewed prior to administration', 1, 'CheckBox', NULL, 0),
			('Medication', 8, 'All of the above', 1, 'CheckBoxCheckChildren', NULL, 0),

          	('OralMedication', 1, 'Verbal order read back and verified', 1, 'CheckBox', NULL, 0),
			('OralMedication', 2, 'Amount Given', 1, 'FreeText', NULL, 1),
			('OralMedication', 3, 'Administration of this medication is documented elsewhere in chart', 1, 'CheckBox', NULL, 0),
			('OralMedication', 4, 'Site', 1, 'DropDownListBox', NULL, 1),
			('OralMedication', 5, 'Medication crushed prior to administration', 1, 'CheckBox', NULL, 0),
			('OralMedication', 6, 'Mixed in', 1, 'FreeText', NULL, 0),
			('OralMedication', 7, 'Patient vomited during or soon after administration', 1, 'CheckBox', NULL, 0),
			('OralMedication', 8, 'Correct patient, time, route, dose and medication confirmed prior to administration', 1, 'CheckBox', NULL, 0),
			('OralMedication', 9, 'Patient advised of actions and side-effects prior to administration', 1, 'CheckBox', NULL, 0),
			('OralMedication', 10, 'Allergies confirmed and medications reviewed prior to administration', 1, 'CheckBox', NULL, 0),
			('OralMedication', 11, 'All of the above', 1, 'CheckBox', NULL, 0),

			('Emotional', 1, 'Emotional support needed and given', 1, 'CheckBox', NULL, 0),
			('Emotional', 2, 'Tolerated Procedure', 1, 'DropDownListBox', NULL, 0),
			('Emotional', 3, 'Additional Staff Required', 1, 'DropDownListBox', NULL, 0),
			('Emotional', 4, 'Reason', 1, 'DropDownListBox', NULL, 0),
			('Emotional', 5, 'Administered by', 1, 'FreeText', NULL, 0),
			('Safety', 1, 'Patient in position of comfort', 1, 'CheckBox', NULL, 0),
			('Safety', 2, 'Side rails up', 1, 'CheckBox', NULL, 0),
			('Safety', 3, 'Cart in lowest position', 1, 'CheckBox', NULL, 0),
			('Safety', 4, 'Family at bedside', 1, 'CheckBox', NULL, 0),
			('Safety', 5, 'All of the above', 1, 'CheckBoxCheckChildren', NULL, 0),
			('Safety', 6, 'Friend at beside', 1, 'CheckBox', NULL, 0),
			('Safety', 7, 'Call light in reach', 1, 'CheckBox', NULL, 0),
			('Safety', 8, 'Other:', 1, 'MultiLineFreeText', NULL, 0),
			('CancelReason', 1, 'Symptoms resolved', 1, 'CheckBox', NULL, 0),
			('CancelReason', 2, 'Patient refused', 1, 'CheckBox', NULL, 0),
			('CancelReason', 3, 'Change in medication plan', 1, 'CheckBox', NULL, 0),
            
			('Notes_At_Notify', 1, 'Notes', 1, 'MultiLineFreeText', NULL, 0),
			('Notes_At_Notify', 2, 'At', 1, 'DateTime', 'Now', 1),
			('Notes_At_Notify', 3, 'Notify', 1, 'Notify', NULL, 0),
			('RescheduleDetails', 1, 'Reschedule to', 1, 'DateTime', 'Now', 1),
			('RescheduleDetails', 2, 'All future administration times will be updated based on the previously entered frequency.', 1, 'Information', NULL, 0)
			,
			('HoldAndMissedDose', 1, 'Vital signs out of range', 1, 'CheckBox', NULL, 0),
			('HoldAndMissedDose', 2, 'Vital signs stabilized', 1, 'CheckBox', NULL, 0),
			('HoldAndMissedDose', 3, 'Patient refused', 1, 'CheckBox', NULL, 0),
			('HoldAndMissedDose', 4, 'Pain controlled at present', 1, 'CheckBox', NULL, 0),
			('HoldAndMissedDose', 5, 'Symptoms controlled at present', 1, 'CheckBox', NULL, 0),
			('HoldAndMissedDose', 6, 'Awaiting order confirmation', 1, 'CheckBox', NULL, 0),
			('HoldAndMissedDose', 7, 'Catheter/tube placement can not be confirmed', 1, 'CheckBox', NULL, 0),
			('HoldAndMissedDose', 8, 'Administration route unavailable', 1, 'CheckBox', NULL, 0),
			('HoldAndMissedDose', 9, 'Attending physician aware', 1, 'CheckBox', NULL, 0),
			('HoldAndMissedDose', 10, 'Out of department', 1, 'CheckBox', NULL, 0),

			('Delete', 1, 'Are you sure you want to delete this order?', 1, 'Information', NULL, 0),
			('Unhold', 1, 'Vital signs improved', 1, 'CheckBox', NULL, 0),
			('Unhold', 2, 'Patient currently in department', 1, 'CheckBox', NULL, 0),
			('Unhold', 3, 'Patient consents', 1, 'CheckBox', NULL, 0),
			('Unhold', 4, 'Pain not controlled at present', 1, 'CheckBox', NULL, 0),
			('Unhold', 5, 'Received order confirmation', 1, 'CheckBox', NULL, 0),
			('Unhold', 6, 'Returned to department', 1, 'CheckBox', NULL, 0),

			('GenericGive', 1, 'Notes', 1, 'MultiLineFreeText', NULL, 0),
			('GenericGive', 2, 'Given At', 1, 'DateTime', 'Now', 1),
			('GenericGive', 3, 'Self Administered', 1, 'CheckBox', NULL, 0),
			('GenericGive', 4, 'Patient Supplied', 1, 'CheckBox', NULL, 0),
			('GenericGive', 5, 'Notify', 1, 'Notify', NULL, 0),

			('IntraMuscMedication', 1, 'Verbal order read back and verified', 1, 'CheckBox', NULL, 0),

			('IntraMuscMedication', 2, 'IM (Not an antibiotic or immunization)', 1, 'CheckBoxShowChildren', NULL, 0),
			('IntraMuscMedication', 3, 'Site (non-antibiotic/immunization)', 1, 'DropDownListBox', NULL, 1),
			('IntraMuscMedication', 4, 'Amount given (non-antibiotic/immunization)', 1, 'CheckBox', NULL, 0),
			('IntraMuscMedication', 5, 'Combined with (non-antibiotic/immunization)', 1, 'CheckBox', NULL, 0),

			('IntraMuscMedication', 6, 'IM antibiotic', 1, 'CheckBoxShowChildren', NULL, 0),
			('IntraMuscMedication', 7, 'Site (antibiotic)', 1, 'DropDownListBox', NULL, 1),
			('IntraMuscMedication', 8, 'Amount given (antibiotic)', 1, 'CheckBox', NULL, 0),
			('IntraMuscMedication', 9, 'Combined with (antibiotic)', 1, 'CheckBox', NULL, 0),

			('IntraMuscMedication', 10, 'IM immunization', 1, 'CheckBoxShowChildren', NULL, 0),
			('IntraMuscMedication', 11, 'Site (immunization)', 1, 'DropDownListBox', NULL, 1),
			('IntraMuscMedication', 12, 'Amount given (immunization)', 1, 'CheckBox', NULL, 0),
			('IntraMuscMedication', 13, 'Combined with (immunization)', 1, 'CheckBox', NULL, 0),

			('Assessment', 1, 'O2 Stat', 1, 'DropDownListBox', NULL, 1),
			('Assessment', 2, 'O2 Amount', 1, 'DropDownListBox', NULL, 1),
			('Assessment', 3, 'O2 Type', 1, 'DropDownListBox', NULL, 1),
			('Assessment', 4, 'Rhythm', 1, 'DropDownListBox', NULL, 1),
			('Assessment', 5, 'Ectopy', 1, 'DropDownListBox', NULL, 1),
			('Assessment', 6, 'St changes', 1, 'DropDownListBox', NULL, 1)			,
			('Assessment', 7, 'Correct patient, time, route, dose and medication confirmed prior to administration', 1, 'CheckBox', NULL, 0),
			('Assessment', 8, 'Patient advised of actions and side-effects prior to administration', 1, 'CheckBox', NULL, 0),
			('Assessment', 9, 'Allergies confirmed and medications reviewed prior to administration', 1, 'CheckBox', NULL, 0),
			('Assessment', 10, 'All of the above', 1, 'CheckBoxCheckChildren', NULL, 0)
       ) as [items]
       ([prompt_group_name], [sequence], [prompt], [is_active], [prompt_type], [prompt_default], [required]);

--Get Releated ID's for relational tables

update [target] set    
    [prompt_group_id] = [source].[id]
from   @prompts [target]
       inner join [dbo].[prompt_groups] [source] on [source].[name] = [target].[prompt_group_name];

/****************
*** [prompts] ***
****************/

merge into [dbo].[prompts] [target]
using @prompts [source]
on [target].[prompt_group_id] = [source].[prompt_group_id]
   and [target].[sequence] = [source].[sequence]
    when matched and([target].[prompt] <> [source].[prompt]
                     or [target].[is_active] <> [source].[is_active]
                     or [target].[prompt_type] <> [source].[prompt_type]
                     or [target].[prompt_default] <> [source].[prompt_default]
                     or [target].[required] <> [source].[required])
        then update set 
    [prompt] = [source].[prompt]
  , [is_active] = [source].[is_active]
  , [prompt_type] = [source].[prompt_type]
  , [prompt_default] = [source].[prompt_default]
  , [required] = [source].[required]
    when not matched by target
        then
      insert([prompt_group_id]
           , [sequence]
           , [prompt]
           , [is_active]
           , [prompt_type]
           , [prompt_default]
           , [required])
      values
    ([prompt_group_id], [sequence], [prompt], [is_active], [prompt_type], [prompt_default], [required])
    when not matched by source
        then delete;




/**** prompts_choices.sql ****/
print 'Loading Table: prompt_choices';

declare 
    @prompt_choices table
    (
      [prompt_id]         [int] null
    , [sequence]          [smallint] not null
    , [choice_text]       [nvarchar](200) not null
    , [prompt_group_id]   [int] null
    , [prompt_sequence]   [int] null
    , [prompt_group_name] [varchar](50) not null
    , [prompt]            [nvarchar](200) not null);

insert into @prompt_choices
    ([prompt_group_name]
   , [prompt]
   , [sequence]
   , [choice_text]
    )
select [prompt_group_name]
     , [prompt]
     , [sequence]
     , [choice_text]
from   (values
			('Medication', 'Site', 1, 'Left'),
			('Medication', 'Site', 2, 'Right'),
			('Medication', 'Site', 3, 'Bilaterally'),

			('OralMedication', 'Site', 1, 'P.O.'),
			('OralMedication', 'Site', 2, 'S.L.'),
			('OralMedication', 'Site', 3, 'Buccal'),

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
		
			('IntraMuscMedication', 'Site (non-antibiotic/immunization)', 1, 'Left deltoid'),
			('IntraMuscMedication', 'Site (non-antibiotic/immunization)', 2, 'Right deltoid'),
			('IntraMuscMedication', 'Site (non-antibiotic/immunization)', 3, 'Left buttock'),
			('IntraMuscMedication', 'Site (non-antibiotic/immunization)', 4, 'Right buttock'),
			('IntraMuscMedication', 'Site (non-antibiotic/immunization)', 5, 'Left hip'),
			('IntraMuscMedication', 'Site (non-antibiotic/immunization)', 6, 'Right hip'),
			('IntraMuscMedication', 'Site (non-antibiotic/immunization)', 7, 'Left thigh'),
			('IntraMuscMedication', 'Site (non-antibiotic/immunization)', 8, 'Right thigh'),
			('IntraMuscMedication', 'Site (non-antibiotic/immunization)', 9, 'Other IV sites - TODO'),

			('IntraMuscMedication', 'Site (antibiotic)', 1, 'Left deltoid'),
			('IntraMuscMedication', 'Site (antibiotic)', 2, 'Right deltoid'),
			('IntraMuscMedication', 'Site (antibiotic)', 3, 'Left buttock'),
			('IntraMuscMedication', 'Site (antibiotic)', 4, 'Right buttock'),
			('IntraMuscMedication', 'Site (antibiotic)', 5, 'Left hip'),
			('IntraMuscMedication', 'Site (antibiotic)', 6, 'Right hip'),
			('IntraMuscMedication', 'Site (antibiotic)', 7, 'Left thigh'),
			('IntraMuscMedication', 'Site (antibiotic)', 8, 'Right thigh'),
			('IntraMuscMedication', 'Site (antibiotic)', 9, 'Other IV sites - TODO'),

			('IntraMuscMedication', 'Site (immunization)', 1, 'Left deltoid'),
			('IntraMuscMedication', 'Site (immunization)', 2, 'Right deltoid'),
			('IntraMuscMedication', 'Site (immunization)', 3, 'Left buttock'),
			('IntraMuscMedication', 'Site (immunization)', 4, 'Right buttock'),
			('IntraMuscMedication', 'Site (immunization)', 5, 'Left hip'),
			('IntraMuscMedication', 'Site (immunization)', 6, 'Right hip'),
			('IntraMuscMedication', 'Site (immunization)', 7, 'Left thigh'),
			('IntraMuscMedication', 'Site (immunization)', 8, 'Right thigh'),
			('IntraMuscMedication', 'Site (immunization)', 9, 'Other IV sites - TODO'),

			('Assessment', 'O2 Stat', 1, '100%'),
			('Assessment', 'O2 Stat', 2, '99%'),
			('Assessment', 'O2 Stat', 3, '98%'),
			('Assessment', 'O2 Stat', 4, '97%'),
			('Assessment', 'O2 Stat', 5, '96%'),
			('Assessment', 'O2 Stat', 6, '95%'),
			('Assessment', 'O2 Stat', 7, '94%'),
			('Assessment', 'O2 Stat', 8, '93%'),
			('Assessment', 'O2 Stat', 9, '92%'),
			('Assessment', 'O2 Stat', 10, '91%'),
			('Assessment', 'O2 Stat', 11, '90%'),
			('Assessment', 'O2 Stat', 12, '89%'),
			('Assessment', 'O2 Stat', 13, '88%'),
			('Assessment', 'O2 Stat', 14, '87%'),
			('Assessment', 'O2 Stat', 15, '86%'),
			('Assessment', 'O2 Stat', 16, '85%'),
			('Assessment', 'O2 Stat', 17, '84%'),
			('Assessment', 'O2 Stat', 18, '83%'),
			('Assessment', 'O2 Stat', 19, '82%'),
			('Assessment', 'O2 Stat', 20, '81%'),
			('Assessment', 'O2 Stat', 21, '80%'),
			('Assessment', 'O2 Stat', 22, '<80%'),

			('Medication', 'All of the above', 0, 'Correct patient, time, route, dose and medication confirmed prior to administration'),
			('Medication', 'All of the above', 0, 'Patient advised of actions and side-effects prior to administration'),
			('Medication', 'All of the above', 0, 'Allergies confirmed and medications reviewed prior to administration'),

			('Safety', 'All of the above', 0, 'Patient in position of comfort'),
			('Safety', 'All of the above', 0, 'Side rails up'),
			('Safety', 'All of the above', 0, 'Cart in lowest position'),
			('Safety', 'All of the above', 0, 'Family at bedside'),

			('IntraMuscMedication', 'IM (Not an antibiotic or immunization)', 0, 'Site (non-antibiotic/immunization)'),
			('IntraMuscMedication', 'IM (Not an antibiotic or immunization)', 0, 'Amount given (non-antibiotic/immunization)'),
			('IntraMuscMedication', 'IM (Not an antibiotic or immunization)', 0, 'Combined with (non-antibiotic/immunization)'),

			('IntraMuscMedication', 'IM antibiotic', 0, 'Site (antibiotic)'),
			('IntraMuscMedication', 'IM antibiotic', 0, 'Amount given (antibiotic)'),
			('IntraMuscMedication', 'IM antibiotic', 0, 'Combined with (antibiotic)'),

			('IntraMuscMedication', 'IM immunization', 0, 'Site (immunization)'),
			('IntraMuscMedication', 'IM immunization', 0, 'Amount given (immunization)'),
			('IntraMuscMedication', 'IM immunization', 0, 'Combined with (immunization)'),

			('Assessment', 'All of the above', 0, 'Correct patient, time, route, dose and medication confirmed prior to administration'),
			('Assessment', 'All of the above', 0, 'Patient advised of actions and side-effects prior to administration'),
			('Assessment', 'All of the above', 0, 'Allergies confirmed and medications reviewed prior to administration')

			,('OralMedication', 'All of the above', 0, 'Correct patient, time, route, dose and medication confirmed prior to administration')
			,('OralMedication', 'All of the above', 0, 'Patient advised of actions and side-effects prior to administration')
			,('OralMedication', 'All of the above', 0, 'Allergies confirmed and medications reviewed prior to administration')
       ) as [items]
       ([prompt_group_name], [prompt], [sequence], [choice_text]);

--Get Releated ID's for relational tables

update [target] set    
    [prompt_group_id] = [source].[id]
from   @prompt_choices [target]
       inner join [dbo].[prompt_groups] [source] on [source].[name] = [target].[prompt_group_name];

update [target] set    
    [prompt_sequence] = [source].[sequence]
from   @prompt_choices [target]
       inner join [dbo].[prompts] [source] on [source].[prompt] = [target].[prompt]
                                              and [source].[prompt_group_id] = [target].[prompt_group_id];

update [target] set    
    [choice_text] = [source].[id]
from   @prompt_choices [target]
       inner join [dbo].[prompts] [source] on [source].[prompt] = [target].[choice_text]
                                              and [source].[prompt_group_id] = [target].[prompt_group_id]
where  [target].[sequence] = 0;

update [target] set    
    [prompt_id] = [source].[id]
from   @prompt_choices [target]
       inner join [dbo].[prompts] [source] on [source].[prompt] = [target].[prompt]
                                              and [source].[prompt_group_id] = [target].[prompt_group_id];

/***********************
*** [prompt_choices] ***
***********************/

merge into [dbo].[prompt_choices] [target]
using @prompt_choices [source]
on [target].[prompt_id] = [source].[prompt_id]
   and [target].[sequence] = [source].[sequence]
   and [target].[choice_text] = [source].[choice_text]
    when not matched by target
        then
      insert([prompt_id]
           , [sequence]
           , [choice_text])
      values
    ([prompt_id], [sequence], [choice_text])
    when not matched by source
        then delete;




/**** [action_route_templates] ****/
WITH NonRouteSpecificMappings AS (
	SELECT	ActionName, RouteName, TemplateName, SiteId
	FROM   (VALUES
		('Cancel', NULL, NULL, 'CancelOrder')
		,('Reschedule', NULL, NULL, 'Reschedule')
		,('Delete', NULL, NULL, 'Delete')
		,('Hold', NULL, NULL, 'Hold')
		,('MissedDose', NULL, NULL, 'MissedDose')
		,('Unhold', NULL, NULL, 'Unhold')
		,('CompleteDiscontinue', NULL, NULL, 'Discontinued')
		,('Give', NULL, NULL, 'GenericGive')
	) AS s (ActionName, RouteName, SiteId, TemplateName)
),
UniqueRouteTemplateCombos AS (
        SELECT name, misc
        FROM   ibex.dbo.idx 
        WHERE  type = 'AC'
        AND            LTRIM(ISNULL(misc, '')) != ''
        GROUP BY name, misc
)
, DuplicatedRoutes AS (
        SELECT name
        FROM   UniqueRouteTemplateCombos
        GROUP BY name
        HAVING count(*) > 1
)
, NonSiteSpecificMappings AS (
        SELECT DISTINCT 
                       RouteName = u.name, 
                       TemplateName = CASE i.misc WHEN 'intramuscular5.7' THEN 'intramuscular' ELSE i.misc END,
                       site_id = CONVERT(tinyint, NULL)
        FROM   UniqueRouteTemplateCombos u
        LEFT JOIN DuplicatedRoutes d
                       ON u.name = d.name
        JOIN   ibex.dbo.idx i 
                       ON type = 'AC'
                       AND u.name = i.name
        WHERE  d.name IS NULL
        AND            LTRIM(ISNULL(i.misc, '')) != ''
)
, SiteSpecificMappings AS (
        SELECT DISTINCT 
                       RouteName = u.name, 
                       TemplateName = CASE i.misc WHEN 'intramuscular5.7' THEN 'intramuscular' ELSE i.misc END,
                       site_id = i.site
        FROM   UniqueRouteTemplateCombos u
        JOIN   DuplicatedRoutes d
                       ON u.name = d.name
        JOIN   ibex.dbo.idx i 
                       ON type = 'AC'
                       AND u.name = i.name
)
, SourceMappings AS (
        SELECT 'Give' as action, * FROM NonSiteSpecificMappings
			UNION
        SELECT 'Give', * FROM SiteSpecificMappings
			UNION
		SELECT * FROM NonRouteSpecificMappings
)
, src AS (
	SELECT	a.id as action_id
			,r.id as medication_route_id
			,t.id as template_id
			,CONVERT(int, s.internal_id) as site_id
	FROM	SourceMappings m
	JOIN	dbo.templates t
			ON m.TemplateName = t.name
	JOIN	dbo.actions a
			ON m.action = a.name
	LEFT JOIN dbo.external_ids s -- convert ibex site to emar site
			ON convert(varchar(10), m.site_id) = s.external_id
			AND s.entity = 'sites'
			AND s.vendor = 'pulsecheck'
	LEFT JOIN dbo.medication_routes r
			ON m.RouteName = r.name
)
MERGE INTO [dbo].action_route_templates tar
USING src
	ON tar.action_id = src.action_id
	AND ISNULL(tar.medication_route_id, - 1) = ISNULL(src.medication_route_id, -1)
	AND ISNULL(tar.site_id, - 1) = ISNULL(src.site_id, - 1)
WHEN NOT MATCHED THEN
	INSERT	(action_id, medication_route_id, site_id, template_id)
	VALUES	(action_id, medication_route_id, site_id, template_id)
WHEN MATCHED AND tar.template_id != src.template_id THEN
	UPDATE SET template_id = src.template_id;
