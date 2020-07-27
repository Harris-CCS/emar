create procedure [dbo].[export_ibex_patients]
as
    begin

        select [source].[ibex] as                          [source_id]
             , ltrim(rtrim([source].[site])) as            [site_id]
             , ltrim(rtrim([source].[medrec])) as          [medical_record_number]
             , ltrim(rtrim([source].[acctnum])) as         [account_number]
             , ltrim(rtrim([source].[lname])) as           [last_name]
             , ltrim(rtrim([source].[fname])) as           [first_name]
             , ltrim(rtrim([source].[mname])) as           [middle_name]
             , ltrim(rtrim([source].[suffix])) as          [name_suffix]
             , ltrim(rtrim([source].[gender])) as          [gender]
             , case
                   when isdate([source].[dob]) = 1
                       then cast([source].[dob] as date)
                   else null
               end as                                      [date_of_birth]
             , ltrim(rtrim([source].[age])) as             [age]
             , ltrim(rtrim([source].[ageunits])) as        [age_units]
             , ltrim(rtrim([source].[complaint])) as       [complaint]
             , ltrim(rtrim([source].[height])) as          [height_in_cm]
             , ltrim(rtrim([source].[weight])) as          [weight_in_kg]
             , ltrim(rtrim([source].[bed])) as             [room_bed_code]
             , ltrim(rtrim([source].[ward])) as            [ward_code]
             , ltrim(rtrim([source].[dept])) as            [department_code]
             , ltrim(rtrim([source].[ord42])) as           [urgency]
             , ltrim(rtrim([source].[ord23])) as           [urgency_color]
             , case
                   when [source].[naalert] = 'Y'
                       then 1
                   else 0
               end as                                      [name_alert]
             , case
                   when [source].[withdraw] = 'Y'
                       then 1
                   else 0
               end as                                      [withdraw_consent]
             , ltrim(rtrim([source].[vsdate])) as          [vs_datetime]
             , ltrim(rtrim([source].[ord11])) as           [vs_blood_pressure_indicator]
             , ltrim(rtrim([source].[vssys])) as           [vs_systolic]
             , ltrim(rtrim([source].[vsdia])) as           [vs_diastolic]
             , ltrim(rtrim([source].[ord12])) as           [vs_pulse_indicator]
             , ltrim(rtrim([source].[vspulse])) as         [vs_pulse]
             , ltrim(rtrim([source].[vsmaplevel])) as      [vs_map_level]
             , ltrim(rtrim([source].[vsmap])) as           [vs_map]
             , ltrim(rtrim([source].[ord13])) as           [vs_respiratory_indicator]
             , ltrim(rtrim([source].[vsresp])) as          [vs_respiratory]
             , ltrim(rtrim([source].[ord14])) as           [vs_temperature_indicator]
             , ltrim(rtrim([source].[vstemp])) as          [vs_temperature]
             , ltrim(rtrim([source].[vsendtidallevel])) as [vs_end_tidal_level]
             , ltrim(rtrim([source].[vsendtidal])) as      [vs_end_tidal]
             , ltrim(rtrim([source].[ord23])) as           [vs_oxygen_saturation_indicator]
             , ltrim(rtrim([source].[vso2])) as            [vs_oxygen_saturation]
             , ltrim(rtrim([source].[ord15])) as           [vs_pain_scale_indicator]
             , ltrim(rtrim([source].[vspain])) as          [vs_pain_scale]
        from   [ibex].[dbo].[pat] as [source]
               inner join [ibex].[dbo].[org] as [sites] on [sites].[site] = [source].[site]
        order by [source].[lname]
               , [source].[fname]
               , case
                     when isdate([source].[dob]) = 1
                         then cast([source].[dob] as date)
                     else null
                 end
               , [source].[gender];
    end;
go

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'Procedure used to export ibex patients in emar format'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'PROCEDURE'
  , @level1name = N'export_ibex_patients';
go