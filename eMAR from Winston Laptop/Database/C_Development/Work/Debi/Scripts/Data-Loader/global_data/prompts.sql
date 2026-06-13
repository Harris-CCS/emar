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
		  , [placeholder_text]  [varchar](100) null
	      , [display_child_prompts_value] [varchar](100) null);
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
  , [placeholder_text]
  , [display_child_prompts_value]
)
select
    [prompt_group_name]
  , [sequence]
  , [prompt]
  , [is_active]
  , [prompt_type]
  , [prompt_default]
  , [required]
  , [placeholder_text]
  , [display_child_prompts_value]
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

  , ('InhalationMedication', 1, 'Verbal order read back and verified', 1, 'CheckBox', NULL, 0, NULL, NULL)
  , ('InhalationMedication', 2, 'Amount given', 1, 'FreeText', NULL, 1, NULL, NULL)
  , ('InhalationMedication', 3, 'Amount wasted', 1, 'FreeText', NULL, 1, NULL, NULL)
  , ('InhalationMedication', 4, 'Administration of this medication is documented elsewhere in chart', 1, 'CheckBox', NULL, 0, NULL, NULL)
  , ('InhalationMedication', 5, 'Administered Via', 1, 'DropDownListBox', NULL, 1, NULL, NULL)
  
  , ('InhalationMedication', 6, 'Medication combined for administration with', 1, 'CheckBox', NULL, 0, NULL, 'true')
  , ('InhalationMedication', 7, 'Albuterol Dose', 1, 'CheckBox', NULL, 0, NULL, NULL)
  , ('InhalationMedication', 8, 'Atrovent Dose', 1, 'CheckBox', NULL, 0, NULL, NULL)
  , ('InhalationMedication', 9, 'Xopenex Dose', 1, 'CheckBox', NULL, 0, NULL, NULL)
  , ('InhalationMedication', 10, 'Combivent Dose', 1, 'CheckBox', NULL, 0, NULL, NULL)
  , ('InhalationMedication', 11, 'Racemic Epinephrine Dose', 1, 'CheckBox', NULL, 0, NULL, NULL)
  
  , ('InhalationMedication', 12, 'With oxygen', 1, 'CheckBox', NULL, 0, NULL, NULL)
  , ('InhalationMedication', 13, 'With air', 1, 'CheckBox', NULL, 0, NULL, NULL)
  
  , ('NasalMedication', 1, 'Verbal order read back and verified', 1, 'CheckBox', NULL, 0, NULL, NULL)
  
  , ('NasalMedication', 2, 'Site ~~(immunization)', 1, 'DropDownListBox', NULL, 1, NULL, 'true')
  , ('NasalMedication', 3, 'Amount given', 1, 'FreeText', NULL, 0, NULL, NULL)
  , ('NasalMedication', 4, 'Vaccination information sheet given to patient', 1, 'CheckBox', NULL, 0, NULL, NULL)
  , ('NasalMedication', 5, 'Date of publication', 1, 'Date', NULL, 0, NULL, NULL)
  , ('NasalMedication', 6, 'Name of publication', 1, 'FreeText', NULL, 0, NULL, NULL)
  , ('NasalMedication', 7, 'Manufacturer', 1, 'FreeText', NULL, 0, NULL, NULL)
  , ('NasalMedication', 8, 'Lot number', 1, 'FreeText', NULL, 0, NULL, NULL)
  , ('NasalMedication', 9, 'Expiration', 1, 'Date', NULL, 0, NULL, NULL),
  
  , ('NasalMedication', 10, 'Instructed to blow nose prior to administration', 1, 'CheckBox', NULL, 0, NULL, NULL)
  , ('NasalMedication', 11, 'Correct patient, time, route, dose and medication confirmed prior to administration', 1, 'CheckBox', NULL, 0, NULL, NULL)
  , ('NasalMedication', 12, 'Patient advised of actions and side-effects prior to administration', 1, 'CheckBox', NULL, 0, NULL, NULL)
  , ('NasalMedication', 13, 'Allergies confirmed and medications reviewed prior to administration', 1, 'CheckBox', NULL, 0, NULL, NULL)
  , ('NasalMedication', 14, 'All of the above', 1, 'CheckBox', NULL, 0, NULL, NULL)
	
  , ('EnteralMedication', 1, 'Verbal order read back and verified', 1, 'CheckBox', NULL, 0, NULL, NULL)
  , ('EnteralMedication', 2, 'Amount Given', 1, 'FreeText', NULL, 1, NULL, NULL)
  , ('EnteralMedication', 3, 'Medication combined for administration with', 1, 'FreeText', NULL, 1, NULL, NULL)
  , ('EnteralMedication', 4, 'Administration of this medication is documented elsewhere in chart', 1, 'CheckBox', NULL, 0, NULL, NULL)
  , ('EnteralMedication', 5, 'Site', 1, 'DropDownListBox', NULL, 1, NULL, NULL)
  , ('EnteralMedication', 6, 'Tube position confirmed via aspiration prior to administration', 1, 'CheckBox', NULL, 0, NULL, NULL)
  , ('EnteralMedication', 7, 'Correct patient, time, route, dose and medication confirmed prior to administration', 1, 'CheckBox', NULL, 0, NULL, NULL)
  , ('EnteralMedication', 8, 'Patient advised of actions and side-effects prior to administration', 1, 'CheckBox', NULL, 0, NULL, NULL)
  , ('EnteralMedication', 9, 'Allergies confirmed and medications reviewed prior to administration', 1, 'CheckBox', NULL, 0, NULL, NULL)
  , ('EnteralMedication', 10, 'Flushed with water after administration', 1, 'CheckBox', NULL, 0, NULL, NULL)
  , ('EnteralMedication', 11, 'All of the above', 1, 'CheckBox', NULL, 0, NULL, NULL)
  
  , ('IntraDermMedication', 1, 'Verbal order read back and verified', 1, 'CheckBox', NULL, 0, NULL, NULL)
  , ('IntraDermMedication', 2, 'Amount given', 1, 'FreeText', NULL, 1, NULL, NULL)
  , ('IntraDermMedication', 3, 'Administration of this medication is documented elsewhere in chart', 1, 'CheckBox', NULL, 0, NULL, NULL)
  , ('IntraDermMedication', 4, 'Site', 1, 'DropDownListBox', NULL, 1, NULL, NULL)
  , ('IntraDermMedication', 5, 'Manufacturer', 1, 'FreeText', NULL, 1, NULL, NULL)
  , ('IntraDermMedication', 6, 'Lot number', 1, 'FreeText', NULL, 1, NULL, NULL)
  , ('IntraDermMedication', 7, 'Expiration', 1, 'Date', NULL, 1, NULL, NULL)
  , ('IntraDermMedication', 8, 'Correct patient, time, route, dose and medication confirmed prior to administration', 1, 'CheckBox', NULL, 0, NULL, NULL)
  , ('IntraDermMedication', 9, 'Patient advised of actions and side-effects prior to administration', 1, 'CheckBox', NULL, 0, NULL, NULL)
  , ('IntraDermMedication', 10, 'Allergies confirmed and medications reviewed prior to administration', 1, 'CheckBox', NULL, 0, NULL, NULL)
  , ('IntraDermMedication', 11, 'All of the above', 1, 'CheckBox', NULL, 0, NULL, NULL)
  
  , ('IntraOssMedication', 1, 'Verbal order read back and verified', 1, 'CheckBox', NULL, 0, NULL, NULL)
  , ('IntraOssMedication', 2, 'Amount given', 1, 'FreeText', NULL, 1, NULL, NULL)
  , ('IntraOssMedication', 3, 'Administration of this medication is documented elsewhere in chart', 1, 'CheckBox', NULL, 0, NULL, NULL)
  , ('IntraOssMedication', 4, 'Site', 1, 'DropDownListBox', NULL, 1, NULL, NULL)
	
  , ('RectalMedication', 1, 'Verbal order read back and verified', 1, 'CheckBox', NULL, 0, NULL, NULL)
  , ('RectalMedication', 2, 'Medication administered rectally', 1, 'CheckBox', NULL, 1, NULL, NULL)
  , ('RectalMedication', 3, 'Amount given', 1, 'FreeText', NULL, 1, NULL, NULL)
  , ('RectalMedication', 4, 'Amount wasted', 1, 'FreeText', NULL, 1, NULL, NULL)
  , ('RectalMedication', 5, 'Administration of this medication is documented elsewhere in chart', 1, 'CheckBox', NULL, 0, NULL, NULL)
  , ('RectalMedication', 6, 'Patient administered medication after instruction by staff on correct technique', 1, 'CheckBox', NULL, 1, NULL, NULL)
  , ('RectalMedication', 7, 'Lubricant used for administration', 1, 'CheckBox', NULL, 1, NULL, NULL)
  , ('RectalMedication', 8, 'Medication retained after administration', 1, 'CheckBox', NULL, 1, NULL, NULL)	
  , ('RectalMedication', 9, 'Correct patient, time, route, dose and medication confirmed prior to administration', 1, 'CheckBox', NULL, 0, NULL, NULL)
  , ('RectalMedication', 10, 'Patient advised of actions and side-effects prior to administration', 1, 'CheckBox', NULL, 0, NULL, NULL)
  , ('RectalMedication', 11, 'Allergies confirmed and medications reviewed prior to administration', 1, 'CheckBox', NULL, 0, NULL, NULL)
  , ('RectalMedication', 12, 'All of the above', 1, 'CheckBox', NULL, 0, NULL, NULL)
  
  , ('TransDermMedication', 1, 'Verbal order read back and verified', 1, 'CheckBox', NULL, 0, NULL, NULL)
  , ('TransDermMedication', 2, 'Medication applied transdermally topically', 1, 'CheckBox', NULL, 0, NULL, NULL)
  , ('TransDermMedication', 3, 'Amount Given', 1, 'FreeText', NULL, 1, NULL, NULL)
  , ('TransDermMedication', 4, 'Administration of this medication is documented elsewhere in chart', 1, 'CheckBox', NULL, 0, NULL, NULL)
  , ('TransDermMedication', 5, 'Site', 1, 'FreeText', NULL, 1, NULL, NULL)
  , ('TransDermMedication', 6, 'Skin cleansed prior to administration', 1, 'FreeText', NULL, 1, NULL, NULL)
  , ('TransDermMedication', 7, 'Shaving required prior to administration', 1, 'FreeText', NULL, 1, NULL, NULL)
  , ('TransDermMedication', 8, 'Correct patient, time, route, dose and medication confirmed prior to administration', 1, 'CheckBox', NULL, 0, NULL, NULL)
  , ('TransDermMedication', 9, 'Patient advised of actions and side-effects prior to administration', 1, 'CheckBox', NULL, 0, NULL, NULL)
  , ('TransDermMedication', 10, 'Allergies confirmed and medications reviewed prior to administration', 1, 'CheckBox', NULL, 0, NULL, NULL)
  , ('TransDermMedication', 11, 'All of the above', 1, 'CheckBox', NULL, 0, NULL, NULL)
  
  , ('VaginalMedication', 1, 'Medication given vaginally by', 1, 'CheckBox', NULL, 1, NULL, NULL)
  , ('VaginalMedication', 2, 'Time given', 1, 'FreeText', NULL, 1, NULL, NULL)
  , ('VaginalMedication', 3, 'Verbal order read back and verified', 1, 'CheckBox', NULL, 0, NULL, NULL)
  , ('VaginalMedication', 4, 'Amount given', 1, 'FreeText', NULL, 1, NULL, NULL)
  , ('VaginalMedication', 5, 'Administration of this medication is documented elsewhere in chart', 1, 'CheckBox', NULL, 0, NULL, NULL)
  , ('VaginalMedication', 6, 'Patient administered medication after instruction by staff on correct technique', 1, 'CheckBox', NULL, 1, NULL, NULL)
  , ('VaginalMedication', 7, 'Patient voided prior to administration', 1, 'CheckBox', NULL, 1, NULL, NULL)
  , ('VaginalMedication', 8, 'Lubricant used for administration', 1, 'CheckBox', NULL, 1, NULL, NULL)
  , ('VaginalMedication', 9, 'Medication retained after administration', 1, 'CheckBox', NULL, 1, NULL, NULL)	
  , ('VaginalMedication', 10, 'Correct patient, time, route, dose and medication confirmed prior to administration', 1, 'CheckBox', NULL, 0, NULL, NULL)
  , ('VaginalMedication', 11, 'Patient advised of actions and side-effects prior to administration', 1, 'CheckBox', NULL, 0, NULL, NULL)
  , ('VaginalMedication', 12, 'Allergies confirmed and medications reviewed prior to administration', 1, 'CheckBox', NULL, 0, NULL, NULL)
  , ('VaginalMedication', 13, 'All of the above', 1, 'CheckBox', NULL, 0, NULL, NULL)
  
  , ('SubcutanMedication', 1, 'Verbal order read back and verified', 1, 'CheckBox', NULL, 0, NULL, NULL)
  , ('SubcutanMedication', 2, 'Amount given', 1, 'FreeText', NULL, 1, NULL, NULL)
  , ('SubcutanMedication', 3, 'Administration of this medication is documented elsewhere in chart', 1, 'CheckBox', NULL, 0, NULL, NULL)
  , ('SubcutanMedication', 4, 'Site', 1, 'DropDownListBox', NULL, 1, NULL, NULL)
  , ('SubcutanMedication', 5, 'Vaccination information sheet given to patient', 1, 'CheckBox', NULL, 1, NULL, NULL)
  , ('SubcutanMedication', 6, 'Manufacturer', 1, 'FreeText', NULL, 1, NULL, NULL)
  , ('SubcutanMedication', 7, 'Lot number', 1, 'FreeText', NULL, 1, NULL, NULL)
  , ('SubcutanMedication', 8, 'Expiration', 1, 'Date', NULL, 1, NULL, NULL)
  , ('SubcutanMedication', 9, 'Correct patient, time, route, dose and medication confirmed prior to administration', 1, 'CheckBox', NULL, 0, NULL, NULL)
  , ('SubcutanMedication', 10, 'Patient advised of actions and side-effects prior to administration', 1, 'CheckBox', NULL, 0, NULL, NULL)
  , ('SubcutanMedication', 11, 'Allergies confirmed and medications reviewed prior to administration', 1, 'CheckBox', NULL, 0, NULL, NULL)
  , ('SubcutanMedication', 12, 'All of the above', 1, 'CheckBox', NULL, 0, NULL, NULL)
  
  , ('IVMedication', 1, 'Verbal order read back and verified', 1, 'CheckBox', NULL, 0, NULL, NULL)
  , ('IVMedication', 2, 'Amount given', 1, 'FreeText', NULL, 0, NULL, NULL)
  , ('IVMedication', 3, '1st IV Location', 1, 'DropDownListBox', NULL, 0, NULL, true)				
  ,-- ('IVMedication', 2, 'IV fluids ~~(1st IV Location)', 1, 'CheckBox', NULL, 1, NULL, 'true')
  ,-- ('IVMedication', 4, 'IVPs ~~(1st IV Location)', 1, 'CheckBox', NULL, 0, NULL, NULL)
  ,-- ('IVMedication', 5, 'Added to existing IV Fluids ~~(1st IV Location)', 1, 'CheckBox', NULL, 0, NULL, NULL)
  ,-- ('IVMedication', 6, 'IVPB/drip ~~(1st IV Location)', 1, 'FreeText', NULL, 0, NULL, NULL)
  
  ,-- ('IVMedication', 3, '2nd IV Location', 1, 'DropDownListBox', NULL, 0, NULL, true)				
  ,-- ('IVMedication', 2, 'IV fluids ~~(2nd IV Location)', 1, 'CheckBox', NULL, 1, NULL, 'true')
  ,-- ('IVMedication', 4, 'IVPs ~~(2nd IV Location)', 1, 'CheckBox', NULL, 0, NULL, NULL)
  ,-- ('IVMedication', 5, 'Added to existing IV Fluids ~~(2nd IV Location)', 1, 'CheckBox', NULL, 0, NULL, NULL)
  ,-- ('IVMedication', 6, 'IVPB/drip ~~(2nd IV Location)', 1, 'FreeText', NULL, 0, NULL, NULL)
  
  ,-- ('IVMedication', 7, 'Manufacturer', 1, 'FreeText', NULL, 0, NULL, NULL)
  ,-- ('IVMedication', 8, 'Lot number', 1, 'FreeText', NULL, 0, NULL, NULL)
  ,-- ('IVMedication', 9, 'Expiration', 1, 'Date', NULL, 0, NULL, NULL)
			
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

  ,	('AmbulateSafety', 1, 'Advised not to ambulate without assistance', 1, 'CheckBox', NULL, 0, NULL, NULL)
  ,	('AmbulateSafety', 2, 'Patient in position of comfort', 1, 'CheckBox', NULL, 0, NULL, NULL)
  ,	('AmbulateSafety', 3, 'Side rails up', 1, 'CheckBox', NULL, 0, NULL, NULL)
  ,	('AmbulateSafety', 4, 'Cart in lowest position', 1, 'CheckBox', NULL, 0, NULL, NULL)
  ,	('AmbulateSafety', 5, 'Family at bedside', 1, 'CheckBox', NULL, 0, NULL, NULL)
  ,	('AmbulateSafety', 6, 'All of the above', 1, 'CheckBox', NULL, 0, NULL, NULL)
  ,	('AmbulateSafety', 7, 'Friend at bedside', 1, 'CheckBox', NULL, 0, NULL, NULL)
  ,	('AmbulateSafety', 8, 'Call light in reach', 1, 'CheckBox', NULL, 0, NULL, NULL)
  ,	('AmbulateSafety', 9, 'Other:', 1, 'MultiLineFreeText', NULL, 0, NULL, NULL)
  ,
  ,	('IVSafety', 1, 'Patient in position of comfort', 1, 'CheckBox', NULL, 0, NULL, NULL)
  ,	('IVSafety', 2, 'Side rails up', 1, 'CheckBox', NULL, 0, NULL, NULL)
  ,	('IVSafety', 3, 'Cart in lowest position', 1, 'CheckBox', NULL, 0, NULL, NULL)
  ,	('IVSafety', 4, 'Call light in reach', 1, 'CheckBox', NULL, 0, NULL, NULL)
  ,	('IVSafety', 5, 'All of the above', 1, 'CheckBox', NULL, 0, NULL, NULL)
  ,	('IVSafety', 6, 'Family at bedside', 1, 'CheckBox', NULL, 0, NULL, NULL)
  ,	('IVSafety', 7, 'Friend at bedside', 1, 'CheckBox', NULL, 0, NULL, NULL)
  ,	('IVSafety', 8, 'Other:', 1, 'MultiLineFreeText', NULL, 0, NULL, NULL)

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

  ,	('DefaultGive', 1, 'Decription', 1, 'MultiLineFreeText', NULL, 0, NULL, NULL)
  ,	('DefaultGive', 2, 'Administered by', 1, 'FreeText', NULL, 0, 'Myself', NULL)

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
  ,	('IntraMuscMedication', 14, 'Combined with ~~(immunization)', 1, 'FreeText', NULL, 0, NULL, NULL)
  ,	('IntraMuscMedication', 15, 'Vaccination information sheet given to patient', 1, 'CheckBox', NULL, 0, NULL, NULL)
  ,	('IntraMuscMedication', 16, 'Date of publication', 1, 'Date', NULL, 0, NULL, NULL)
  ,	('IntraMuscMedication', 17, 'Name of publication', 1, 'FreeText', NULL, 0, NULL, NULL)
  ,	('IntraMuscMedication', 18, 'Manufacturer', 1, 'FreeText', NULL, 0, NULL, NULL)
  ,	('IntraMuscMedication', 19, 'Lot number', 1, 'FreeText', NULL, 0, NULL, NULL)
  ,	('IntraMuscMedication', 20, 'Expiration', 1, 'Date', NULL, 0, NULL, NULL)

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

  , ('InhalationAssessment', 1, 'Peak-Flow prior', 1, 'CheckBox', NULL, 1, NULL, NULL)
  , ('InhalationAssessment', 2, 'O2 Sat', 1, 'DropDownListBox', NULL, 1, NULL, NULL)
  , ('InhalationAssessment', 3, 'O2 Amount', 1, 'DropDownListBox', NULL, 1, NULL, NULL)
  , ('InhalationAssessment', 4, 'O2 Type', 1, 'DropDownListBox', NULL, 1, NULL, NULL)
  , ('InhalationAssessment', 5, 'Rhythm', 1, 'DropDownListBox', NULL, 1, NULL, NULL)
  , ('InhalationAssessment', 6, 'Ectopy', 1, 'DropDownListBox', NULL, 1, NULL, NULL)
  , ('InhalationAssessment', 7, 'St Changes', 1, 'DropDownListBox', NULL, 1, NULL, NULL)
  , ('InhalationAssessment', 8, 'Correct patient, time, route, dose and medication confirmed prior to administration', 1, 'CheckBox', NULL, 0, NULL, NULL)
  , ('InhalationAssessment', 9, 'Patient advised of actions and side-effects prior to administration', 1, 'CheckBox', NULL, 0, NULL, NULL)
  , ('InhalationAssessment', 10, 'Allergies confirmed and medications reviewed prior to administration', 1, 'CheckBox', NULL, 0, NULL, NULL)
  , ('InhalationAssessment', 11, 'All of the above', 1, 'CheckBox', NULL, 0, NULL, NULL)

  ,	('IntraOssAssessment', 1, 'O2 Sat', 1, 'DropDownListBox', NULL, 1, NULL, NULL)
  ,	('IntraOssAssessment', 2, 'O2 Amount', 1, 'DropDownListBox', NULL, 1, NULL, NULL)
  ,	('IntraOssAssessment', 3, 'O2 Type', 1, 'DropDownListBox', NULL, 1, NULL, NULL)
  ,	('IntraOssAssessment', 4, 'Rhythm', 1, 'DropDownListBox', NULL, 1, NULL, NULL)
  ,	('IntraOssAssessment', 5, 'Ectopy', 1, 'DropDownListBox', NULL, 1, NULL, NULL)
  ,	('IntraOssAssessment', 6, 'St Changes', 1, 'DropDownListBox', NULL, 1, NULL, NULL)
  ,	('IntraOssAssessment', 7, 'Needle placement confirmed via aspiration prior to administration', 1, 'CheckBox', NULL, 0, NULL, NULL)
  ,	('IntraOssAssessment', 8, 'Correct patient, time, route, dose and medication confirmed prior to administration', 1, 'CheckBox', NULL, 0, NULL, NULL)
  ,	('IntraOssAssessment', 9, 'Patient advised of actions and side-effects prior to administration', 1, 'CheckBox', NULL, 0, NULL, NULL)
  ,	('IntraOssAssessment', 10, 'Allergies confirmed and medications reviewed prior to administration', 1, 'CheckBox', NULL, 0, NULL, NULL)
  ,	('IntraOssAssessment', 11, 'All of the above', 1, 'CheckBox', NULL, 0, NULL, NULL)

  ,	('IVAssessment', 1, 'Connections checked prior to administration', 1, 'CheckBox', NULL, 1, NULL, NULL)
  ,	('IVAssessment', 2, 'Line traced prior to administration', 1, 'CheckBox', NULL, 1, NULL, NULL)
  ,	('IVAssessment', 3, 'Catheter placement confirmed via flush prior to administration', 1, 'CheckBox', NULL, 1, NULL, NULL)
  ,	('IVAssessment', 4, 'IV site without s/sx of infiltration during medication administration', 1, 'CheckBox', NULL, 1, NULL, NULL)
  ,	('IVAssessment', 5, 'No swelling during administration', 1, 'CheckBox', NULL, 1, NULL, NULL)
  ,	('IVAssessment', 6, 'No drainage during administration', 1, 'CheckBox', NULL, 1, NULL, NULL)
  ,	('IVAssessment', 7, 'IV flushed after administration', 1, 'CheckBox', NULL, 0, NULL, NULL)
  ,	('IVAssessment', 8, 'Correct patient, time, route, dose and medication confirmed prior to administration', 1, 'CheckBox', NULL, 0, NULL, NULL)
  ,	('IVAssessment', 9, 'Patient advised of actions and side-effects prior to administration', 1, 'CheckBox', NULL, 0, NULL, NULL)
  ,	('IVAssessment', 10, 'Allergies confirmed and medications reviewed prior to administration', 1, 'CheckBox', NULL, 0, NULL, NULL)
  ,	('IVAssessment', 11, 'All of the above', 1, 'CheckBox', NULL, 0, NULL, NULL)

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
            or ISNULL([target].[prompt_default], CHAR(0)) <> ISNULL([source].[prompt_default], CHAR(0))
            or [target].[required] <> [source].[required]) 
			or ISNULL([target].[placeholder_text], CHAR(0)) != ISNULL([source].[placeholder_text], CHAR(0))
			or ISNULL([target].[display_child_prompts_value], CHAR(0)) != ISNULL([source].[display_child_prompts_value], CHAR(0))) then
        update set
            [prompt]         = [source].[prompt]
          , [is_active]      = [source].[is_active]
          , [prompt_type]    = [source].[prompt_type]
          , [prompt_default] = [source].[prompt_default]
          , [required]       = [source].[required]
		  , [placeholder_text] = [source].[placeholder_text]
		  , [display_child_prompts_value] = [source].[display_child_prompts_value]
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
		  , [placeholder_text]
		  , [display_child_prompts_value]
        )
        values
            ([prompt_group_id], [sequence], [prompt], [is_active], [prompt_type], [prompt_default], [required], [placeholder_text], [display_child_prompts_value])
    when not matched by source then
        delete;