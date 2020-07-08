declare 
    @max_id bigint;

/************************************
- create temporary tables for loading
************************************/
/*********************************
LVL: 000 SEQ: 001 TBL: dbo.actions
*********************************/
/*******************************************
LVL: 000 SEQ: 002 TBL: dbo.medication_routes
*******************************************/
/*********************************
LVL: 000 SEQ: 003 TBL: dbo.options
*********************************/
/*************************************
LVL: 000 SEQ: 004 TBL: dbo.permissions
*************************************/
/*******************************
LVL: 000 SEQ: 005 TBL: dbo.sites
*******************************/

drop table if exists [#sites];

create table [#sites]
    (
      [source_id] [varchar](40)
    , [name]      [varchar](40)
    , [is_active] [bit]);

/******************************************
LVL: 001 SEQ: 001 TBL: dbo.override_reasons
******************************************/
/**********************************
LVL: 001 SEQ: 002 TBL: dbo.patients
**********************************/

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
    , [vs_datetime]                    [datetimeoffset](7) null
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
    , [vs_pain_scale]                  [char](14) null);

/******************************************
LVL: 001 SEQ: 003 TBL: dbo.site_code_shares
******************************************/
/****************************************
LVL: 001 SEQ: 004 TBL: dbo.site_formulary
****************************************/
/**********************************************
LVL: 001 SEQ: 005 TBL: dbo.site_formulary_match
**********************************************/
/**************************************
LVL: 001 SEQ: 006 TBL: dbo.site_options
**************************************/
/*********************************************
LVL: 001 SEQ: 007 TBL: dbo.site_preferred_list
*********************************************/
/*******************************
LVL: 001 SEQ: 008 TBL: dbo.users
*******************************/

drop table if exists [#users];

create table [#users]
    (
      [source_id]               [varchar](40) not null
    , [site_id]                 [varchar](25) not null
    , [type]                    [char](1) not null
    , [is_active]               [bit] not null
    , [initials_display]        [varchar](4) not null
    , [first_name]              [varchar](20) not null
    , [last_name]               [varchar](20) not null
    , [ordering_only_physician] [bit] null
    , [name_display_preference] [bit] null
    , [login_name]              [varchar](255) not null
    , [login_password]          [varchar](255) not null
    , [salt]                    [binary](16) not null
    , [last_login_time]         [datetimeoffset](7) null
    , [failed_login_attempts]   [int] not null, );

/*******************************************
LVL: 002 SEQ: 001 TBL: dbo.patient_allergies
*******************************************/
/***************************************
LVL: 002 SEQ: 002 TBL: dbo.patient_carts
***************************************/
/**************************************************
LVL: 002 SEQ: 003 TBL: dbo.patient_home_medications
**************************************************/
/********************************************
LVL: 002 SEQ: 004 TBL: dbo.patient_indicators
********************************************/
/****************************************
LVL: 002 SEQ: 005 TBL: dbo.patient_orders
****************************************/
/******************************************
LVL: 002 SEQ: 006 TBL: dbo.user_permissions
******************************************/
/*****************************************
LVL: 002 SEQ: 007 TBL: dbo.user_quick_list
*****************************************/
/***********************************************
LVL: 003 SEQ: 001 TBL: dbo.order_administrations
***********************************************/
/**********************************************
LVL: 003 SEQ: 002 TBL: dbo.patient_cart_details
**********************************************/
/****************************************************
LVL: 004 SEQ: 001 TBL: dbo.order_administration_notes
****************************************************/
/**************************************
LVL: 004 SEQ: 002 TBL: dbo.order_events
**************************************/
/*********************************************
LVL: 005 SEQ: 001 TBL: dbo.order_event_details
*********************************************/
/**************************************
LVL: 099 SEQ: 001 TBL: dbo.external_ids
**************************************/