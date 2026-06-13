/*****************************************************************************************/
/* Data largely derived from $/Presentation/C_Development/C_DevTrunk/emar/src/app/mockup */
/*****************************************************************************************/
use emar;
go
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
			,(23, 'Intraosseous', 1, 'Intraosseous Give Template', 'Give', 'Cancel')
			,(24, 'FollowUp', 1, 'Follow-up Template', 'Enter', 'Cancel')
			,(25, 'OrderDiscontinue', 1, 'Order Discontinue Template', 'Enter', 'Cancel')
			,(26, 'PharmVerification', 1, 'Pharmacy Verification Needed Template', 'Verify', 'Cancel')
			,(27, 'CoSign', 1, 'Cosign Template', 'Co-Sign', 'Cancel')
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
            (1, 'Medication', 'Medication'),
            (2, 'Emotional', ''),
            (3, 'Safety', 'Safety Interventions'),
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
			,(18, 'OralMedication', 'Medication')
			,(19, 'DefaultGive', '')
			,(20, 'EnteralMedication', 'Medication')
			,(21, 'AmbulateSafety', 'Safety Interventions')
			,(22, 'NasalMedication', 'Medication')
			,(23, 'InhalationMedication', 'Medication')
			,(24, 'InhalationAssessment', 'Pre-Administration Assessment')
			,(25, 'IntraDermMedication', 'Medication')
			,(26, 'IntraOssMedication', 'Medication')
			,(27, 'IntraOssAssessment', 'Pre-Administration Assessment')
			,(28, 'RectalMedication', 'Medication')
			,(29, 'TransDermMedication', 'Medication')
			,(30, 'VaginalMedication', 'Medication')
			,(31, 'SubcutanMedication', 'Medication')
			,(32, 'IVMedication', 'Medication')
			,(33, 'IVAssessment', 'Pre-Administration Assessment')
			,(34, 'IVSafety', 'Safety Interventions')
			,(35, 'GeneralAssessment', 'Assessment')
			,(36, 'SiteInspection', 'Site Inspection')
			,(37, 'IVFollowUp', 'Intravenous')
			,(38, 'StopTime', 'Stop Time')
			,(39, 'FollowUpSafety', 'Safety Interventions')
			,(40, 'VitalSigns', 'Vital Signs')
			,(41, 'FollowUpGeneric', '')
			,(42, 'IVInIMedication', 'Medication')
			,(43, 'IVInIAssessment', 'Pre-Administration Assessment')
			,(44, 'IVInISafety', 'Safety Interventions')
			,(45, 'IVInIEmotional', '')
			,(46, 'EyeMedication', 'Medication')
			,(47, 'DiscontinueReason', 'Discontinue Reason')
			,(48, 'PharmVerification', 'Pharmacy Verification Needed')
			,(49, 'CoSign', 'Co-Sign')
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
			,('Ear', 4, 'GenericGive', 0)
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
			,('GenericGive', 1, 'DefaultGive', 0)
			,('GenericGive', 2, 'GenericGive', 0)
			,('Enteral', 1, 'EnteralMedication', 0)
			,('Enteral', 2, 'Emotional', 0)
			,('Enteral', 3, 'AmbulateSafety', 0)
			,('Enteral', 4, 'GenericGive', 0)
			,('Eye', 1, 'EyeMedication', 0)
			,('Eye', 2, 'Emotional', 0)
			,('Eye', 3, 'AmbulateSafety', 0)
			,('Eye', 4, 'GenericGive', 0)
			,('Nasal', 1, 'NasalMedication', 0)
			,('Nasal', 2, 'Emotional', 0)
			,('Nasal', 3, 'Safety', 0)
			,('Nasal', 4, 'GenericGive', 0)
			,('Inhalation', 1, 'InhalationMedication', 0)
			,('Inhalation', 2, 'InhalationAssessment', 0)
			,('Inhalation', 3, 'Emotional', 0)
			,('Inhalation', 4, 'Safety', 0)
			,('Inhalation', 5, 'GenericGive', 0)
			,('Intradermal', 1, 'IntraDermMedication', 0)
			,('Intradermal', 2, 'Emotional', 0)
			,('Intradermal', 3, 'Safety', 0)
			,('Intradermal', 4, 'GenericGive', 0)
			,('Intraosseous', 1, 'IntraOssMedication', 0)
			,('Intraosseous', 2, 'IntraOssAssessment', 0)
			,('Intraosseous', 3, 'Emotional', 0)
			,('Intraosseous', 4, 'Safety', 0)
			,('Intraosseous', 5, 'GenericGive', 0)
			,('Rectal', 1, 'RectalMedication', 0)
			,('Rectal', 2, 'Emotional', 0)
			,('Rectal', 3, 'Safety', 0)
			,('Rectal', 4, 'GenericGive', 0)
			,('Transdermal', 1, 'TransDermMedication', 0)
			,('Transdermal', 2, 'Emotional', 0)
			,('Transdermal', 3, 'AmbulateSafety', 0)
			,('Transdermal', 4, 'GenericGive', 0)
			,('Vaginal', 1, 'VaginalMedication', 0)
			,('Vaginal', 2, 'Emotional', 0)
			,('Vaginal', 3, 'AmbulateSafety', 0)
			,('Vaginal', 4, 'GenericGive', 0)
			,('Subcutaneous', 1, 'SubcutanMedication', 0)
			,('Subcutaneous', 2, 'Emotional', 0)
			,('Subcutaneous', 3, 'AmbulateSafety', 0)
			,('Subcutaneous', 4, 'GenericGive', 0)
			,('Intravenous', 1, 'IVMedication', 0)
			,('Intravenous', 2, 'IVAssessment', 0)
			,('Intravenous', 3, 'IVSafety', 0)
			,('Intravenous', 4, 'Emotional', 0)
			,('Intravenous', 5, 'GenericGive', 0)
			,('FollowUp', 1, 'GeneralAssessment', 0)
			,('FollowUp', 2, 'SiteInspection', 0)
			,('FollowUp', 3, 'IVFollowUp', 0)
			,('FollowUp', 4, 'StopTime', 0)
			,('FollowUp', 5, 'FollowUpSafety', 0)
			,('FollowUp', 6, 'VitalSigns', 0)
			,('FollowUp', 7, 'FollowUpGeneric', 0)
			,('IntravenousInI', 1, 'IVInIMedication', 0)
			,('IntravenousInI', 2, 'IVInIAssessment', 0)
			,('IntravenousInI', 3, 'IVInISafety', 0)
			,('IntravenousInI', 4, 'IVInIEmotional', 0)
			,('IntravenousInI', 5, 'GenericGive', 0)
			,('OrderDiscontinue', 1, 'DiscontinueReason', 0)
			,('OrderDiscontinue', 2, 'Notes_At_Notify', 0)
			,('PharmVerification', 1, 'PharmVerification', 0)
			,('CoSign', 1, 'Cosign', 0)


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
      [prompt_group_id]				[int] null
    , [sequence]					[smallint] not null
    , [prompt]						[nvarchar](200) not null
    , [is_active]					[bit] not null
    , [prompt_type]					[varchar](25) not null
    , [prompt_default]				[varchar](100) null
    , [required]					[bit] not null
    , [prompt_group_name]			[varchar](25) not null
	, [placeholder_text]			[varchar](100) null
	, [display_child_prompts_value] [varchar](100) null
	, [is_on_newline]				[bit] not null 
	, [chart_markup]				[nvarchar](256) null);

insert into @prompts
    ([prompt_group_name]
   , [sequence]
   , [prompt]
   , [is_active]
   , [prompt_type]
   , [prompt_default]
   , [required]
   , [placeholder_text]
   , [display_child_prompts_value]
   , [is_on_newline]
   , [chart_markup]
    )
select [prompt_group_name]
     , [sequence]
     , [prompt]
     , [is_active]
     , [prompt_type]
     , [prompt_default]
     , [required]
	 , [placeholder_text]
	 , [display_child_prompts_value]
	 , [is_on_newline]
	 , [chart_markup]
from   (values
          	('Medication', 1, 'Verbal order read back and verified', 1, 'CheckBox', NULL, 0, NULL, NULL, 1, '^D=Verbal order read back and verified')
			,('Medication', 2, 'Amount given', 1, 'FreeText', NULL, 1, NULL, NULL, 1, '^CAmount given:=')
			,('Medication', 3, 'Amount wasted', 1, 'FreeText', NULL, 1, NULL, NULL, 1, '^CAmount wasted:=')
			,('Medication', 4, 'Administration of this medication is documented elsewhere in chart', 1, 'CheckBox', NULL, 0, NULL, NULL, 1, '^D=Administration of this medication is documented elsewhere in chart')
			,('Medication', 5, 'Site', 1, 'DropDownListBox', NULL, 1, NULL, NULL, 1, '')
			,('Medication', 6, 'Correct patient, time, route, dose and medication confirmed prior to administration', 1, 'CheckBox', NULL, 0, NULL, NULL, 1, '^D=Correct patient, time, route, dose and medication confirmed prior to administration')
			,('Medication', 7, 'Patient advised of actions and side-effects prior to administration', 1, 'CheckBox', NULL, 0, NULL, NULL, 1, '^D=Patient advised of actions and side-effects prior to administration')
			,('Medication', 8, 'Allergies confirmed and medications reviewed prior to administration', 1, 'CheckBox', NULL, 0, NULL, NULL, 1, '^D=Allergies confirmed and medications reviewed prior to administration')
			,('Medication', 9, 'All of the above', 1, 'CheckBox', NULL, 0, NULL, NULL, 1, '')

			,('EyeMedication', 1, 'Verbal order read back and verified', 1, 'CheckBox', NULL, 0, NULL, NULL, 1, '^D=Verbal order read back and verified')
			,('EyeMedication', 2, 'Amount given', 1, 'FreeText', NULL, 1, NULL, NULL, 1, '^CAmount given:=')
			,('EyeMedication', 3, 'Amount wasted', 1, 'FreeText', NULL, 1, NULL, NULL, 1, '^CAmount wasted:=')
			,('EyeMedication', 4, 'Administration of this medication is documented elsewhere in chart', 1, 'CheckBox', NULL, 0, NULL, NULL, 1, '^D=Administration of this medication is documented elsewhere in chart')
			,('EyeMedication', 5, 'Site', 1, 'DropDownListBox', NULL, 1, NULL, NULL, 1, '')
			,('EyeMedication', 6, 'Correct patient, time, route, dose and medication confirmed prior to administration', 1, 'CheckBox', NULL, 0, NULL, NULL, 1, '^D=Correct patient, time, route, dose and medication confirmed prior to administration')
			,('EyeMedication', 7, 'Patient advised of actions and side-effects prior to administration', 1, 'CheckBox', NULL, 0, NULL, NULL, 1, '^D=Patient advised of actions and side-effects prior to administration')
			,('EyeMedication', 8, 'Allergies confirmed and medications reviewed prior to administration', 1, 'CheckBox', NULL, 0, NULL, NULL, 1, '^D=Allergies confirmed and medications reviewed prior to administration')
			,('EyeMedication', 9, 'All of the above', 1, 'CheckBox', NULL, 0, NULL, NULL, 1, '')

          	,('OralMedication', 1, 'Verbal order read back and verified', 1, 'CheckBox', NULL, 0, NULL, NULL, 1, '^D=Verbal order read back and verified')
			,('OralMedication', 2, 'Amount given', 1, 'FreeText', NULL, 1, NULL, NULL, 1, '^CAmount given:=')
			,('OralMedication', 3, 'Amount wasted', 1, 'FreeText', NULL, 1, NULL, NULL, 1, '^CAmount wasted:=')
			,('OralMedication', 4, 'Administration of this medication is documented elsewhere in chart', 1, 'CheckBox', NULL, 0, NULL, NULL, 1, '^D=Administration of this medication is documented elsewhere in chart')
			,('OralMedication', 5, 'Site', 1, 'DropDownListBox', NULL, 1, NULL, NULL, 1, '')
			,('OralMedication', 6, 'Medication crushed prior to administration', 1, 'CheckBox', NULL, 0, NULL, NULL, 1, '^D=Medication crushed prior to administration')
			,('OralMedication', 7, 'Mixed in', 1, 'FreeText', NULL, 0, NULL, NULL, 1, '^CMixed in=')
			,('OralMedication', 8, 'Patient vomited during or soon after administration', 1, 'CheckBox', NULL, 0, NULL, NULL, 1, '^D=Patient vomited during or soon after administration')
			,('OralMedication', 9, 'Snack given with administration', 1, 'CheckBox', NULL, 0, NULL, NULL, 1, '^D=Snack given with administration')
			,('OralMedication', 10, 'Mouth check preformed after administration of medication', 1, 'CheckBox', NULL, 0, NULL, NULL, 1, '^D=Mouth check performed after administration of medication')
			,('OralMedication', 11, 'Correct patient, time, route, dose and medication confirmed prior to administration', 1, 'CheckBox', NULL, 0, NULL, NULL, 1, '^D=Correct patient, time, route, dose and medication confirmed prior to administration')
			,('OralMedication', 12, 'Patient advised of actions and side-effects prior to administration', 1, 'CheckBox', NULL, 0, NULL, NULL, 1, '^D=Patient advised of actions and side-effects prior to administration')
			,('OralMedication', 13, 'Allergies confirmed and medications reviewed prior to administration', 1, 'CheckBox', NULL, 0, NULL, NULL, 1, '^D=Allergies confirmed and medications reviewed prior to administration')
			,('OralMedication', 14, 'All of the above', 1, 'CheckBox', NULL, 0, NULL, NULL, 1, '')

        	,('InhalationMedication', 1, 'Verbal order read back and verified', 1, 'CheckBox', NULL, 0, NULL, NULL, 1, '^D=Verbal order read back and verified')
			,('InhalationMedication', 2, 'Amount given', 1, 'FreeText', NULL, 1, NULL, NULL, 1, '^CAmount given:=')
			,('InhalationMedication', 3, 'Amount wasted', 1, 'FreeText', NULL, 1, NULL, NULL, 1, '^CAmount wasted:=')
			,('InhalationMedication', 4, 'Administration of this medication is documented elsewhere in chart', 1, 'CheckBox', NULL, 0, NULL, NULL, 1, '^D=Administration of this medication is documented elsewhere in chart')
			,('InhalationMedication', 5, 'Administered via', 1, 'DropDownListBox', NULL, 1, NULL, NULL, 1, '')

			,('InhalationMedication', 6, 'Medication combined for administration with', 1, 'CheckBox', NULL, 0, NULL, 'true', 1, '^D=Medication combined for administration with')
			,('InhalationMedication', 7, 'Albuterol', 1, 'CheckBox', NULL, 0, NULL, NULL, 1, '^D=Albuterol')
			,('InhalationMedication', 8, 'Albuterol Dose', 1, 'FreeText', NULL, 0, NULL, NULL, 1, '^CDose:^albuterol=')
			,('InhalationMedication', 9, 'Atrovent', 1, 'CheckBox', NULL, 0, NULL, NULL, 1, '^D=Atrovent')
			,('InhalationMedication', 10, 'Atrovent Dose', 1, 'FreeText', NULL, 0, NULL, NULL, 1, '^CDose:^Atrovent=')
			,('InhalationMedication', 11, 'Xopenex', 1, 'CheckBox', NULL, 0, NULL, NULL, 1, '^D=Xopenex')
			,('InhalationMedication', 12, 'Xopenex Dose', 1, 'FreeText', NULL, 0, NULL, NULL, 1, '^CDose:^Xopenex=')
			,('InhalationMedication', 13, 'Combivent', 1, 'CheckBox', NULL, 0, NULL, NULL, 1, '^D=Combivent')
			,('InhalationMedication', 14, 'Combivent Dose', 1, 'FreeText', NULL, 0, NULL, NULL, 1, '^CDose:^Combivent=')
			,('InhalationMedication', 15, 'Racemic Epinephrine', 1, 'CheckBox', NULL, 0, NULL, NULL, 1, '^D=Racemic Epinephrine')
			,('InhalationMedication', 16, 'Racemic Epinephrine Dose', 1, 'FreeText', NULL, 0, NULL, NULL, 1, '^CDose:^Racemic=')

			,('InhalationMedication', 17, 'With oxygen', 1, 'CheckBox', NULL, 0, NULL, NULL, 1, '^D=With oxygen')
			,('InhalationMedication', 18, 'With air', 1, 'CheckBox', NULL, 0, NULL, NULL, 1, '^D=With air')

			,('NasalMedication', 1, 'Verbal order read back and verified', 1, 'CheckBox', NULL, 0, NULL, NULL, 1, '^D=Verbal order read back and verified')
			,('NasalMedication', 2, 'Site', 1, 'DropDownListBox', NULL, 1, NULL, NULL, 1, '')
			,('NasalMedication', 3, 'Amount given', 1, 'FreeText', NULL, 1, NULL, NULL, 1, '^CAmount given:=')
			,('NasalMedication', 4, 'Immunization Details', 1, 'CheckBox', NULL, 0, NULL, 'true', 1, '')
			,('NasalMedication', 5, 'Vaccination information sheet given to patient', 1, 'CheckBox', NULL, 0, NULL, NULL, 1, '^D^Itram^^medsvc_vac_sheetgiven=Vaccination information sheet given to patient')
			,('NasalMedication', 6, 'Date of publication', 1, 'Date', NULL, 0, NULL, NULL, 1, '^Cdate of publication:=')
			,('NasalMedication', 7, 'Name of publication', 1, 'FreeText', NULL, 0, NULL, NULL, 1, '^Cname of publication:^^^^medsvc_vac_pubname=')
			,('NasalMedication', 8, 'Manufacturer', 1, 'FreeText', NULL, 0, NULL, NULL, 1, '^Cmanufacturer:^^^^medsvc_vac_manuf=')
			,('NasalMedication', 9, 'Lot number', 1, 'FreeText', NULL, 0, NULL, NULL, 1, '^Clot number:^^^^medsvc_vac_lot=')
			,('NasalMedication', 10, 'Expiration', 1, 'Date', NULL, 0, NULL, NULL, 1, '^Cexpiration:^intram^^^medsvc_vac_exp=')

			,('NasalMedication', 11, 'Instructed to blow nose prior to administration', 1, 'CheckBox', NULL, 0, NULL, NULL, 1, '^D=nstructed to blow nose prior to administration')
			,('NasalMedication', 12, 'Correct patient, time, route, dose and medication confirmed prior to administration', 1, 'CheckBox', NULL, 0, NULL, NULL, 1, '^D=Correct patient, time, route, dose and medication confirmed prior to administration')
			,('NasalMedication', 13, 'Patient advised of actions and side-effects prior to administration', 1, 'CheckBox', NULL, 0, NULL, NULL, 1, '^D=Patient advised of actions and side-effects prior to administration')
			,('NasalMedication', 14, 'Allergies confirmed and medications reviewed prior to administration', 1, 'CheckBox', NULL, 0, NULL, NULL, 1, '^D=Allergies confirmed and medications reviewed prior to administration')
			,('NasalMedication', 15, 'All of the above', 1, 'CheckBox', NULL, 0, NULL, NULL, 1, '')
		
			,('EnteralMedication', 1, 'Verbal order read back and verified', 1, 'CheckBox', NULL, 0, NULL, NULL, 1, '^D=Verbal order read back and verified')
			,('EnteralMedication', 2, 'Amount given', 1, 'FreeText', NULL, 1, NULL, NULL, 1, '^CAmount given:=')
			,('EnteralMedication', 3, 'Amount wasted', 1, 'FreeText', NULL, 1, NULL, NULL, 1, '^CAmount wasted:=')
			,('EnteralMedication', 4, 'Medication combined for administration with', 1, 'FreeText', NULL, 1, NULL, NULL, 1, '^CMedication combined for administration with=')
			,('EnteralMedication', 5, 'Administration of this medication is documented elsewhere in chart', 1, 'CheckBox', NULL, 0, NULL, NULL, 1, '^D=Administration of this medication is documented elsewhere in chart')
			,('EnteralMedication', 6, 'Site', 1, 'DropDownListBox', NULL, 1, NULL, NULL, 1, '')
			,('EnteralMedication', 7, 'Tube position confirmed via aspiration prior to administration', 1, 'CheckBox', NULL, 0, NULL, NULL, 1, '^D=Tube position confirmed via aspiration prior to administration')
			,('EnteralMedication', 8, 'Correct patient, time, route, dose and medication confirmed prior to administration', 1, 'CheckBox', NULL, 0, NULL, NULL, 1, '^D=Correct patient, time, route, dose and medication confirmed prior to administration')
			,('EnteralMedication', 9, 'Patient advised of actions and side-effects prior to administration', 1, 'CheckBox', NULL, 0, NULL, NULL, 1, '^D=Patient advised of actions and side-effects prior to administration')
			,('EnteralMedication', 10, 'Allergies confirmed and medications reviewed prior to administration', 1, 'CheckBox', NULL, 0, NULL, NULL, 1, '^D=Allergies confirmed and medications reviewed prior to administration')
			,('EnteralMedication', 11, 'Flushed with water after administration', 1, 'CheckBox', NULL, 0, NULL, NULL, 1, '^D=Flushed with water after administration')
			,('EnteralMedication', 12, 'All of the above', 1, 'CheckBox', NULL, 0, NULL, NULL, 1, '')

			,('IntraDermMedication', 1, 'Verbal order read back and verified', 1, 'CheckBox', NULL, 0, NULL, NULL, 1, '^D=Verbal order read back and verified')
			,('IntraDermMedication', 2, 'Amount given', 1, 'FreeText', NULL, 1, NULL, NULL, 1, '^CAmount given:=')
			,('IntraDermMedication', 3, 'Amount wasted', 1, 'FreeText', NULL, 1, NULL, NULL, 1, '^CAmount wasted:=')
			,('IntraDermMedication', 4, 'Administration of this medication is documented elsewhere in chart', 1, 'CheckBox', NULL, 0, NULL, NULL, 1, '^D=Administration of this medication is documented elsewhere in chart')
			,('IntraDermMedication', 5, 'Site', 1, 'DropDownListBox', NULL, 0, NULL, NULL, 1, '')
			,('IntraDermMedication', 6, 'Other site', 1, 'FreeText', NULL, 0, NULL, NULL, 1, '^CMedication administered to ^Location=')
			,('IntraDermMedication', 7, 'Manufacturer', 1, 'FreeText', NULL, 1, NULL, NULL, 1, '^CManufacturer:=')
			,('IntraDermMedication', 8, 'Lot number', 1, 'FreeText', NULL, 1, NULL, NULL, 1, '^CLot number:=')
			,('IntraDermMedication', 9, 'Expiration', 1, 'Date', NULL, 1, NULL, NULL, 1, '^CExpiration:^IM=')
			,('IntraDermMedication', 10, 'Correct patient, time, route, dose and medication confirmed prior to administration', 1, 'CheckBox', NULL, 0, NULL, NULL, 1, '^D=Correct patient, time, route, dose and medication confirmed prior to administration')
			,('IntraDermMedication', 11, 'Patient advised of actions and side-effects prior to administration', 1, 'CheckBox', NULL, 0, NULL, NULL, 1, '^D=Patient advised of actions and side-effects prior to administration')
			,('IntraDermMedication', 12, 'Allergies confirmed and medications reviewed prior to administration', 1, 'CheckBox', NULL, 0, NULL, NULL, 1, '^D=Allergies confirmed and medications reviewed prior to administration')
			,('IntraDermMedication', 13, 'All of the above', 1, 'CheckBox', NULL, 0, NULL, NULL, 1, '')

          	,('IntraOssMedication', 1, 'Verbal order read back and verified', 1, 'CheckBox', NULL, 0, NULL, NULL, 1, '^D=Verbal order read back and verified')
			,('IntraOssMedication', 2, 'Amount given', 1, 'FreeText', NULL, 1, NULL, NULL, 1, '^CAmount given:=')
			,('IntraOssMedication', 3, 'Amount wasted', 1, 'FreeText', NULL, 1, NULL, NULL, 1, '^CAmount wasted:=')
			,('IntraOssMedication', 4, 'Administration of this medication is documented elsewhere in chart', 1, 'CheckBox', NULL, 0, NULL, NULL, 1, '^D=Administration of this medication is documented elsewhere in chart')
			,('IntraOssMedication', 5, 'Site', 1, 'DropDownListBox', NULL, 1, NULL, NULL, 1, '')
			,('IntraOssMedication', 6, 'Other Site', 1, 'FreeText', NULL, 0, NULL, NULL, 1, '^CMedication administered to ^Location=')
			
			,('RectalMedication', 1, 'Verbal order read back and verified', 1, 'CheckBox', NULL, 0, NULL, NULL, 1, '^D=Verbal order read back and verified')
			,('RectalMedication', 2, 'Medication administered rectally', 1, 'CheckBox', NULL, 1, NULL, NULL, 1, '^D^rectal^QX215=Medication administered rectally')
			,('RectalMedication', 3, 'Amount given', 1, 'FreeText', NULL, 1, NULL, NULL, 1, '^CAmount given:=')
			,('RectalMedication', 4, 'Amount wasted', 1, 'FreeText', NULL, 1, NULL, NULL, 1, '^CAmount wasted:=')
			,('RectalMedication', 5, 'Administration of this medication is documented elsewhere in chart', 1, 'CheckBox', NULL, 0, NULL, NULL, 1, '^D=Administration of this medication is documented elsewhere in chart')
			,('RectalMedication', 6, 'Patient administered medication after instruction by staff on correct technique', 1, 'CheckBox', NULL, 0, NULL, NULL, 1, '^D=Patient administered medication after instruction by staff on correct technique')
			,('RectalMedication', 7, 'Lubricant used for administration', 1, 'CheckBox', NULL, 1, NULL, NULL, 1, '^D=Lubricant used for administration')
			,('RectalMedication', 8, 'Medication retained after administration', 1, 'CheckBox', NULL, 0, NULL, NULL, 1, '^D=Medication retained after administration')	
			,('RectalMedication', 9, 'Correct patient, time, route, dose and medication confirmed prior to administration', 1, 'CheckBox', NULL, 0, NULL, NULL, 1, '^D=Correct patient, time, route, dose and medication confirmed prior to administration')
			,('RectalMedication', 10, 'Patient advised of actions and side-effects prior to administration', 1, 'CheckBox', NULL, 0, NULL, NULL, 1, '^D=Patient advised of actions and side-effects prior to administration')
			,('RectalMedication', 11, 'Allergies confirmed and medications reviewed prior to administration', 1, 'CheckBox', NULL, 0, NULL, NULL, 1, '^D=Allergies confirmed and medications reviewed prior to administration')
			,('RectalMedication', 12, 'All of the above', 1, 'CheckBox', NULL, 0, NULL, NULL, 1, '')

			,('TransDermMedication', 1, 'Verbal order read back and verified', 1, 'CheckBox', NULL, 0, NULL, NULL, 1, '^D=Verbal order read back and verified')
			,('TransDermMedication', 2, 'Medication applied transdermally topically', 1, 'CheckBox', NULL, 0, NULL, NULL, 1, '^D^Topical^QX2201=Medication applied transdermally topically')
			,('TransDermMedication', 3, 'Amount given', 1, 'FreeText', NULL, 1, NULL, NULL, 1, '^CAmount given:=')
			,('TransDermMedication', 4, 'Amount wasted', 1, 'FreeText', NULL, 1, NULL, NULL, 1, '^CAmount wasted:=')
			,('TransDermMedication', 5, 'Administration of this medication is documented elsewhere in chart', 1, 'CheckBox', NULL, 0, NULL, NULL, 1, '^D=Administration of this medication is documented elsewhere in chart')
			,('TransDermMedication', 6, 'Site', 1, 'FreeText', NULL, 1, NULL, NULL, 1, '^CSite:=')
			,('TransDermMedication', 7, 'Skin cleansed prior to administration', 1, 'CheckBox', NULL, 0, NULL, NULL, 1, '^D=Skin cleansed prior to administration')
			,('TransDermMedication', 8, 'Shaving required prior to administration', 1, 'CheckBox', NULL, 0, NULL, NULL, 1, '^D=Shaving required prior to administration')
			,('TransDermMedication', 9, 'Correct patient, time, route, dose and medication confirmed prior to administration', 1, 'CheckBox', NULL, 0, NULL, NULL, 1, '^D=Correct patient, time, route, dose and medication confirmed prior to administration')
			,('TransDermMedication', 10, 'Patient advised of actions and side-effects prior to administration', 1, 'CheckBox', NULL, 0, NULL, NULL, 1, '^D=Patient advised of actions and side-effects prior to administration')
			,('TransDermMedication', 11, 'Allergies confirmed and medications reviewed prior to administration', 1, 'CheckBox', NULL, 0, NULL, NULL, 1, '^D=Allergies confirmed and medications reviewed prior to administration')
			,('TransDermMedication', 12, 'All of the above', 1, 'CheckBox', NULL, 0, NULL, NULL, 1, '')

			,('VaginalMedication', 1, 'Medication given vaginally', 1, 'CheckBox', NULL, 1, NULL, NULL, 1, '^D^^QX4745=Medication given vaginally')
			,('VaginalMedication', 2, 'Given by', 1, 'FreeText', NULL, 1, NULL, NULL, 1, '^Cby^vaginal=')
			,('VaginalMedication', 3, 'Time given', 1, 'FreeText', NULL, 1, NULL, NULL, 1, '^CTime given:=')
			,('VaginalMedication', 4, 'Verbal order read back and verified', 1, 'CheckBox', NULL, 0, NULL, NULL, 1, '^D=Verbal order read back and verified')
			,('VaginalMedication', 5, 'Amount given', 1, 'FreeText', NULL, 1, NULL, NULL, 1, '^CAmount given:=')
			,('VaginalMedication', 6, 'Amount wasted', 1, 'FreeText', NULL, 1, NULL, NULL, 1, '^CAmount wasted:=')
			,('VaginalMedication', 7, 'Administration of this medication is documented elsewhere in chart', 1, 'CheckBox', NULL, 0, NULL, NULL, 1, '^D=Administration of this medication is documented elsewhere in chart')
			,('VaginalMedication', 8, 'Patient administered medication after instruction by staff on correct technique', 1, 'CheckBox', NULL, 0, NULL, NULL, 1, '^D=Patient administered medication after instruction by staff on correct technique')
			,('VaginalMedication', 9, 'Patient voided prior to administration', 1, 'CheckBox', NULL, 1, NULL, NULL, 1, '^D=Patient voided prior to administration')
			,('VaginalMedication', 10, 'Lubricant used for administration', 1, 'CheckBox', NULL, 1, NULL, NULL, 1, '^D=Lubricant used for administration')
			,('VaginalMedication', 11, 'Medication retained after administration', 1, 'CheckBox', NULL, 0, NULL, NULL, 1, '^D=Medication retained after administration')	
			,('VaginalMedication', 12, 'Correct patient, time, route, dose and medication confirmed prior to administration', 1, 'CheckBox', NULL, 0, NULL, NULL, 1, '^D=Correct patient, time, route, dose and medication confirmed prior to administration')
			,('VaginalMedication', 13, 'Patient advised of actions and side-effects prior to administration', 1, 'CheckBox', NULL, 0, NULL, NULL, 1, '^D=Patient advised of actions and side-effects prior to administration')
			,('VaginalMedication', 14, 'Allergies confirmed and medications reviewed prior to administration', 1, 'CheckBox', NULL, 0, NULL, NULL, 1, '^D=Allergies confirmed and medications reviewed prior to administration')
			,('VaginalMedication', 15, 'All of the above', 1, 'CheckBox', NULL, 0, NULL, NULL, 1, '')

			,('SubcutanMedication', 1, 'Verbal order read back and verified', 1, 'CheckBox', NULL, 0, NULL, NULL, 1, '^D=Verbal order read back and verified')
			,('SubcutanMedication', 2, 'Amount given', 1, 'FreeText', NULL, 1, NULL, NULL, 1, '^CAmount given:=')
			,('SubcutanMedication', 3, 'Amount wasted', 1, 'FreeText', NULL, 1, NULL, NULL, 1, '^CAmount wasted:=')
			,('SubcutanMedication', 4, 'Administration of this medication is documented elsewhere in chart', 1, 'CheckBox', NULL, 0, NULL, NULL, 1, '^D=Administration of this medication is documented elsewhere in chart')
			,('SubcutanMedication', 5, 'Medication site', 1, 'DropDownListBox', NULL, 0, NULL, NULL, 1, '')
			,('SubcutanMedication', 6, 'Other site', 1, 'FreeText', NULL, 0, NULL, NULL, 1, '^CMedication administered to ^Location=')
			,('SubcutanMedication', 7, 'Immunization', 1, 'CheckBox', NULL, 0, NULL, 'true', 1, '^D^Simmunizations=Medication is an immunization')
			,('SubcutanMedication', 8, 'Immunization site', 1, 'DropDownListBox', NULL, 0, NULL, NULL, 1, '')
			,('SubcutanMedication', 9, 'Vaccination information sheet given to patient', 1, 'CheckBox', NULL, 0, NULL, NULL, 1, '^D^Itram^^medsvc_vac_sheetgiven=Vaccination information sheet given to patient')
			,('SubcutanMedication', 10, 'Date of publication', 1, 'Date', NULL, 1, NULL, NULL, 1, '^Cdate of publication:^im^^^medsvc_vac_pubdate=')
			,('SubcutanMedication', 11, 'Name of publication', 1, 'FreeText', NULL, 1, NULL, NULL, 1, '^Cname of publication:^^^^medsvc_vac_pubname=')
			,('SubcutanMedication', 12, 'Manufacturer', 1, 'FreeText', NULL, 1, NULL, NULL, 1, '^Cmanufacturer:^^^^medsvc_vac_manuf=')
			,('SubcutanMedication', 13, 'Lot number', 1, 'FreeText', NULL, 1, NULL, NULL, 1, '^Clot number:^^^^medsvc_vac_lot=')
			,('SubcutanMedication', 14, 'Expiration', 1, 'Date', NULL, 1, NULL, NULL, 1, '^Cexpiration:^intram^^^medsvc_vac_exp=')
			,('SubcutanMedication', 15, 'Dose number', 1, 'DropDownListBox', NULL, 1, NULL, NULL, 1, '')
			,('SubcutanMedication', 16, 'Other dose number', 1, 'FreeText', NULL, 0, NULL, NULL, 1, '^C^^^^medsvc_vac_dosenum=')
			,('SubcutanMedication', 17, 'Previous dose confirmed by', 1, 'DropDownListBox', NULL, 1, NULL, NULL, 1, '')
			,('SubcutanMedication', 18, 'Provided Emergency Use Authorization (EUA) fact sheet', 1, 'CheckBox', NULL, 0, NULL, NULL, 1, '^D^Itram^^medsvc_vac_EAUgiven=Provided Emergency Use Authorization (EUA) fact sheet')
			,('SubcutanMedication', 19, 'Provided a completed vaccination record card', 1, 'CheckBox', NULL, 0, NULL, NULL, 1, '^D^Itram^^medsvc_vac_Cardgiven=Provided a completed COVID-19 vaccination record card')
			,('SubcutanMedication', 20, 'Correct patient, time, route, dose and medication confirmed prior to administration', 1, 'CheckBox', NULL, 0, NULL, NULL, 1, '^D=Correct patient, time, route, dose and medication confirmed prior to administration')
			,('SubcutanMedication', 21, 'Patient advised of actions and side-effects prior to administration', 1, 'CheckBox', NULL, 0, NULL, NULL, 1, '^D=Patient advised of actions and side-effects prior to administration')
			,('SubcutanMedication', 22, 'Allergies confirmed and medications reviewed prior to administration', 1, 'CheckBox', NULL, 0, NULL, NULL, 1, '^D=Allergies confirmed and medications reviewed prior to administration')
			,('SubcutanMedication', 23, 'All of the above', 1, 'CheckBox', NULL, 0, NULL, NULL, 1, '')

			-- IV InI medication
			,('IVInIMedication', 1, 'Verbal order read back and verified', 1, 'CheckBox', NULL, 0, NULL, NULL, 1, '^D=Verbal order read back and verified')
			,('IVInIMedication', 2, 'Administrered by', 1, 'FreeText', NULL, 0, NULL, NULL, 1, '^CAdministered by=')
			,('IVInIMedication', 3, 'Amount given/hung', 1, 'FreeText', NULL, 1, NULL, NULL, 1, '^CAmount given/hung:=')
			,('IVInIMedication', 4, 'Location', 1, 'DropDownListBox', NULL, 1, NULL, 'true', 1, '')
			,('IVInIMedication', 5, 'Other IV Site', 1, 'FreeText', NULL, 0, NULL, NULL, 1, '^Cinto^IV_Location=')
			,('IVInIMedication', 6, 'IV Number', 1, 'DropDownListBox', NULL, 1, NULL, 'true', 1, '')
			,('IVInIMedication', 11, 'ETT', 1, 'CheckBox', NULL, 0, NULL, 'true', 1, '^D^ETT^QX556=Medication administered via Endotracheal tube')
			,('IVInIMedication', 12, 'No IV', 1, 'CheckBox', NULL, 0, NULL, NULL, 1, '^D^ett=no IV')
			,('IVInIMedication', 13, 'IV infiltrated', 1, 'CheckBox', NULL, 0, NULL, NULL, 1, '^D^ett=IV infiltrated')
			,('IVInIMedication', 14, 'IV fluids', 1, 'CheckBox', NULL, 0, NULL, 'true', 1, '^D^hydration=IV fluids established')
			,('IVInIMedication', 15, 'Bag number', 1, 'DropDownListBox', NULL, 0, NULL, NULL, 1, '')
			,('IVInIMedication', 16, 'Amount', 1, 'DropDownListBox', NULL, 0, NULL, NULL, 1, '')
			,('IVInIMedication', 17, 'Tubing', 1, 'DropDownListBox', NULL, 0, NULL, NULL, 1, '')
			,('IVInIMedication', 18, 'In buretrol', 1, 'CheckBox', NULL, 0, NULL, 'true', 1, '^D^buretolone^QX422=via Buretrol')
			,('IVInIMedication', 19, 'Amount (ml)', 1, 'FreeText', NULL, 0, NULL, NULL, 1, '^Cinitial fluid (ml):=')
			,('IVInIMedication', 20, 'On IV pump', 1, 'CheckBox', NULL, 0, NULL, NULL, 1, '^D^Pump^QX421=on IV pump')
			,('IVInIMedication', 21, 'Syringe pump', 1, 'CheckBox', NULL, 0, NULL, NULL, 1, '^D^Pump^QX6402=on syringe pump')
			,('IVInIMedication', 22, 'Rapid infuser used', 1, 'CheckBox', NULL, 0, NULL, NULL, 1, '^D^Pump^QX299=via rapid infuser')
			,('IVInIMedication', 23, 'Fluid warmer used', 1, 'CheckBox', NULL, 0, NULL, NULL, 1, '^D^Pump^QX298=Fluid warmer used')
			,('IVInIMedication', 24, 'Bolus', 1, 'Label', NULL, 0, NULL, NULL, 1, '')
			,('IVInIMedication', 25, 'Amount (Bolus)', 1, 'DropDownListBox', NULL, 0, NULL, NULL, 1, '')
			,('IVInIMedication', 26, 'Other bolus (ml)', 1, 'FreeText', NULL, 0, NULL, NULL, 1, '^COther bolus amount (ml):')
			,('IVInIMedication', 27, 'Rate', 1, 'DropDownListBox', NULL, 0, NULL, NULL, 1, '')
			,('IVInIMedication', 28, 'Other (ml/hr)', 1, 'FreeText', NULL, 0, NULL, NULL, 1, '^COther rate of infusion (ml/hr)')
			,('IVInIMedication', 29, 'Rate after Bolus', 1, 'DropDownListBox', NULL, 0, NULL, NULL, 1, '')
			,('IVInIMedication', 30, 'Repeat bolus', 1, 'DropDownListBox', NULL, 0, NULL, NULL, 1, '')
			,('IVInIMedication', 31, 'Other (ml)', 1, 'FreeText', NULL, 0, NULL, NULL, 1, '^COther repeat bolus amount (ml)')
			,('IVInIMedication', 32, 'Non-Bolus', 1, 'Label', NULL, 0, NULL, NULL, 1, '')
			,('IVInIMedication', 33, 'Amount (Non-bolus)', 1, 'DropDownListBox', NULL, 0, NULL, NULL, 1, '')
			,('IVInIMedication', 34, 'Other non-bolus (ml/hr)', 1, 'FreeText', NULL, 0, NULL, NULL, 1, '^COther non-bolus amount (ml):')
			,('IVInIMedication', 35, 'IVP', 1, 'CheckBox', NULL, 0, NULL, 'true', 1, '^D^IVP^QX116')
			,('IVInIMedication', 36, 'Slowly', 1, 'CheckBox', NULL, 0, NULL, NULL, 1, '^D=Slowly')
			,('IVInIMedication', 37, 'Rapidly', 1, 'CheckBox', NULL, 0, NULL, NULL, 1, '^D=Rapidly')
			,('IVInIMedication', 38, 'Added to existing IV fluid', 1, 'CheckBox', NULL, 0, NULL, 'true', 1, '^D^IVaddseven^QX1036=added to existing IV Fluid')
			,('IVInIMedication', 39, 'Type', 1, 'FreeText', NULL, 0, NULL, NULL, 1, '^CType:=')
			,('IVInIMedication', 40, 'Amount of fluid remaining ', 1, 'FreeText', NULL, 0, NULL, NULL, 1, '^CAmount of fluid remaining:=')
			,('IVInIMedication', 41, 'IVPB/drip', 1, 'CheckBox', NULL, 0, NULL, 'true', 1, '^D=IVPB or drip')
			,('IVInIMedication', 42, 'Rate (IVPB/drip)', 1, 'DropDownListBox', NULL, 0, NULL, NULL, 1, '')
			,('IVInIMedication', 43, 'Other rate', 1, 'FreeText', NULL, 0, NULL, NULL, 1, '^Cat')
			,('IVInIMedication', 44, 'Premixed', 1, 'CheckBox', NULL, 0, NULL, NULL, 1, '^D=Premixed')
			,('IVInIMedication', 45, 'Mixed in', 1, 'DropDownListBox', NULL, 0, NULL, NULL, 1, '')
			,('IVInIMedication', 46, 'Other mixed in', 1, 'FreeText', NULL, 0, NULL, NULL, 1, '^CIVPB mixed in:=')
			,('IVInIMedication', 47, 'Fluid', 1, 'DropDownListBox', NULL, 0, NULL, NULL, 1, '')
			,('IVInIMedication', 48, 'Tubing (IVPB/drip)', 1, 'DropDownListBox', NULL, 0, NULL, NULL, 1, '')
			,('IVInIMedication', 49, 'In buretrol (IVPB/drip)', 1, 'CheckBox', NULL, 0, NULL, 'true', 1, '^D^buretoltwo^QX422=via Buretrol')
			,('IVInIMedication', 50, 'IVPB/drip amount (ml)', 1, 'FreeText', NULL, 0, NULL, NULL, 1, '^Cinitial fluid=')
			,('IVInIMedication', 51, 'On IV Pump (IVPB/drip)', 1, 'CheckBox', NULL, 0, NULL, NULL, 1, '^D^Pump^QX421=on IV pump')
			,('IVInIMedication', 52, 'Syringe Pump (IVPB/drip)', 1, 'CheckBox', NULL, 0, NULL, NULL, 1, '^D^Pump^QX6402=on syringe pump')
			,('IVInIMedication', 53, 'Filter Used', 1, 'CheckBox', NULL, 0, NULL, NULL, 1, '^D^filterneed=Filter used with administration')

			--- IV medication
			,('IVMedication', 1, 'Verbal order read back and verified', 1, 'CheckBox', NULL, 0, NULL, NULL, 1, '^D=Verbal order read back and verified')
			,('IVMedication', 2, 'Amount given/hung', 1, 'FreeText', NULL, 1, NULL, NULL, 1, '^CAmount given/hung:=')
			,('IVMedication', 3, 'IV number', 1, 'DropDownListBox', NULL, 1, NULL, 'true', 1, NULL)
			,('IVMedication', 4, 'Location', 1, 'DropDownListBox', NULL, 1, NULL, 'true', 1, NULL)
			,('IVMedication', 5, 'Other Location', 1, 'FreeText', NULL, 0, NULL, NULL, 1, '^CIV SITE into^IV_Location+')
			,('IVMedication', 6, 'ETT', 1, 'CheckBox', NULL, 0, NULL, 'true', 1, '^D^ETT^QX556=Medication administered via Endotracheal tube')	
			,('IVMedication', 7, 'No IV', 1, 'CheckBox', NULL, 0, NULL, NULL, 1, '^D=no IV')		
			,('IVMedication', 8, 'IV infiltrated', 1, 'CheckBox', NULL, 0, NULL, NULL, 1, '^D=IV infiltrated')	
														
			,('IVMedication', 9, 'IV Fluids', 1, 'CheckBox', NULL, 0, NULL, 'true', 1, '')
			,('IVMedication', 10, 'Site', 1, 'DropDownListBox', NULL, 1, NULL, 'true', 1, NULL)
			,('IVMedication', 11, 'Bag number', 1, 'DropDownListBox', NULL, 0, NULL, 'true', 1, NULL)
			,('IVMedication', 12, 'Amount', 1, 'DropDownListBox', NULL, 0, NULL, 'true', 1, NULL)
			,('IVMedication', 13, 'Bolus', 1, 'Label', NULL, 0, NULL, NULL, 1, NULL)
			,('IVMedication', 14, 'Bolus amount', 1, 'DropDownListBox', NULL, 0, NULL, 'true', 1, NULL)
			,('IVMedication', 15, 'Other bolus amount (ml)', 1, 'FreeText', NULL, 0, NULL, NULL, 1, '^COther Bolus Amount (ml):=')
			,('IVMedication', 16, 'Rate of bolus', 1, 'CheckBox', NULL, 0, NULL, 'true', 1, '^D^bolus=Rate of bolus:')
			,('IVMedication', 17, 'Wide open', 1, 'CheckBox', NULL, 0, NULL, NULL, 1, '^D=wide open')
			,('IVMedication', 18, '1000 ml/hr', 1, 'CheckBox', NULL, 0, NULL, NULL, 1, '^D=1000 ml/hr')
			,('IVMedication', 19, '500 ml/hr', 1, 'CheckBox', NULL, 0, NULL, NULL, 1, '^D=500 ml/hr')
			,('IVMedication', 20, '250 ml/hr', 1, 'CheckBox', NULL, 0, NULL, NULL, 1, '^D=250 ml/hr')
			,('IVMedication', 21, 'Other rate >>', 1, 'CheckBox', NULL, 0, NULL, 'true', 1, '^D=Other rate of bolus:')
			,('IVMedication', 22, 'Other rate of bolus (ml/hr)', 1, 'FreeText', NULL, 0, NULL, NULL, 1, '^COther rate of bolus (ml/hr):=')
			,('IVMedication', 23, 'Rate after bolus', 1, 'DropDownListBox', NULL, 0, NULL, 'true', 1, NULL)
			,('IVMedication', 24, 'Repeat bolus', 1, 'DropDownListBox', NULL, 0, NULL, 'true', 1, NULL)
			,('IVMedication', 25, 'Other repeat bolus (ml)', 1, 'FreeText', NULL, 0, NULL, NULL, 1, '^COther repeat bolus (ml):=')
			
			,('IVMedication', 26, 'Rate of infusion (non-bolus)', 1, 'Label', NULL, 0, NULL, NULL, 1, NULL)
			,('IVMedication', 27, 'Rate', 1, 'DropDownListBox', NULL, 0, NULL, 'true', 1, NULL)
			,('IVMedication', 28, 'Other rate (ml/hr)', 1, 'FreeText', NULL, 0, NULL, NULL, 1, '^COther infusion rate (ml/hr):=')
			,('IVMedication', 29, 'Rate change', 1, 'DropDownListBox', NULL, 0, NULL, 'true', 1, NULL)
			,('IVMedication', 30, 'Other rate change (ml/hr)', 1, 'FreeText', NULL, 0, NULL, NULL, 1, '^COther rate change (ml/hr):=')
			
			,('IVMedication', 31, 'Tubing', 1, 'Label', NULL, 0, NULL, NULL, 1, NULL)
			,('IVMedication', 32, 'Primary tubing', 1, 'CheckBox', NULL, 0, NULL, NULL, 1, '^D^^QX1748=via primary tubing')
			,('IVMedication', 33, 'Gravity tubing', 1, 'CheckBox', NULL, 0, NULL, NULL, 1, '^D^^QX424=via gravity tubing')
			,('IVMedication', 34, 'Blood tubing', 1, 'CheckBox', NULL, 0, NULL, NULL, 1, '^D^^QX423=via blood tubing')
			,('IVMedication', 35, 'Pump tubing', 1, 'CheckBox', NULL, 0, NULL, NULL, 1, '^D^^QX1493=via pump tubing')
			,('IVMedication', 36, 'Secondary tubing', 1, 'CheckBox', NULL, 0, NULL, NULL, 1, '^D^^QX425=via secondary tubing')
			,('IVMedication', 37, 'In buretrol (Tubing)', 1, 'CheckBox', NULL, 0, NULL, 'true', 1, '^D^^QX422=via Buretrol')
			,('IVMedication', 38, 'Initial fluid (ml)', 1, 'FreeText', NULL, 0, NULL, NULL, 1, '^Cinitial fluid (ml):=')
			,('IVMedication', 39, 'Additional fluid (ml)', 1, 'FreeText', NULL, 0, NULL, NULL, 1, '^Cadditional fluid (ml):=')	
				
			,('IVMedication', 40, 'Pump', 1, 'Label', NULL, 0, NULL, NULL, 1, NULL)
			,('IVMedication', 41, 'On IV pump', 1, 'CheckBox', NULL, 0, NULL, NULL, 1, '^D^^QX421=on IV pump')
			,('IVMedication', 42, 'Syringe pump', 1, 'CheckBox', NULL, 0, NULL, NULL, 1, '^D^^QX6402=on syringe pump')
			,('IVMedication', 43, 'Rapid infuser pump', 1, 'CheckBox', NULL, 0, NULL, NULL, 1, '^D^^QX299=via rapid infuser')
			,('IVMedication', 44, 'Fluid warmer pump', 1, 'CheckBox', NULL, 0, NULL, NULL, 1, '^D^^QX298=Fluid warmer used')
			
			,('IVMedication', 45, 'Tubing changed', 1, 'Label', NULL, 0, NULL, NULL, 1, NULL)
			,('IVMedication', 46, 'Pump tubing changed', 1, 'CheckBox', NULL, 0, NULL, NULL, 1, '^D^^QX1493^=Tubing changed to pump tubing')
			,('IVMedication', 47, 'In buretrol tubing changed', 1, 'CheckBox', NULL, 0, NULL, NULL, 1, '^D^^QX422^=Tubing changed to in Buretrol')
			,('IVMedication', 48, 'Blood tubing changed', 1, 'CheckBox', NULL, 0, NULL, NULL, 1, '^D^^QX423^=Tubing changed to blood tubing')
			,('IVMedication', 49, 'Gravity tubing changed', 1, 'CheckBox', NULL, 0, NULL, NULL, 1, '^D^^QX424^=Tubing changed to gravity tubing')
			,('IVMedication', 50, 'Secondary tubing changed', 1, 'CheckBox', NULL, 0, NULL, NULL, 1, '^D^^QX425^=Tubing changed to secondary tubing')
			
			,('IVMedication', 51, 'IVP', 1, 'CheckBox', NULL, 0, NULL, 'true', 1, '^D^^QX116=IVP')
			,('IVMedication', 52, 'Initial (First medication given IVP)', 1, 'CheckBox', NULL, 0, NULL, 'true', 1, '^D^^QX6279=initial medication')
			,('IVMedication', 53, 'Slowly (Initial)', 1, 'CheckBox', NULL, 0, NULL, NULL, 1, '^D=Slowly')
			,('IVMedication', 54, 'Rapidly (Initial)', 1, 'CheckBox', NULL, 0, NULL, NULL, 1, '^D=Rapidly')
			,('IVMedication', 55, 'Subsequent (Each different medication given IVP after initial medication IVP)', 1, 'CheckBox', NULL, 0, NULL, 'true', 1, '^D^^QX4594=subsequent different medication')
			,('IVMedication', 56, 'Slowly (Subsequent)', 1, 'CheckBox', NULL, 0, NULL, NULL, 1, '^D=Slowly')
			,('IVMedication', 57, 'Rapidly (Subsequent)', 1, 'CheckBox', NULL, 0, NULL, NULL, 1, '^D=Rapidly')
			,('IVMedication', 58, 'Repeat (Repeat dose of previous medication given)', 1, 'CheckBox', NULL, 0, NULL, 'true', 1, '^D^^QX6268=repeat same medication')
			,('IVMedication', 59, 'Slowly (Repeat)', 1, 'CheckBox', NULL, 0, NULL, NULL, 1, '^D=Slowly')
			,('IVMedication', 60, 'Rapidly (Repeat)', 1, 'CheckBox', NULL, 0, NULL, NULL, 1, '^D=Rapidly')
			
			,('IVMedication', 61, 'Added to existing IV fluid', 1, 'CheckBox', NULL, 0, NULL, 'true', 1, '^D^IVadd^QX1036=added to existing IV Fluid')
			,('IVMedication', 62, 'Type', 1, 'FreeText', NULL, 0, NULL, NULL, 1, '^CType:^type=')
			,('IVMedication', 63, 'Amount of fluid remaining', 1, 'FreeText', NULL, 0, NULL, NULL, 1, '^CAmount of fluid remaining:^amont=')
			
			,('IVMedication', 64, 'IVPB/drip', 1, 'CheckBox', NULL, 0, NULL, 'true', 1, '^D^drip=IVPB or drip')
			,('IVMedication', 65, 'IVPB/drip type', 1, 'DropDownListBox', 'Initial (1st medication infusion this IV site)', 0, NULL, 'true', 1, '')
			,('IVMedication', 66, 'Premixed', 1, 'CheckBox', NULL, 0, NULL, NULL, 1, '^D=Premixed')
			,('IVMedication', 67, 'IVPB mixed in', 1, 'DropDownListBox', NULL, 0, NULL, 'true', 1, NULL)
			,('IVMedication', 68, 'Other IVPB mixed in', 1, 'FreeText', NULL, 0, NULL, NULL, 1, '^COther IVPB mixed in:^other=')
			,('IVMedication', 69, 'Fluid', 1, 'DropDownListBox', NULL, 0, NULL, 'true', 1, NULL)
			,('IVMedication', 70, 'IVPB/drip tubing', 1, 'Label', NULL, 0, NULL, NULL, 1, NULL)
			,('IVMedication', 71, 'Primary IVPB/drip tubing', 1, 'CheckBox', NULL, 0, NULL, NULL, 1, '^D^^QX1748=via primary tubing')
			,('IVMedication', 72, 'Gravity IVPB/drip tubing', 1, 'CheckBox', NULL, 0, NULL, NULL, 1, '^D^^QX424=via gravity tubing')
			,('IVMedication', 73, 'Blood IVPB/drip tubing', 1, 'CheckBox', NULL, 0, NULL, NULL, 1, '^D^^QX423=via blood tubing')
			,('IVMedication', 74, 'Pump IVPB/drip tubing', 1, 'CheckBox', NULL, 0, NULL, NULL, 1, '^D^^QX1493=via pump tubing')
			,('IVMedication', 75, 'On an IV pump', 1, 'CheckBox', NULL, 0, NULL, NULL, 1, '^D^^QX421=on an IV pump')
			,('IVMedication', 76, 'Secondary IVPB/drip tubing', 1, 'CheckBox', NULL, 0, NULL, NULL, 1, '^D^^QX425=via secondary tubing')
			,('IVMedication', 77, 'In buretrol (IVPB/drip Tubing)', 1, 'CheckBox', NULL, 0, NULL, NULL, 1, '^D^^QX422=via Buretrol')
			,('IVMedication', 78, 'Rate (IVPB/drip)', 1, 'DropDownListBox', NULL, 0, NULL, 'true', 1, NULL)
			,('IVMedication', 79, 'Pediatric rate (IVPB/drip)', 1, 'DropDownListBox', NULL, 0, NULL, 'true', 1, NULL)
			,('IVMedication', 80, 'Other rate (IVPB/drip)', 1, 'FreeText', NULL, 0, NULL, NULL, 1, '^COther rate at:=')
			,('IVMedication', 81, 'Filter Needle used with administration', 1, 'CheckBox', NULL, 0, NULL, NULL, 1, '^D=Filter needle used with administration')
			,('IVMedication', 82, 'This is considered a thrombolytic infusion (See Thrombolytic Record)', 1, 'CheckBox', NULL, 0, NULL, NULL, 1, '^D=This is considered a thrombolytic infusion (See Thrombolytic Record)')
			,('IVMedication', 83, 'This is considered a sedation medication (See Sedation Record)', 1, 'CheckBox', NULL, 0, NULL, NULL, 1, '^D=This is considered a procedural sedation medication (See Sedation Record)')
		
			,('Emotional', 1, 'Emotional support needed and given', 1, 'CheckBox', NULL, 0, NULL, NULL, 1, '^D^^QX85=Emotional support needed and given')
			,('Emotional', 2, 'Tolerated Procedure', 1, 'DropDownListBox', NULL, 0, NULL, NULL, 1, '')
			,('Emotional', 3, 'Additional Staff Required', 1, 'DropDownListBox', NULL, 0, NULL, NULL, 1, '')
			,('Emotional', 4, 'Reason', 1, 'DropDownListBox', NULL, 0, NULL, NULL, 1, '')
			,('Emotional', 5, 'Administered by', 1, 'FreeText', NULL, 0, 'Myself', NULL, 1, '^CAdministered by=')

			,('IVInIEmotional', 1, 'Emotional support needed and given', 1, 'CheckBox', NULL, 0, NULL, NULL, 1, '^D^^QX85=Emotional support needed and given')
			,('IVInIEmotional', 2, 'Tolerated Procedure', 1, 'DropDownListBox', NULL, 0, NULL, NULL, 1, '')
			,('IVInIEmotional', 3, 'Additional Staff Required', 1, 'DropDownListBox', NULL, 0, NULL, NULL, 1, '')
			,('IVInIEmotional', 4, 'Reason', 1, 'DropDownListBox', NULL, 0, NULL, NULL, 1, '')

			,('Safety', 1, 'Patient in position of comfort', 1, 'CheckBox', NULL, 0, NULL, NULL, 1, '^D=Patient in position of comfort')
			,('Safety', 2, 'Side rails up', 1, 'CheckBox', NULL, 0, NULL, NULL, 1, '^D=Side rails up')
			,('Safety', 3, 'Cart in lowest position', 1, 'CheckBox', NULL, 0, NULL, NULL, 1, '^D=Cart in lowest position')
			,('Safety', 4, 'Family at bedside', 1, 'CheckBox', NULL, 0, NULL, NULL, 1, '^D=Family at bedside')
			,('Safety', 5, 'All of the above', 1, 'CheckBox', NULL, 0, NULL, NULL, 1, '')
			,('Safety', 6, 'Friend at bedside', 1, 'CheckBox', NULL, 0, NULL, NULL, 1, '^D=Friend at bedside')
			,('Safety', 7, 'Call light in reach', 1, 'CheckBox', NULL, 0, NULL, NULL, 1, '^D=Call light in reach')
			,('Safety', 8, 'Other:', 1, 'MultiLineFreeText', NULL, 0, NULL, NULL, 1, '^C=')

			,('AmbulateSafety', 1, 'Advised not to ambulate without assistance', 1, 'CheckBox', NULL, 0, NULL, NULL, 1, '^D=Advised not to ambulate without assistance')
			,('AmbulateSafety', 2, 'Patient in position of comfort', 1, 'CheckBox', NULL, 0, NULL, NULL, 1, '^D=Patient in position of comfort')
			,('AmbulateSafety', 3, 'Side rails up', 1, 'CheckBox', NULL, 0, NULL, NULL, 1, '^D=Side rails up')
			,('AmbulateSafety', 4, 'Cart in lowest position', 1, 'CheckBox', NULL, 0, NULL, NULL, 1, '^D=Cart in lowest position')
			,('AmbulateSafety', 5, 'Family at bedside', 1, 'CheckBox', NULL, 0, NULL, NULL, 1, '^D=Family at bedside')
			,('AmbulateSafety', 6, 'All of the above', 1, 'CheckBox', NULL, 0, NULL, NULL, 1, '')
			,('AmbulateSafety', 7, 'Friend at bedside', 1, 'CheckBox', NULL, 0, NULL, NULL, 1, '^D=Friend at bedside')
			,('AmbulateSafety', 8, 'Call light in reach', 1, 'CheckBox', NULL, 0, NULL, NULL, 1, '^D=Call light in reach')
			,('AmbulateSafety', 9, 'Other:', 1, 'MultiLineFreeText', NULL, 0, NULL, NULL, 1, '^C=')

			,('IVSafety', 1, 'Patient in position of comfort', 1, 'CheckBox', NULL, 0, NULL, NULL, 1, '^D=Patient in position of comfort')
			,('IVSafety', 2, 'Side rails up', 1, 'CheckBox', NULL, 0, NULL, NULL, 1, '^D=Side rails up')
			,('IVSafety', 3, 'Cart in lowest position', 1, 'CheckBox', NULL, 0, NULL, NULL, 1,'^D=Cart in lowest position')
			,('IVSafety', 4, 'Call light in reach', 1, 'CheckBox', NULL, 0, NULL, NULL, 1, '^D=Call light in reach')
			,('IVSafety', 5, 'All of the above', 1, 'CheckBox', NULL, 0, NULL, NULL, 1, '')
			,('IVSafety', 6, 'Family at bedside', 1, 'CheckBox', NULL, 0, NULL, NULL, 1, '^D=Family at bedside')
			,('IVSafety', 7, 'Friend at bedside', 1, 'CheckBox', NULL, 0, NULL, NULL, 1, '^D=Friend at bedside')
			,('IVSafety', 8, 'Other:', 1, 'MultiLineFreeText', NULL, 0, NULL, NULL, 1, '^C=')

			,('IVInISafety', 1, 'Patient in position of comfort', 1, 'CheckBox', NULL, 0, NULL, NULL, 1, '^D=Patient in position of comfort')
			,('IVInISafety', 2, 'Side rails up', 1, 'CheckBox', NULL, 0, NULL, NULL, 1, '^D=Side rails up')
			,('IVInISafety', 3, 'Cart in lowest position', 1, 'CheckBox', NULL, 0, NULL, NULL, 1, '^D=Cart in lowest position')
			,('IVInISafety', 4, 'Call light in reach', 1, 'CheckBox', NULL, 0, NULL, NULL, 1, '^D=Call light in reach')
			,('IVInISafety', 5, 'All of the above', 1, 'CheckBox', NULL, 0, NULL, NULL, 1, '')
			,('IVInISafety', 6, 'Family at bedside', 1, 'CheckBox', NULL, 0, NULL, NULL, 1, '^D=Family at bedside')
			,('IVInISafety', 7, 'Friend at bedside', 1, 'CheckBox', NULL, 0, NULL, NULL, 1, '^D=Friend at bedside')
			,('IVInISafety', 8, 'Advise no ambulate w/o help', 1, 'CheckBox', NULL, 0, NULL, NULL, 1, '^D=Advise no ambulate without help')

			,('CancelReason', 1, 'Symptoms resolved', 1, 'CheckBox', NULL, 0, NULL, NULL, 1, '^D=Symptoms resolved')
			,('CancelReason', 2, 'Patient refused', 1, 'CheckBox', NULL, 0, NULL, NULL, 1, '^D=Patient refused')
			,('CancelReason', 3, 'Change in medication plan', 1, 'CheckBox', NULL, 0, NULL, NULL, 1, '^D=Change in medication plan')
            
			,('Notes_At_Notify', 1, 'Notes', 1, 'MultiLineFreeText', NULL, 0, NULL, NULL, 1, '^C=')
			,('Notes_At_Notify', 2, 'At', 1, 'DateTime', 'Now', 1, NULL, NULL, 1, '^CTime canceled:=')
			,('Notes_At_Notify', 3, 'Notify', 1, 'Notify', NULL, 0, NULL, NULL, 1, '^CNotified:=')

			,('RescheduleDetails', 1, 'Reschedule to', 1, 'DateTime', 'Now', 1, '~~future', NULL, 1, '^CTime rescheduled to:')
			,('RescheduleDetails', 2, 'All future administration times will be updated based on the previously entered frequency.', 1, 'Information', NULL, 0, NULL, NULL, 1, '^D=All future administration times will be updated based on the previously entered frequency')
			
			,('HoldAndMissedDose', 1, 'Vital signs out of range', 1, 'CheckBox', NULL, 0, NULL, NULL, 1, '^D=Vital signs out of range')
			,('HoldAndMissedDose', 2, 'Vital signs stabilized', 1, 'CheckBox', NULL, 0, NULL, NULL, 1, '^D=Vital signs stabilized')
			,('HoldAndMissedDose', 3, 'Patient refused', 1, 'CheckBox', NULL, 0, NULL, NULL, 1, '^D=Patient refused')
			,('HoldAndMissedDose', 4, 'Pain controlled at present', 1, 'CheckBox', NULL, 0, NULL, NULL, 1, '^D=Pain controlled at present')
			,('HoldAndMissedDose', 5, 'Symptoms controlled at present', 1, 'CheckBox', NULL, 0, NULL, NULL, 1, '^D=Symptoms controlled at present')
			,('HoldAndMissedDose', 6, 'Awaiting order confirmation', 1, 'CheckBox', NULL, 0, NULL, NULL, 1, '^D=Awaiting order confirmation')
			,('HoldAndMissedDose', 7, 'Catheter/tube placement can not be confirmed', 1, 'CheckBox', NULL, 0, NULL, NULL, 1, '^D=Catheter/tube placement can not be confirmed')
			,('HoldAndMissedDose', 8, 'Administration route unavailable', 1, 'CheckBox', NULL, 0, NULL, NULL, 1, '^D=Administration route unavailable')
			,('HoldAndMissedDose', 9, 'Attending physician aware', 1, 'CheckBox', NULL, 0, NULL, NULL, 1, '^D=Attending physician aware')
			,('HoldAndMissedDose', 10, 'Out of department', 1, 'CheckBox', NULL, 0, NULL, NULL, 1, '^D=Out of department')

			,('DiscontinueReason', 1, 'Vital signs out of range', 1, 'CheckBox', NULL, 0, NULL, NULL, 1, '^D^Discontinue=Vital signs out of range')
			,('DiscontinueReason', 2, 'Vital signs stabilized', 1, 'CheckBox', NULL, 0, NULL, NULL, 1, '^D^Discontinue=Vital signs stabilized')
			,('DiscontinueReason', 3, 'Patient refused', 1, 'CheckBox', NULL, 0, NULL, NULL, 1, '^D^Discontinue=Patient refused')
			,('DiscontinueReason', 4, 'Pain controlled at present', 1, 'CheckBox', NULL, 0, NULL, NULL, 1, '^D^Discontinue=Pain controlled at present')
			,('DiscontinueReason', 5, 'Symptoms controlled at present', 1, 'CheckBox', NULL, 0, NULL, NULL, 1, '^D^Discontinue=Symptoms controlled at present')
			,('DiscontinueReason', 6, 'Awaiting order confirmation', 1, 'CheckBox', NULL, 0, NULL, NULL, 1, '^D^Discontinue=Awaiting order confirmation')
			,('DiscontinueReason', 7, 'Catheter/tube placement can not be confirmed', 1, 'CheckBox', NULL, 0, NULL, NULL, 1, '^D^Discontinue=Catheter/tube placement can not be confirmed')
			,('DiscontinueReason', 8, 'Administration route unavailable', 1, 'CheckBox', NULL, 0, NULL, NULL, 1, '^D^Discontinue=Administration route unavailable')
			,('DiscontinueReason', 9, 'Attending physician aware', 1, 'CheckBox', NULL, 0, NULL, NULL, 1, '^D^Discontinue=Attending physician aware')
			,('DiscontinueReason', 10, 'Out of department', 1, 'CheckBox', NULL, 0, NULL, NULL, 1, '^D^Discontinue=Out of department')
			,('DiscontinueReason', 11, 'Verbal order given', 1, 'CheckBox', NULL, 0, NULL, NULL, 1, '^D^Discontinue=Verbal order given')

			,('Delete', 1, 'Are you sure you want to delete this order?', 1, 'Information', NULL, 0, NULL, NULL, 1, '')

			,('Unhold', 1, 'Vital signs improved', 1, 'CheckBox', NULL, 0, NULL, NULL, 1, '^D^UNHOLD=Vital signs improved')
			,('Unhold', 2, 'Patient currently in department', 1, 'CheckBox', NULL, 0, NULL, NULL, 1, '^D^UNHOLD=Patient currently in department')
			,('Unhold', 3, 'Patient consents', 1, 'CheckBox', NULL, 0, NULL, NULL, 1, '^D^UNHOLD=Patient consents')
			,('Unhold', 4, 'Pain not controlled at present', 1, 'CheckBox', NULL, 0, NULL, NULL, 1, '^D^UNHOLD=Pain not controlled at present')
			,('Unhold', 5, 'Received order confirmation', 1, 'CheckBox', NULL, 0, NULL, NULL, 1, '^D^UNHOLD=Received order confirmation')
			,('Unhold', 6, 'Returned to department', 1, 'CheckBox', NULL, 0, NULL, NULL, 1, '^D^UNHOLD=Returned to department')

			,('GenericGive', 1, 'Notes', 1, 'MultiLineFreeText', NULL, 0, NULL, NULL, 1, '^CNotes:=')
			,('GenericGive', 2, 'Given At', 1, 'DateTime', 'Now', 1, NULL, NULL, 1, '^CTime given:=')
			,('GenericGive', 3, 'Self Administered', 1, 'CheckBox', NULL, 0, NULL, NULL, 1, '^D=Self Administered')
			,('GenericGive', 4, 'Patient Supplied', 1, 'CheckBox', NULL, 0, NULL, NULL, 1, '^D=Patient Supplied')
			,('GenericGive', 5, 'First Dose Urgent Ordered by Provider', 1, 'CheckBox', NULL, 0, NULL, NULL, 1, '^D=First Dose Urgent Ordered by Provider')
			,('GenericGive', 6, 'Notify', 1, 'Notify', NULL, 0, NULL, NULL, 1, '^CNotified:=')

			,('DefaultGive', 1, 'Description', 1, 'MultiLineFreeText', NULL, 0, NULL, NULL, 1, '^CDescription:=')
			,('DefaultGive', 2, 'Administered by', 1, 'FreeText', NULL, 0, 'Myself', NULL, 1, '^CAdministered by:=')

			,('IntraMuscMedication', 1, 'Verbal order read back and verified', 1, 'CheckBox', NULL, 0, NULL, NULL, 1, '^D=Verbal order read back and verified')

			,('IntraMuscMedication', 2, 'IM (Not an antibiotic or immunization)', 1, 'CheckBox', NULL, 0, NULL, 'true', 1, '^D^^QX216=IM medication')
			,('IntraMuscMedication', 3, 'Site ~~(non-antibiotic/immunization)', 1, 'DropDownListBox', NULL, 0, NULL, NULL, 1, '')
			,('IntraMuscMedication', 4, 'Other site ~~(non-antibiotic/immunization)', 1, 'FreeText', NULL, 0, NULL, NULL, 1, '^CMedication administered to ^Location=')
			,('IntraMuscMedication', 5, 'Amount given ~~(non-antibiotic/immunization)', 1, 'FreeText', NULL, 0, NULL, NULL, 1, '^CAmount given:^^^^medsvc_amtgiven=')
			,('IntraMuscMedication', 6, 'Combined with ~~(non-antibiotic/immunization)', 1, 'FreeText', NULL, 0, NULL, NULL, 1, '^CMedication combined for administration with=')

			,('IntraMuscMedication', 7, 'IM antibiotic', 1, 'CheckBox', NULL, 0, NULL, 'true', 1, '^D^^QX217=IM antibiotic')
			,('IntraMuscMedication', 8, 'Site ~~(antibiotic)', 1, 'DropDownListBox', NULL, 0, NULL, NULL, 1, '')
			,('IntraMuscMedication', 9, 'Other site ~~(antibiotic)', 1, 'FreeText', NULL, 0, NULL, NULL, 1, '^CMedication administered to ^Location2=')
			,('IntraMuscMedication', 10, 'Amount given ~~(antibiotic)', 1, 'FreeText', NULL, 0, NULL, NULL, 1, '^CAmount given:=')
			,('IntraMuscMedication', 11, 'Combined with ~~(antibiotic)', 1, 'FreeText', NULL, 0, NULL, NULL, 1, '^CMedication combined for administration with=')

			,('IntraMuscMedication', 12, 'IM immunization', 1, 'CheckBox', NULL, 0, NULL, 'true', 1, '^D^^QX218^medsvc_vac_route=IM immunization')
			,('IntraMuscMedication', 13, 'Site ~~(immunization)', 1, 'DropDownListBox', NULL, 0, NULL, NULL, 1, '')
			,('IntraMuscMedication', 14, 'Other site ~~(immunization)', 1, 'FreeText', NULL, 0, NULL, NULL, 1, '^CMedication administered to ^Location^^^medsvc_vac_site=')
			,('IntraMuscMedication', 15, 'Amount given ~~(immunization)', 1, 'FreeText', NULL, 0, NULL, NULL, 1, '^CAmount given:^^^^medsvc_vac_amt=')
			,('IntraMuscMedication', 16, 'unit', 1, 'FreeText', NULL, 0, NULL, NULL, 0, '^C^^^^medsvc_vac_unit=')
			,('IntraMuscMedication', 17, 'Combined with ~~(immunization)', 1, 'FreeText', NULL, 0, NULL, NULL, 1, '^CMedication combined for administration with=')
			,('INtraMuscMedication', 18, 'Dose number', 1, 'DropDownListBox', NULL, 1, NULL, NULL, 1, '')
			,('INtraMuscMedication', 19, 'Other dose number', 1, 'FreeText', NULL, 0, NULL, NULL, 1, '^C^^^^medsvc_vac_dosenum=')
			,('INtraMuscMedication', 20, 'Previous dose confirmed by', 1, 'DropDownListBox', NULL, 0, NULL, NULL, 1, '')
			,('IntraMuscMedication', 21, 'Vaccination information sheet given to patient', 1, 'CheckBox', NULL, 0, NULL, NULL, 1, '^D^Itram^^medsvc_vac_sheetgiven=Vaccination information sheet given to patient')
			,('IntraMuscMedication', 22, 'Date of publication', 1, 'Date', NULL, 0, NULL, NULL, 1, '^Cdate of publication:^im^^^medsvc_vac_pubdate=')
			,('IntraMuscMedication', 23, 'Name of publication', 1, 'FreeText', NULL, 0, NULL, NULL, 1, '^Cname of publication:^^^^medsvc_vac_pubname=')
			,('IntraMuscMedication', 24, 'Manufacturer', 1, 'FreeText', NULL, 0, NULL, NULL, 1, '^Cmanufacturer:^^^^medsvc_vac_manuf=')
			,('IntraMuscMedication', 25, 'Lot number', 1, 'FreeText', NULL, 0, NULL, NULL, 1, '^Clot number:^^^^medsvc_vac_lot=')
			,('IntraMuscMedication', 26, 'Expiration', 1, 'Date', NULL, 0, NULL, NULL, 1, '^Cexpiration:^intram^^^medsvc_vac_exp=')

			,('Assessment', 1, 'O2 Sat', 1, 'DropDownListBox', NULL, 0, NULL, NULL, 1, '')
			,('Assessment', 2, 'O2 Amount', 1, 'DropDownListBox', NULL, 0, NULL, NULL, 1, '')
			,('Assessment', 3, 'O2 Type', 1, 'DropDownListBox', NULL, 0, NULL, NULL, 1, '')
			,('Assessment', 4, 'Assessment notes', 1, 'FreeText', NULL, 0, NULL, NULL, 1, '^C^assessnotes=')
			,('Assessment', 7, 'Correct patient, time, route, dose and medication confirmed prior to administration', 1, 'CheckBox', NULL, 0, NULL, NULL, 1, '^D=Correct patient, time, route, dose and medication confirmed prior to administration')
			,('Assessment', 8, 'Patient advised of actions and side-effects prior to administration', 1, 'CheckBox', NULL, 0, NULL, NULL, 1, '^D=Patient advised of actions and side-effects prior to administration')
			,('Assessment', 9, 'Allergies confirmed and medications reviewed prior to administration', 1, 'CheckBox', NULL, 0, NULL, NULL, 1, '^D=Patient advised of actions and side-effects prior to administration')
			,('Assessment', 10, 'All of the above', 1, 'CheckBox', NULL, 0, NULL, NULL, 1, '')

			,('InhalationAssessment', 1, 'Peak-Flow prior', 1, 'CheckBox', NULL, 0, NULL, NULL, 1, '^D^peakflow^QX1197=Pre-administration assessment shows peak-flow')
			,('InhalationAssessment', 2, 'O2 Sat', 1, 'DropDownListBox', NULL, 1, NULL, NULL, 1, '')
			,('InhalationAssessment', 3, 'O2 Amount', 1, 'DropDownListBox', NULL, 1, NULL, NULL, 1, '')
			,('InhalationAssessment', 4, 'O2 Type', 1, 'DropDownListBox', NULL, 1, NULL, NULL, 1, '')
			,('InhalationAssessment', 5, 'Rhythm', 1, 'DropDownListBox', NULL, 0, NULL, NULL, 1, '')
			,('InhalationAssessment', 6, 'Ectopy', 1, 'DropDownListBox', NULL, 0, NULL, NULL, 1, '')
			,('InhalationAssessment', 7, 'St Changes', 1, 'DropDownListBox', NULL, 0, NULL, NULL, 1, '')
			,('InhalationAssessment', 8, 'Correct patient, time, route, dose and medication confirmed prior to administration', 1, 'CheckBox', NULL, 0, NULL, NULL, 1, '^D=Correct patient, time, route, dose and medication confirmed prior to administration')
			,('InhalationAssessment', 9, 'Patient advised of actions and side-effects prior to administration', 1, 'CheckBox', NULL, 0, NULL, NULL, 1, '^D=Patient advised of actions and side-effects prior to administration')
			,('InhalationAssessment', 10, 'Allergies confirmed and medications reviewed prior to administration', 1, 'CheckBox', NULL, 0, NULL, NULL, 1, '^D=Patient advised of actions and side-effects prior to administration')
			,('InhalationAssessment', 11, 'All of the above', 1, 'CheckBox', NULL, 0, NULL, NULL, 1, '')

			,('IntraOssAssessment', 1, 'O2 Sat', 1, 'DropDownListBox', NULL, 1, NULL, NULL, 1, '')
			,('IntraOssAssessment', 2, 'O2 Amount', 1, 'DropDownListBox', NULL, 1, NULL, NULL, 1, '')
			,('IntraOssAssessment', 3, 'O2 Type', 1, 'DropDownListBox', NULL, 1, NULL, NULL, 1, '')
			,('IntraOssAssessment', 4, 'Rhythm', 1, 'DropDownListBox', NULL, 1, NULL, NULL, 1, '')
			,('IntraOssAssessment', 5, 'Ectopy', 1, 'DropDownListBox', NULL, 1, NULL, NULL, 1, '')
			,('IntraOssAssessment', 6, 'St Changes', 1, 'DropDownListBox', NULL, 1, NULL, NULL, 1, '')
			,('IntraOssAssessment', 7, 'Needle placement confirmed via aspiration prior to administration', 1, 'CheckBox', NULL, 0, NULL, NULL, 1, '^D=Needle placement confirmed via aspiration prior to administration')
			,('IntraOssAssessment', 8, 'Correct patient, time, route, dose and medication confirmed prior to administration', 1, 'CheckBox', NULL, 0, NULL, NULL, 1, '^D=Correct patient, time, route, dose and medication confirmed prior to administration')
			,('IntraOssAssessment', 9, 'Patient advised of actions and side-effects prior to administration', 1, 'CheckBox', NULL, 0, NULL, NULL, 1, '^D=Patient advised of actions and side-effects prior to administration')
			,('IntraOssAssessment', 10, 'Allergies confirmed and medications reviewed prior to administration', 1, 'CheckBox', NULL, 0, NULL, NULL, 1, '^D=Patient advised of actions and side-effects prior to administration')
			,('IntraOssAssessment', 11, 'All of the above', 1, 'CheckBox', NULL, 0, NULL, NULL, 1, '')

			,('IVAssessment', 1, 'Correct patient, time, route, dose and medication confirmed prior to administration', 1, 'CheckBox', NULL, 1, NULL, NULL, 1, '^D=Correct patient, time, route, dose and medication confirmed prior to administration')
			,('IVAssessment', 2, 'Patient advised of actions and side-effects prior to administration', 1, 'CheckBox', NULL, 1, NULL, NULL, 1, '^D=Patient advised of actions and side-effects prior to administration')
			,('IVAssessment', 3, 'Allergies confirmed and medications reviewed prior to administration', 1, 'CheckBox', NULL, 1, NULL, NULL, 1, '^D=Allergies confirmed and medications reviewed prior to administration')
			,('IVAssessment', 4, 'Connections checked prior to administration', 1, 'CheckBox', NULL, 1, NULL, NULL, 1, '^D=Connections checked prior to administration')
			,('IVAssessment', 5, 'Line traced prior to administration', 1, 'CheckBox', NULL, 1, NULL, NULL, 1, '^D=Line traced prior to administration')
			,('IVAssessment', 6, 'Catheter placement confirmed via flush prior to administration', 1, 'CheckBox', NULL, 1, NULL, NULL, 1, '^D=Catheter placement confirmed via flush prior to administration')
			,('IVAssessment', 7, 'IV site without s/sx of infiltration during medication administration', 1, 'CheckBox', NULL, 0, NULL, NULL, 1, '^D=IV site without signs or symptoms of infiltration during medication administration')
			,('IVAssessment', 8, 'No swelling during administration', 1, 'CheckBox', NULL, 0, NULL, NULL, 1, '^D=No swelling during administration')
			,('IVAssessment', 9, 'No drainage during administration', 1, 'CheckBox', NULL, 0, NULL, NULL, 1, '^D=No drainage during administration')
			,('IVAssessment', 10, 'IV flushed after administration', 1, 'CheckBox', NULL, 0, NULL, NULL, 1, '^D=No drainage during administration')
			,('IVAssessment', 11, 'All of the above', 1, 'CheckBox', NULL, 0, NULL, NULL, 1, '')

			,('IVInIAssessment', 1, 'Correct patient, time, route, dose and medication/fluid confirmed', 1, 'CheckBox', NULL, 1, NULL, NULL, 1, '^D=Correct patient, time, route, dose and medication/fluid confirmed')
			,('IVInIAssessment', 2, 'Patient advised of actions and side-effects ', 1, 'CheckBox', NULL, 1, NULL, NULL, 1, '^D=Patient advised of actions and side-effects')
			,('IVInIAssessment', 3, 'Allergies confirmed and medications reviewed', 1, 'CheckBox', NULL, 1, NULL, NULL, 1, '^D=Allergies confirmed and medications reviewed')
			,('IVInIAssessment', 4, 'Connection checked', 1, 'CheckBox', NULL, 1, NULL, NULL, 1, '^D=Connection checked')
			,('IVInIAssessment', 5, 'Line traced', 1, 'CheckBox', NULL, 1, NULL, NULL, 1, '^D=Line traced')
			,('IVInIAssessment', 6, 'Catheter placement confirmed via flush', 1, 'CheckBox', NULL, 1, NULL, NULL, 1, '^D=Catheter placement confirmed via flush')
			,('IVInIAssessment', 7, 'IV site without s/sx of infiltration', 1, 'CheckBox', NULL, 0, NULL, NULL, 1, '^D=IV site without signs or symptoms of infiltration')
			,('IVInIAssessment', 8, 'No swelling', 1, 'CheckBox', NULL, 0, NULL, NULL, 1, '^D=No swelling')
			,('IVInIAssessment', 9, 'No drainage', 1, 'CheckBox', NULL, 0, NULL, NULL, 1, '^D=No drainage')
			,('IVInIAssessment', 10, 'IV flushed after administration', 1, 'CheckBox', NULL, 0, NULL, NULL, 1, '^D=IV flushed after administration')
			,('IVInIAssessment', 11, 'All of the above', 1, 'CheckBox', NULL, 0, NULL, NULL, 1, '')

			,('GeneralAssessment', 1, 'Symptoms', 1, 'threeStateButton', NULL, 0, NULL, NULL, 1, '')
			,('GeneralAssessment', 2, 'Pain', 1, 'threeStateButton', NULL, 0, NULL, NULL, 1, '')
			,('GeneralAssessment', 3, 'Heart rate', 1, 'threeStateButton', NULL, 0, NULL, NULL, 1, '')
			,('GeneralAssessment', 4, 'Blood pressure', 1, 'threeStateButton', NULL, 0, NULL, NULL, 1, '')
			,('GeneralAssessment', 5, 'Temperature', 1, 'threeStateButton', NULL, 0, NULL, NULL, 1, '')
			,('GeneralAssessment', 6, 'Nausea', 1, 'threeStateButton', NULL, 0, NULL, NULL, 1, '')
			,('GeneralAssessment', 7, 'Vomiting', 1, 'threeStateButton', NULL, 0, NULL, NULL, 1, '')
			,('GeneralAssessment', 8, 'Rash', 1, 'threeStateButton', NULL, 0, NULL, NULL, 1, '')
			,('GeneralAssessment', 9, 'Respiratory rate', 1, 'threeStateButton', NULL, 0, NULL, NULL, 1, '')
			,('GeneralAssessment', 10, 'Respiratory effort', 1, 'threeStateButton', NULL, 0, NULL, NULL, 1, '')
			,('GeneralAssessment', 11, 'Breath sounds improved', 1, 'threeStateButton', NULL, 0, NULL, NULL, 1, '')
			,('GeneralAssessment', 12, 'Mental status', 1, 'threeStateButton', NULL, 0, NULL, NULL, 1, '')
			,('GeneralAssessment', 13, 'Urine output', 1, 'threeStateButton', NULL, 0, NULL, NULL, 1, '')
			,('GeneralAssessment', 14, 'Constipation', 1, 'threeStateButton', NULL, 0, NULL, NULL, 1, '')
			
			,('SiteInspection', 1, 'Swelling ', 1, 'CheckBox', NULL, 0, NULL, NULL, 1, '^D=Swelling at site^^U')
			,('SiteInspection', 2, 'No swelling', 1, 'CheckBox', NULL, 0, NULL, NULL, 1, '^D=No swelling at site')
			,('SiteInspection', 3, 'Drainage', 1, 'CheckBox', NULL, 0, NULL, NULL, 1, '^D=Drainage at site^^U')
			,('SiteInspection', 4, 'No drainage', 1, 'CheckBox', NULL, 0, NULL, NULL, 1, '^D=No drainage at site')
			,('SiteInspection', 5, 'Bleeding', 1, 'CheckBox', NULL, 0, NULL, NULL, 1, '^D=Bleeding at site^^U')
			,('SiteInspection', 6, 'No bleeding', 1, 'CheckBox', NULL, 0, NULL, NULL, 1, '^D=No bleeding at site')
			,('SiteInspection', 7, 'Bruising', 1, 'CheckBox', NULL, 0, NULL, NULL, 1, '^D=Bruising at site^^U')
			,('SiteInspection', 8, 'No bruising', 1, 'CheckBox', NULL, 0, NULL, NULL, 1, '^D=No bruising at site ')
			,('SiteInspection', 9, 'All normal', 1, 'CheckBox', NULL, 0, NULL, NULL, 1, '')
			,('SiteInspection', 10, 'No S/S of allergic reaction', 1, 'CheckBox', NULL, 0, NULL, NULL, 1, '^D=No signs and symptoms of allergic reaction')
			,('SiteInspection', 11, 'Dressing applied', 1, 'CheckBox', NULL, 0, NULL, NULL, 1, '^D^^=Dressing applied')
			,('SiteInspection', 12, 'Warm compress applied', 1, 'CheckBox', NULL, 0, NULL, NULL, 1, '^D^^=Warm compress applied')

			,('IVFollowUp', 1, 'Bag number', 1, 'DropDownListBox', NULL, 0, NULL, NULL, 1, '')
			,('IVFollowUp', 2, 'Bag status', 1, 'DropDownListBox', NULL, 0, NULL, NULL, 1, '')
			,('IVFollowUp', 3, 'Titrating to patient response', 1, 'CheckBox', NULL, 0, NULL, 'true', 1, '^D^titrate=Titrating to patient response')
			,('IVFollowUp', 4, 'Dose increased', 1, 'CheckBox', NULL, 0, NULL, NULL, 1, '^D^dose=Dose increased^^U')
			,('IVFollowUp', 5, 'Dose decreased', 1, 'CheckBox', NULL, 0, NULL, NULL, 1, '^D^dose=Dose decreased^^U')
			,('IVFollowUp', 6, 'Med infusion changed to', 1, 'FreeText', NULL, 0, NULL, NULL, 1, '^CMed Infusion changed to:=')
			
			,('StopTime', 1, 'Infusion Discontinued', 1, 'CheckBox', NULL, 0, NULL, 'true', 1, '^D=Infusion discontinued^^U')
			,('StopTime', 2, 'Date/Time ~~(infusionDiscontinued)', 1, 'DateTime', '', 0, '~~afterGiven', NULL, 1, '^Con=')
			,('StopTime', 3, 'Removed catheter intact', 1, 'CheckBox', NULL, 0, NULL, NULL, 1, '^D=Removed catheter intact')
			,('StopTime', 4, 'IV Line flushed after administration', 1, 'CheckBox', NULL, 0, NULL, NULL, 1, '^D=IV Line flushed after administration')
			,('StopTime', 5, 'Total Amount Infused ~~(infusionDiscontinued)', 1, 'FreeText', NULL, 0, NULL, NULL, 1, '^CTotal Amount Infused:=')
			,('StopTime', 6, 'Stop time unknown', 1, 'CheckBox', NULL, 0, NULL, NULL, 1, '^D^CORE_unknown_stoptime=Unknown stop time^^U')
			,('StopTime', 7, 'Continued upon Transfer', 1, 'CheckBox', NULL, 0, NULL, 'true', 1, '^D=Infusion continued upon transfer from emergency department^^U')
			,('StopTime', 8, 'Date/Time ~~(infusionContinuedUponTransfer)', 1, 'DateTime', '', 0, '~~afterGiven', NULL, 1,'^Con=')
			,('StopTime', 9, 'Total Amount Infused ~~(infusionContinuedUponTransfer)', 1, 'FreeText', NULL, 0, NULL, NULL, 1, '^CTotal Amount Infused:=')
			
			,('FollowUpSafety', 1, 'Advised not to ambulate without assistance', 1, 'CheckBox', NULL, 0, NULL, NULL, 1, '^D=Advised not to ambulate without assistance')
			,('FollowUpSafety', 2, 'Patient in position of comfort', 1, 'CheckBox', NULL, 0, NULL, NULL, 1, '^D=Patient in position of comfort')
			,('FollowUpSafety', 3, 'Side rails up', 1, 'CheckBox', NULL, 0, NULL, NULL, 1, '^D=Side rails up')
			,('FollowUpSafety', 4, 'Cart in lowest position', 1, 'CheckBox', NULL, 0, NULL, NULL, 1, '^D=Cart in lowest position')
			,('FollowUpSafety', 5, 'Call light in reach', 1, 'CheckBox', NULL, 0, NULL, NULL, 1, '^D=Call light in reach')
			,('FollowUpSafety', 6, 'All of the above', 1, 'CheckBox', NULL, 0, NULL, NULL, 1, '')
			,('FollowUpSafety', 7, 'Emotional support needed and given', 1, 'CheckBox', NULL, 0, NULL, NULL, 1, '^D=Emotional support needed and given')
			,('FollowUpSafety', 8, 'Family at bedside', 1, 'CheckBox', NULL, 0, NULL, NULL, 1, '^D=Family at bedside')
			,('FollowUpSafety', 9, 'Friend at bedside', 1, 'CheckBox', NULL, 0, NULL, NULL, 1, '^D=Friend at bedside')
			,('FollowUpSafety', 10, 'Attending physician aware', 1, 'CheckBox', NULL, 0, NULL, NULL, 1, '^D=Attending physician aware')
			,('FollowUpSafety', 11, 'Physican Name:', 1, 'FreeText', NULL, 0, NULL, NULL, 1, '^CPhysician Name:=')

			,('VitalSigns', 1, 'BP (Systolic)', 1, 'FreeText', NULL, 0, NULL, NULL, 1, '^CBP (Systolic):=')
			,('VitalSigns', 2, 'BP (Diastolic)', 1, 'FreeText', NULL, 0, NULL, NULL, 1, '^CBP (Diastolic):=')
			,('VitalSigns', 3, ' ~~(bpCondition)', 1, 'DropDownListBox', NULL, 0, NULL, NULL, 1, '')
			,('VitalSigns', 4, ' ~~(bpSite)', 1, 'DropDownListBox', NULL, 0, NULL, NULL, 1, '')
			,('VitalSigns', 5, 'MAP', 1, 'FreeText', NULL, 0, NULL, NULL, 1, '^CMap:=')
			,('VitalSigns', 6, ' ~~(mapSelect1)', 1, 'DropDownListBox', NULL, 0, NULL, NULL, 1, '')
			,('VitalSigns', 7, ' ~~(mapSelect2)', 1, 'DropDownListBox', NULL, 0, NULL, NULL, 1, '')
			,('VitalSigns', 8, 'PULSE', 1, 'FreeText', NULL, 0, NULL, NULL, 1, '^CPulse:=')
			,('VitalSigns', 9, ' ~~(pulseSelect1)', 1, 'DropDownListBox', NULL, 0, NULL, NULL, 1, '')
			,('VitalSigns', 10, ' ~~(pulseSelect2)', 1, 'DropDownListBox', NULL, 0, NULL, NULL, 1, '')
			,('VitalSigns', 11, 'TEMPERATURE', 1, 'FreeText', NULL, 0, NULL, NULL, 1, '^CTemperature:=')
			,('VitalSigns', 12, ' ~~(temperatureSelect1)', 1, 'DropDownListBox', NULL, 0, NULL, NULL, 1, '')
			,('VitalSigns', 13, ' ~~(temperatureSelect2)', 1, 'DropDownListBox', NULL, 0, NULL, NULL, 1, '')
			,('VitalSigns', 14, 'O2 SAT', 1, 'FreeText', NULL, 0, NULL, NULL, 1, '^CO2 Sat:=')
			,('VitalSigns', 15, 'on', 1, 'FreeText', NULL, 0, NULL, NULL, 1, '^Con=')
			,('VitalSigns', 16, 'RESPIRATORY', 1, 'FreeText', NULL, 0, NULL, NULL, 1, '^CRespiratory:=')
			,('VitalSigns', 17, ' ~~(respiratorySelect1)', 1, 'DropDownListBox', NULL, 0, NULL, NULL, 1, '')
			,('VitalSigns', 18, ' ~~(respiratorySelect2)', 1, 'DropDownListBox', NULL, 0, NULL, NULL, 1, '')
			,('VitalSigns', 19, 'PAIN', 1, 'FreeText', NULL, 0, NULL, NULL, 1, '^CPain:=')
			,('VitalSigns', 20, ' ~~(painSelect1)', 1, 'DropDownListBox', NULL, 0, NULL, NULL, 1, '')
			,('VitalSigns', 21, ' ~~(painSelect2)', 1, 'DropDownListBox', NULL, 0, NULL, NULL, 1, '')
			,('VitalSigns', 22, 'END-TIDAL CO2', 1, 'FreeText', NULL, 0, NULL, NULL, 1, '^CEnd-Tidal CO2:=')
			,('VitalSigns', 23, ' ~~(end-tidalCo2Select1)', 1, 'DropDownListBox', NULL, 0, NULL, NULL, 1, '')
			,('VitalSigns', 24, ' ~~(end-tidalCo2Select2)', 1, 'DropDownListBox', NULL, 0, NULL, NULL, 1, '')

			,('FollowUpGeneric', 1, 'Notes', 1, 'MultiLineFreeText', NULL, 0, NULL, NULL, 1, '^C=')
			,('FollowUpGeneric', 2, 'Documented At', 1, 'DateTime', 'Now', 1, NULL, NULL, 1, '^CDocumentated at:=')
			,('FollowUpGeneric', 3, 'Notify', 1, 'Notify', NULL, 0, NULL, NULL, 1, '^CNotified=')

			,('PharmVerification', 1, 'Notes', 1, 'MultiLineFreeText', NULL, 0, NULL, NULL, 1, '^CPharmary Verification Notes:=')

			,('CoSign', 1, 'I have verified the accuracy of the dose and route for this high alert medication', 1, 'CheckBox', NULL, 0, NULL, NULL, 1, '^D=Verified the accuracy of the dose and route for this high alert medication')
			,('CoSign', 2, 'Notes', 1, 'MultiLineFreeText', NULL, 0, NULL, NULL, 1, '^CCoSign Notes:=')
		   																						 
       ) as [items]
       ([prompt_group_name], [sequence], [prompt], [is_active], [prompt_type], [prompt_default], [required], [placeholder_text], [display_child_prompts_value], [is_on_newline], [chart_markup]);

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
                     or ISNULL([target].[prompt_default], CHAR(0)) <> ISNULL([source].[prompt_default], CHAR(0))
                     or [target].[required] <> [source].[required]
					 or ISNULL([target].[placeholder_text], CHAR(0)) != ISNULL([source].[placeholder_text], CHAR(0))
					 or ISNULL([target].[display_child_prompts_value], CHAR(0)) != ISNULL([source].[display_child_prompts_value], CHAR(0))
					 or [target].[is_on_newline] <> [source].[is_on_newline]
					 or ISNULL([target].[chart_markup], CHAR(0)) != ISNULL([source].[chart_markup], CHAR(0)))
        then update set 
    [prompt] = [source].[prompt]
  , [is_active] = [source].[is_active]
  , [prompt_type] = [source].[prompt_type]
  , [prompt_default] = [source].[prompt_default]
  , [required] = [source].[required]
  , [placeholder_text] = [source].[placeholder_text]
  , [display_child_prompts_value] = [source].[display_child_prompts_value]
  , [is_on_newline] = [source].[is_on_newline]
  , [chart_markup] = [source].[chart_markup]
    when not matched by target
        then
      insert([prompt_group_id]
           , [sequence]
           , [prompt]
           , [is_active]
           , [prompt_type]
           , [prompt_default]
           , [required]
		   , [placeholder_text]
		   , [display_child_prompts_value]
		   , [is_on_newline]
		   , [chart_markup] 
		   )
      values
    ([prompt_group_id], [sequence], [prompt], [is_active], [prompt_type], [prompt_default], [required], [placeholder_text], [display_child_prompts_value], [is_on_newline],[chart_markup])
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
    , [prompt]            [nvarchar](200) not null
	, [chart_markup]	  [nvarchar](256) null
	);

insert into @prompt_choices
    ([prompt_group_name]
   , [prompt]
   , [sequence]
   , [choice_text]
   , [chart_markup]
    )
select [prompt_group_name]
     , [prompt]
     , [sequence]
     , [choice_text]
	 , [chart_markup]
from   (values
			 ('Medication', 'Site', 1, 'Left', '^SSite:=Medication administered on the left side^QX3749')
			,('Medication', 'Site', 2, 'Right', '^SSite:=Medication administered on the right side^QX3749')
			,('Medication', 'Site', 3, 'Bilaterally', '^SSite:=Medication administered bilaterally^QX3749')

			,('EyeMedication', 'Site', 1, 'Left', '^SSite:=Medication administered on the left side^QX2198')
			,('EyeMedication', 'Site', 2, 'Right', '^SSite:=Medication administered on the right side^QX2198')
			,('EyeMedication', 'Site', 3, 'Bilaterally', '^SSite:=Medication administered bilaterally^QX2198')

			,('OralMedication', 'Site', 1, 'P.O.', '^SSite:=Medication administered P.O.^QX214')
			,('OralMedication', 'Site', 2, 'S.L.', '^SSite:=Medication administered S.L.^QX214')
			,('OralMedication', 'Site', 3, 'Buccal', '^SSite:=Medication administered buccal')

			,('NasalMedication', 'Site', 1, 'Left nare', '^SSite:=Medication administered on the left nare^QX2219')
			,('NasalMedication', 'Site', 2, 'Right nare', '^SSite:=Medication administered on the right nare^QX2219')
			,('NasalMedication', 'Site', 3, 'Bilaterally nares', '^SSite:=Medication administered bilateral nares^QX2219')
			,('NasalMedication', 'Site', 4, 'Left Nare Immunization', '^SSite:=Medication administered bilateral nares^QX2219')
			,('NasalMedication', 'Site', 5, 'Right Nare Immunization', '^SSite:=Medication administered on the right nare^QX2219^^medsvc_vac_site')
			,('NasalMedication', 'Site', 6, 'Bilaterally Nares Immunization', '^SSite:=Medication administered on the right nare^QX2219^^medsvc_vac_site')

			,('EnteralMedication', 'Site', 1, 'G-tube', '^SSite:=Medication administered in G-tube^QX2685')
			,('EnteralMedication', 'Site', 2, 'J-tube', '^SSite:=Medication administered in J-tube^QX4746')
			,('EnteralMedication', 'Site', 3, 'NG tube', '^SSite:=Medication administered in NG tube^QX2201')
			,('EnteralMedication', 'Site', 4, 'Orogastric tube', '^SSite:=Medication administered in Orogastric tube^QX2201')

			,('Emotional', 'Tolerated Procedure', 1, 'Well', '^S=Patient tolerated procedure well^QX258')
			,('Emotional', 'Tolerated Procedure', 2, 'With Difficulty', '^S=Patient tolerated procedure with difficulty^QX259^U')
			,('Emotional', 'Tolerated Procedure', 3, 'Uncooperative', '^S=Patient was uncooperative^QX260^U')
			,('Emotional', 'Additional Staff Required', 1, '1 additional staff', '^S=1 additional staff was required to perform this procedure^QX134')
			,('Emotional', 'Additional Staff Required', 2, '2 additional staff', '^S=2 additional staff were required to perform this procedure^QX3725')
			,('Emotional', 'Additional Staff Required', 3, '3 additional staff', '^S=3 additional staff were required to perform this procedure^QX3726')
			,('Emotional', 'Additional Staff Required', 4, '4 additional staff', '^S=4 additional staff were required to perform this procedure^QX3767')
			,('Emotional', 'Reason', 1, 'Age', '^S=due to the patients age^^U')
			,('Emotional', 'Reason', 2, 'Combative', '^S=due to the patient being combative^^U')
			,('Emotional', 'Reason', 3, 'Confused', '^S=due to the patient being confused^^U')
			,('Emotional', 'Reason', 4, 'Distraction', '^S=to distract the patient^^U')
			,('Emotional', 'Reason', 5, 'Uncooperative', '^S=due to uncooperative behavior of the patient^^U')

			,('IVInIEmotional', 'Tolerated Procedure', 1, 'Well', '^S=Patient tolerated procedure well^QX258')
			,('IVInIEmotional', 'Tolerated Procedure', 2, 'With Difficulty', '^S=Patient tolerated procedure with difficulty^QX259^U')
			,('IVInIEmotional', 'Tolerated Procedure', 3, 'Uncooperative', '^S=Patient was uncooperative^QX260^U')
			,('IVInIEmotional', 'Additional Staff Required', 1, '1 additional staff', '^S=1 additional staff was required to perform this procedure^QX134')
			,('IVInIEmotional', 'Additional Staff Required', 2, '2 additional staff', '^S=2 additional staff were required to perform this procedure^QX3725')
			,('IVInIEmotional', 'Additional Staff Required', 3, '3 additional staff', '^S=3 additional staff were required to perform this procedure^QX3726')
			,('IVInIEmotional', 'Additional Staff Required', 4, '4 additional staff', '^S=4 additional staff were required to perform this procedure^QX3767')
			,('IVInIEmotional', 'Reason', 1, 'Age', '^S=due to the patients age^^U')
			,('IVInIEmotional', 'Reason', 2, 'Combative', '^S=due to the patient being combative^^U')
			,('IVInIEmotional', 'Reason', 3, 'Confused', '^S=due to the patient being confused^^U')
			,('IVInIEmotional', 'Reason', 4, 'Distraction', '^S=to distract the patient^^U')
			,('IVInIEmotional', 'Reason', 5, 'Uncooperative', '^S=due to uncooperative behavior of the patient^^U')
		
			,('IntraMuscMedication', 'Site ~~(non-antibiotic/immunization)', 1, 'Deltiod, Left', '^S^Location=Medication administered to left deltoid^^left deltoid')
			,('IntraMuscMedication', 'Site ~~(non-antibiotic/immunization)', 2, 'Deltiod, Right', '^S^Location=Medication administered to right deltoid^^right deltoid')
			,('IntraMuscMedication', 'Site ~~(non-antibiotic/immunization)', 3, 'Buttock, Left', '^S^Location=Medication administered to left buttock^^left buttock')
			,('IntraMuscMedication', 'Site ~~(non-antibiotic/immunization)', 4, 'Buttock, Right', '^S^Location=Medication administered to right buttock^^right buttock')
			,('IntraMuscMedication', 'Site ~~(non-antibiotic/immunization)', 5, 'Hip, Left', '^S^Location=Medication administered to left hip^^left hip')
			,('IntraMuscMedication', 'Site ~~(non-antibiotic/immunization)', 6, 'Hip, Right', '^S^Location=Medication administered to right hip^^right hip')
			,('IntraMuscMedication', 'Site ~~(non-antibiotic/immunization)', 7, 'Thigh, Left', '^S^Location=Medication administered to left thigh^^left thigh')
			,('IntraMuscMedication', 'Site ~~(non-antibiotic/immunization)', 8, 'Thigh, Right', '^S^Location=Medication administered to right thigh^^right thigh')
			,('IntraMuscMedication', 'Site ~~(non-antibiotic/immunization)', 9, 'Other sites', '^S^Location=Other Site')

			,('IntraMuscMedication', 'Site ~~(antibiotic)', 1, 'Deltiod, Left', '^S^Location=Medication administered to left deltoid^^left deltoid')
			,('IntraMuscMedication', 'Site ~~(antibiotic)', 2, 'Deltiod, Right', '^S^Location=Medication administered to right deltoid^^right deltoid')
			,('IntraMuscMedication', 'Site ~~(antibiotic)', 3, 'Buttock, Left', '^S^Location=Medication administered to left buttock^^left buttock')
			,('IntraMuscMedication', 'Site ~~(antibiotic)', 4, 'Buttock, Right', '^S^Location=Medication administered to right buttock^^right buttock')
			,('IntraMuscMedication', 'Site ~~(antibiotic)', 5, 'Hip, Left', '^S^Location=Medication administered to left hip^^left hip')
			,('IntraMuscMedication', 'Site ~~(antibiotic)', 6, 'Hip, Right', '^S^Locationv=Medication administered to right hip^^right hip')
			,('IntraMuscMedication', 'Site ~~(antibiotic)', 7, 'Thigh, Left', '^S^Location=Medication administered to left thigh^^left thigh')
			,('IntraMuscMedication', 'Site ~~(antibiotic)', 8, 'Thigh, Right', '^S^Location=Medication administered to right thigh^^right thigh')
			,('IntraMuscMedication', 'Site ~~(antibiotic)', 9, 'Other sites', '^S^Location=Other Site')

			,('IntraMuscMedication', 'Site ~~(immunization)', 1, 'Deltiod, Left', '^S^Location=Medication administered to left deltoid^^left deltoid')
			,('IntraMuscMedication', 'Site ~~(immunization)', 2, 'Deltiod, Right', '^S^Location=Medication administered to right deltoid^^right deltoid')
			,('IntraMuscMedication', 'Site ~~(immunization)', 3, 'Buttock, Left', '^S^Location=Medication administered to left buttock^^left buttock')
			,('IntraMuscMedication', 'Site ~~(immunization)', 4, 'Buttock, Right', '^S^Location=Medication administered to right buttock^^right buttock')
			,('IntraMuscMedication', 'Site ~~(immunization)', 5, 'Hip, Left', '^S^Location=Medication administered to left hip^^left hip')
			,('IntraMuscMedication', 'Site ~~(immunization)', 6, 'Hip, Right', '^S^Location=Medication administered to right hip^^right hip')
			,('IntraMuscMedication', 'Site ~~(immunization)', 7, 'Thigh, Left', '^S^Location=Medication administered to left thigh^^left thigh')
			,('IntraMuscMedication', 'Site ~~(immunization)', 8, 'Thigh, Right', '^S^Location=Medication administered to right thigh^^right thigh')
			,('IntraMuscMedication', 'Site ~~(immunization)', 9, 'Other sites', '^S^Location=Other Site')

			,('IntraDermMedication', 'Site', 1, 'Chest, Left', '^S^Location=Medication administered to left chest^QX1676^left chest')
			,('IntraDermMedication', 'Site', 2, 'Chest, Right', '^S^Location=Medication administered to right chest^QX1676^right chest')
			,('IntraDermMedication', 'Site', 3, 'Forearm, Left', '^S^Location=Medication administered to left forearm^QX1676^left forearm')
			,('IntraDermMedication', 'Site', 4, 'Forearm, Right', '^S^Location=Medication administered to right forearm^QX1676^right forearm')
			,('IntraDermMedication', 'Site', 5, 'Back, Left upper', '^S^Location=Medication administered to left upper back^QX1676^left upper back')
			,('IntraDermMedication', 'Site', 6, 'Back, Right upper', '^S^Location=Medication administered to right upper back^QX1676^rigth upper back')
			,('IntraDermMedication', 'Site', 7, 'Abdomen, Left', '^S^Location=Medication administered to left abdomen^QX1676^left abdomen')
			,('IntraDermMedication', 'Site', 8, 'Abdomen, Right', '^S^Location=Medication administered to right abdomen^QX1676^right abdomen')
			,('IntraDermMedication', 'Site', 9, 'Other sites', '^S^Location=Other Site')

			,('IntraOssMedication', 'Site', 1, 'Tibia, Left proximal', '^S^Location=Medication administered to left proximal tibia^QX4747^left proximal tibia')
			,('IntraOssMedication', 'Site', 2, 'Tibia, Right proximal', '^S^Location=Medication administered to right proximal tibia^QX4747^right proximal tibia')
			,('IntraOssMedication', 'Site', 3, 'Tibia, Left distal', '^S^Location=Medication administered to left distal tibia^QX4747^left distal tibia')
			,('IntraOssMedication', 'Site', 4, 'Tibia, Right distal', '^S^Location=Medication administered to right distal tibia^QX4747^right distal tibia')
			,('IntraOssMedication', 'Site', 5, 'Femur, Left', '^S^Location=Medication administered to left femur^QX4747^left femur')
			,('IntraOssMedication', 'Site', 6, 'Femur, Right', '^S^Location=Medication administered to right femur^QX4747^right femur')
			,('IntraOssMedication', 'Site', 7, 'Sternal', '^S^Location=Medication administered to sternum^QX4747^sternum')
			,('IntraOssMedication', 'Site', 8, 'Other sites', '^S^Location=Other Site')

			,('Assessment', 'O2 Sat', 1, '100%', '^SPre-administration assessment shows ^O2 SAT:=O2 saturation reading 100%')
			,('Assessment', 'O2 Sat', 2, '99%', '^SPre-administration assessment shows ^O2 SAT:=O2 saturation reading 99%')
			,('Assessment', 'O2 Sat', 3, '98%', '^SPre-administration assessment shows ^O2 SAT:=O2 saturation reading 98%')
			,('Assessment', 'O2 Sat', 4, '97%', '^SPre-administration assessment shows ^O2 SAT:=O2 saturation reading 97%')
			,('Assessment', 'O2 Sat', 5, '96%', '^SPre-administration assessment shows ^O2 SAT:=O2 saturation reading 96%')
			,('Assessment', 'O2 Sat', 6, '95%', '^SPre-administration assessment shows ^O2 SAT:=O2 saturation reading 95%')
			,('Assessment', 'O2 Sat', 7, '94%', '^SPre-administration assessment shows ^O2 SAT:=O2 saturation reading 94%')
			,('Assessment', 'O2 Sat', 8, '93%', '^SPre-administration assessment shows ^O2 SAT:=O2 saturation reading 93%')
			,('Assessment', 'O2 Sat', 9, '92%', '^SPre-administration assessment shows ^O2 SAT:=O2 saturation reading 92%')
			,('Assessment', 'O2 Sat', 10, '91%', '^SPre-administration assessment shows ^O2 SAT:=O2 saturation reading 91%')
			,('Assessment', 'O2 Sat', 11, '90%', '^SPre-administration assessment shows ^O2 SAT:=O2 saturation reading 90%')
			,('Assessment', 'O2 Sat', 12, '89%', '^SPre-administration assessment shows ^O2 SAT:=O2 saturation reading 89%')
			,('Assessment', 'O2 Sat', 13, '88%', '^SPre-administration assessment shows ^O2 SAT:=O2 saturation reading 88%')
			,('Assessment', 'O2 Sat', 14, '87%', '^SPre-administration assessment shows ^O2 SAT:=O2 saturation reading 87%')
			,('Assessment', 'O2 Sat', 15, '86%', '^SPre-administration assessment shows ^O2 SAT:=O2 saturation reading 86%')
			,('Assessment', 'O2 Sat', 16, '85%', '^SPre-administration assessment shows ^O2 SAT:=O2 saturation reading 85%')
			,('Assessment', 'O2 Sat', 17, '84%', '^SPre-administration assessment shows ^O2 SAT:=O2 saturation reading 84%')
			,('Assessment', 'O2 Sat', 18, '83%', '^SPre-administration assessment shows ^O2 SAT:=O2 saturation reading 83%')
			,('Assessment', 'O2 Sat', 19, '82%', '^SPre-administration assessment shows ^O2 SAT:=O2 saturation reading 82%')
			,('Assessment', 'O2 Sat', 20, '81%', '^SPre-administration assessment shows ^O2 SAT:=O2 saturation reading 81%')
			,('Assessment', 'O2 Sat', 21, '80%', '^SPre-administration assessment shows ^O2 SAT:=O2 saturation reading 80%')
			,('Assessment', 'O2 Sat', 22, '<80%', '^SPre-administration assessment shows ^O2 SAT:=O2 saturation reading less than 80%')

			,('Assessment', 'O2 Amount', 1, 'R.A', '^SPre-administration assessment shows O2 AMT:=R.A.')
			,('Assessment', 'O2 Amount', 2, '0.5L', '^SPre-administration assessment shows O2 AMT:=0.5L')
			,('Assessment', 'O2 Amount', 3, '1L', '^SPre-administration assessment shows O2 AMT:=1L')
			,('Assessment', 'O2 Amount', 4, '2L', '^SPre-administration assessment shows O2 AMT:=2L')
			,('Assessment', 'O2 Amount', 5, '3L', '^SPre-administration assessment shows O2 AMT:=3L')
			,('Assessment', 'O2 Amount', 6, '4L', '^SPre-administration assessment shows O2 AMT:=4L')
			,('Assessment', 'O2 Amount', 7, '5L', '^SPre-administration assessment shows O2 AMT:=5L')
			,('Assessment', 'O2 Amount', 8, '6L', '^SPre-administration assessment shows O2 AMT:=6L')
			,('Assessment', 'O2 Amount', 9, '40%', '^SPre-administration assessment shows O2 AMT:=40%')
			,('Assessment', 'O2 Amount', 10, '50%', '^SPre-administration assessment shows O2 AMT:=50%')
			,('Assessment', 'O2 Amount', 11, '60%', '^SPre-administration assessment shows O2 AMT:=60%')
	 		,('Assessment', 'O2 Amount', 12, '80%', '^SPre-administration assessment shows O2 AMT:=80%')
			,('Assessment', 'O2 Amount', 13, '100%', '^SPre-administration assessment shows O2 AMT:=100%')

			,('Assessment', 'O2 Type', 1, 'Room air', '^SPre-administration assessment shows^RA=on room air')
			,('Assessment', 'O2 Type', 2, 'On oxygen', '^SPre-administration assessment shows^RA=On oxygen')
			
			,('InhalationMedication', 'Administered via', 1, 'Single dose nebulizer', '^SSite:=Medication administered via Hand-held nebulizer^QX1198')
			,('InhalationMedication', 'Administered via', 2, 'Continuous nebulizer', '^SSite:=Medication administered via continuous nebulizer^QX1200')
			,('InhalationMedication', 'Administered via', 3, 'MDI', '^SSite:=Medication administered via MDI^QX3735')
			,('InhalationMedication', 'Administered via', 4, 'MDI with spacer', '^SSite:=Medication administered via MDI with spacer^QX4748')

			,('InhalationMedication', 'Medication combined for administration with', 0, 'Albuterol', '^D=Albuterol')
			,('InhalationMedication', 'Medication combined for administration with', 0, 'Albuterol Dose', '^CDose:^albuterol')
			,('InhalationMedication', 'Medication combined for administration with', 0, 'Atrovent', '^D=Atrovent')
			,('InhalationMedication', 'Medication combined for administration with', 0, 'Atrovent Dose', '^CDose:^Atrovent')
			,('InhalationMedication', 'Medication combined for administration with', 0, 'Xopenex', '^D=Xopenex')
			,('InhalationMedication', 'Medication combined for administration with', 0, 'Xopenex Dose', '^CDose:^Xopenex')
			,('InhalationMedication', 'Medication combined for administration with', 0, 'Combivent', '^D=Combivent')
			,('InhalationMedication', 'Medication combined for administration with', 0, 'Combivent Dose', '^CDose:^Combivent')
			,('InhalationMedication', 'Medication combined for administration with', 0, 'Racemic Epinephrine', '^D=Racemic Epinephrine')
			,('InhalationMedication', 'Medication combined for administration with', 0, 'Racemic Epinephrine Dose', '^CDose:^Racemic')

			,('InhalationAssessment', 'O2 Sat', 1, '100%', '^SPre-administration assessment shows ^O2 SAT:=O2 saturation reading 100%^deletethiscode61')
			,('InhalationAssessment', 'O2 Sat', 2, '99%', '^SPre-administration assessment shows ^O2 SAT:=O2 saturation reading 99%^deletethiscode61')
			,('InhalationAssessment', 'O2 Sat', 3, '98%', '^SPre-administration assessment shows ^O2 SAT:=O2 saturation reading 98%^deletethiscode61')
			,('InhalationAssessment', 'O2 Sat', 4, '97%', '^SPre-administration assessment shows ^O2 SAT:=O2 saturation reading 97%^deletethiscode61')
			,('InhalationAssessment', 'O2 Sat', 5, '96%', '^SPre-administration assessment shows ^O2 SAT:=O2 saturation reading 96%^deletethiscode61')
			,('InhalationAssessment', 'O2 Sat', 6, '95%', '^SPre-administration assessment shows ^O2 SAT:=O2 saturation reading 95%^deletethiscode61')
			,('InhalationAssessment', 'O2 Sat', 7, '94%', '^SPre-administration assessment shows ^O2 SAT:=O2 saturation reading 94%^deletethiscode61')
			,('InhalationAssessment', 'O2 Sat', 8, '93%', '^SPre-administration assessment shows ^O2 SAT:=O2 saturation reading 93%^deletethiscode61')
			,('InhalationAssessment', 'O2 Sat', 9, '92%', '^SPre-administration assessment shows ^O2 SAT:=O2 saturation reading 92%^deletethiscode61')
			,('InhalationAssessment', 'O2 Sat', 10, '91%', '^SPre-administration assessment shows ^O2 SAT:=O2 saturation reading 91%^deletethiscode61')
			,('InhalationAssessment', 'O2 Sat', 11, '90%', '^SPre-administration assessment shows ^O2 SAT:=O2 saturation reading 90%^deletethiscode61')
			,('InhalationAssessment', 'O2 Sat', 12, '89%', '^SPre-administration assessment shows ^O2 SAT:=O2 saturation reading 89%^deletethiscode61')
			,('InhalationAssessment', 'O2 Sat', 13, '88%', '^SPre-administration assessment shows ^O2 SAT:=O2 saturation reading 88%^deletethiscode61')
			,('InhalationAssessment', 'O2 Sat', 14, '87%', '^SPre-administration assessment shows ^O2 SAT:=O2 saturation reading 87%^deletethiscode61')
			,('InhalationAssessment', 'O2 Sat', 15, '86%', '^SPre-administration assessment shows ^O2 SAT:=O2 saturation reading 86%^deletethiscode61')
			,('InhalationAssessment', 'O2 Sat', 16, '85%', '^SPre-administration assessment shows ^O2 SAT:=O2 saturation reading 85%^deletethiscode61')
			,('InhalationAssessment', 'O2 Sat', 17, '84%', '^SPre-administration assessment shows ^O2 SAT:=O2 saturation reading 84%^deletethiscode61')
			,('InhalationAssessment', 'O2 Sat', 18, '83%', '^SPre-administration assessment shows ^O2 SAT:=O2 saturation reading 83%^deletethiscode61')
			,('InhalationAssessment', 'O2 Sat', 19, '82%', '^SPre-administration assessment shows ^O2 SAT:=O2 saturation reading 82%^deletethiscode61')
			,('InhalationAssessment', 'O2 Sat', 20, '81%', '^SPre-administration assessment shows ^O2 SAT:=O2 saturation reading 81%^deletethiscode61')
			,('InhalationAssessment', 'O2 Sat', 21, '80%', '^SPre-administration assessment shows ^O2 SAT:=O2 saturation reading 80%^deletethiscode61')
			,('InhalationAssessment', 'O2 Sat', 22, '<80%', '^SPre-administration assessment shows ^O2 SAT:=O2 saturation reading less than 80%^deletethiscode61')

			,('InhalationAssessment', 'O2 Amount', 1, 'R.A', '^SPre-administration assessment shows O2 AMT:=R.A.^deletethiscode1602')
			,('InhalationAssessment', 'O2 Amount', 2, '0.5L', '^SPre-administration assessment shows O2 AMT:=0.5L^deletethiscode1347')
			,('InhalationAssessment', 'O2 Amount', 3, '1L', '^SPre-administration assessment shows O2 AMT:=1L^deletethiscode1347')
			,('InhalationAssessment', 'O2 Amount', 4, '2L', '^SPre-administration assessment shows O2 AMT:=2L^deletethiscode1347')
			,('InhalationAssessment', 'O2 Amount', 5, '3L', '^SPre-administration assessment shows O2 AMT:=3L^deletethiscode1347')
			,('InhalationAssessment', 'O2 Amount', 6, '4L', '^SPre-administration assessment shows O2 AMT:=4L^deletethiscode1347')
			,('InhalationAssessment', 'O2 Amount', 7, '5L', '^SPre-administration assessment shows O2 AMT:=5L^deletethiscode1347')
			,('InhalationAssessment', 'O2 Amount', 8, '6L', '^SPre-administration assessment shows O2 AMT:=6L^deletethiscode1347')
			,('InhalationAssessment', 'O2 Amount', 9, '40%', '^SPre-administration assessment shows O2 AMT:=40%^deletethiscode1601')
			,('InhalationAssessment', 'O2 Amount', 10, '50%', '^SPre-administration assessment shows O2 AMT:=50%^deletethiscode1601')
			,('InhalationAssessment', 'O2 Amount', 11, '60%', '^SPre-administration assessment shows O2 AMT:=60%^deletethiscode1601')
	 		,('InhalationAssessment', 'O2 Amount', 12, '80%', '^SPre-administration assessment shows O2 AMT:=80%^deletethiscode1601')
			,('InhalationAssessment', 'O2 Amount', 13, '100%', '^SPre-administration assessment shows O2 AMT:=100%^deletethiscode1601')

			,('InhalationAssessment', 'O2 Type', 1, 'Room air', '^SPre-administration assessment shows ^RA=on room air')
			,('InhalationAssessment', 'O2 Type', 2, 'On oxygen', '^SPre-administration assessment shows ^RA=On oxygen')
			
			,('InhalationAssessment', 'Rhythm', 1, 'Normal Sinus', '^SPre-administration assessment shows Patient on cardiac monitor showing=normal sinus rhythm^^U')
			,('InhalationAssessment', 'Rhythm', 2, 'Atrial Fibrillation', '^SPre-administration assessment shows Patient on cardiac monitor showing=atrial fibrillation^^U')
			,('InhalationAssessment', 'Rhythm', 3, 'Artial Flutter', '^SPre-administration assessment shows Patient on cardiac monitor showing=atrial flutter^^U')
			,('InhalationAssessment', 'Rhythm', 4, 'Artial Tachycardia', '^SPre-administration assessment shows Patient on cardiac monitor showing=atrial tachycardia^^U')
			,('InhalationAssessment', 'Rhythm', 5, 'Paced', '^SPre-administration assessment shows Patient on cardiac monitor showing=paced rhythm^^U')
			,('InhalationAssessment', 'Rhythm', 6, 'PSVT', '^SPre-administration assessment shows Patient on cardiac monitor showing=PSVT^^U')
			,('InhalationAssessment', 'Rhythm', 7, 'Sinus Bradycardia', '^SPre-administration assessment shows Patient on cardiac monitor showing=sinus bradycardia^^U')
			,('InhalationAssessment', 'Rhythm', 8, 'Sinus Tachycardia', '^SPre-administration assessment shows Patient on cardiac monitor showing=sinus tachycardia^^U')
			,('InhalationAssessment', 'Rhythm', 9, '1 degree AV Block', '^SPre-administration assessment shows Patient on cardiac monitor showing=1 degree AV block^^U')
			,('InhalationAssessment', 'Rhythm', 10, '2 degree AV Block Type I', '^SPre-administration assessment shows Patient on cardiac monitor showing=2 degree AV block Type I^^U')
			,('InhalationAssessment', 'Rhythm', 11, '2 degree AV Block Type II', '^SPre-administration assessment shows Patient on cardiac monitor showing=2 degree AV block Type II^^U')
			,('InhalationAssessment', 'Rhythm', 12, '3 degree AV Block', '^SPre-administration assessment shows Patient on cardiac monitor showing=3 degree AV block^^U')
			,('InhalationAssessment', 'Rhythm', 13, 'Junctional', '^SPre-administration assessment shows Patient on cardiac monitor showing=junctional rhythm^^U')
			,('InhalationAssessment', 'Rhythm', 14, 'Verticular Tachycardia', '^SPre-administration assessment shows Patient on cardiac monitor showing=ventricular tachycardia^^U')
			,('InhalationAssessment', 'Rhythm', 15, 'Verticular Fibrillation', '^SPre-administration assessment shows Patient on cardiac monitor showing=ventricular fibrillation^^U')
			,('InhalationAssessment', 'Rhythm', 16, 'PEA', '^SPre-administration assessment shows Patient on cardiac monitor showing=PEA^^U')
			,('InhalationAssessment', 'Rhythm', 17, 'Asystole', '^SPre-administration assessment shows Patient on cardiac monitor showing=asystole^^U')
			,('InhalationAssessment', 'Rhythm', 18, 'Agonal', '^SPre-administration assessment shows Patient on cardiac monitor showing=agonal^^U')

			,('InhalationAssessment', 'Ectopy', 1, 'UNI PVCs', '^S=with unifocal premature ventricular contractions^^U')
			,('InhalationAssessment', 'Ectopy', 2, 'Multi PVCs', '^S=with multifocal premature ventricular contractions^^U')
			,('InhalationAssessment', 'Ectopy', 3, 'Couplets', '^S=with couplets^^U')
			,('InhalationAssessment', 'Ectopy', 4, 'Frequent PVCs', '^S=with frequent premature ventricular contractions^^U')
			,('InhalationAssessment', 'Ectopy', 5, 'Infrequent PVCs', '^S=with infrequent premature ventricular contractions^^U')
			,('InhalationAssessment', 'Ectopy', 6, 'PJCs', '^S=with premature junctional contractions^^U')
			,('InhalationAssessment', 'Ectopy', 7, 'PACs', '^S=with premature atrial contractions^^U')
			,('InhalationAssessment', 'Ectopy', 8, 'Bigeminy', '^S=in bigeminy^^U')
			,('InhalationAssessment', 'Ectopy', 9, 'Trigeminy', '^S=in trigeminy^^U')
			,('InhalationAssessment', 'Ectopy', 10, 'Aberrant', '^S=with aberrant conduction^^U')

			,('InhalationAssessment', 'St Changes', 1, 'None', '^S=No ST changes noted^^U')
			,('InhalationAssessment', 'St Changes', 2, 'Elevation', '^S=ST elevation noted^^U')
			,('InhalationAssessment', 'St Changes', 3, 'Depression', '^S=ST depression noted^^U')
	
			,('IntraOssAssessment', 'O2 Sat', 1, '100%', '^SPre-administration assessment shows ^O2 SAT:=O2 saturation reading 100%^deletethiscode61')
			,('IntraOssAssessment', 'O2 Sat', 2, '99%', '^SPre-administration assessment shows ^O2 SAT:=O2 saturation reading 99%^deletethiscode61')
			,('IntraOssAssessment', 'O2 Sat', 3, '98%', '^SPre-administration assessment shows ^O2 SAT:=O2 saturation reading 98%^deletethiscode61')
			,('IntraOssAssessment', 'O2 Sat', 4, '97%', '^SPre-administration assessment shows ^O2 SAT:=O2 saturation reading 97%^deletethiscode61')
			,('IntraOssAssessment', 'O2 Sat', 5, '96%', '^SPre-administration assessment shows ^O2 SAT:=O2 saturation reading 96%^deletethiscode61')
			,('IntraOssAssessment', 'O2 Sat', 6, '95%', '^SPre-administration assessment shows ^O2 SAT:=O2 saturation reading 95%^deletethiscode61')
			,('IntraOssAssessment', 'O2 Sat', 7, '94%', '^SPre-administration assessment shows ^O2 SAT:=O2 saturation reading 94%^deletethiscode61')
			,('IntraOssAssessment', 'O2 Sat', 8, '93%', '^SPre-administration assessment shows ^O2 SAT:=O2 saturation reading 93%^deletethiscode61')
			,('IntraOssAssessment', 'O2 Sat', 9, '92%', '^SPre-administration assessment shows ^O2 SAT:=O2 saturation reading 92%^deletethiscode61')
			,('IntraOssAssessment', 'O2 Sat', 10, '91%', '^SPre-administration assessment shows ^O2 SAT:=O2 saturation reading 91%^deletethiscode61')
			,('IntraOssAssessment', 'O2 Sat', 11, '90%', '^SPre-administration assessment shows ^O2 SAT:=O2 saturation reading 90%^deletethiscode61')
			,('IntraOssAssessment', 'O2 Sat', 12, '89%', '^SPre-administration assessment shows ^O2 SAT:=O2 saturation reading 89%^deletethiscode61')
			,('IntraOssAssessment', 'O2 Sat', 13, '88%', '^SPre-administration assessment shows ^O2 SAT:=O2 saturation reading 88%^deletethiscode61')
			,('IntraOssAssessment', 'O2 Sat', 14, '87%', '^SPre-administration assessment shows ^O2 SAT:=O2 saturation reading 87%^deletethiscode61')
			,('IntraOssAssessment', 'O2 Sat', 15, '86%', '^SPre-administration assessment shows ^O2 SAT:=O2 saturation reading 86%^deletethiscode61')
			,('IntraOssAssessment', 'O2 Sat', 16, '85%', '^SPre-administration assessment shows ^O2 SAT:=O2 saturation reading 85%^deletethiscode61')
			,('IntraOssAssessment', 'O2 Sat', 17, '84%', '^SPre-administration assessment shows ^O2 SAT:=O2 saturation reading 84%^deletethiscode61')
			,('IntraOssAssessment', 'O2 Sat', 18, '83%', '^SPre-administration assessment shows ^O2 SAT:=O2 saturation reading 83%^deletethiscode61')
			,('IntraOssAssessment', 'O2 Sat', 19, '82%', '^SPre-administration assessment shows ^O2 SAT:=O2 saturation reading 82%^deletethiscode61')
			,('IntraOssAssessment', 'O2 Sat', 20, '81%', '^SPre-administration assessment shows ^O2 SAT:=O2 saturation reading 81%^deletethiscode61')
			,('IntraOssAssessment', 'O2 Sat', 21, '80%', '^SPre-administration assessment shows ^O2 SAT:=O2 saturation reading 80%^deletethiscode61')
			,('IntraOssAssessment', 'O2 Sat', 22, '<80%', '^SPre-administration assessment shows ^O2 SAT:=O2 saturation reading less than 80%^deletethiscode61')

			,('IntraOssAssessment', 'O2 Amount', 1, 'R.A', '^SPre-administration assessment shows O2 AMT:=R.A.^deletethiscode1602')
			,('IntraOssAssessment', 'O2 Amount', 2, '0.5L', '^SPre-administration assessment shows O2 AMT:=0.5L^deletethiscode1347')
			,('IntraOssAssessment', 'O2 Amount', 3, '1L', '^SPre-administration assessment shows O2 AMT:=1L^deletethiscode1347')
			,('IntraOssAssessment', 'O2 Amount', 4, '2L', '^SPre-administration assessment shows O2 AMT:=2L^deletethiscode1347')
			,('IntraOssAssessment', 'O2 Amount', 5, '3L', '^SPre-administration assessment shows O2 AMT:=3L^deletethiscode1347')
			,('IntraOssAssessment', 'O2 Amount', 6, '4L', '^SPre-administration assessment shows O2 AMT:=4L^deletethiscode1347')
			,('IntraOssAssessment', 'O2 Amount', 7, '5L', '^SPre-administration assessment shows O2 AMT:=5L^deletethiscode1347')
			,('IntraOssAssessment', 'O2 Amount', 8, '6L', '^SPre-administration assessment shows O2 AMT:=6L^deletethiscode1347')
			,('IntraOssAssessment', 'O2 Amount', 9, '40%', '^SPre-administration assessment shows O2 AMT:=40%^deletethiscode1601')
			,('IntraOssAssessment', 'O2 Amount', 10, '50%', '^SPre-administration assessment shows O2 AMT:=50%^deletethiscode1601')
			,('IntraOssAssessment', 'O2 Amount', 11, '60%', '^SPre-administration assessment shows O2 AMT:=60%^deletethiscode1601')
	 		,('IntraOssAssessment', 'O2 Amount', 12, '80%', '^SPre-administration assessment shows O2 AMT:=80%^deletethiscode1601')
			,('IntraOssAssessment', 'O2 Amount', 13, '100%', '^SPre-administration assessment shows O2 AMT:=100%^deletethiscode1601')

			,('IntraOssAssessment', 'O2 Type', 1, 'Room air', '^SPre-administration assessment shows ^RA=on room air')
			,('IntraOssAssessment', 'O2 Type', 2, 'On oxygen', '^SPre-administration assessment shows ^RA=On oxygen')
			
			,('IntraOssAssessment', 'Rhythm', 1, 'Normal Sinus', '^SPre-administration assessment shows Patient on cardiac monitor showing=normal sinus rhythm^^U')
			,('IntraOssAssessment', 'Rhythm', 2, 'Atrial Fibrillation', '^SPre-administration assessment shows Patient on cardiac monitor showing=atrial fibrillation^^U')
			,('IntraOssAssessment', 'Rhythm', 3, 'Artial Flutter', '^SPre-administration assessment shows Patient on cardiac monitor showing=atrial flutter^^U')
			,('IntraOssAssessment', 'Rhythm', 4, 'Artial Tachycardia', '^SPre-administration assessment shows Patient on cardiac monitor showing=atrial tachycardia^^U')
			,('IntraOssAssessment', 'Rhythm', 5, 'Paced', '^SPre-administration assessment shows Patient on cardiac monitor showing=paced rhythm^^U')
			,('IntraOssAssessment', 'Rhythm', 6, 'PSVT', '^SPre-administration assessment shows Patient on cardiac monitor showing=PSVT^^U')
			,('IntraOssAssessment', 'Rhythm', 7, 'Sinus Bradycardia', '^SPre-administration assessment shows Patient on cardiac monitor showing=sinus bradycardia^^U')
			,('IntraOssAssessment', 'Rhythm', 8, 'Sinus Tachycardia', '^SPre-administration assessment shows Patient on cardiac monitor showing=sinus tachycardia^^U')
			,('IntraOssAssessment', 'Rhythm', 9, '1 degree AV Block', '^SPre-administration assessment shows Patient on cardiac monitor showing=1 degree AV block^^U')
			,('IntraOssAssessment', 'Rhythm', 10, '2 degree AV Block Type I', '^SPre-administration assessment shows Patient on cardiac monitor showing=2 degree AV block Type I^^U')
			,('IntraOssAssessment', 'Rhythm', 11, '2 degree AV Block Type II', '^SPre-administration assessment shows Patient on cardiac monitor showing=2 degree AV block Type II^^U')
			,('IntraOssAssessment', 'Rhythm', 12, '3 degree AV Block', '^SPre-administration assessment shows Patient on cardiac monitor showing=3 degree AV block^^U')
			,('IntraOssAssessment', 'Rhythm', 13, 'Junctional', '^SPre-administration assessment shows Patient on cardiac monitor showing=junctional rhythm^^U')
			,('IntraOssAssessment', 'Rhythm', 14, 'Verticular Tachycardia', '^SPre-administration assessment shows Patient on cardiac monitor showing=ventricular tachycardia^^U')
			,('IntraOssAssessment', 'Rhythm', 15, 'Verticular Fibrillation', '^SPre-administration assessment shows Patient on cardiac monitor showing=ventricular fibrillation^^U')
			,('IntraOssAssessment', 'Rhythm', 16, 'PEA', '^SPre-administration assessment shows Patient on cardiac monitor showing=PEA^^U')
			,('IntraOssAssessment', 'Rhythm', 17, 'Asystole', '^SPre-administration assessment shows Patient on cardiac monitor showing=asystole^^U')
			,('IntraOssAssessment', 'Rhythm', 18, 'Agonal', '^SPre-administration assessment shows Patient on cardiac monitor showing=agonal^^U')

			,('IntraOssAssessment', 'Ectopy', 1, 'UNI PVCs', '^S=with unifocal premature ventricular contractions^^U')
			,('IntraOssAssessment', 'Ectopy', 2, 'Multi PVCs', '^S=with multifocal premature ventricular contractions^^U')
			,('IntraOssAssessment', 'Ectopy', 3, 'Couplets', '^S=with couplets^^U')
			,('IntraOssAssessment', 'Ectopy', 4, 'Frequent PVCs', '^S=with frequent premature ventricular contractions^^U')
			,('IntraOssAssessment', 'Ectopy', 5, 'Infrequent PVCs', '^S=with infrequent premature ventricular contractions^^U')
			,('IntraOssAssessment', 'Ectopy', 6, 'PJCs', '^S=with premature junctional contractions^^U')
			,('IntraOssAssessment', 'Ectopy', 7, 'PACs', '^S=with premature atrial contractions^^U')
			,('IntraOssAssessment', 'Ectopy', 8, 'Bigeminy', '^S=in bigeminy^^U')
			,('IntraOssAssessment', 'Ectopy', 9, 'Trigeminy', '^S=in trigeminy^^U')
			,('IntraOssAssessment', 'Ectopy', 10, 'Aberrant', '^S=with aberrant conduction^^U')

			,('IntraOssAssessment', 'St Changes', 1, 'None', '^S=No ST changes noted^^U')
			,('IntraOssAssessment', 'St Changes', 2, 'Elevation', '^S=ST elevation noted^^U')
			,('IntraOssAssessment', 'St Changes', 3, 'Depression', '^S=ST depression noted^^U')
			
			,('Medication', 'All of the above', 0, 'Correct patient, time, route, dose and medication confirmed prior to administration', '^D=Correct patient, time, route, dose and medication confirmed prior to administration')
			,('Medication', 'All of the above', 0, 'Patient advised of actions and side-effects prior to administration', '^D=Patient advised of actions and side-effects prior to administration')
			,('Medication', 'All of the above', 0, 'Allergies confirmed and medications reviewed prior to administration', '^D=Allergies confirmed and medications reviewed prior to administration')

			,('EyeMedication', 'All of the above', 0, 'Correct patient, time, route, dose and medication confirmed prior to administration', '^D=Correct patient, time, route, dose and medication confirmed prior to administration')
			,('EyeMedication', 'All of the above', 0, 'Patient advised of actions and side-effects prior to administration', '^D=Patient advised of actions and side-effects prior to administration')
			,('EyeMedication', 'All of the above', 0, 'Allergies confirmed and medications reviewed prior to administration', '^D=Allergies confirmed and medications reviewed prior to administration')

			,('EnteralMedication', 'All of the above', 0, 'Tube position confirmed via aspiration prior to administration', '^D=Tube position confirmed via aspiration prior to administration')
			,('EnteralMedication', 'All of the above', 0, 'Correct patient, time, route, dose and medication confirmed prior to administration', '^D=Correct patient, time, route, dose and medication confirmed prior to administration')
			,('EnteralMedication', 'All of the above', 0, 'Patient advised of actions and side-effects prior to administration', '^D=Patient advised of actions and side-effects prior to administration')
			,('EnteralMedication', 'All of the above', 0, 'Allergies confirmed and medications reviewed prior to administration', '^D=Allergies confirmed and medications reviewed prior to administration')
			,('EnteralMedication', 'All of the above', 0, 'Flushed with water after administration', '^D=Flushed with water after administration')

			,('Safety', 'All of the above', 0, 'Patient in position of comfort', '^D=Patient in position of comfort')
			,('Safety', 'All of the above', 0, 'Side rails up', '^D=Side rails up')
			,('Safety', 'All of the above', 0, 'Cart in lowest position', '^D=Cart in lowest position')
			,('Safety', 'All of the above', 0, 'Family at bedside', '^D=Family at bedside')

			,('AmbulateSafety', 'All of the above', 0, 'Advised not to ambulate without assistance', '^D=Advised not to ambulate without assistance')
			,('AmbulateSafety', 'All of the above', 0, 'Patient in position of comfort', '^D=Patient in position of comfort')
			,('AmbulateSafety', 'All of the above', 0, 'Side rails up', '^D=Side rails up')
			,('AmbulateSafety', 'All of the above', 0, 'Cart in lowest position', '^D=Cart in lowest position')
			,('AmbulateSafety', 'All of the above', 0, 'Family at bedside', '^D=Family at bedside')

			,('IntraMuscMedication', 'IM (Not an antibiotic or immunization)', 0, 'Site ~~(non-antibiotic/immunization)', '')
			,('IntraMuscMedication', 'IM (Not an antibiotic or immunization)', 0, 'Other site ~~(non-antibiotic/immunization)', '^CMedication administered to ^Location=')
			,('IntraMuscMedication', 'IM (Not an antibiotic or immunization)', 0, 'Amount given ~~(non-antibiotic/immunization)', '^CAmount given:^^^^medsvc_amtgiven=')
			,('IntraMuscMedication', 'IM (Not an antibiotic or immunization)', 0, 'Combined with ~~(non-antibiotic/immunization)', '^CMedication combined for administration with=')

			,('IntraMuscMedication', 'IM antibiotic', 0, 'Site ~~(antibiotic)', '')
			,('IntraMuscMedication', 'IM antibiotic', 0, 'Other site ~~(antibiotic)', '^CMedication administered to ^Location2')
			,('IntraMuscMedication', 'IM antibiotic', 0, 'Amount given ~~(antibiotic)', '^CAmount given:=')
			,('IntraMuscMedication', 'IM antibiotic', 0, 'Combined with ~~(antibiotic)', '^CMedication combined for administration with=')

			,('IntraMuscMedication', 'IM immunization', 0, 'Site ~~(immunization)', '')
			,('IntraMuscMedication', 'IM immunization', 0, 'Other site ~~(immunization)', '^CMedication administered to ^Location^^^medsvc_vac_site')
			,('IntraMuscMedication', 'IM immunization', 0, 'Amount given ~~(immunization)', '^CAmount given:^^^^medsvc_vac_amt=')
			,('IntraMuscMedication', 'IM immunization', 0, 'unit', '^C^^^^medsvc_vac_unit=')
			,('IntraMuscMedication', 'IM immunization', 0, 'Combined with ~~(immunization)', '^CMedication combined for administration with=')
			,('IntraMuscMedication', 'IM immunization', 0, 'Dose number', '')
			,('IntraMuscMedication', 'IM immunization', 0, 'Other dose number', '^C^^^^medsvc_vac_dosenum=')
			,('IntraMuscMedication', 'IM immunization', 0, 'Previous dose confirmed by', '')
			,('IntraMuscMedication', 'IM immunization', 0, 'Vaccination information sheet given to patient', '^D^Itram^^medsvc_vac_sheetgiven=Vaccination information sheet given to patient')
			,('IntraMuscMedication', 'IM immunization', 0, 'Date of publication', '^Cdate of publication:^im^^^medsvc_vac_pubdate=')
			,('IntraMuscMedication', 'IM immunization', 0, 'Name of publication', '^Cname of publication:^^^^medsvc_vac_pubname=')
			,('IntraMuscMedication', 'IM immunization', 0, 'Manufacturer', '^Cmanufacturer:^^^^medsvc_vac_manuf=')
			,('IntraMuscMedication', 'IM immunization', 0, 'Lot number', '^Clot number:^^^^medsvc_vac_lot=')
			,('IntraMuscMedication', 'IM immunization', 0, 'Expiration', '^Cexpiration:^intram^^^medsvc_vac_exp=')

			,('IntraMuscMedication', 'Dose number', 1, '1 of 1 - Complete', '^S^DoseNumber=Dose 1 of 1.  Immunization Complete^^^medsvc_vac_dosenum')
			,('IntraMuscMedication', 'Dose number', 2, 'Dose 1 of 2', '^S^DoseNumber=Dose 1 of 2^^^medsvc_vac_dosenum')
			,('IntraMuscMedication', 'Dose number', 3, '2 of 2- Complete', '^S^DoseNumber=Dose 2 of 2.  Immunization Complete^^^medsvc_vac_dosenum')
			,('IntraMuscMedication', 'Dose number', 4, 'Dose 1 of 3', '^S^DoseNumber=Dose 1 of 3^^^medsvc_vac_dosenum')
			,('IntraMuscMedication', 'Dose number', 5, 'Dose 2 of 3', '^S^DoseNumber=Dose 2 of 3^^^medsvc_vac_dosenum')
			,('IntraMuscMedication', 'Dose number', 6, '3 of 3- Complete', '^S^DoseNumber=Dose 3 of 3.  Immunization Complete^^^medsvc_vac_dosenum')
			,('IntraMuscMedication', 'Dose number', 7, 'Other dose number', '^S^DoseNumber=Dose Number')

			,('IntraMuscMedication', 'Previous dose confirmed by', 1, 'Vaccine Card', '^S^confirmedprevious=Previous dose confirmed by Vaccine Card^^^medsvc_vac_prevdoseconfirmed')
			,('IntraMuscMedication', 'Previous dose confirmed by', 2, 'Immunization Information System', '^S^confirmedprevious=Previous dose confirmed by Immunization Information System^^^medsvc_vac_prevdoseconfirmed')
			,('IntraMuscMedication', 'Previous dose confirmed by', 3, 'Medical Record', '^S^confirmedprevious=Previous dose confirmed by medical Record^^^medsvc_vac_prevdoseconfirmed')
			,('IntraMuscMedication', 'Previous dose confirmed by', 4, 'Patient', '^S^confirmedprevious=Previous dose confirmed by patient^^^medsvc_vac_prevdoseconfirmed')
			,('IntraMuscMedication', 'Previous dose confirmed by', 5, 'Other', '^S^confirmedprevious=Previous dose confirmed^^^medsvc_vac_prevdoseconfirmed')

			,('Assessment', 'All of the above', 0, 'Correct patient, time, route, dose and medication confirmed prior to administration', '^D=Correct patient, time, route, dose and medication confirmed prior to administration')
			,('Assessment', 'All of the above', 0, 'Patient advised of actions and side-effects prior to administration', '^D=Patient advised of actions and side-effects prior to administration')
			,('Assessment', 'All of the above', 0, 'Allergies confirmed and medications reviewed prior to administration', '^D=Allergies confirmed and medications reviewed prior to administration')

			,('InhalationAssessment', 'All of the above', 0, 'Correct patient, time, route, dose and medication confirmed prior to administration', '^D=Correct patient, time, route, dose and medication confirmed prior to administration')
			,('InhalationAssessment', 'All of the above', 0, 'Patient advised of actions and side-effects prior to administration', '^D=Patient advised of actions and side-effects prior to administration')
			,('InhalationAssessment', 'All of the above', 0, 'Allergies confirmed and medications reviewed prior to administration', '^D=Allergies confirmed and medications reviewed prior to administration')
			
			,('OralMedication', 'All of the above', 0, 'Correct patient, time, route, dose and medication confirmed prior to administration', '^D=Correct patient, time, route, dose and medication confirmed prior to administration')
			,('OralMedication', 'All of the above', 0, 'Patient advised of actions and side-effects prior to administration', '^D=Patient advised of actions and side-effects prior to administration')
			,('OralMedication', 'All of the above', 0, 'Allergies confirmed and medications reviewed prior to administration', '^D=Allergies confirmed and medications reviewed prior to administration')
		
			,('NasalMedication', 'Immunization Details', 0, 'Vaccination information sheet given to patient', '^D^Itram^^medsvc_vac_sheetgiven=Vaccination information sheet given to patient')
			,('NasalMedication', 'Immunization Details', 0, 'Date of publication', '^Cdate of publication:^im^^^medsvc_vac_pubdate')
			,('NasalMedication', 'Immunization Details', 0, 'Name of publication', '^Cname of publication:^^^^medsvc_vac_pubname')
			,('NasalMedication', 'Immunization Details', 0, 'Manufacturer', '^Cmanufacturer:^^^^medsvc_vac_manuf')
			,('NasalMedication', 'Immunization Details', 0, 'Lot number', '^Clot number:^^^^medsvc_vac_lot')
			,('NasalMedication', 'Immunization Details', 0, 'Expiration', '^Cexpiration:^intram^^^medsvc_vac_exp')

			,('NasalMedication', 'All of the above', 0, 'Instructed to blow nose prior to administration', '^D=Instructed to blow nose prior to administration')
			,('NasalMedication', 'All of the above', 0, 'Correct patient, time, route, dose and medication confirmed prior to administration', '^D=Correct patient, time, route, dose and medication confirmed prior to administration')
			,('NasalMedication', 'All of the above', 0, 'Patient advised of actions and side-effects prior to administration', '^D=Patient advised of actions and side-effects prior to administration')
			,('NasalMedication', 'All of the above', 0, 'Allergies confirmed and medications reviewed prior to administration', '^D=Allergies confirmed and medications reviewed prior to administration')

			,('IntraDermMedication', 'All of the above', 0, 'Correct patient, time, route, dose and medication confirmed prior to administration', '^D=Correct patient, time, route, dose and medication confirmed prior to administration')
			,('IntraDermMedication', 'All of the above', 0, 'Patient advised of actions and side-effects prior to administration', '^D=Patient advised of actions and side-effects prior to administration')
			,('IntraDermMedication', 'All of the above', 0, 'Allergies confirmed and medications reviewed prior to administration', '^D=Allergies confirmed and medications reviewed prior to administration')

			,('IntraOssAssessment', 'All of the above', 0, 'Needle placement confirmed via aspiration prior to administration', '^D=Needle placement confirmed via aspiration prior to administration')
			,('IntraOssAssessment', 'All of the above', 0, 'Correct patient, time, route, dose and medication confirmed prior to administration', '^D=Correct patient, time, route, dose and medication confirmed prior to administration')
			,('IntraOssAssessment', 'All of the above', 0, 'Patient advised of actions and side-effects prior to administration', '^D=Patient advised of actions and side-effects prior to administration')
			,('IntraOssAssessment', 'All of the above', 0, 'Allergies confirmed and medications reviewed prior to administration', '^D=Allergies confirmed and medications reviewed prior to administration')

			,('RectalMedication', 'All of the above', 0, 'Correct patient, time, route, dose and medication confirmed prior to administration', '^D=Correct patient, time, route, dose and medication confirmed prior to administration')
			,('RectalMedication', 'All of the above', 0, 'Patient advised of actions and side-effects prior to administration', '^D=Patient advised of actions and side-effects prior to administration')
			,('RectalMedication', 'All of the above', 0, 'Allergies confirmed and medications reviewed prior to administration', '^D=Allergies confirmed and medications reviewed prior to administration')

			,('TransDermMedication', 'All of the above', 0, 'Skin cleansed prior to administration', '^D=Skin cleansed prior to administration')
			,('TransDermMedication', 'All of the above', 0, 'Shaving required prior to administration', '^D=Shaving required prior to administration')
			,('TransDermMedication', 'All of the above', 0, 'Correct patient, time, route, dose and medication confirmed prior to administration', '^D=Correct patient, time, route, dose and medication confirmed prior to administration')
			,('TransDermMedication', 'All of the above', 0, 'Patient advised of actions and side-effects prior to administration', '^D=Patient advised of actions and side-effects prior to administration')
			,('TransDermMedication', 'All of the above', 0, 'Allergies confirmed and medications reviewed prior to administration', '^D=Allergies confirmed and medications reviewed prior to administration')

			,('VaginalMedication', 'All of the above', 0, 'Correct patient, time, route, dose and medication confirmed prior to administration', '^D=Correct patient, time, route, dose and medication confirmed prior to administration')
			,('VaginalMedication', 'All of the above', 0, 'Patient advised of actions and side-effects prior to administration', '^D=Patient advised of actions and side-effects prior to administration')
			,('VaginalMedication', 'All of the above', 0, 'Allergies confirmed and medications reviewed prior to administration', '^D=Allergies confirmed and medications reviewed prior to administration')

			,('SubcutanMedication', 'Medication site', 1, 'Arm, Left upper', '^S^Location=Medication administered to left upper arm^QX216^left upper arm')
			,('SubcutanMedication', 'Medication site', 2, 'Arm, Right upper', '^S^Location=Medication administered to right upper arm^QX216^right upper arm')
			,('SubcutanMedication', 'Medication site', 3, 'Back, Left upper', '^S^Location=Medication administered to left upper back^QX216^left upper back')
			,('SubcutanMedication', 'Medication site', 4, 'Back, Right upper', '^S^Location=Medication administered to right upper back^QX216^right upper back')
			,('SubcutanMedication', 'Medication site', 5, 'Back, Left lower', '^S^Location=Medication administered to left lower back^QX216^left lower back')
			,('SubcutanMedication', 'Medication site', 6, 'Back, Right lower', '^S^Location=Medication administered to right lower back^QX216^right lower back')
			,('SubcutanMedication', 'Medication site', 7, 'Abdomen, Left', '^S^Location=Medication administered to left abdomen^QX216^left abdomen')
			,('SubcutanMedication', 'Medication site', 8, 'Abdomen, Right', '^S^Location=Medication administered to right abdomen^QX216^right abdomen')
			,('SubcutanMedication', 'Medication site', 9, 'Thigh, Left', '^S^Location=Medication administered to left thigh^QX216^left thigh')
			,('SubcutanMedication', 'Medication site', 10, 'Thigh, Right', '^S^Location=Medication administered to right thigh^QX216^right thigh')
			,('SubcutanMedication', 'Medication site', 11, 'Other sites', '^S^Location=Other Site')

			,('SubcutanMedication', 'Immunization site', 1, 'Arm, Left upper', '^S^Location=Immunization administered to left upper arm^^left upper arm^medsvc_vac_site')
			,('SubcutanMedication', 'Immunization site', 2, 'Arm, Right upper', '^S^Location=Immunization administered to right upper arm^^right upper arm^medsvc_vac_site')
			,('SubcutanMedication', 'Immunization site', 3, 'Back, Left upper', '^S^Location=Immunization administered to left upper back^^left upper back^medsvc_vac_site')
			,('SubcutanMedication', 'Immunization site', 4, 'Back, Right upper', '^S^Location=Immunization administered to right upper back^^right upper back^medsvc_vac_site')
			,('SubcutanMedication', 'Immunization site', 5, 'Back, Left lower', '^S^Location=Immunization administered to left lower back^^left lower back^medsvc_vac_site')
			,('SubcutanMedication', 'Immunization site', 6, 'Back, Right lower', '^S^Location=Immunization administered to right lower back^^right lower back^medsvc_vac_site')
			,('SubcutanMedication', 'Immunization site', 7, 'Abdomen, Left', '^S^Location=Immunization administered to left abdomen^^left abdomen^medsvc_vac_site')
			,('SubcutanMedication', 'Immunization site', 8, 'Abdomen, Right', '^S^Location=Immunization administered to right abdomen^^right abdomen^medsvc_vac_site')
			,('SubcutanMedication', 'Immunization site', 9, 'Thigh, Left', '^S^Location=Immunization administered to left thigh^^left thigh^medsvc_vac_site')
			,('SubcutanMedication', 'Immunization site', 10, 'Thigh, Right', '^S^Location=Immunization administered to right thigh^^right thigh^medsvc_vac_site')
			,('SubcutanMedication', 'Immunization site', 11, 'Other sites', '^S^Location=Immunization administered to other site^^^^medsvc_vac_site')

			,('SubcutanMedication', 'Immunization', 0, 'Immunization site', '')
			,('SubcutanMedication', 'Immunization', 0, 'Vaccination information sheet given to patient', '^D^Itram^^medsvc_vac_sheetgiven=Vaccination information sheet given to patient')
			,('SubcutanMedication', 'Immunization', 0, 'Date of publication', '^Cdate of publication:^im^^^medsvc_vac_pubdate=')
			,('SubcutanMedication', 'Immunization', 0, 'Name of publication', '^Cname of publication:^^^^medsvc_vac_pubname=')
			,('SubcutanMedication', 'Immunization', 0, 'Manufacturer', '^Cmanufacturer:^^^^medsvc_vac_manuf=')
			,('SubcutanMedication', 'Immunization', 0, 'Lot number', '^Clot number:^^^^medsvc_vac_lot=')
			,('SubcutanMedication', 'Immunization', 0, 'Expiration', '^Cexpiration:^intram^^^medsvc_vac_exp=')
			,('SubcutanMedication', 'Immunization', 0, 'Dose number', '')
			,('SubcutanMedication', 'Immunization', 0, 'Other Dose number', '^C^^^^medsvc_vac_dosenum=')
			,('SubcutanMedication', 'Immunization', 0, 'Previous dose confirmed by', '')
			,('SubcutanMedication', 'Immunization', 0, 'Provided Emergency Use Authorization (EUA) fact sheet', '^D^Itram^^medsvc_vac_EAUgiven=Provided Emergency Use Authorization (EUA) fact sheet')
			,('SubcutanMedication', 'Immunization', 0, 'Provided a completed vaccination record card', '^D^Itram^^medsvc_vac_Cardgiven=Provided a completed COVID-19 vaccination record card')

			,('SubcutanMedication', 'Dose number', 1, '1 of 1 - Complete', '^S^DoseNumber=Dose 1 of 1.  Immunization Complete^^^medsvc_vac_dosenum')
			,('SubcutanMedication', 'Dose number', 2, 'Dose 1 of 2', '^S^DoseNumber=Dose 1 of 2^^^medsvc_vac_dosenum')
			,('SubcutanMedication', 'Dose number', 3, '2 of 2- Complete', '^S^DoseNumber=Dose 2 of 2.  Immunization Complete^^^medsvc_vac_dosenum')
			,('SubcutanMedication', 'Dose number', 4, 'Dose 1 of 3', '^S^DoseNumber=Dose 1 of 3^^^medsvc_vac_dosenum')
			,('SubcutanMedication', 'Dose number', 5, 'Dose 2 of 3', '^S^DoseNumber=Dose 2 of 3^^^medsvc_vac_dosenum')
			,('SubcutanMedication', 'Dose number', 6, '3 of 3- Complete', '^S^DoseNumber=Dose 3 of 3.  Immunization Complete^^^medsvc_vac_dosenum')
			,('SubcutanMedication', 'Dose number', 7, 'Other dose number', '^S^DoseNumber=Dose Number')

			,('SubcutanMedication', 'Previous dose confirmed by', 1, 'Vaccine Card', '^S^confirmedprevious=Previous dose confirmed by Vaccine Card^^^medsvc_vac_prevdoseconfirmed')
			,('SubcutanMedication', 'Previous dose confirmed by', 2, 'Immunization Information System', '^S^confirmedprevious=Previous dose confirmed by Immunization Information System^^^medsvc_vac_prevdoseconfirmed')
			,('SubcutanMedication', 'Previous dose confirmed by', 3, 'Medical Record', '^S^confirmedprevious=Previous dose confirmed by medical Record^^^medsvc_vac_prevdoseconfirmed')
			,('SubcutanMedication', 'Previous dose confirmed by', 4, 'Patient', '^S^confirmedprevious=Previous dose confirmed by patient^^^medsvc_vac_prevdoseconfirmed')
			,('SubcutanMedication', 'Previous dose confirmed by', 5, 'Other', '^S^confirmedprevious=Previous dose confirmed^^^medsvc_vac_prevdoseconfirmed')

			,('SubcutanMedication', 'All of the above', 0, 'Correct patient, time, route, dose and medication confirmed prior to administration', '^D=Correct patient, time, route, dose and medication confirmed prior to administration')
			,('SubcutanMedication', 'All of the above', 0, 'Patient advised of actions and side-effects prior to administration', '^D=Patient advised of actions and side-effects prior to administration')
			,('SubcutanMedication', 'All of the above', 0, 'Allergies confirmed and medications reviewed prior to administration', '^D=Allergies confirmed and medications reviewed prior to administration')

			,('GeneralAssessment', 'Symptoms', 1, 'INCR', '^S^symptoms=Increased symptoms^^U')
			,('GeneralAssessment', 'Symptoms', 2, 'Unchg', '^S^symptoms=No change in symptoms')
			,('GeneralAssessment', 'Symptoms', 3, 'DECR', '^S^symptoms=Decreased symptoms')
			,('GeneralAssessment', 'Pain', 1, 'INCR', '^S^pain=Increased pain^^U')
			,('GeneralAssessment', 'Pain', 2, 'Unchg', '^S^pain=No change in pain')
			,('GeneralAssessment', 'Pain', 3, 'DECR', '^S^pain=Decreased pain')
			,('GeneralAssessment', 'Heart rate', 1, 'INCR', '^S^hr=Increased heart rate^^U')
			,('GeneralAssessment', 'Heart rate', 2, 'Unchg', '^S^hr=No change in heart rate')
			,('GeneralAssessment', 'Heart rate', 3, 'DECR', '^S^hr=Decreased heart rate^^U')
			,('GeneralAssessment', 'Blood pressure', 1, 'INCR', '^S^bp=Increase in blood pressure^^U')
			,('GeneralAssessment', 'Blood pressure', 2, 'Unchg', '^S^bp=No change in blood pressure')
			,('GeneralAssessment', 'Blood pressure', 3, 'DECR', '^S^bp=Decreased blood pressure^^U')
			,('GeneralAssessment', 'Temperature', 1, 'INCR', '^S^temperature=Increased temperature^^U')
			,('GeneralAssessment', 'Temperature', 2, 'Unchg', '^S^temperature=No change in temperature')
			,('GeneralAssessment', 'Temperature', 3, 'DECR', '^S^temperature=Decreased temperature^^U')
			,('GeneralAssessment', 'Nausea', 1, 'INCR', '^S^Nausea=Increased nausea^^U')
			,('GeneralAssessment', 'Nausea', 2, 'Unchg', '^S^Nausea=No change in nausea')
			,('GeneralAssessment', 'Nausea', 3, 'DECR', '^S^Nausea=Decreased nausea')
			,('GeneralAssessment', 'Vomiting', 1, 'INCR', '^S^Vomiting=Increased vomiting^^U')
			,('GeneralAssessment', 'Vomiting', 2, 'Unchg', '^S^Vomiting=No change in vomiting')
			,('GeneralAssessment', 'Vomiting', 3, 'DECR', '^S^Vomiting=Decreased vomiting')
			,('GeneralAssessment', 'Rash', 1, 'INCR', '^S^Rash=Increased rash^^U')
			,('GeneralAssessment', 'Rash', 2, 'Unchg', '^S^Rash=No change in rash')
			,('GeneralAssessment', 'Rash', 3, 'DECR', '^S^Rash=Decreased rash')
			,('GeneralAssessment', 'Respiratory rate', 1, 'INCR', '^S^rr=Increased respiratory rate^^U')
			,('GeneralAssessment', 'Respiratory rate', 2, 'Unchg', '^S^rr=No change in respiratory rate')
			,('GeneralAssessment', 'Respiratory rate', 3, 'DECR', '^S^rr=Decreased respiratory rate^^U')
			,('GeneralAssessment', 'Respiratory effort', 1, 'INCR', '^S^reff=Increased respiratory effort^^U')
			,('GeneralAssessment', 'Respiratory effort', 2, 'Unchg', '^S^reff=No change in respiratory effort')
			,('GeneralAssessment', 'Respiratory effort', 3, 'DECR', '^S^reff=Decreased respiratory effort^^U')
			,('GeneralAssessment', 'Breath sounds improved', 1, 'INCR', '^S^breathsounds=Breath sounds improved')
			,('GeneralAssessment', 'Breath sounds improved', 2, 'Unchg', '^S^breathsounds=No change in breath sounds')
			,('GeneralAssessment', 'Breath sounds improved', 3, 'DECR', '^S^breathsounds=Breath sounds worsended^^U')
			,('GeneralAssessment', 'Mental status', 1, 'INCR', '^S^mentalstat=Increased mental status')
			,('GeneralAssessment', 'Mental status', 2, 'Unchg', '^S^mentalstat=No change in mental status')
			,('GeneralAssessment', 'Mental status', 3, 'DECR', '^S^mentalstat=Decreased mental status^^U')
			,('GeneralAssessment', 'Urine output', 1, 'INCR', '^S^urineout=Increased urine output^^U')
			,('GeneralAssessment', 'Urine output', 2, 'Unchg', '^S^urineout=No change in urine output')
			,('GeneralAssessment', 'Urine output', 3, 'DECR', '^S^urineout=Decreased urine output^^U')
			,('GeneralAssessment', 'Constipation', 1, 'INCR', '^S^Constipation=Increased constipation^^U')
			,('GeneralAssessment', 'Constipation', 2, 'Unchg', '^S^Constipation=No change in constipation')
			,('GeneralAssessment', 'Constipation', 3, 'DECR', '^S^Constipation=Decreased constipation')

			,('IVFollowUp', 'Bag number', 1, '1st', '^SIV SITE^BNUM=1st bag hung^QX3651')
			,('IVFollowUp', 'Bag number', 2, '2nd', '^SIV SITE^BNUM=2nd bag hung^QX3652')
			,('IVFollowUp', 'Bag number', 3, '3rd', '^SIV SITE^BNUM=3rd bag hung^QX3653')
			,('IVFollowUp', 'Bag number', 4, '4th', '^SIV SITE^BNUM=4th bag hung^QX3654')
			,('IVFollowUp', 'Bag number', 5, '5th', '^SIV SITE^BNUM=5th bag hung^QX3655')
			,('IVFollowUp', 'Bag number', 6, '6th', '^SIV SITE^BNUM=6th bag hung^QX3656')
			,('IVFollowUp', 'Bag number', 7, '7th', '^SIV SITE^BNUM=7th bag hung^QX3657')
			,('IVFollowUp', 'Bag number', 8, '8th', '^SIV SITE^BNUM=8th bag hung^QX3658')
			,('IVFollowUp', 'Bag number', 9, '9th', '^SIV SITE^BNUM=9th bag hung^QX3659')
			,('IVFollowUp', 'Bag number', 10, '10th', '^SIV SITE^BNUM=10th bag hung^QX3660')
	
			,('IVFollowUp', 'Bag status', 1, 'New bag', '^S^Bag status=new')
			,('IVFollowUp', 'Bag status', 2, 'Ongoing bag', '^S^Bag status=ongoing')
			
			,('IVFollowUp', 'Titrating to patient response', 0, 'Dose increased', '^D=Dose increased')
			,('IVFollowUp', 'Titrating to patient response', 0, 'Dose decreased', '^D=Dose decreased')

			,('StopTime', 'Infusion Discontinued', 0, 'Date/Time ~~(infusionDiscontinued)', '^Con=')
			,('StopTime', 'Infusion Discontinued', 0, 'Removed catheter intact', '^D=Removed catheter intact')
			,('StopTime', 'Infusion Discontinued', 0, 'IV Line flushed after administration', '^D=IV Line flushed after administration')
			,('StopTime', 'Infusion Discontinued', 0, 'Total Amount Infused ~~(infusionDiscontinued)', '^CTotal Amount Infused:=')

			,('StopTime', 'Continued upon Transfer', 0, 'Date/Time ~~(infusionContinuedUponTransfer)', '^Con=')
			,('StopTime', 'Continued upon Transfer', 0, 'Total Amount Infused ~~(infusionContinuedUponTransfer)', '^CTotal Amount Infused:=')

			,('FollowUpSafety', 'All of the above', 0, 'Advised not to ambulate without assistance', '^D=Advised not to ambulate without assistance')
			,('FollowUpSafety', 'All of the above', 0, 'Patient in position of comfort', '^D=Patient in position of comfort')
			,('FollowUpSafety', 'All of the above', 0, 'Side rails up', '^D=Side rails up')
			,('FollowUpSafety', 'All of the above', 0, 'Cart in lowest position', '^D=Cart in lowest position')
			,('FollowUpSafety', 'All of the above', 0, 'Call light in reach', '^D=Call light in reach')

			,('VitalSigns', ' ~~(bpCondition)', 1, 'Well', '^S=Patient tolerated procedure well')
			,('VitalSigns', ' ~~(bpCondition)', 2, 'With Difficulty', '^S=Patient tolerated procedure with difficulty')
			,('VitalSigns', ' ~~(bpCondition)', 3, 'Uncooperative', '^S=Patient was uncooperative')
			,('VitalSigns', ' ~~(bpSite)', 1, 'Left Arm', '^S=BP taken on left arm')
			,('VitalSigns', ' ~~(bpSite)', 2, 'Right Arm', '^S=BP taken on right arm')

			,('VitalSigns', ' ~~(pulseSelect1)', 1, 'Sitting', '^S=Pulse taken with sitting')
			,('VitalSigns', ' ~~(pulseSelect1)', 2, 'Standing', '^S=Pulse taken with standing')
			,('VitalSigns', ' ~~(pulseSelect2)', 1, 'PulseCheck #1', '^S=PulseCheck #1')
			,('VitalSigns', ' ~~(pulseSelect2)', 2, 'PulseCheck #2', '^S=PulseCheck #2')
			,('VitalSigns', ' ~~(pulseSelect2)', 3, 'PulseCheck #3', '^S=PulseCheck #3')
			
			,('VitalSigns', ' ~~(temperatureSelect1)', 1, 'Oral', '^S=Temperature taken orally')
			,('VitalSigns', ' ~~(temperatureSelect1)', 2, 'Rectal', '^S=Temperature taken rectally')
			,('VitalSigns', ' ~~(temperatureSelect1)', 3, 'Ear', '^S=Temperature taken in the ear')
			,('VitalSigns', ' ~~(temperatureSelect2)', 1, 'Sitting', '^S=Temperature taken while sitting')
			,('VitalSigns', ' ~~(temperatureSelect2)', 2, 'Lying down', '^S=Temperature taken while lying down')
			
			,('VitalSigns', ' ~~(mapSelect1)', 1, 'MAP #1', '^S=MAP #1')
			,('VitalSigns', ' ~~(mapSelect1)', 2, 'MAP #2', '^S=MAP #2')
			,('VitalSigns', ' ~~(mapSelect1)', 3, 'MAP #3', '^S=MAP #3')
			,('VitalSigns', ' ~~(mapSelect2)', 1, 'Clear and acceptable', '^S=Map was clear and acceptable')
			,('VitalSigns', ' ~~(mapSelect2)', 2, 'MAP 1%', '^S=Map was 1%')
			,('VitalSigns', ' ~~(mapSelect2)', 3, 'MAP > 100%', '^S=Map was greater than 100%')

			,('VitalSigns', ' ~~(respiratorySelect1)', 1, 'Room Air', '^S=room air')
			,('VitalSigns', ' ~~(respiratorySelect1)', 2, 'Room Vacuum', '^S=room vaccuum')
			,('VitalSigns', ' ~~(respiratorySelect1)', 3, 'Room Neg Pressure', '^S=room negative pressure')
			,('VitalSigns', ' ~~(respiratorySelect2)', 1, 'RESP NO', '^S=respiration no')
			,('VitalSigns', ' ~~(respiratorySelect2)', 2, 'RESP YES', '^S=respiration yes')
			
			,('VitalSigns', ' ~~(painSelect1)', 1, 'Continuous', '^S=Pain contunuous')
			,('VitalSigns', ' ~~(painSelect1)', 2, 'Pain Attributes', '^S=Pain attributes')
			,('VitalSigns', ' ~~(painSelect1)', 3, 'Not in pain', '^S=Not in pain')
			,('VitalSigns', ' ~~(painSelect2)', 1, 'Pain > 10%', '^S=Pain is greater than 10%')
			,('VitalSigns', ' ~~(painSelect2)', 2, 'Pain > 20%', '^S=Pain is greater than 20%')
			,('VitalSigns', ' ~~(painSelect2)', 3, 'Pain > 30%', '^S=Pain is greater than 30%')
			,('VitalSigns', ' ~~(painSelect2)', 4, 'Pain > 40%', '^S=Pain is greater than 40%')
			,('VitalSigns', ' ~~(painSelect2)', 5, 'Pain > 50%', '^S=Pain is greater than 50%')
			,('VitalSigns', ' ~~(painSelect2)', 6, 'Pain > 60%', '^S=Pain is greater than 60%')
			,('VitalSigns', ' ~~(painSelect2)', 7, 'Pain > 70%', '^S=Pain is greater than 70%')
			,('VitalSigns', ' ~~(painSelect2)', 8, 'Pain > 80%', '^S=Pain is greater than 80%')
			,('VitalSigns', ' ~~(painSelect2)', 9, 'Pain > 90%', '^S=Pain is greater than 90%')
			
			,('VitalSigns', ' ~~(end-tidalCo2Select1)', 1, 'End-Tidal. 1', '^S=End-Tidal. 1')
			,('VitalSigns', ' ~~(end-tidalCo2Select1)', 2, 'End-Tidal. 2', '^S=End-Tidal. 2')
			,('VitalSigns', ' ~~(end-tidalCo2Select1)', 3, 'End-Tidal. 3', '^S=End-Tidal. 3')
			,('VitalSigns', ' ~~(end-tidalCo2Select2)', 1, 'End-Tidal. @4', '^S=End-Tidal. @4')
			,('VitalSigns', ' ~~(end-tidalCo2Select2)', 2, 'End-Tidal. @5', '^S=End-Tidal. @5')

			-- IV for I and I  -------------------------------------------
		
			,('IVInIMedication', 'Location', 1, 'Forearm, Left', '^S^IV_Location=into left forearm^^left forearm')
			,('IVInIMedication', 'Location', 2, 'Forearm, Right', '^S^IV_Location=into right forearm^^right forearm')
			,('IVInIMedication', 'Location', 3, 'AC, Left', '^S^IV_Location=into left antecubital^^left antecubital')
			,('IVInIMedication', 'Location', 4, 'AC, Right', '^S^IV_Location=into right antecubital^^right antecubital')
			,('IVInIMedication', 'Location', 5, 'Wrist, Left', '^S^IV_Location=into left wrist^^left wrist')
			,('IVInIMedication', 'Location', 6, 'Wrist, Right', '^S^IV_Location=into right wrist^^right wrist')
			,('IVInIMedication', 'Location', 7, 'Hand, Left', '^S^IV_Location=into left hand^^left hand')
			,('IVInIMedication', 'Location', 8, 'Hand, Right', '^S^IV_Location=into right hand^^right hand')
			,('IVInIMedication', 'Location', 9, 'EJ, Left', '^S^IV_Location=into left EJ^^left EJ')
			,('IVInIMedication', 'Location', 10, 'EJ, Right', '^S^IV_Location=into right EJ^^right EJ')
			,('IVInIMedication', 'Location', 11, 'Scalp, Left', '^S^IV_Location=into left scalp^^left scalp')
			,('IVInIMedication', 'Location', 12, 'Scalp, Right', '^S^IV_Location=into right scalp^^right scalp')
			,('IVInIMedication', 'Location', 13, 'Shoulder, Left', '^S^IV_Location=into left shoulder^^left shoulder')
			,('IVInIMedication', 'Location', 14, 'Shoulder, Right', '^S^IV_Location=into right shoulder^^right shoulder')
			,('IVInIMedication', 'Location', 15, 'Groin, Left', '^S^IV_Location=into left groin^^left groin')
			,('IVInIMedication', 'Location', 16, 'Groin, Right', '^S^IV_Location=into right groin^^right groin')
			,('IVInIMedication', 'Location', 17, 'Leg, Left', '^S^IV_Location=into left leg^^left leg')
			,('IVInIMedication', 'Location', 18, 'Leg, Right', '^S^IV_Location=into right leg^^right leg')
			,('IVInIMedication', 'Location', 19, 'Foot, Left', '^S^IV_Location=into left foot^^left foot')
			,('IVInIMedication', 'Location', 20, 'Foot, Right', '^S^IV_Location=into right foot^^right foot')
			,('IVInIMedication', 'Location', 21, 'PICC', '^S^IV_Location=into PICC^^PICC')
			,('IVInIMedication', 'Location', 22, 'Port-a-Cath', '^S^IV_Location=into port-a-Cath^^port-a-Cath')
			,('IVInIMedication', 'Location', 23, 'Hickman-cath', '^S^IV_Location=into hickman cath^^hickman cath')
			,('IVInIMedication', 'Location', 24, 'Shunt', '^S^IV_Location=into shunt^^shunt')
			,('IVInIMedication', 'Location', 25, 'Sternum', '^S^IV_Location=into sternum^^sternum')
			,('IVInIMedication', 'Location', 26, 'Tibia, Left', '^S^IV_Location=into left tibia^^left tibia')
			,('IVInIMedication', 'Location', 27, 'Tibia, Right', '^S^IV_Location=into right tibia^^right tibia')
			,('IVInIMedication', 'Location', 28, 'ETT', '^S^IV_Location=ETT^^right tibia')
			,('IVInIMedication', 'Location', 29, 'Other Location', '^S^IV_Location=Other Location')
			
			,('IVInIMedication', 'Location', 0, 'IV fluids', '^D^hydration=IV fluids established')
			,('IVInIMedication', 'Location', 0, 'IVP', '^D^^QX116=IVP')
			,('IVInIMedication', 'Location', 0, 'Added to existing IV Fluid', '^D^IVaddseven^QX1036=added to existing IV Fluid')
			,('IVInIMedication', 'Location', 0, 'IVPB/drip', '^D=IVPB or drip')

			,('IVInIMedication', 'IV Number', 1, '1st IV', '^SIV number=1st IV')
			,('IVInIMedication', 'IV Number', 2, '2nd IV', '^SIV number=2nd IV')
			,('IVInIMedication', 'IV Number', 3, '3rd IV', '^SIV number=3rd IV')
			,('IVInIMedication', 'IV Number', 4, '4th IV', '^SIV number=4th IV')

			,('IVInIMedication', 'ETT', 0, 'No IV', '^D=No IV')	
			,('IVInIMedication', 'ETT', 0, 'IV infiltrated', '^D=IV infiltrated')		
			
			,('IVInIMedication', 'IV fluids', 0, 'Bag number', '')
			,('IVInIMedication', 'IV fluids', 0, 'Amount', '')
			,('IVInIMedication', 'IV fluids', 0, 'Tubing', '')
			,('IVInIMedication', 'IV fluids', 0, 'In buretrol', '^D^buretolone^QX422=via Buretrol')
			,('IVInIMedication', 'IV fluids', 0, 'On IV pump', '^D^Pump^QX421=on IV pump')
			,('IVInIMedication', 'IV fluids', 0, 'Syringe pump', '^D^Pump^QX6402=on syringe pump')
			,('IVInIMedication', 'IV fluids', 0, 'Rapid infuser used', '^D^Pump^QX299=via rapid infuser')
			,('IVInIMedication', 'IV fluids', 0, 'Fluid warmer used', '^D^Pump^QX299=Fluid warmer used')
			,('IVInIMedication', 'IV fluids', 0, 'Bolus', '')
			,('IVInIMedication', 'IV fluids', 0, 'Amount (Bolus)', '')
			,('IVInIMedication', 'IV fluids', 0, 'Other bolus (ml)', '^COther bolus amount (ml):=')
			,('IVInIMedication', 'IV fluids', 0, 'Rate', '')
			,('IVInIMedication', 'IV fluids', 0, 'Other (ml/hr)', '^COther bolus rate (ml/hr):=')
			,('IVInIMedication', 'IV fluids', 0, 'Rate after Bolus', '')
			,('IVInIMedication', 'IV fluids', 0, 'Repeat bolus', '')
			,('IVInIMedication', 'IV fluids', 0, 'Other (ml)', '^COther repeat bolus amount (ml):=')
			,('IVInIMedication', 'IV fluids', 0, 'Non-bolus', '')
			,('IVInIMedication', 'IV fluids', 0, 'Amount (Non-bolus)', '')
			,('IVInIMedication', 'IV fluids', 0, 'Other non-bolus (ml/hr)', '^COther repeat non-bolus amount (ml):=')
							
			,('IVInIMedication', 'In buretrol', 0, 'Amount (ml)', '^Cinitial fluid (ml)')	
			
			,('IVInIMedication', 'Bag Number', 1, '1st', '^S^BNUM=1st bag hung^QX3651')
			,('IVInIMedication', 'Bag Number', 2, '2nd', '^S^BNUM=2nd bag hung^QX3652')
			,('IVInIMedication', 'Bag Number', 3, '3rd', '^S^BNUM=3rd bag hung^QX3653')
			,('IVInIMedication', 'Bag Number', 4, '4th', '^S^BNUM=4th bag hung^QX3654')
			,('IVInIMedication', 'Bag Number', 5, '5th', '^S^BNUM=5th bag hung^QX3655')
			,('IVInIMedication', 'Bag Number', 6, '6th', '^S^BNUM=6th bag hung^QX3656')
			,('IVInIMedication', 'Bag Number', 7, '7th', '^S^BNUM=7th bag hung^QX3657')
			,('IVInIMedication', 'Bag Number', 8, '8th', '^S^BNUM=8th bag hung^QX3658')
			,('IVInIMedication', 'Bag Number', 9, '9th', '^S^BNUM=9th bag hung^QX3659')
			,('IVInIMedication', 'Bag Number', 10, '10th', '^S^BNUM=10th bag hung^QX3660')
			
			,('IVInIMedication', 'Amount', 1, '50ml', '^Samount^fluids=50ml')
			,('IVInIMedication', 'Amount', 2, '100ml', '^Samount^fluids=100ml')
			,('IVInIMedication', 'Amount', 3, '150ml', '^Samount^fluids=150ml')
			,('IVInIMedication', 'Amount', 4, '250ml', '^Samount^fluids=250ml')
			,('IVInIMedication', 'Amount', 5, '500ml', '^Samount^fluids=500ml')
			,('IVInIMedication', 'Amount', 6, '1 Liter', '^Samount^fluids=1 Liter')
			
			,('IVInIMedication', 'Tubing', 1, 'Primary', '^S^tubetube=via primary tubing')
			,('IVInIMedication', 'Tubing', 2, 'Gravity', '^S^tubetube=via gravity tubing')
			,('IVInIMedication', 'Tubing', 3, 'Blood', '^S^tubetube=via blood tubing')
			,('IVInIMedication', 'Tubing', 4, 'Pump', '^S^tubetube=via pump tubing')
			,('IVInIMedication', 'Tubing', 5, 'Secondary', '^S^tubetube=via secondary tubing')

			,('IVInIMedication', 'Amount (Bolus)', 1, '1000ml', '^S^bolus=bolus of 1000 ml established^QX411')
			,('IVInIMedication', 'Amount (Bolus)', 2, '900ml', '^S^bolus=bolus of 900 ml established^QX412')
			,('IVInIMedication', 'Amount (Bolus)', 3, '800ml', '^S^bolus=bolus of 800 ml established^QX413')
			,('IVInIMedication', 'Amount (Bolus)', 4, '700ml', '^S^bolus=bolus of 700 ml established^QX414')
			,('IVInIMedication', 'Amount (Bolus)', 5, '600ml', '^S^bolus=bolus of 600 ml established^QX415')
			,('IVInIMedication', 'Amount (Bolus)', 6, '500ml', '^S^bolus=bolus of 500 ml established^QX416')
			,('IVInIMedication', 'Amount (Bolus)', 7, '400ml', '^S^bolus=bolus of 400 ml established^QX417')
			,('IVInIMedication', 'Amount (Bolus)', 8, '300ml', '^S^bolus=bolus of 300 ml established^QX418')
			,('IVInIMedication', 'Amount (Bolus)', 9, '250ml', '^S^bolus=bolus of 250 ml established^QX2180')
			,('IVInIMedication', 'Amount (Bolus)', 10, '200ml', '^S^bolus=bolus of 200 ml established^QX419')
			,('IVInIMedication', 'Amount (Bolus)', 11, '100ml', '^S^bolus=bolus of 100 ml established^QX420')
			,('IVInIMedication', 'Amount (Bolus)', 12, 'Other >>', '^S^bolus=bolus established^QX77')
			
			,('IVInIMedication', 'Rate', 1, 'Wide open', '^S^rateofbolus=wide open')
			,('IVInIMedication', 'Rate', 2, '1000 ml/hr', '^S^rateofbolus=1000 ml/hr')
			,('IVInIMedication', 'Rate', 3, '500 ml/hr', '^S^rateofbolus=500 ml/hr')
			,('IVInIMedication', 'Rate', 4, '250 ml/hr', '^S^rateofbolus=250 ml/hr')
			,('IVInIMedication', 'Rate', 5, 'Other rate >>', '^S^rateofbolus=Rate of bolus')
			
			,('IVInIMedication', 'Rate after Bolus', 1, '50 ml/hr', '^S^RATEafter=After bolus completed rate changed to 50 ml/hr^QX900')
			,('IVInIMedication', 'Rate after Bolus', 2, '100 ml/hr', '^S^RATEafter=After bolus completed rate changed to 100 ml/hr^QX900')
			,('IVInIMedication', 'Rate after Bolus', 3, '125 ml/hr', '^S^RATEafter=After bolus completed rate changed to 125 ml/hr^QX900')
			,('IVInIMedication', 'Rate after Bolus', 4, '150 ml/hr', '^S^RATEafter=After bolus completed rate changed to 150 ml/hr^QX900')
			,('IVInIMedication', 'Rate after Bolus', 5, '200 ml/hr', '^S^RATEafter=After bolus completed rate changed to 200 ml/hr^QX900')
			,('IVInIMedication', 'Rate after Bolus', 6, '250 ml/hr', '^S^RATEafter=After bolus completed rate changed to 250 ml/hr^QX900')
			,('IVInIMedication', 'Rate after Bolus', 7, '500 ml/ml', '^S^RATEafter=After bolus completed rate changed to 500 ml/hr^QX900')
			,('IVInIMedication', 'Rate after Bolus', 8, 'KVO', '^S^RATEafter=After bolus completed rate changed to KVO^QX903')

			,('IVInIMedication', 'Repeat bolus', 1, '1000ml', '^SRepeat bolus^REPEAT=of 1000 ml established')
			,('IVInIMedication', 'Repeat bolus', 2, '900ml', '^SRepeat bolus^REPEAT=of 900 ml established')
			,('IVInIMedication', 'Repeat bolus', 3, '800ml', '^SRepeat bolus^REPEAT=of 800 ml established')
			,('IVInIMedication', 'Repeat bolus', 4, '700ml', '^SRepeat bolus^REPEAT=of 700 ml established')
			,('IVInIMedication', 'Repeat bolus', 5, '600ml', '^SRepeat bolus^REPEAT=of 600 ml established')
			,('IVInIMedication', 'Repeat bolus', 6, '500ml', '^SRepeat bolus^REPEAT=of 500 ml established')
			,('IVInIMedication', 'Repeat bolus', 7, '400ml', '^SRepeat bolus^REPEAT=of 400 ml established')
			,('IVInIMedication', 'Repeat bolus', 8, '300ml', '^SRepeat bolus^REPEAT=of 300 ml established')
			,('IVInIMedication', 'Repeat bolus', 9, '250ml', '^SRepeat bolus^REPEAT=of 250 ml established')
			,('IVInIMedication', 'Repeat bolus', 10, '200ml', '^SRepeat bolus^REPEAT=of 200 ml established')
			,('IVInIMedication', 'Repeat bolus', 11, '100ml', '^SRepeat bolus^REPEAT=of 100 ml established')
			,('IVInIMedication', 'Repeat bolus', 12, 'Other >>', '^SRepeat bolus^REPEAT=established')
			
			,('IVInIMedication', 'Amount (Non-bolus)', 1, '1000 ml/hr', '^SRate of infusion (non-bolus)^InfuRATE=Infusing at 1000 ml/hr^QX77')
			,('IVInIMedication', 'Amount (Non-bolus)', 2, '500 ml/hr', '^SRate of infusion (non-bolus)^InfuRATE=Infusing at 500 ml/hr^QX77')
			,('IVInIMedication', 'Amount (Non-bolus)', 3, '450 ml/hr', '^SRate of infusion (non-bolus)^InfuRATE=Infusing at 450 ml/hr^QX77')
			,('IVInIMedication', 'Amount (Non-bolus)', 4, '400 ml/hr', '^SRate of infusion (non-bolus)^InfuRATE=Infusing at 400 ml/hr^QX77')
			,('IVInIMedication', 'Amount (Non-bolus)', 5, '350 ml/hr', '^SRate of infusion (non-bolus)^InfuRATE=Infusing at 350 ml/hr^QX77')
			,('IVInIMedication', 'Amount (Non-bolus)', 6, '300 ml/hr', '^SRate of infusion (non-bolus)^InfuRATE=Infusing at 300 ml/hr^QX77')
			,('IVInIMedication', 'Amount (Non-bolus)', 7, '250 ml/hr', '^SRate of infusion (non-bolus)^InfuRATE=Infusing at 250 ml/hr^QX77')
			,('IVInIMedication', 'Amount (Non-bolus)', 8, '200 ml/hr', '^SRate of infusion (non-bolus)^InfuRATE=Infusing at 200 ml/hr^QX77')
			,('IVInIMedication', 'Amount (Non-bolus)', 9, '175 ml/hr', '^SRate of infusion (non-bolus)^InfuRATE=Infusing at 175 ml/hr^QX77')
			,('IVInIMedication', 'Amount (Non-bolus)', 10, '150 ml/hr', '^SRate of infusion (non-bolus)^InfuRATE=Infusing at 150 ml/hr^QX77')
			,('IVInIMedication', 'Amount (Non-bolus)', 11, '125 ml/hr', '^SRate of infusion (non-bolus)^InfuRATE=Infusing at 125 ml/hr^QX77')
			,('IVInIMedication', 'Amount (Non-bolus)', 12, '100 ml/hr', '^SRate of infusion (non-bolus)^InfuRATE=Infusing at 100 ml/hr^QX77')
			,('IVInIMedication', 'Amount (Non-bolus)', 13, '75 ml/hr', '^SRate of infusion (non-bolus)^InfuRATE=Infusing at 75 ml/hr^QX77')
			,('IVInIMedication', 'Amount (Non-bolus)', 14, '50 ml/hr', '^SRate of infusion (non-bolus)^InfuRATE=Infusing at 50 ml/hr^QX77')
			,('IVInIMedication', 'Amount (Non-bolus)', 15, '42 ml/hr', '^SRate of infusion (non-bolus)^InfuRATE=Infusing at 42 ml/hr^QX77')
			,('IVInIMedication', 'Amount (Non-bolus)', 16, 'WO', '^S Rate of infusion (non-bolus)^InfuRATE=Infusing at wide open^QX77')
			,('IVInIMedication', 'Amount (Non-bolus)', 17, 'KVO', '^S Rate of infusion (non-bolus)^InfuRATE=Infusing at KVO^QX78')
			,('IVInIMedication', 'Amount (Non-bolus)', 18, 'Other >>', '^S Rate of infusion (non-bolus)^InfuRATE=Infusing at')

			,('IVInIMedication', 'IVP', 0, 'Slowly', '^D=Slowly')
			,('IVInIMedication', 'IVP', 0, 'Rapidly', '^D=Rapidly')

			,('IVInIMedication', 'Added to existing IV fluid', 0, 'Type', '^CType:=')
			,('IVInIMedication', 'Added to existing IV fluid', 0, 'Amount of fluid remaining', '^CAmount of fluid remaining:=')

			,('IVInIMedication', 'IVPB/drip', 0, 'Rate (IVPB/drip)', '')
			,('IVInIMedication', 'IVPB/drip', 0, 'Other rate', '^Cat=')
			,('IVInIMedication', 'IVPB/drip', 0, 'Premixed', '^D=Premixed')
			,('IVInIMedication', 'IVPB/drip', 0, 'Mixed in', '')
			,('IVInIMedication', 'IVPB/drip', 0, 'Other mixed in', '^CIVPB mixed in:=')
			,('IVInIMedication', 'IVPB/drip', 0, 'Fluid', '')
			,('IVInIMedication', 'IVPB/drip', 0, 'Tubing (IVPB/drip)', '')
			,('IVInIMedication', 'IVPB/drip', 0, 'In buretrol (IVPB/drip)', '^D^buretoltwo^QX422=via Buretrol')
			,('IVInIMedication', 'IVPB/drip', 0, 'On IV Pump (IVPB/drip)', '^D^Pump^QX421=on IV pump')
			,('IVInIMedication', 'IVPB/drip', 0, 'Syringe Pump (IVPB/drip)', '^D^Pump^QX6402=on syringe pump')
			,('IVInIMedication', 'IVPB/drip', 0, 'Filter Used', '^D^filterneed=Filter used with administration')

			,('IVInIMedication', 'In buretrol (IVPB/drip)', 0, 'IVPB/drip amount (ml)', '^Cinitial fluid (ml)')
			
			,('IVInIMedication', 'Rate (IVPB/drip)', 1, '1000 ml/hr', '^Sat^ivrate=1000 ml/hr')
			,('IVInIMedication', 'Rate (IVPB/drip)', 2, '500 ml/hr', '^Sat^ivrate=500 ml/hr')
			,('IVInIMedication', 'Rate (IVPB/drip)', 3, '450 ml/hr', '^Sat^ivrate=450 ml/hr')
			,('IVInIMedication', 'Rate (IVPB/drip)', 4, '400 ml/hr', '^Sat^ivrate=400 ml/hr')
			,('IVInIMedication', 'Rate (IVPB/drip)', 5, '350 ml/hr', '^Sat^ivrate=350 ml/hr')
			,('IVInIMedication', 'Rate (IVPB/drip)', 6, '300 ml/hr', '^Sat^ivrate=300 ml/hr')
			,('IVInIMedication', 'Rate (IVPB/drip)', 7, '250 ml/hr', '^Sat^ivrate=250 ml/hr')
			,('IVInIMedication', 'Rate (IVPB/drip)', 8, '200 ml/hr', '^Sat^ivrate=200 ml/hr')
			,('IVInIMedication', 'Rate (IVPB/drip)', 9, '175 ml/hr', '^Sat^ivrate=175 ml/hr')
			,('IVInIMedication', 'Rate (IVPB/drip)', 10, '150 ml/hr', '^Sat^ivrate=150 ml/hr')
			,('IVInIMedication', 'Rate (IVPB/drip)', 11, '125 ml/hr', '^Sat^ivrate=125 ml/hr')
			,('IVInIMedication', 'Rate (IVPB/drip)', 12, '100 ml/hr', '^Sat^ivrate=100 ml/hr')
			,('IVInIMedication', 'Rate (IVPB/drip)', 13, '90 ml/hr', '^Sat^ivrate=90 ml/hr')
			,('IVInIMedication', 'Rate (IVPB/drip)', 14, '80 ml/hr', '^Sat^ivrate=80 ml/hr')
			,('IVInIMedication', 'Rate (IVPB/drip)', 15, '75 ml/hr', '^Sat^ivrate=75 ml/hr')
			,('IVInIMedication', 'Rate (IVPB/drip)', 16, '70 ml/hr', '^Sat^ivrate=70 ml/hr')
			,('IVInIMedication', 'Rate (IVPB/drip)', 17, '60 ml/hr', '^Sat^ivrate=60 ml/hr')
			,('IVInIMedication', 'Rate (IVPB/drip)', 18, '50 ml/hr', '^Sat^ivrate=50 ml/hr')
			,('IVInIMedication', 'Rate (IVPB/drip)', 19, '45 ml/hr', '^Sat^ivrate=45 ml/hr')
			,('IVInIMedication', 'Rate (IVPB/drip)', 20, '42 ml/hr', '^Sat^ivrate=42 ml/hr')
		    ,('IVInIMedication', 'Rate (IVPB/drip)', 21, '40 ml/hr', '^Sat^ivrate=40 ml/hr')
			,('IVInIMedication', 'Rate (IVPB/drip)', 22, '35 ml/hr', '^Sat^ivrate=35 ml/hr')
			,('IVInIMedication', 'Rate (IVPB/drip)', 23, '30 ml/hr', '^Sat^ivrate=30 ml/hr')
			,('IVInIMedication', 'Rate (IVPB/drip)', 24, '25 ml/hr', '^Sat^ivrate=25 ml/hr')
			,('IVInIMedication', 'Rate (IVPB/drip)', 25, '20 ml/hr', '^Sat^ivrate=20 ml/hr')
			,('IVInIMedication', 'Rate (IVPB/drip)', 26, '15 ml/hr', '^Sat^ivrate=15 ml/hr')
			,('IVInIMedication', 'Rate (IVPB/drip)', 27, '10 ml/hr', '^Sat^ivrate=10 ml/hr')
			,('IVInIMedication', 'Rate (IVPB/drip)', 28, 'KVO', '^Sat^ivrate=KVO')
			,('IVInIMedication', 'Rate (IVPB/drip)', 29, 'Other >>', '^Sat^ivrate=at')

			,('IVInIMedication', 'Mixed in', 1, '50ml', '^SIVPB mixed in:=50ml^QX1050')
			,('IVInIMedication', 'Mixed in', 2, '100ml', '^SIVPB mixed in:=100ml^QX1051')
			,('IVInIMedication', 'Mixed in', 3, '250ml', '^SIVPB mixed in:=250ml^QX1052')
			,('IVInIMedication', 'Mixed in', 4, '500ml', '^SIVPB mixed in:=500ml^QX1053')
			,('IVInIMedication', 'Mixed in', 5, '1000ml', '^SIVPB mixed in:=1000ml^QX1054')
			,('IVInIMedication', 'Mixed in', 6, 'Other >>', '^SIVPB mixed in:=Other^QX1055')
			
			,('IVInIMedication', 'Fluid', 1, '0.9NS', '^SFluid:=0.9NS^QX1056')
			,('IVInIMedication', 'Fluid', 2, 'LR', '^SFluid:=LR^QX1057')
			,('IVInIMedication', 'Fluid', 3, 'D5.2NS', '^SFluid:=D5.2NS^QX1058')
			,('IVInIMedication', 'Fluid', 4, 'D5.45NS', '^SFluid:=D5.45NS^QX1059')
			,('IVInIMedication', 'Fluid', 5, 'D5.9NS', '^SFluid:=D5.9NS^QX1060')
			,('IVInIMedication', 'Fluid', 6, 'D5LR', '^SFluid:=D5LR^QX1061')
			,('IVInIMedication', 'Fluid', 7, 'D5W', '^SFluid:=D5W^QX1062')	
			
			,('IVInIMedication', 'Tubing (IVPB/drip)', 1, 'Primary', '^S=via primary tubing')
			,('IVInIMedication', 'Tubing (IVPB/drip)', 2, 'Gravity', '^S=via gravity tubing')
			,('IVInIMedication', 'Tubing (IVPB/drip)', 3, 'Blood', '^S=via blood tubing')
			,('IVInIMedication', 'Tubing (IVPB/drip)', 4, 'Pump', '^S=via pump tubing')
			,('IVInIMedication', 'Tubing (IVPB/drip)', 5, 'Secondary', '^S=via pump tubing')

		-- IV  template ------------------------------------------------
			,('IVMedication', 'Location', 1, 'Forearm, Left', '^S^IV_Location=into left forearm^QX1115^left forearm')
			,('IVMedication', 'Location', 2, 'Forearm, Right', '^S^IV_Location=into right forearm^QX1116^right forearm')
			,('IVMedication', 'Location', 3, 'AC, Left', '^S^IV_Location=into left antecubital^QX1039^left antecubital')
			,('IVMedication', 'Location', 4, 'AC, Right', '^S^IV_Location=into right antecubital^QX1040^right antecubital')
			,('IVMedication', 'Location', 5, 'Wrist, Left', '^S^IV_Location=into left wrist^QX4513^left wrist')
			,('IVMedication', 'Location', 6, 'Wrist, Right', '^S^IV_Location=into right wrist^QX4514^right wrist')
			,('IVMedication', 'Location', 7, 'Hand, Left', '^S^IV_Location=into left hand^QX1037^left hand')
			,('IVMedication', 'Location', 8, 'Hand, Right', '^S^IV_Location=into right hand^QX1038^right hand')
			,('IVMedication', 'Location', 9, 'EJ, Left', '^S^IV_Location=into left EJ^QX1117^left EJ')
			,('IVMedication', 'Location', 10, 'EJ, Right', '^S^IV_Location=into right EJ^QX1118^right EJ')
			,('IVMedication', 'Location', 11, 'Scalp, Left', '^S^IV_Location=into left scalp^^left scalp')
			,('IVMedication', 'Location', 12, 'Scalp, Right', '^S^IV_Location=into right scalp^^right scalp')
			,('IVMedication', 'Location', 13, 'Shoulder, Left', '^S^IV_Location=into left shoulder^^left shoulder')
			,('IVMedication', 'Location', 14, 'Shoulder, Right', '^S^IV_Location=into right shoulder^^right shoulder')
			,('IVMedication', 'Location', 15, 'Groin, Left', '^S^IV_Location=into left groin^^left groin')
			,('IVMedication', 'Location', 16, 'Groin, Right', '^S^IV_Location=into right groin^^right groin')
			,('IVMedication', 'Location', 17, 'Leg, Left', '^S^IV_Location=into left leg^^left leg')
			,('IVMedication', 'Location', 18, 'Leg, Right', '^S^IV_Location=into right leg^^right leg')
			,('IVMedication', 'Location', 19, 'Foot, Left', '^S^IV_Location=into left foot^^left foot')
			,('IVMedication', 'Location', 20, 'Foot, Right', '^S^IV_Location=into right foot^^right foot')
			,('IVMedication', 'Location', 21, 'PICC', '^S^IV_Location=into PICC^QX145^PICC')
			,('IVMedication', 'Location', 22, 'Port-a-Cath', '^S^IV_Location=into port-a-Cath^QX3700^port-a-Cath')
			,('IVMedication', 'Location', 23, 'Hickman-cath', '^S^IV_Location=into hickman cath^QX3701^hickman cath')
			,('IVMedication', 'Location', 24, 'Shunt', '^S^IV_Location=into shunt^QX3702^shunt')
			,('IVMedication', 'Location', 25, 'Sternum', '^S^IV_Location=into sternum^^sternum')
			,('IVMedication', 'Location', 26, 'Tibia, Left', '^S^IV_Location=into left tibia^^left tibia')
			,('IVMedication', 'Location', 27, 'Tibia, Right', '^S^IV_Location=into right tibia^^right tibia')
			,('IVMedication', 'Location', 28, 'Other Location', '^S^IV_Location=Other Location')
			
			,('IVMedication', 'Location', 0, 'IV Fluids', '')
			,('IVMedication', 'Location', 0, 'IVP', '^D^^QX116=IVP')
			,('IVMedication', 'Location', 0, 'Added to existing IV fluid', '^D^^QX1036=Added to existing IV fluid')
			,('IVMedication', 'Location', 0, 'IVPB/drip', '^D=IVPB or drip')
			
			,('IVMedication', 'IV number', 1, '1st IV', '^SIV number=1st IV')
			,('IVMedication', 'IV number', 2, '2nd IV', '^SIV number=2nd IV')
			,('IVMedication', 'IV number', 3, '3rd IV', '^SIV number=3rd IV')
			,('IVMedication', 'IV number', 4, '4th IV', '^SIV number=4th IV')

			,('IVMedication', 'Site', 1, '1st IV', '^D^^QX6883=IV SITE #1 IV fluids established')
			,('IVMedication', 'Site', 2, '2nd IV', '^D^^QX6884=IV SITE #2 IV fluids established')
			,('IVMedication', 'Site', 3, '3rd IV', '^D^^QX6885=IV SITE #3 IV fluids established')
			,('IVMedication', 'Site', 4, '4th IV', '^D^^QX6886=IV SITE #4 IV fluids established')
			
			,('IVMedication', 'ETT', 0, 'No IV', '^D=No IV')	
			,('IVMedication', 'ETT', 0, 'IV infiltrated', '^D=IV infiltrated')		
			
			,('IVMedication', 'IV Fluids', 0, 'Site', '')
			,('IVMedication', 'IV Fluids', 0, 'Bag Number', '')
			,('IVMedication', 'IV Fluids', 0, 'Amount', '')
			
			,('IVMedication', 'IV Fluids', 0, 'Bolus', '')
			,('IVMedication', 'IV Fluids', 0, 'Bolus amount', '')
			,('IVMedication', 'IV Fluids', 0, 'Other bolus amount (ml)', '^COther Bolus Amount (ml):=')
			,('IVMedication', 'IV Fluids', 0, 'Rate of bolus', '^D^bolus=Rate of bolus:')
			,('IVMedication', 'IV Fluids', 0, 'Rate after bolus', '')
			,('IVMedication', 'IV Fluids', 0, 'Repeat bolus', '')
			,('IVMedication', 'IV Fluids', 0, 'Other repeat bolus (ml)', '^COther rate of bolus (ml/hr):=')
			
			,('IVMedication', 'IV Fluids', 0, 'Rate of infusion (non-bolus)', '')
			,('IVMedication', 'IV Fluids', 0, 'Rate', '')
			,('IVMedication', 'IV Fluids', 0, 'Other rate (ml/hr)', '^COther infusion rate (nl/hr):=')
			,('IVMedication', 'IV Fluids', 0, 'Rate change', '')
			,('IVMedication', 'IV Fluids', 0, 'Other rate change (ml/hr)', '^COther rate change (mL/hr):=')
		
			,('IVMedication', 'IV Fluids', 0, 'Tubing', '')
			,('IVMedication', 'IV Fluids', 0, 'Primary tubing', '^D^^QX1748=via primary tubing')
			,('IVMedication', 'IV Fluids', 0, 'Gravity tubing', '^D^^QX424=via gravity tubing')
			,('IVMedication', 'IV Fluids', 0, 'Blood tubing', '^D^^QX423=via blood tubing')
			,('IVMedication', 'IV Fluids', 0, 'Pump tubing', '^D^^QX1493=via pump tubing')
			,('IVMedication', 'IV Fluids', 0, 'Secondary tubing', '^D^^QX425=via secondary tubing')
			,('IVMedication', 'IV Fluids', 0, 'In buretrol (Tubing)', '^D^^QX422=via Buretrol')
			
			,('IVMedication', 'IV Fluids', 0, 'Pump', '')
			,('IVMedication', 'IV Fluids', 0, 'On IV pump', '^D^^QX421=on IV pump')
			,('IVMedication', 'IV Fluids', 0, 'Syringe pump', '^D^^QX6402=on syringe pump')
			,('IVMedication', 'IV Fluids', 0, 'Rapid infuser pump', '^D^^QX299=via rapid infuser')
			,('IVMedication', 'IV Fluids', 0, 'Fluid warmer pump', '^D^^QX298=Fluid warmer used')
			
			,('IVMedication', 'IV Fluids', 0, 'Tubing Changed', '')
			,('IVMedication', 'IV Fluids', 0, 'Pump tubing changed', '^D^^QX1493^=Tubing changed to pump tubing')
			,('IVMedication', 'IV Fluids', 0, 'In buretrol tubing changed', '^D^^QX422^=Tubing changed to in Buretrol')
			,('IVMedication', 'IV Fluids', 0, 'Blood tubing changed', '^D^^QX423^=Tubing changed to blood tubing')
			,('IVMedication', 'IV Fluids', 0, 'Gravity tubing changed', '^D^^QX424^=Tubing changed to gravity tubing')
			,('IVMedication', 'IV Fluids', 0, 'Secondary tubing changed', '^D^^QX425^=Tubing changed to secondary tubing')	
			
			,('IVMedication', 'Rate of bolus', 0, 'Wide open', '^D^RATE=wide open')
			,('IVMedication', 'Rate of bolus', 0, '1000 ml/hr', '^D^RATE=1000 ml/hr')
			,('IVMedication', 'Rate of bolus', 0, '500 ml/hr', '^D^RATE=500 ml/hr')
			,('IVMedication', 'Rate of bolus', 0, '250 ml/hr', '^D^RATE=250 ml/hr')
			,('IVMedication', 'Rate of bolus', 0, 'Other rate >>', '^D^RATE=Rate of bolus')
			,('IVMedication', 'Rate of bolus', 0, 'Other rate of bolus (ml/hr)', '^COther rate of bolus (ml/hr):=')
			
			,('IVMedication', 'Bag number', 1, '1st', '^SIV SITE^BNUM=1st bag hung^QX3651')
			,('IVMedication', 'Bag number', 2, '2nd', '^SIV SITE^BNUM=2nd bag hung^QX3652')
			,('IVMedication', 'Bag number', 3, '3rd', '^SIV SITE^BNUM=3rd bag hung^QX3653')
			,('IVMedication', 'Bag number', 4, '4th', '^SIV SITE^BNUM=4th bag hung^QX3654')
			,('IVMedication', 'Bag number', 5, '5th', '^SIV SITE^BNUM=5th bag hung^QX3655')
			,('IVMedication', 'Bag number', 6, '6th', '^SIV SITE^BNUM=6th bag hung^QX3656')
			,('IVMedication', 'Bag number', 7, '7th', '^SIV SITE^BNUM=7th bag hung^QX3657')
			,('IVMedication', 'Bag number', 8, '8th', '^SIV SITE^BNUM=8th bag hung^QX3658')
			,('IVMedication', 'Bag number', 9, '9th', '^SIV SITE^BNUM=9th bag hung^QX3659')
			,('IVMedication', 'Bag number', 10, '10th', '^SIV SITE^BNUM=10th bag hung^QX3660')
			
			,('IVMedication', 'Amount', 1, '50ml', '^Samount^fluids=50ml')
			,('IVMedication', 'Amount', 2, '100ml', '^Samount^fluids=100ml')
			,('IVMedication', 'Amount', 3, '150ml', '^Samount^fluids=150ml')
			,('IVMedication', 'Amount', 4, '250ml', '^Samount^fluids=250ml')
			,('IVMedication', 'Amount', 5, '500ml', '^Samount^fluids=500ml')
			,('IVMedication', 'Amount', 6, '1 Liter', '^Samount^fluids=1 Liter')
			
			,('IVMedication', 'Bolus amount', 1, '1000ml', '^SIV SITE^bolus=bolus of 1000 ml established^QX411')
			,('IVMedication', 'Bolus amount', 2, '900ml', '^SIV SITE^bolus=bolus of 900 ml established^QX412')
			,('IVMedication', 'Bolus amount', 3, '800ml', '^SIV SITE^bolus=bolus of 800 ml established^QX413')
			,('IVMedication', 'Bolus amount', 4, '700ml', '^SIV SITE^bolus=bolus of 700 ml established^QX414')
			,('IVMedication', 'Bolus amount', 5, '600ml', '^SIV SITE^bolus=bolus of 600 ml established^QX415')
			,('IVMedication', 'Bolus amount', 6, '500ml', '^SIV SITE^bolus=bolus of 500 ml established^QX416')
			,('IVMedication', 'Bolus amount', 7, '400ml', '^SIV SITE^bolus=bolus of 400 ml established^QX417')
			,('IVMedication', 'Bolus amount', 8, '300ml', '^SIV SITE^bolus=bolus of 300 ml established^QX418')
			,('IVMedication', 'Bolus amount', 9, '250ml', '^SIV SITE^bolus=bolus of 250 ml established^QX2180')
			,('IVMedication', 'Bolus amount', 10, '200ml', '^SIV SITE^bolus=bolus of 200 ml established^QX419')
			,('IVMedication', 'Bolus amount', 11, '100ml', '^SIV SITE^bolus=bolus of 100 ml established^QX420')
			,('IVMedication', 'Bolus amount', 12, 'Other >>', '^SIV SITE^bolus=bolus established^QX77')
			
			,('IVMedication', 'Rate after bolus', 1, '50 ml/hr', '^SIV SITE^RATEafter=After bolus completed rate changed to 50 ml/hr^QX900')
			,('IVMedication', 'Rate after bolus', 2, '100 ml/hr', '^SIV SITE^RATEafter=After bolus completed rate changed to 100 ml/hr^QX900')
			,('IVMedication', 'Rate after bolus', 3, '125 ml/hr', '^SIV SITE^RATEafter=After bolus completed rate changed to 125 ml/hr^QX900')
			,('IVMedication', 'Rate after bolus', 4, '150 ml/hr', '^SIV SITE^RATEafter=After bolus completed rate changed to 150 ml/hr^QX900')
			,('IVMedication', 'Rate after bolus', 5, '200 ml/hr', '^SIV SITE^RATEafter=After bolus completed rate changed to 200 ml/hr^QX900')
			,('IVMedication', 'Rate after bolus', 6, '250 ml/hr', '^SIV SITE^RATEafter=After bolus completed rate changed to 250 ml/hr^QX900')
			,('IVMedication', 'Rate after bolus', 7, '500 ml/ml', '^SIV SITE^RATEafter=After bolus completed rate changed to 500 ml/hr^QX900')
			,('IVMedication', 'Rate after bolus', 8, 'KVO', '^SIV SITE^RATEafter=After bolus completed rate changed to KVO^QX903')
			
			,('IVMedication', 'Repeat bolus', 1, '1000ml', '^SIV SITE Repeat bolus^REPEAT=of 1000 ml established')
			,('IVMedication', 'Repeat bolus', 2, '900ml', '^SIV SITE Repeat bolus^REPEAT=of 900 ml established')
			,('IVMedication', 'Repeat bolus', 3, '800ml', '^SIV SITE Repeat bolus^REPEAT=of 800 ml established')
			,('IVMedication', 'Repeat bolus', 4, '700ml', '^SIV SITE Repeat bolus^REPEAT=of 700 ml established')
			,('IVMedication', 'Repeat bolus', 5, '600ml', '^SIV SITE Repeat bolus^REPEAT=of 600 ml established')
			,('IVMedication', 'Repeat bolus', 6, '500ml', '^SIV SITE Repeat bolus^REPEAT=of 500 ml established')
			,('IVMedication', 'Repeat bolus', 7, '400ml', '^SIV SITE Repeat bolus^REPEAT=of 400 ml established')
			,('IVMedication', 'Repeat bolus', 8, '300ml', '^SIV SITE Repeat bolus^REPEAT=of 300 ml established')
			,('IVMedication', 'Repeat bolus', 9, '250ml', '^SIV SITE Repeat bolus^REPEAT=of 250 ml established')
			,('IVMedication', 'Repeat bolus', 10, '200ml', '^SIV SITE Repeat bolus^REPEAT=of 200 ml established')
			,('IVMedication', 'Repeat bolus', 11, '100ml', '^SIV SITE Repeat bolus^REPEAT=of 100 ml established')
			,('IVMedication', 'Repeat bolus', 12, 'Other >>', '^SIV SITE Repeat bolus^REPEAT=established')
			
			,('IVMedication', 'Rate', 1, '1000 ml/hr', '^SIV SITE Rate of infusion (non-bolus)^InfuRATE=Infusing at 1000 ml/hr^QX77')
			,('IVMedication', 'Rate', 2, '500 ml/hr', '^SIV SITE Rate of infusion (non-bolus)^InfuRATE=Infusing at 500 ml/hr^QX77')
			,('IVMedication', 'Rate', 3, '450 ml/hr', '^SIV SITE Rate of infusion (non-bolus)^InfuRATE=Infusing at 450 ml/hr^QX77')
			,('IVMedication', 'Rate', 4, '400 ml/hr', '^SIV SITE Rate of infusion (non-bolus)^InfuRATE=Infusing at 400 ml/hr^QX77')
			,('IVMedication', 'Rate', 5, '350 ml/hr', '^SIV SITE Rate of infusion (non-bolus)^InfuRATE=Infusing at 350 ml/hr^QX77')
			,('IVMedication', 'Rate', 6, '300 ml/hr', '^SIV SITE Rate of infusion (non-bolus)^InfuRATE=Infusing at 300 ml/hr^QX77')
			,('IVMedication', 'Rate', 7, '250 ml/hr', '^SIV SITE Rate of infusion (non-bolus)^InfuRATE=Infusing at 250 ml/hr^QX77')
			,('IVMedication', 'Rate', 8, '200 ml/hr', '^SIV SITE Rate of infusion (non-bolus)^InfuRATE=Infusing at 200 ml/hr^QX77')
			,('IVMedication', 'Rate', 9, '175 ml/hr', '^SIV SITE Rate of infusion (non-bolus)^InfuRATE=Infusing at 175 ml/hr^QX77')
			,('IVMedication', 'Rate', 10, '150 ml/hr', '^SIV SITE Rate of infusion (non-bolus)^InfuRATE=Infusing at 150 ml/hr^QX77')
			,('IVMedication', 'Rate', 11, '125 ml/hr', '^SIV SITE Rate of infusion (non-bolus)^InfuRATE=Infusing at 125 ml/hr^QX77')
			,('IVMedication', 'Rate', 12, '100 ml/hr', '^SIV SITE Rate of infusion (non-bolus)^InfuRATE=Infusing at 100 ml/hr^QX77')
			,('IVMedication', 'Rate', 13, '75 ml/hr', '^SIV SITE Rate of infusion (non-bolus)^InfuRATE=Infusing at 75 ml/hr^QX77')
			,('IVMedication', 'Rate', 14, '50 ml/hr', '^SIV SITE Rate of infusion (non-bolus)^InfuRATE=Infusing at 50 ml/hr^QX77')
			,('IVMedication', 'Rate', 15, '42 ml/hr', '^SIV SITE Rate of infusion (non-bolus)^InfuRATE=Infusing at 42 ml/hr^QX77')
			,('IVMedication', 'Rate', 16, 'WO', '^SIV SITE Rate of infusion (non-bolus)^InfuRATE=Infusing at wide open^QX77')
			,('IVMedication', 'Rate', 17, 'KVO', '^SIV SITE Rate of infusion (non-bolus)^InfuRATE=Infusing at KVO^QX78')
			,('IVMedication', 'Rate', 18, 'Other >>', '^SIV SITE Rate of infusion (non-bolus)^InfuRATE=Infusing at')
			
			,('IVMedication', 'Rate change', 1, '1000 ml/hr', '^SIV SITE Rate change^change=Infusing at 1000 ml/hr')
			,('IVMedication', 'Rate change', 2, '500 ml/hr', '^SIV SITE Rate change^change=Infusing at 500 ml/hr')
			,('IVMedication', 'Rate change', 3, '450 ml/hr', '^SIV SITE Rate change^change=Infusing at 450 ml/hr')
			,('IVMedication', 'Rate change', 4, '400 ml/hr', '^SIV SITE Rate change^change=Infusing at 400 ml/hr')
			,('IVMedication', 'Rate change', 5, '350 ml/hr', '^SIV SITE Rate change^change=Infusing at 350 ml/hr')
			,('IVMedication', 'Rate change', 6, '300 ml/hr', '^SIV SITE Rate change^change=Infusing at 300 ml/hr')
			,('IVMedication', 'Rate change', 7, '250 ml/hr', '^SIV SITE Rate change^change=Infusing at 250 ml/hr')
			,('IVMedication', 'Rate change', 8, '200 ml/hr', '^SIV SITE Rate change^change=Infusing at 200 ml/hr')
			,('IVMedication', 'Rate change', 9, '175 ml/hr', '^SIV SITE Rate change^change=Infusing at 175 ml/hr')
			,('IVMedication', 'Rate change', 10, '150 ml/hr', '^SIV SITE Rate change^change=Infusing at 150 ml/hr')
			,('IVMedication', 'Rate change', 11, '125 ml/hr', '^SIV SITE Rate change^change=Infusing at 125 ml/hr')
			,('IVMedication', 'Rate change', 12, '100 ml/hr', '^SIV SITE Rate change^change=Infusing at 100 ml/hr')
			,('IVMedication', 'Rate change', 13, '75 ml/hr', '^SIV SITE Rate change^change=Infusing at 75 ml/hr')
			,('IVMedication', 'Rate change', 14, '50 ml/hr', '^SIV SITE Rate change^change=Infusing at 50 ml/hr')
			,('IVMedication', 'Rate change', 15, '42 ml/hr', '^SIV SITE Rate change^change=Infusing at 42 ml/hr')
			,('IVMedication', 'Rate change', 16, 'WO', '^SIV SITE Rate change^change=Infusing at wide open')
			,('IVMedication', 'Rate change', 17, 'KVO', '^SIV SITE Rate change^change=Infusing at KVO')
			,('IVMedication', 'Rate change', 18, 'Other >>', '^SIV SITE Rate change^change=Infusing at')

			,('IVMedication', 'In buretrol (Tubing)', 0, 'Initial fluid (ml)', '^Cinitial fluid=')
			,('IVMedication', 'In buretrol (Tubing)', 0, 'Additional fluid (ml)', '^Cadditional fluid=')
			
			,('IVMedication', 'IVP', 0, 'Initial (First medication given IVP)', '^D^^QX6279=initial medication')
			,('IVMedication', 'IVP', 0, 'Subsequent (Each different medication given IVP after initial medication IVP)', '^D^^QX4594=subsequent different medication')
			,('IVMedication', 'IVP', 0, 'Repeat (Repeat dose of previous medication given)', '^D^^QX6268=repeat same medication')
			
			,('IVMedication', 'Initial (First medication given IVP)', 0, 'Slowly (Initial)', '^D=Slowly')
			,('IVMedication', 'Initial (First medication given IVP)', 0, 'Rapidly (Initial)', '^D=Rapidly')
			
			,('IVMedication', 'Subsequent (Each different medication given IVP after initial medication IVP)', 0, 'Slowly (Subsequent)', '^D=Slowly')
			,('IVMedication', 'Subsequent (Each different medication given IVP after initial medication IVP)', 0, 'Rapidly (Subsequent)', '^D=Rapidly')
			
			,('IVMedication', 'Repeat (Repeat dose of previous medication given)', 0, 'Slowly (Repeat)', '^D=Slowly')
			,('IVMedication', 'Repeat (Repeat dose of previous medication given)', 0, 'Rapidly (Repeat)', '^D=Rapidly')
			
			,('IVMedication', 'Added to existing IV fluid', 0, 'Type', '^CType:^type=')
			,('IVMedication', 'Added to existing IV fluid', 0, 'Amount of fluid remaining', '^CAmount of fluid remaining:^amont=')	
				
			,('IVMedication', 'IVPB/drip', 0, 'IVPB/drip type', '')
			,('IVMedication', 'IVPB/drip', 0, 'Premixed', '^D=Premixed')
			,('IVMedication', 'IVPB/drip', 0, 'IVPB mixed in', '')	
			,('IVMedication', 'IVPB/drip', 0, 'Other IVPB mixed in', '')	
			,('IVMedication', 'IVPB/drip', 0, 'Fluid', '')	
			,('IVMedication', 'IVPB/drip', 0, 'IVPB/drip tubing', '')	
			,('IVMedication', 'IVPB/drip', 0, 'Primary IVPB/drip tubing', '')
			,('IVMedication', 'IVPB/drip', 0, 'Gravity IVPB/drip tubing', '')
			,('IVMedication', 'IVPB/drip', 0, 'Blood IVPB/drip tubing', '')
			,('IVMedication', 'IVPB/drip', 0, 'Pump IVPB/drip tubing', '')
			,('IVMedication', 'IVPB/drip', 0, 'On an IV pump', '')
			,('IVMedication', 'IVPB/drip', 0, 'Secondary IVPB/drip tubing', '')
			,('IVMedication', 'IVPB/drip', 0, 'In buretrol (IVPB/drip Tubing)', '')
			,('IVMedication', 'IVPB/drip', 0, 'Rate (IVPB/drip)', '')
			,('IVMedication', 'IVPB/drip', 0, 'Pediatric rate (IVPB/drip)', '')
			,('IVMedication', 'IVPB/drip', 0, 'Other rate (IVPB/drip)', '')
			,('IVMedication', 'IVPB/drip', 0, 'Filter Needle used with administration', '')
			,('IVMedication', 'IVPB/drip', 0, 'This is considered a thrombolytic infusion (See Thrombolytic Record)', '')
			,('IVMedication', 'IVPB/drip', 0, 'This is considered a sedation medication (See Sedation Record)', '')								
			
			,('IVMedication', 'IVPB/drip type', 1, 'Initial (1st medication infusion this IV site)', '^D^^QX2425=initial infusion')
			,('IVMedication', 'IVPB/drip type', 2, 'Subsequent (infusion of additional different medication in this IV site)', '^D^^QX6845=subsequent infusion')
			,('IVMedication', 'IVPB/drip type', 3, 'Repeat (repeat dose of previous medication in this IV site)', '^D^^QX6845=repeat infusion')
			,('IVMedication', 'IVPB/drip type', 4, 'Concurrent (infusion of additional different medication this IV site)', '^D^^QX6846=concurrent infusion')	
			
			,('IVMedication', 'IVPB mixed in', 1, '50ml', '^SIVPB mixed in:=50ml^QX1050')
			,('IVMedication', 'IVPB mixed in', 2, '100ml', '^SIVPB mixed in:=100ml^QX1051')
			,('IVMedication', 'IVPB mixed in', 3, '250ml', '^SIVPB mixed in:=250ml^QX1052')
			,('IVMedication', 'IVPB mixed in', 4, '500ml', '^SIVPB mixed in:=500ml^QX1053')
			,('IVMedication', 'IVPB mixed in', 5, '1000ml', '^SIVPB mixed in:=1000ml^QX1054')
			,('IVMedication', 'IVPB mixed in', 6, 'Other >>', '^SIVPB mixed in:=Other^QX1055')
			
			,('IVMedication', 'Fluid', 1, '0.9NS', '^SFluid:=0.9NS^QX1056')
			,('IVMedication', 'Fluid', 2, 'LR', '^SFluid:=LR^QX1057')
			,('IVMedication', 'Fluid', 3, 'D5.2NS', '^SFluid:=D5.2NS^QX1058')
			,('IVMedication', 'Fluid', 4, 'D5.45NS', '^SFluid:=D5.45NS^QX1059')
			,('IVMedication', 'Fluid', 5, 'D5.9NS', '^SFluid:=D5.9NS^QX1060')
			,('IVMedication', 'Fluid', 6, 'D5LR', '^SFluid:=D5LR^QX1061')
			,('IVMedication', 'Fluid', 7, 'D5W', '^SFluid:=D5W^QX1062')		
			
			,('IVMedication', 'Rate (IVPB/drip)', 1, '1000 ml/hr', '^Sat^ivrate=1000 ml/hr')
			,('IVMedication', 'Rate (IVPB/drip)', 2, '500 ml/hr', '^Sat^ivrate=500 ml/hr')
			,('IVMedication', 'Rate (IVPB/drip)', 3, '450 ml/hr', '^Sat^ivrate=450 ml/hr')
			,('IVMedication', 'Rate (IVPB/drip)', 4, '400 ml/hr', '^Sat^ivrate=400 ml/hr')
			,('IVMedication', 'Rate (IVPB/drip)', 5, '350 ml/hr', '^Sat^ivrate=350 ml/hr')
			,('IVMedication', 'Rate (IVPB/drip)', 6, '300 ml/hr', '^Sat^ivrate=300 ml/hr')
			,('IVMedication', 'Rate (IVPB/drip)', 7, '250 ml/hr', '^Sat^ivrate=250 ml/hr')
			,('IVMedication', 'Rate (IVPB/drip)', 8, '200 ml/hr', '^Sat^ivrate=200 ml/hr')
			,('IVMedication', 'Rate (IVPB/drip)', 9, '175 ml/hr', '^Sat^ivrate=175 ml/hr')
			,('IVMedication', 'Rate (IVPB/drip)', 10, '150 ml/hr', '^Sat^ivrate=150 ml/hr')
			,('IVMedication', 'Rate (IVPB/drip)', 11, '125 ml/hr', '^Sat^ivrate=125 ml/hr')
			,('IVMedication', 'Rate (IVPB/drip)', 12, '100 ml/hr', '^Sat^ivrate=100 ml/hr')
			,('IVMedication', 'Rate (IVPB/drip)', 13, '75 ml/hr', '^Sat^ivrate=75 ml/hr')
			,('IVMedication', 'Rate (IVPB/drip)', 14, '50 ml/hr', '^Sat^ivrate=50 ml/hr')
			,('IVMedication', 'Rate (IVPB/drip)', 15, '42 ml/hr', '^Sat^ivrate=42 ml/hr')
			,('IVMedication', 'Rate (IVPB/drip)', 16, 'KVO', '^Sat^ivrate=KVO')
			
			,('IVMedication', 'Pediatric rate (IVPB/drip)', 1, '100 ml/hr', '^Sat^prate=100 ml/hr')
			,('IVMedication', 'Pediatric rate (IVPB/drip)', 2, '90 ml/hr', '^Sat^prate=90 ml/hr')
			,('IVMedication', 'Pediatric rate (IVPB/drip)', 3, '80 ml/hr', '^Sat^prate=80 ml/hr')
			,('IVMedication', 'Pediatric rate (IVPB/drip)', 4, '75 ml/hr', '^Sat^prate=75 ml/hr')
			,('IVMedication', 'Pediatric rate (IVPB/drip)', 5, '70 ml/hr', '^Sat^prate=70 ml/hr')
			,('IVMedication', 'Pediatric rate (IVPB/drip)', 6, '60 ml/hr', '^Sat^prate=60 ml/hr')
			,('IVMedication', 'Pediatric rate (IVPB/drip)', 7, '50 ml/hr', '^Sat^prate=50 ml/hr')
			,('IVMedication', 'Pediatric rate (IVPB/drip)', 8, '45 ml/hr', '^Sat^prate=45 ml/hr')
			,('IVMedication', 'Pediatric rate (IVPB/drip)', 9, '40 ml/hr', '^Sat^prate=40 ml/hr')
			,('IVMedication', 'Pediatric rate (IVPB/drip)', 10, '35 ml/hr', '^Sat^prate=35 ml/hr')
			,('IVMedication', 'Pediatric rate (IVPB/drip)', 11, '30 ml/hr', '^Sat^prate=30 ml/hr')
			,('IVMedication', 'Pediatric rate (IVPB/drip)', 12, '25 ml/hr', '^Sat^prate=25 ml/hr')
			,('IVMedication', 'Pediatric rate (IVPB/drip)', 13, '20 ml/hr', '^Sat^prate=20 ml/hr')
			,('IVMedication', 'Pediatric rate (IVPB/drip)', 14, '10 ml/hr', '^Sat^prate=10 ml/hr')
			,('IVMedication', 'Pediatric rate (IVPB/drip)', 15, 'KVO', '^Sat^prate=KVO')

			,('IVAssessment', 'All of the above', 0, 'Correct patient, time, route, dose and medication confirmed prior to administration', '^D=Correct patient, time, route, dose and medication confirmed prior to administration')
			,('IVAssessment', 'All of the above', 0, 'Patient advised of actions and side-effects prior to administration', '^D=Patient advised of actions and side-effects prior to administration')
			,('IVAssessment', 'All of the above', 0, 'Allergies confirmed and medications reviewed prior to administration', '^D=Allergies confirmed and medications reviewed prior to administration')
			,('IVAssessment', 'All of the above', 0, 'Connections checked prior to administration', '^D=Connections checked prior to administration')
			,('IVAssessment', 'All of the above', 0, 'Line traced prior to administration', '^D=Line traced prior to administration')
			,('IVAssessment', 'All of the above', 0, 'Catheter placement confirmed via flush prior to administration', '^D=Catheter placement confirmed via flush prior to administration')
			,('IVAssessment', 'All of the above', 0, 'IV site without s/sx of infiltration during medication administration', '^D=IV site without signs or symptoms of infiltration during medication administration')
			,('IVAssessment', 'All of the above', 0, 'No swelling during administration', '^D=No swelling during administration')
			,('IVAssessment', 'All of the above', 0, 'No drainage during administration', '^D=No drainage during administration')
			,('IVAssessment', 'All of the above', 0, 'IV flushed after administration', '^D=IV flushed after administration')

			,('IVInIAssessment', 'All of the above', 0, 'Correct patient, time, route, dose and medication/fluid confirmed', '^D=Correct patient, time, route, dose and medication/fluid confirmed')
			,('IVInIAssessment', 'All of the above', 0, 'Patient advised of actions and side-effects', '^D=Patient advised of actions and side-effects')
			,('IVInIAssessment', 'All of the above', 0, 'Allergies confirmed and medications reviewed', '^D=Allergies confirmed and medications reviewed')
			,('IVInIAssessment', 'All of the above', 0, 'Connection checked', '^D=Connection checked')
			,('IVInIAssessment', 'All of the above', 0, 'Line traced', '^D=Line traced')
			,('IVInIAssessment', 'All of the above', 0, 'Catheter placement confirmed via flush', '^D=Catheter placement confirmed via flush')
			,('IVInIAssessment', 'All of the above', 0, 'IV site without s/sx of infiltration', '^D=IV site without signs or symptoms of infiltration')
			,('IVInIAssessment', 'All of the above', 0, 'No swelling', '^D=No swelling')
			,('IVInIAssessment', 'All of the above', 0, 'No drainage', '^D=No drainage')
			,('IVInIAssessment', 'All of the above', 0, 'IV flushed after administration', '^D=IV flushed after administration')
		
			,('IVSafety', 'All of the above', 0, 'Patient in position of comfort', '^D=Patient in position of comfort')
			,('IVSafety', 'All of the above', 0, 'Side rails up', '^D=Side rails up')
			,('IVSafety', 'All of the above', 0, 'Cart in lowest position', '^D=Cart in lowest position')
			,('IVSafety', 'All of the above', 0, 'Call light in reach', '^D=Call light in reach')

			,('IVInISafety', 'All of the above', 0, 'Patient in position of comfort', '^D=Patient in position of comfort')
			,('IVInISafety', 'All of the above', 0, 'Side rails up', '^D=Side rails up')
			,('IVInISafety', 'All of the above', 0, 'Cart in lowest position', '^D=Cart in lowest position')
			,('IVInISafety', 'All of the above', 0, 'Call light in reach', '^D=Call light in reach')
			
			,('SiteInspection', 'All normal', 0, 'No swelling', '^D=No swelling at site')
			,('SiteInspection', 'All normal', 0, 'No drainage', '^D=No drainage at site')
			,('SiteInspection', 'All normal', 0, 'No bleeding', '^D=No bleeding at site')
			,('SiteInspection', 'All normal', 0, 'No bruising', '^D=No bruising at site ')
	

       ) as [items]
       ([prompt_group_name], [prompt], [sequence], [choice_text], [chart_markup]);

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
     when matched and ( ISNULL([target].[chart_markup], CHAR(0)) != ISNULL([source].[chart_markup], CHAR(0)) ) 
	   then update set 
       [chart_markup] = [source].[chart_markup] 
     when not matched by target
        then
      insert([prompt_id]
           , [sequence]
           , [choice_text]
		   , [chart_markup]
		   )
      values
    ([prompt_id], [sequence], [choice_text],[chart_markup])
    when not matched by source
        then delete;



/**** [action_route_templates] ****/
;WITH NonRouteSpecificMappings AS (
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
		,('FollowUp', NULL, NULL, 'FollowUp')
		,('OrderDiscontinue', NULL, NULL, 'OrderDiscontinue')
		,('PharmVerification', NULL, NULL, 'PharmVerification')
		,('CoSign', NULL, NULL, 'CoSign')
	) AS s (ActionName, RouteName, SiteId, TemplateName)
)
, IdxMappingData AS (
       SELECT	route_name = name, 
				template_name = case when RIGHT(RTRIM(misc), 3) = '5.7' then LEFT(misc, LEN(misc) - 3) ELSE misc END,
				internal_site_id = CONVERT(int, s.internal_id)
        FROM	ibex.dbo.idx i
		JOIN	dbo.external_ids s -- convert ibex site to emar site
				ON convert(varchar(10), i.site) = s.external_id
				AND s.entity = 'sites'
				AND s.vendor = 'pulsecheck'
        WHERE  type = 'AC'
        AND            LTRIM(ISNULL(misc, '')) != ''
 )
, UniqueRouteTemplateCombos AS (
        SELECT route_name, template_name
        FROM   IdxMappingData
        GROUP BY route_name, template_name
)
, DuplicatedRoutes AS (
        SELECT route_name
        FROM   UniqueRouteTemplateCombos
        GROUP BY route_name
        HAVING count(*) > 1
)
, NonSiteSpecificMappings AS (
        SELECT DISTINCT 
                       u.route_name, 
                       u.template_name,
                       internal_site_id = CONVERT(int, NULL)
        FROM   UniqueRouteTemplateCombos u
        LEFT JOIN DuplicatedRoutes d
                       ON u.route_name = d.route_name
        WHERE  d.route_name IS NULL
)
, SiteSpecificMappings AS (
        SELECT DISTINCT 
                       i.route_name, 
                       i.template_name,
                       i.internal_site_id
        FROM	IdxMappingData i
        JOIN	DuplicatedRoutes d
				ON i.route_name = d.route_name
)
, SourceMappings AS (
        SELECT 'Give' as action, * FROM NonSiteSpecificMappings 
			UNION
        SELECT 'Give' as action, * FROM SiteSpecificMappings
			UNION
		SELECT * FROM NonRouteSpecificMappings
)
, src AS (
	SELECT	
			a.id as action_id
			,r.id as medication_route_id
			,t.id as template_id
			,m.internal_site_id
	FROM	SourceMappings m
	JOIN	dbo.templates t
			ON m.template_name = t.name
	JOIN	dbo.actions a
			ON m.action = a.name
	LEFT JOIN	dbo.medication_routes r
			ON m.route_name = r.name
	WHERE	r.name IS NOT NULL OR m.route_name IS NULL
)
MERGE INTO [dbo].action_route_templates tar
USING src
	ON tar.action_id = src.action_id
	AND ISNULL(tar.medication_route_id, - 1) = ISNULL(src.medication_route_id, -1)
	AND ISNULL(tar.site_id, - 1) = ISNULL(src.internal_site_id, - 1)
WHEN NOT MATCHED THEN
	INSERT	(action_id, medication_route_id, site_id, template_id)
	VALUES	(action_id, medication_route_id, internal_site_id, template_id)
WHEN MATCHED AND tar.template_id != src.template_id THEN
	UPDATE SET template_id = src.template_id;

DECLARE @GiveAtId bigint = (
SELECT p.id 
  FROM dbo.prompts p
  left join dbo.prompt_groups g on (g.id=p.prompt_group_id)
  where g.name='GenericGive' and p.prompt='Given At'
  );
UPDATE dbo.templates
set event_datetime_prompt_id=@GiveAtId
WHERE save_button_text='Give'

DECLARE @RescheduleToId bigint = (
SELECT p.id 
  FROM dbo.prompts p
  left join dbo.prompt_groups g on (g.id=p.prompt_group_id)
  WHERE g.name='RescheduleDetails' AND p.prompt= 'Reschedule to'
  );
UPDATE dbo.templates
set event_datetime_prompt_id=@RescheduleToId
WHERE name='Reschedule'
