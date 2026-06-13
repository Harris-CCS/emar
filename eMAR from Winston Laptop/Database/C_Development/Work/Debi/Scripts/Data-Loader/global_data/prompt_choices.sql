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

, ('NasalMedication', 'Site ~~(immunization)', 1, 'Left nare')
, ('NasalMedication', 'Site ~~(immunization)', 2, 'Right nare')
, ('NasalMedication', 'Site ~~(immunization)', 3, 'Bilaterally nares')
, ('NasalMedication', 'Site ~~(immunization)', 4, 'Left Nare Immunization')
, ('NasalMedication', 'Site ~~(immunization)', 5, 'Right Nare Immunization')
, ('NasalMedication', 'Site ~~(immunization)', 6, 'Bilaterally Nares Immunization')
, 
, ('EnteralMedication', 'Site', 1, 'G-tube')
, ('EnteralMedication', 'Site', 2, 'J-tube')
, ('EnteralMedication', 'Site', 3, 'NG tube')
, ('EnteralMedication', 'Site', 4, 'Orogastric tube')

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

, ('IntraDermMedication', 'Site', 1, 'Left chest')
, ('IntraDermMedication', 'Site', 2, 'Right chest')
, ('IntraDermMedication', 'Site', 3, 'Left forearm')
, ('IntraDermMedication', 'Site', 4, 'Right forearm')
, ('IntraDermMedication', 'Site', 5, 'Left upper back')
, ('IntraDermMedication', 'Site', 6, 'Right upper back')
, ('IntraDermMedication', 'Site', 7, 'Left abdomen')
, ('IntraDermMedication', 'Site', 8, 'Right abdomen')
, ('IntraDermMedication', 'Site', 9, 'Other IV sites')
, 
, ('IntraOssMedication', 'Site', 1, 'Left proximal tibia')
, ('IntraOssMedication', 'Site', 2, 'Right proximal tibia')
, ('IntraOssMedication', 'Site', 3, 'Left distal tibia')
, ('IntraOssMedication', 'Site', 4, 'Right distal tibia')
, ('IntraOssMedication', 'Site', 5, 'Left femur')
, ('IntraOssMedication', 'Site', 6, 'Right femur')
, ('IntraOssMedication', 'Site', 7, 'Sternal')
, ('IntraOssMedication', 'Site', 8, 'Other IV sites')

, ('Assessment', 'O2 Sat', 1, '100%')
, ('Assessment', 'O2 Sat', 2, '99%')
, ('Assessment', 'O2 Sat', 3, '98%')
, ('Assessment', 'O2 Sat', 4, '97%')
, ('Assessment', 'O2 Sat', 5, '96%')
, ('Assessment', 'O2 Sat', 6, '95%')
, ('Assessment', 'O2 Sat', 7, '94%')
, ('Assessment', 'O2 Sat', 8, '93%')
, ('Assessment', 'O2 Sat', 9, '92%')
, ('Assessment', 'O2 Sat', 10, '91%')
, ('Assessment', 'O2 Sat', 11, '90%')
, ('Assessment', 'O2 Sat', 12, '89%')
, ('Assessment', 'O2 Sat', 13, '88%')
, ('Assessment', 'O2 Sat', 14, '87%')
, ('Assessment', 'O2 Sat', 15, '86%')
, ('Assessment', 'O2 Sat', 16, '85%')
, ('Assessment', 'O2 Sat', 17, '84%')
, ('Assessment', 'O2 Sat', 18, '83%')
, ('Assessment', 'O2 Sat', 19, '82%')
, ('Assessment', 'O2 Sat', 20, '81%')
, ('Assessment', 'O2 Sat', 21, '80%')
, ('Assessment', 'O2 Sat', 22, '<80%')

, ('Assessment', 'O2 Amount', 1, 'R.A')
, ('Assessment', 'O2 Amount', 2, '0.5L')
, ('Assessment', 'O2 Amount', 3, '1L')
, ('Assessment', 'O2 Amount', 4, '2L')
, ('Assessment', 'O2 Amount', 5, '3L')
, ('Assessment', 'O2 Amount', 6, '4L')
, ('Assessment', 'O2 Amount', 7, '5L')
, ('Assessment', 'O2 Amount', 8, '6L')
, ('Assessment', 'O2 Amount', 9, '40%')
, ('Assessment', 'O2 Amount', 10, '50%')
, ('Assessment', 'O2 Amount', 11, '60%')
, ('Assessment', 'O2 Amount', 12, '80%')
, ('Assessment', 'O2 Amount', 13, '100%')

, ('Assessment', 'O2 Type', 1, 'Room air')
, ('Assessment', 'O2 Type', 2, 'On oxygen')

, ('InhalationAssessment', 'Rhythm', 1, 'Normal Sinus')
, ('InhalationAssessment', 'Rhythm', 2, 'Atrial Fibrillation')
, ('InhalationAssessment', 'Rhythm', 3, 'Artial Flutter')
, ('InhalationAssessment', 'Rhythm', 4, 'Artial Tachycardia')
, ('InhalationAssessment', 'Rhythm', 5, 'Paced')
, ('InhalationAssessment', 'Rhythm', 6, 'PSVT')
, ('InhalationAssessment', 'Rhythm', 7, 'Sinus Bradycardia')
, ('InhalationAssessment', 'Rhythm', 8, 'Sinus Tachycardia')
, ('InhalationAssessment', 'Rhythm', 9, '1 degree AV Block')
, ('InhalationAssessment', 'Rhythm', 10, '2 degree AV Block Type I')
, ('InhalationAssessment', 'Rhythm', 11, '2 degree AV Block Type II')
, ('InhalationAssessment', 'Rhythm', 12, '3 degree AV Block')
, ('InhalationAssessment', 'Rhythm', 13, 'Junctional')
, ('InhalationAssessment', 'Rhythm', 14, 'Verticular Tachycardia')
, ('InhalationAssessment', 'Rhythm', 15, 'Verticular Fibrillation')
, ('InhalationAssessment', 'Rhythm', 16, 'PEA')
, ('InhalationAssessment', 'Rhythm', 17, 'Asystole')
, ('InhalationAssessment', 'Rhythm', 18, 'Agonal')

, ('InhalationAssessment', 'Ectopy', 1, 'UNI PVCs')
, ('InhalationAssessment', 'Ectopy', 2, 'Multi PVCs')
, ('InhalationAssessment', 'Ectopy', 3, 'Couplets')
, ('InhalationAssessment', 'Ectopy', 4, 'Frequent PVCs')
, ('InhalationAssessment', 'Ectopy', 5, 'Infrequent PVCs')
, ('InhalationAssessment', 'Ectopy', 6, 'PJCs')
, ('InhalationAssessment', 'Ectopy', 7, 'PACs')
, ('InhalationAssessment', 'Ectopy', 8, 'Bigeminy')
, ('InhalationAssessment', 'Ectopy', 9, 'Trigeminy')
, ('InhalationAssessment', 'Ectopy', 10, 'Aberrant')

, ('InhalationAssessment', 'St Changes', 1, 'None')
, ('InhalationAssessment', 'St Changes', 2, 'Elevation')
, ('InhalationAssessment', 'St Changes', 3, 'Depression')

, ('IntraOssAssessment', 'O2 Sat', 1, '100%')
, ('IntraOssAssessment', 'O2 Sat', 2, '99%')
, ('IntraOssAssessment', 'O2 Sat', 3, '98%')
, ('IntraOssAssessment', 'O2 Sat', 4, '97%')
, ('IntraOssAssessment', 'O2 Sat', 5, '96%')
, ('IntraOssAssessment', 'O2 Sat', 6, '95%')
, ('IntraOssAssessment', 'O2 Sat', 7, '94%')
, ('IntraOssAssessment', 'O2 Sat', 8, '93%')
, ('IntraOssAssessment', 'O2 Sat', 9, '92%')
, ('IntraOssAssessment', 'O2 Sat', 10, '91%')
, ('IntraOssAssessment', 'O2 Sat', 11, '90%')
, ('IntraOssAssessment', 'O2 Sat', 12, '89%')
, ('IntraOssAssessment', 'O2 Sat', 13, '88%')
, ('IntraOssAssessment', 'O2 Sat', 14, '87%')
, ('IntraOssAssessment', 'O2 Sat', 15, '86%')
, ('IntraOssAssessment', 'O2 Sat', 16, '85%')
, ('IntraOssAssessment', 'O2 Sat', 17, '84%')
, ('IntraOssAssessment', 'O2 Sat', 18, '83%')
, ('IntraOssAssessment', 'O2 Sat', 19, '82%')
, ('IntraOssAssessment', 'O2 Sat', 20, '81%')
, ('IntraOssAssessment', 'O2 Sat', 21, '80%')
, ('IntraOssAssessment', 'O2 Sat', 22, '<80%')
					
, ('IntraOssAssessment', 'O2 Amount', 1, 'R.A')
, ('IntraOssAssessment', 'O2 Amount', 2, '0.5L')
, ('IntraOssAssessment', 'O2 Amount', 3, '1L')
, ('IntraOssAssessment', 'O2 Amount', 4, '2L')
, ('IntraOssAssessment', 'O2 Amount', 5, '3L')
, ('IntraOssAssessment', 'O2 Amount', 6, '4L')
, ('IntraOssAssessment', 'O2 Amount', 7, '5L')
, ('IntraOssAssessment', 'O2 Amount', 8, '6L')
, ('IntraOssAssessment', 'O2 Amount', 9, '40%')
, ('IntraOssAssessment', 'O2 Amount', 10, '50%')
, ('IntraOssAssessment', 'O2 Amount', 11, '60%')
, ('IntraOssAssessment', 'O2 Amount', 12, '80%')
, ('IntraOssAssessment', 'O2 Amount', 13, '100%')
			
, ('IntraOssAssessment', 'O2 Type', 1, 'Room air')
, ('IntraOssAssessment', 'O2 Type', 2, 'On oxygen')
			
, ('IntraOssAssessment', 'Rhythm', 1, 'Normal Sinus')
, ('IntraOssAssessment', 'Rhythm', 2, 'Atrial Fibrillation')
, ('IntraOssAssessment', 'Rhythm', 3, 'Artial Flutter')
, ('IntraOssAssessment', 'Rhythm', 4, 'Artial Tachycardia')
, ('IntraOssAssessment', 'Rhythm', 5, 'Paced')
, ('IntraOssAssessment', 'Rhythm', 6, 'PSVT')
, ('IntraOssAssessment', 'Rhythm', 7, 'Sinus Bradycardia')
, ('IntraOssAssessment', 'Rhythm', 8, 'Sinus Tachycardia')
, ('IntraOssAssessment', 'Rhythm', 9, '1 degree AV Block')
, ('IntraOssAssessment', 'Rhythm', 10, '2 degree AV Block Type I')
, ('IntraOssAssessment', 'Rhythm', 11, '2 degree AV Block Type II')
, ('IntraOssAssessment', 'Rhythm', 12, '3 degree AV Block')
, ('IntraOssAssessment', 'Rhythm', 13, 'Junctional')
, ('IntraOssAssessment', 'Rhythm', 14, 'Verticular Tachycardia')
, ('IntraOssAssessment', 'Rhythm', 15, 'Verticular Fibrillation')
, ('IntraOssAssessment', 'Rhythm', 16, 'PEA')
, ('IntraOssAssessment', 'Rhythm', 17, 'Asystole')
, ('IntraOssAssessment', 'Rhythm', 18, 'Agonal')
			
, ('IntraOssAssessment', 'Ectopy', 1, 'UNI PVCs')
, ('IntraOssAssessment', 'Ectopy', 2, 'Multi PVCs')
, ('IntraOssAssessment', 'Ectopy', 3, 'Couplets')
, ('IntraOssAssessment', 'Ectopy', 4, 'Frequent PVCs')
, ('IntraOssAssessment', 'Ectopy', 5, 'Infrequent PVCs')
, ('IntraOssAssessment', 'Ectopy', 6, 'PJCs')
, ('IntraOssAssessment', 'Ectopy', 7, 'PACs')
, ('IntraOssAssessment', 'Ectopy', 8, 'Bigeminy')
, ('IntraOssAssessment', 'Ectopy', 9, 'Trigeminy')
, ('IntraOssAssessment', 'Ectopy', 10, 'Aberrant')
			
, ('IntraOssAssessment', 'St Changes', 1, 'None')
, ('IntraOssAssessment', 'St Changes', 2, 'Elevation')
, ('IntraOssAssessment', 'St Changes', 3, 'Depression')

, ('InhalationMedication', 'Administered Via', 1, 'Single dose nebulizer')
, ('InhalationMedication', 'Administered Via', 2, 'Continuous nebulizer')
, ('InhalationMedication', 'Administered Via', 3, 'MDI')
, ('InhalationMedication', 'Administered Via', 4, 'MDI with spacer')

, ('Medication', 'All of the above', 0, 'Correct patient, time, route, dose and medication confirmed prior to administration')
, ('Medication', 'All of the above', 0, 'Patient advised of actions and side-effects prior to administration')
, ('Medication', 'All of the above', 0, 'Allergies confirmed and medications reviewed prior to administration')

, ('EnteralMedication', 'All of the above', 0, 'Tube position confirmed via aspiration prior to administration')
, ('EnteralMedication', 'All of the above', 0, 'Correct patient, time, route, dose and medication confirmed prior to administration')
, ('EnteralMedication', 'All of the above', 0, 'Patient advised of actions and side-effects prior to administration')
, ('EnteralMedication', 'All of the above', 0, 'Allergies confirmed and medications reviewed prior to administration')
, ('EnteralMedication', 'All of the above', 0, 'Flushed with water after administration')

, ('Safety', 'All of the above', 0, 'Patient in position of comfort')
, ('Safety', 'All of the above', 0, 'Side rails up')
, ('Safety', 'All of the above', 0, 'Cart in lowest position')
, ('Safety', 'All of the above', 0, 'Family at bedside')

, ('AmbulateSafety', 'All of the above', 0, 'Advised not to ambulate without assistance')
, ('AmbulateSafety', 'All of the above', 0, 'Patient in position of comfort')
, ('AmbulateSafety', 'All of the above', 0, 'Side rails up')
, ('AmbulateSafety', 'All of the above', 0, 'Cart in lowest position')
, ('AmbulateSafety', 'All of the above', 0, 'Family at bedside')

, ('IntraMuscMedication', 'IM (Not an antibiotic or immunization)', 0, 'Site (non-antibiotic/immunization)')
, ('IntraMuscMedication', 'IM (Not an antibiotic or immunization)', 0, 'Amount given (non-antibiotic/immunization)')
, ('IntraMuscMedication', 'IM (Not an antibiotic or immunization)', 0, 'Combined with (non-antibiotic/immunization)')

, ('IntraMuscMedication', 'IM antibiotic', 0, 'Site (antibiotic)')
, ('IntraMuscMedication', 'IM antibiotic', 0, 'Amount given (antibiotic)')
, ('IntraMuscMedication', 'IM antibiotic', 0, 'Combined with (antibiotic)')

, ('IntraMuscMedication', 'IM immunization', 0, 'Site (immunization)')
, ('IntraMuscMedication', 'IM immunization', 0, 'Amount given (immunization)')
, ('IntraMuscMedication', 'IM immunization', 0, 'unit')
, ('IntraMuscMedication', 'IM immunization', 0, 'Combined with (immunization)')
, ('IntraMuscMedication', 'IM immunization', 0, 'Vaccination information sheet given to patient')
, ('IntraMuscMedication', 'IM immunization', 0, 'Date of publication')
, ('IntraMuscMedication', 'IM immunization', 0, 'Name of publication')
, ('IntraMuscMedication', 'IM immunization', 0, 'Manufacturer')
, ('IntraMuscMedication', 'IM immunization', 0, 'Lot number')
, ('IntraMuscMedication', 'IM immunization', 0, 'Expiration')

, ('Assessment', 'All of the above', 0, 'Correct patient, time, route, dose and medication confirmed prior to administration')
, ('Assessment', 'All of the above', 0, 'Patient advised of actions and side-effects prior to administration')
, ('Assessment', 'All of the above', 0, 'Allergies confirmed and medications reviewed prior to administration')

, ('OralMedication', 'All of the above', 0, 'Correct patient, time, route, dose and medication confirmed prior to administration')
, ('OralMedication', 'All of the above', 0, 'Patient advised of actions and side-effects prior to administration')
, ('OralMedication', 'All of the above', 0, 'Allergies confirmed and medications reviewed prior to administration')

, ('InhalationAssessment', 'All of the above', 0, 'Correct patient, time, route, dose and medication confirmed prior to administration')
, ('InhalationAssessment', 'All of the above', 0, 'Patient advised of actions and side-effects prior to administration')
, ('InhalationAssessment', 'All of the above', 0, 'Allergies confirmed and medications reviewed prior to administration')

, ('NasalMedication', 'All of the above', 0, 'Instructed to blow nose prior to administration')
, ('NasalMedication', 'All of the above', 0, 'Correct patient, time, route, dose and medication confirmed prior to administration')
, ('NasalMedication', 'All of the above', 0, 'Patient advised of actions and side-effects prior to administration')
, ('NasalMedication', 'All of the above', 0, 'Allergies confirmed and medications reviewed prior to administration')
  
, ('IntraDermMedication', 'All of the above', 0, 'Correct patient, time, route, dose and medication confirmed prior to administration')
, ('IntraDermMedication', 'All of the above', 0, 'Patient advised of actions and side-effects prior to administration')
, ('IntraDermMedication', 'All of the above', 0, 'Allergies confirmed and medications reviewed prior to administration')
  
, ('IntraOssAssessment', 'All of the above', 0, 'Needle placement confirmed via aspiration prior to administration')
, ('IntraOssAssessment', 'All of the above', 0, 'Correct patient, time, route, dose and medication confirmed prior to administration')
, ('IntraOssAssessment', 'All of the above', 0, 'Patient advised of actions and side-effects prior to administration')
, ('IntraOssAssessment', 'All of the above', 0, 'Allergies confirmed and medications reviewed prior to administration')
  
, ('RectalMedication', 'All of the above', 0, 'Correct patient, time, route, dose and medication confirmed prior to administration')
, ('RectalMedication', 'All of the above', 0, 'Patient advised of actions and side-effects prior to administration')
, ('RectalMedication', 'All of the above', 0, 'Allergies confirmed and medications reviewed prior to administration')
  
, ('TransDermMedication', 'All of the above', 0, 'Skin cleansed prior to administration')
, ('TransDermMedication', 'All of the above', 0, 'Shaving required prior to administration')
, ('TransDermMedication', 'All of the above', 0, 'Correct patient, time, route, dose and medication confirmed prior to administration')
, ('TransDermMedication', 'All of the above', 0, 'Patient advised of actions and side-effects prior to administration')
, ('TransDermMedication', 'All of the above', 0, 'Allergies confirmed and medications reviewed prior to administration')
  
, ('VaginalMedication', 'All of the above', 0, 'Correct patient, time, route, dose and medication confirmed prior to administration')
, ('VaginalMedication', 'All of the above', 0, 'Patient advised of actions and side-effects prior to administration')
, ('VaginalMedication', 'All of the above', 0, 'Allergies confirmed and medications reviewed prior to administration')
  
, ('SubcutanMedication', 'All of the above', 0, 'Correct patient, time, route, dose and medication confirmed prior to administration')
, ('SubcutanMedication', 'All of the above', 0, 'Patient advised of actions and side-effects prior to administration')
, ('SubcutanMedication', 'All of the above', 0, 'Allergies confirmed and medications reviewed prior to administration')
			
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