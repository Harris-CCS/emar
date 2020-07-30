create table [dbo].[patients]
    (
      [id]                             [bigint] identity(1, 1) not null
    , [site_id]                        [int] not null
    , [medical_record_number]          [varchar](25) null
    , [account_number]                 [varchar](25) null
    , [last_name]                      [nvarchar](35) not null
    , [first_name]                     [nvarchar](35) not null
    , [middle_name]                    [nvarchar](35) null
    , [name_suffix]                    [nvarchar](25) null
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
    , [vs_blood_pressure_indicator]    [char](1) null        ---- [ord11]
    , [vs_systolic]                    [varchar](14) null    ---- [vssys]
    , [vs_diastolic]                   [varchar](14) null    ---- [vsdia]
    , [vs_pulse_indicator]             [char](1) null        ---- [ord12]
    , [vs_pulse]                       [varchar](14) null    ---- [vspulse]
    , [vs_map_level]                   [char](1) null        ---- [vsmaplevel]
    , [vs_map]                         [varchar](14) null    ---- [vsmap]
    , [vs_respiratory_indicator]       [char](1) null        ---- [ord13]
    , [vs_respiratory]                 [varchar](14) null    ---- [vsresp]
    , [vs_temperature_indicator]       [char](1) null        ---- [ord14]
    , [vs_temperature]                 [varchar](14) null    ---- [vstemp]
    , [vs_end_tidal_level]             [char](1) null        ---- [vsendtidallevel]
    , [vs_end_tidal]                   [varchar](14) null    ---- [vsendtidal]
    , [vs_oxygen_saturation_indicator] [char](1) null        ---- [ord23]
    , [vs_oxygen_saturation]           [varchar](50) null    ---- [vso2]
    , [vs_pain_scale_indicator]        [char](1) null        ---- [ord15]
    , [vs_pain_scale]                  [varchar](14) null    ---- [vspain]
    , [is_active]                      [bit] not null
    , constraint [pk__patients__id] primary key clustered([id] asc));
go

/********
 Defaults
********/
/*******
 Indexes
*******/
/***********
 Foreign Key
***********/

alter table [dbo].[patients]
add constraint [fk__patients__sites] foreign key([site_id]) references [dbo].[sites]([id]);
go

/***************
 Data Dictionary
    Defaults
***************/
/***************
 Data Dictionary
    Indexes
***************/

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'Primary Key Constraint'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'patients'
  , @level2type = N'CONSTRAINT'
  , @level2name = N'pk__patients__id';
go

/***************
 Data Dictionary
    Table
***************/

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'Patient information'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'patients';
go

/***************
 Data Dictionary
    Columns
***************/

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'Auto increment table ID'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'patients'
  , @level2type = N'COLUMN'
  , @level2name = N'id';
go

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'Hospital identifier foriegn key to site table'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'patients'
  , @level2type = N'COLUMN'
  , @level2name = N'site_id';
go

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'Medical file number of the patient from ADT interface'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'patients'
  , @level2type = N'COLUMN'
  , @level2name = N'medical_record_number';
go

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'Account number of the patient visit from ADT interface'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'patients'
  , @level2type = N'COLUMN'
  , @level2name = N'account_number';
go

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'Last name'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'patients'
  , @level2type = N'COLUMN'
  , @level2name = N'last_name';
go

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'First name'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'patients'
  , @level2type = N'COLUMN'
  , @level2name = N'first_name';
go

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'Middle name'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'patients'
  , @level2type = N'COLUMN'
  , @level2name = N'middle_name';
go

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'Name Suffix'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'patients'
  , @level2type = N'COLUMN'
  , @level2name = N'name_suffix';
go

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'Gender'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'patients'
  , @level2type = N'COLUMN'
  , @level2name = N'gender';
go

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'Date of birth YYYYMMDD'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'patients'
  , @level2type = N'COLUMN'
  , @level2name = N'date_of_birth';
go

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'Age calculated from date of birth, changes when date of birth changes'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'patients'
  , @level2type = N'COLUMN'
  , @level2name = N'age';
go

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'Y=years M=months D=days'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'patients'
  , @level2type = N'COLUMN'
  , @level2name = N'age_units';
go

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'Chief complaint'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'patients'
  , @level2type = N'COLUMN'
  , @level2name = N'complaint';
go

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'Patient height in centimeters'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'patients'
  , @level2type = N'COLUMN'
  , @level2name = N'height_in_cm';
go

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'Patient weight expressed in kilograms 999v9'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'patients'
  , @level2type = N'COLUMN'
  , @level2name = N'weight_in_kg';
go

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'Bed location of patient'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'patients'
  , @level2type = N'COLUMN'
  , @level2name = N'room_bed_code';
go

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'Area location of patient'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'patients'
  , @level2type = N'COLUMN'
  , @level2name = N'ward_code';
go

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'Department location of patient'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'patients'
  , @level2type = N'COLUMN'
  , @level2name = N'department_code';
go

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'Urgency, FKEY to IDX code table record type ''Z'''
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'patients'
  , @level2type = N'COLUMN'
  , @level2name = N'urgency';
go

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'Urgency color value'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'patients'
  , @level2type = N'COLUMN'
  , @level2name = N'urgency_color';
go

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'Y=Name indicator on. 1=True, 0=False'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'patients'
  , @level2type = N'COLUMN'
  , @level2name = N'name_alert';
go

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'Patient has withdrawn permission to share data. 1=True, 0=False'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'patients'
  , @level2type = N'COLUMN'
  , @level2name = N'withdraw_consent';
go

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'Date and time of vital signs entry YYYYMMDDHHMM'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'patients'
  , @level2type = N'COLUMN'
  , @level2name = N'vs_datetime';
go

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'blood pressure indicator'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'patients'
  , @level2type = N'COLUMN'
  , @level2name = N'vs_blood_pressure_indicator';
go

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'Systolic blood pressure'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'patients'
  , @level2type = N'COLUMN'
  , @level2name = N'vs_systolic';
go

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'Diastolic blood pressure'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'patients'
  , @level2type = N'COLUMN'
  , @level2name = N'vs_diastolic';
go

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'Pulse indicator'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'patients'
  , @level2type = N'COLUMN'
  , @level2name = N'vs_pulse_indicator';
go

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'Pulse rate'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'patients'
  , @level2type = N'COLUMN'
  , @level2name = N'vs_pulse';
go

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'MAP level'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'patients'
  , @level2type = N'COLUMN'
  , @level2name = N'vs_map_level';
go

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'MAP value'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'patients'
  , @level2type = N'COLUMN'
  , @level2name = N'vs_map';
go

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'Respiratory rate indicator'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'patients'
  , @level2type = N'COLUMN'
  , @level2name = N'vs_respiratory_indicator';
go

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'Respiratory rate'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'patients'
  , @level2type = N'COLUMN'
  , @level2name = N'vs_respiratory';
go

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'Temperature indicator'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'patients'
  , @level2type = N'COLUMN'
  , @level2name = N'vs_temperature_indicator';
go

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'Temperature'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'patients'
  , @level2type = N'COLUMN'
  , @level2name = N'vs_temperature';
go

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'End-Tidal CO2 level'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'patients'
  , @level2type = N'COLUMN'
  , @level2name = N'vs_end_tidal_level';
go

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'End-Tidal CO2 value'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'patients'
  , @level2type = N'COLUMN'
  , @level2name = N'vs_end_tidal';
go

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'Oxygen saturation indicator'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'patients'
  , @level2type = N'COLUMN'
  , @level2name = N'vs_oxygen_saturation_indicator';
go

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'Oxygen saturation'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'patients'
  , @level2type = N'COLUMN'
  , @level2name = N'vs_oxygen_saturation';
go

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'Pain scale indicator'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'patients'
  , @level2type = N'COLUMN'
  , @level2name = N'vs_pain_scale_indicator';
go

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'Pain scale'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'patients'
  , @level2type = N'COLUMN'
  , @level2name = N'vs_pain_scale';
go

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'is_active 1=True 0=False'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'patients'
  , @level2type = N'COLUMN'
  , @level2name = N'is_active';
go