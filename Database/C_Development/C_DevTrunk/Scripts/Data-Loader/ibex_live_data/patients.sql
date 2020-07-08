begin transaction;

/*********************************
 load temporary tables for staging
*********************************/

insert into [#patients]
    ([source_id]
   , [site_id]
   , [medical_record_number]
   , [account_number]
   , [last_name]
   , [first_name]
   , [middle_name]
   , [name_suffix]
   , [gender]
   , [date_of_birth]
   , [age]
   , [age_units]
   , [complaint]
   , [height_in_cm]
   , [weight_in_kg]
   , [room_bed_code]
   , [ward_code]
   , [department_code]
   , [urgency]
   , [urgency_color]
   , [name_alert]
   , [withdraw_consent]
   , [vs_datetime]
   , [vs_blood_pressure_indicator]
   , [vs_systolic]
   , [vs_diastolic]
   , [vs_pulse_indicator]
   , [vs_pulse]
   , [vs_map_level]
   , [vs_map]
   , [vs_respiratory_indicator]
   , [vs_respiratory]
   , [vs_temperature_indicator]
   , [vs_temperature]
   , [vs_end_tidal_level]
   , [vs_end_tidal]
   , [vs_oxygen_saturation_indicator]
   , [vs_oxygen_saturation]
   , [vs_pain_scale_indicator]
   , [vs_pain_scale]
    )
select [source].[ibex]
     , [source].[site]
     , [source].[medrec]
     , [source].[acctnum]
     , [source].[lname]
     , [source].[fname]
     , [source].[mname]
     , [source].[suffix]
     , [source].[gender]
     , case
           when isdate([source].[dob]) = 1
               then cast([source].[dob] as date)
                else null
       end as [date_of_birth]
     , [source].[age]
     , [source].[ageunits]
     , [source].[complaint]
     , [source].[height]
     , [source].[weight]
     , [source].[bed]
     , [source].[ward]
     , [source].[dept]
     , [source].[ord42]
     , [source].[ord23]
     , case
           when [source].[naalert] = 'Y'
               then 1
                else 0
       end
     , case
           when [source].[withdraw] = 'Y'
               then 1
       else 0
       end
     , case
           when isdate([source].[vsdate]) = 1
               then cast([source].[vsdate] as date)
       else null
       end as [vsdate]
     , [source].[ord11]
     , [source].[vssys]
     , [source].[vsdia]
     , [source].[ord12]
     , [source].[vspulse]
     , [source].[vsmaplevel]
     , [source].[vsmap]
     , [source].[ord13]
     , [source].[vsresp]
     , [source].[ord14]
     , [source].[vstemp]
     , [source].[vsendtidallevel]
     , [source].[vsendtidal]
     , [source].[ord23]
     , [source].[vso2]
     , [source].[ord15]
     , [source].[vspain]
from   [ibex].[dbo].[pat] as [source];

update [source] set    
    [site_id] = isnull([internal_site].[id], -1)
from   [#patients] as [source]
       outer apply [dbo].[get_internal_id]
    ('pulsecheck', 'sites', [source].[site_id]) as [internal_site];

alter table [#patients]
add [id]        [bigint] identity(1, 1)
  , [target_id] [bigint];

/*************************
 get max id for seed value
*************************/

set @max_id = null;

select @max_id = max([id])
from   [dbo].[patients];

set @max_id = isnull(@max_id, 0);

update [source] set    
    [target_id] = [source].[id] + @max_id
from   [#patients] as [source];

/******************************
 begin loading permanent tables
******************************/

set identity_insert [dbo].[patients] on;

insert into [dbo].[patients]
    ([id]
   , [site_id]
   , [medical_record_number]
   , [account_number]
   , [last_name]
   , [first_name]
   , [middle_name]
   , [name_suffix]
   , [gender]
   , [date_of_birth]
   , [age]
   , [age_units]
   , [complaint]
   , [height_in_cm]
   , [weight_in_kg]
   , [room_bed_code]
   , [ward_code]
   , [department_code]
   , [urgency]
   , [urgency_color]
   , [name_alert]
   , [withdraw_consent]
   , [vs_datetime]
   , [vs_blood_pressure_indicator]
   , [vs_systolic]
   , [vs_diastolic]
   , [vs_pulse_indicator]
   , [vs_pulse]
   , [vs_map_level]
   , [vs_map]
   , [vs_respiratory_indicator]
   , [vs_respiratory]
   , [vs_temperature_indicator]
   , [vs_temperature]
   , [vs_end_tidal_level]
   , [vs_end_tidal]
   , [vs_oxygen_saturation_indicator]
   , [vs_oxygen_saturation]
   , [vs_pain_scale_indicator]
   , [vs_pain_scale]
    )
select [source].[target_id]
     , [source].[site_id]
     , [source].[medical_record_number]
     , [source].[account_number]
     , [source].[last_name]
     , [source].[first_name]
     , [source].[middle_name]
     , [source].[name_suffix]
     , [source].[gender]
     , [source].[date_of_birth]
     , [source].[age]
     , [source].[age_units]
     , [source].[complaint]
     , [source].[height_in_cm]
     , [source].[weight_in_kg]
     , [source].[room_bed_code]
     , [source].[ward_code]
     , [source].[department_code]
     , [source].[urgency]
     , [source].[urgency_color]
     , [source].[name_alert]
     , [source].[withdraw_consent]
     , [source].[vs_datetime]
     , [source].[vs_blood_pressure_indicator]
     , [source].[vs_systolic]
     , [source].[vs_diastolic]
     , [source].[vs_pulse_indicator]
     , [source].[vs_pulse]
     , [source].[vs_map_level]
     , [source].[vs_map]
     , [source].[vs_respiratory_indicator]
     , [source].[vs_respiratory]
     , [source].[vs_temperature_indicator]
     , [source].[vs_temperature]
     , [source].[vs_end_tidal_level]
     , [source].[vs_end_tidal]
     , [source].[vs_oxygen_saturation_indicator]
     , [source].[vs_oxygen_saturation]
     , [source].[vs_pain_scale_indicator]
     , [source].[vs_pain_scale]
from   [#patients] as [source];

set identity_insert [dbo].[patients] off;

/********************************
 loading [external_ids] reference
********************************/

insert into [dbo].[external_ids]
    ([internal_id]
   , [vendor]
   , [entity]
   , [external_id]
    )
select [source].[target_id]
     , 'pulsecheck'
     , 'patients'
     , [source].[source_id]
from   [#patients] as [source];

/********************
 end table
********************/

commit transaction;

drop table if exists [#patients];
