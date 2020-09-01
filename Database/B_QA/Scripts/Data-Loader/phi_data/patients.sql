print 'Loading Table: patients';

drop table if exists [#patients];

create table [#patients]
    (
      [source_id]                      [varchar](40) not null
    , [site_id]                        [varchar](25) not null
    , [medical_record_number]          [varchar](25) null
    , [account_number]                 [varchar](25) null
    , [last_name]                      [varchar](35) not null
    , [first_name]                     [varchar](35) not null
    , [middle_name]                    [varchar](35) null
    , [name_suffix]                    [varchar](25) null
    , [gender]                         [varchar](10) not null
    , [date_of_birth]                  [date] null
    , [age]                            [tinyint] null
    , [age_units]                      [char](1) null
    , [complaint]                      [varchar](80) null
    , [height_in_cm]                   [numeric](6, 2) null
    , [weight_in_kg]                   [numeric](6, 2) null
    , [room_bed_code]                  [varchar](15) null
    , [ward_code]                      [varchar](15) null
    , [department_code]                [varchar](15) null
    , [urgency]                        [varchar](50) null
    , [urgency_color]                  [varchar](25) null
    , [name_alert]                     [bit] not null
    , [withdraw_consent]               [bit] not null
    , [vs_datetime]                    [varchar](14) null
    , [vs_blood_pressure_indicator]    [char](1) null
    , [vs_systolic]                    [char](14) null
    , [vs_diastolic]                   [char](14) null
    , [vs_pulse_indicator]             [char](1) null
    , [vs_pulse]                       [char](14) null
    , [vs_map_level]                   [char](1) null
    , [vs_map]                         [varchar](14) null
    , [vs_respiratory_indicator]       [char](1) null
    , [vs_respiratory]                 [char](14) null
    , [vs_temperature_indicator]       [char](1) null
    , [vs_temperature]                 [char](14) null
    , [vs_end_tidal_level]             [char](1) null
    , [vs_end_tidal]                   [varchar](14) null
    , [vs_oxygen_saturation_indicator] [char](1) null
    , [vs_oxygen_saturation]           [varchar](50) null
    , [vs_pain_scale_indicator]        [char](1) null
    , [vs_pain_scale]                  [char](14) null
    , [custom_number]                  [varchar](25) null
    , [person_number]                  [varchar](25) null
    , [visit_start_datetime]           [varchar](25) null);

if '$(load_data)' = 'live'
   and exists
(
    select null
    from   [master].[sys].[databases]
    where  [name] = 'ibex'
)
    begin

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
           , [custom_number]
           , [person_number]
           , [visit_start_datetime]
            )
        execute ('execute dbo.export_ibex_patients');
    end;

if '$(load_data)' = 'sample'
    begin

        bulk insert [#patients] from '$(current_path)Scripts\Data-Loader\sample_data\patients.bcp' with(fieldterminator = '|~', rowterminator = '\n');
    end;

if
(
    select count(*)
    from   [#patients]
) > 0
    begin

        begin transaction;

/****************************************
        load temporary tables for staging
****************************************/

        update [source] set    
            [source_id] = [source].[site_id] + '|' + [source].[source_id]
        from   [#patients] as [source];

        alter table [#patients]
        add [id]        [bigint] identity(1, 1)
          , [target_id] [bigint];

/********************************
        get max id for seed value
********************************/

        set @max_id = null;

        select @max_id = max([id])
        from   [dbo].[patients];

        set @max_id = isnull(@max_id, 0);

        update [source] set    
            [target_id] = [source].[id] + @max_id
        from   [#patients] as [source];

/*************************************
        begin loading permanent tables
*************************************/

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
           , [is_active]
           , [custom_number]
           , [person_number]
           , [visit_start_datetime]
            )
        select [source].[target_id]
             , isnull([internal_site].[id], -1) as [site_id]
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
             , [dbo].[ibex_date_to_offset_date]([source].[vs_datetime], [site].[time_zone_name])
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
             , cast(1 as bit) as                   [is_active]
             , [source].[custom_number]
             , [source].[person_number]
             , [dbo].[ibex_date_to_offset_date]([source].[visit_start_datetime],[site].[time_zone_name]) [visit_start_datetime]
        from   [#patients] as [source]
               outer apply [dbo].[get_internal_id]('pulsecheck', 'sites', [source].[site_id]) as [internal_site]
               left join [dbo].[sites] as [site] on [site].[id] = [internal_site].[id]
        order by [source].[last_name]
               , [source].[first_name];

        set identity_insert [dbo].[patients] off;

/***************************************
        loading [external_ids] reference
***************************************/

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

/****************
        end table
****************/

        commit transaction;
    end;

drop table if exists [#patients];