print 'create view [ibex].[dbo].[emar_archived_patients_retrieve_view];';

set @template = N'
/*** View for retrieving data from the hst table ***/
CREATE OR ALTER VIEW dbo.emar_archived_patients_retrieve_view 
AS

select           [source].[ibex] as                              [external_id]
               , CONVERT(tinyint, [source].[site]) as                    [external_site_id]
               , ltrim(rtrim([source].[medrec])) as              [medical_record_number]
               , ltrim(rtrim([source].[acctnum])) as             [account_number]
               , ltrim(rtrim([source].[lname])) as               [last_name]
               , ltrim(rtrim([source].[fname])) as               [first_name]
               , ltrim(rtrim([source].[mname])) as               [middle_name]
               , ltrim(rtrim([source].[suffix])) as              [name_suffix]
               , ltrim(rtrim([source].[gender])) as              [gender]
               , case
                     when isdate([source].[dob]) = 1
                         then cast([source].[dob] as date)
                     else null
                 end as                                          [date_of_birth]
               , ltrim(rtrim([source].[age])) as                 [age]
               , ltrim(rtrim([source].[ageunits])) as            [age_units]
               , ltrim(rtrim([source].[complaint])) as           [complaint]
               , ltrim(rtrim([source].[height])) as              [height_in_cm]
               , ltrim(rtrim([source].[weight])) as              [weight_in_kg]
               , CONVERT(varchar,NULL) as                 [room_bed_code]
               , ltrim(rtrim([source].[ward])) as                [ward_code]
               , ltrim(rtrim([source].[dept])) as                [department_code]
               , ltrim(rtrim(isnull([urgency].[name], ''''))) as   [urgency]
               , CONVERT(varchar,NULL)                       as    [urgency_color]
               , 0                                       as    [name_alert]
               , case
                     when [source].[withdraw] = ''Y''
                         then 1
                     else 0
                 end as                                          [withdraw_consent]
               , '''' as              [vs_datetime]
               , '''' as               [vs_blood_pressure_indicator]
               , '''' as               [vs_systolic]
               , '''' as               [vs_diastolic]
               , '''' as               [vs_pulse_indicator]
               , '''' as             [vs_pulse]
               , ltrim(rtrim([source].[vsmaplevel])) as          [vs_map_level]
               , ltrim(rtrim([source].[vsmap])) as               [vs_map]
               , '''' as               [vs_respiratory_indicator]
               , '''' as              [vs_respiratory]
               , '''' as               [vs_temperature_indicator]
               , '''' as              [vs_temperature]
               , ltrim(rtrim([source].[vsendtidallevel])) as     [vs_end_tidal_level]
               , ltrim(rtrim([source].[vsendtidal])) as          [vs_end_tidal]
               , '''' as               [vs_oxygen_saturation_indicator]
               , '''' as                [vs_oxygen_saturation]
               , '''' as               [vs_pain_scale_indicator]
               , '''' as              [vs_pain_scale]
               , ltrim(rtrim([source].[custom_insurance_id])) as [custom_number]
               , ltrim(rtrim([source].[person])) as              [person_number]
               , ltrim(rtrim([source].[ibex])) as                [visit_start_datetime]
               , ltrim(rtrim([source].[gender_system])) as       [gender_system]
             , CONVERT(int, 0) as [is_active]
       from      [dbo].hst as [source]
                 inner join [dbo].[org] as [sites] on [sites].[site] = [source].[site]
                 outer apply
       (
           select [css].[cs_site] as [site]
           from   [dbo].[code_share] as [css]
           where  [css].[cs_name] = ''urgency''
                  and [css].[site] = [source].[site]
       ) as [cs]
                 left join [dbo].[idx] as [urgency] on [urgency].[type] = ''Z''
                                                 and [urgency].[id] = [source].[eun]
                                                 and [urgency].[site] = [cs].[site];
';

set @sql_cmd = @template;

execute [ibex].[dbo].[sp_executesql] 
    @statement = @sql_cmd;