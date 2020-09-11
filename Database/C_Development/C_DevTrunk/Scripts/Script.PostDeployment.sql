/*************************************************************************************
Post-Deployment Script Template
--------------------------------------------------------------------------------------
 This file contains SQL statements that will be appended to the build script.
 Use SQLCMD syntax to include a file in the post-deployment script.
 Example:      :r .\myfile.sql
 Use SQLCMD syntax to reference a variable in the post-deployment script.
 Example:      :setvar TableName MyTable
               SELECT * FROM [$(TableName)]
--------------------------------------------------------------------------------------
*************************************************************************************/
set nocount on;

declare
     @max_id bigint

/* Insert table order
LVL: 000 SEQ: 001 TBL: dbo.actions
LVL: 000 SEQ: 002 TBL: dbo.fdb_allergy_name
LVL: 000 SEQ: 003 TBL: dbo.fdb_brand_name
LVL: 000 SEQ: 004 TBL: dbo.fdb_ndc_info
LVL: 000 SEQ: 005 TBL: dbo.frequency_calendar
LVL: 000 SEQ: 006 TBL: dbo.frequency_days
LVL: 000 SEQ: 007 TBL: dbo.frequency_interval_units
LVL: 000 SEQ: 008 TBL: dbo.frequency_minutes
LVL: 000 SEQ: 009 TBL: dbo.frequency_types
LVL: 000 SEQ: 010 TBL: dbo.options
LVL: 000 SEQ: 011 TBL: dbo.permissions
LVL: 000 SEQ: 012 TBL: dbo.prompt_groups
LVL: 000 SEQ: 013 TBL: dbo.sites
LVL: 000 SEQ: 014 TBL: dbo.templates
LVL: 001 SEQ: 001 TBL: dbo.antimicrobial_indication_items
LVL: 001 SEQ: 002 TBL: dbo.antimicrobial_indications
LVL: 001 SEQ: 003 TBL: dbo.frequency_schedules
LVL: 001 SEQ: 004 TBL: dbo.medication_routes
LVL: 001 SEQ: 005 TBL: dbo.medication_units
LVL: 001 SEQ: 006 TBL: dbo.medications
LVL: 001 SEQ: 007 TBL: dbo.order_instructions
LVL: 001 SEQ: 008 TBL: dbo.override_reasons
LVL: 001 SEQ: 009 TBL: dbo.patients
LVL: 001 SEQ: 010 TBL: dbo.prompts
LVL: 001 SEQ: 011 TBL: dbo.site_code_shares
LVL: 001 SEQ: 012 TBL: dbo.site_options
LVL: 001 SEQ: 013 TBL: dbo.template_prompt_groups
LVL: 001 SEQ: 014 TBL: dbo.users
LVL: 002 SEQ: 001 TBL: dbo.action_route_templates
LVL: 002 SEQ: 002 TBL: dbo.department_preferred_list_items
LVL: 002 SEQ: 003 TBL: dbo.frequency_interval_day_times
LVL: 002 SEQ: 004 TBL: dbo.group_list_items
LVL: 002 SEQ: 005 TBL: dbo.medication_details
LVL: 002 SEQ: 006 TBL: dbo.medication_interactions
LVL: 002 SEQ: 007 TBL: dbo.patient_allergies
LVL: 002 SEQ: 008 TBL: dbo.patient_home_medications
LVL: 002 SEQ: 009 TBL: dbo.patient_indicators
LVL: 002 SEQ: 010 TBL: dbo.patient_orders
LVL: 002 SEQ: 011 TBL: dbo.patient_problems
LVL: 002 SEQ: 012 TBL: dbo.preferred_frequency_schedules
LVL: 002 SEQ: 013 TBL: dbo.preferred_medication_doses
LVL: 002 SEQ: 014 TBL: dbo.preferred_medication_routes
LVL: 002 SEQ: 015 TBL: dbo.prompt_choices
LVL: 002 SEQ: 016 TBL: dbo.site_formulary
LVL: 002 SEQ: 017 TBL: dbo.site_formulary_match
LVL: 002 SEQ: 018 TBL: dbo.user_permissions
LVL: 002 SEQ: 019 TBL: dbo.user_quick_list_items
LVL: 003 SEQ: 001 TBL: dbo.order_administrations
LVL: 003 SEQ: 002 TBL: dbo.patient_cart_orders
LVL: 004 SEQ: 001 TBL: dbo.cart_order_administrations
LVL: 004 SEQ: 002 TBL: dbo.order_administration_notes
LVL: 004 SEQ: 003 TBL: dbo.order_events
LVL: 004 SEQ: 004 TBL: dbo.order_interactions
LVL: 005 SEQ: 001 TBL: dbo.order_event_details
*/
drop table if exists [#medication_items]; 

create table [#medication_items]
    (
      [medication_id] [int] default 0
    , [ndc]           [varchar](32) not null
    , [drug_id]       [varchar](32) not null
    , [brand_name]    [nvarchar](255) not null
    , primary key clustered([ndc] asc, [drug_id] asc, [brand_name] asc));

-- https://stackoverflow.com/questions/23923366/specifying-a-relative-path-in-post-deployment-sql-files
:r ..\Scripts\Data-Loader\global_data\fdb_allergy_name.sql
:r ..\Scripts\Data-Loader\global_data\fdb_brand_name.sql
:r ..\Scripts\Data-Loader\global_data\fdb_ndc_info.sql
:r ..\Scripts\Data-Loader\global_data\frequency_calendar.sql
:r ..\Scripts\Data-Loader\global_data\frequency_days.sql
:r ..\Scripts\Data-Loader\global_data\frequency_interval_units.sql
:r ..\Scripts\Data-Loader\global_data\frequency_minutes.sql
:r ..\Scripts\Data-Loader\global_data\frequency_types.sql
:r ..\Scripts\Data-Loader\global_data\options.sql
:r ..\Scripts\Data-Loader\site_data\sites.sql
:r ..\Scripts\Data-Loader\global_data\fdb_medications_loader.sql
:r ..\Scripts\Data-Loader\site_data\antimicrobial_indication_items.sql
:r ..\Scripts\Data-Loader\site_data\antimicrobial_indications.sql
:r ..\Scripts\Data-Loader\site_data\frequency_schedules.sql
:r ..\Scripts\Data-Loader\site_data\medication_routes.sql
:r ..\Scripts\Data-Loader\site_data\medication_units.sql
:r ..\Scripts\Data-Loader\site_data\order_instructions.sql
:r ..\Scripts\Data-Loader\site_data\override_reasons.sql
:r ..\Scripts\Data-Loader\phi_data\patients.sql
:r ..\Scripts\Data-Loader\site_data\site_code_shares.sql
:r ..\Scripts\Data-Loader\site_data\site_options.sql
:r ..\Scripts\Data-Loader\user_data\users.sql
:r ..\Scripts\Data-Loader\site_data\group_list_items.sql
:r ..\Scripts\Data-Loader\phi_data\patient_allergies.sql
:r ..\Scripts\Data-Loader\phi_data\patient_home_medications.sql
:r ..\Scripts\Data-Loader\phi_data\patient_indicators.sql
--- BEGIN: custom data deployments for development
:r ..\Scripts\Data-Loader\development_data\antoni_data.sql
--- END: custom data deployments for development
:r ..\Scripts\Data-Loader\phi_data\patient_orders.sql
:r ..\Scripts\Data-Loader\phi_data\patient_problems.sql
:r ..\Scripts\Data-Loader\site_data\preferred_frequency_schedules.sql
:r ..\Scripts\Data-Loader\site_data\preferred_medication_doses.sql
:r ..\Scripts\Data-Loader\site_data\preferred_medication_routes.sql
:r ..\Scripts\Data-Loader\site_data\site_formulary.sql
:r ..\Scripts\Data-Loader\site_data\site_formulary_match.sql
:r ..\Scripts\Data-Loader\user_data\user_quick_list_items.sql
--- BEGIN: custom data deployments for development
:r ..\Scripts\Data-Loader\development_data\bradley_data.sql
:r ..\Scripts\Data-Loader\development_data\bradley_data_II.sql
:r ..\Scripts\Data-Loader\development_data\hsi-an.sql
--- END: custom data deployments for development

drop table if exists [#medication_items]; 

-- External References are not allowed in a bacpac file.
-- To create the bacpac procedures with an external reference must be dropped
-- After bacpac is created create the dacpac including these procedures
if '$(is_bacpac_build)'='True'
begin
    drop procedure if exists [dbo].[export_ibex_antimicrobial_indication_items];
    drop procedure if exists [dbo].[export_ibex_antimicrobial_indications];
    drop procedure if exists [dbo].[export_ibex_fdb_allergy_name];
    drop procedure if exists [dbo].[export_ibex_fdb_brand_name];
    drop procedure if exists [dbo].[export_ibex_fdb_ndc_info];
    drop procedure if exists [dbo].[export_ibex_frequency_calendar];
    drop procedure if exists [dbo].[export_ibex_frequency_days];
    drop procedure if exists [dbo].[export_ibex_frequency_interval_day_times];
    drop procedure if exists [dbo].[export_ibex_frequency_interval_units];
    drop procedure if exists [dbo].[export_ibex_frequency_minutes];
    drop procedure if exists [dbo].[export_ibex_frequency_schedules];
    drop procedure if exists [dbo].[export_ibex_frequency_types];
    drop procedure if exists [dbo].[export_ibex_group_list_items];
    drop procedure if exists [dbo].[export_ibex_medication_routes];
    drop procedure if exists [dbo].[export_ibex_medication_units];
    drop procedure if exists [dbo].[export_ibex_options];
    drop procedure if exists [dbo].[export_ibex_order_instructions];
    drop procedure if exists [dbo].[export_ibex_override_reasons];
    drop procedure if exists [dbo].[export_ibex_patient_allergies];
    drop procedure if exists [dbo].[export_ibex_patient_home_medications];
    drop procedure if exists [dbo].[export_ibex_patient_indicators];
    drop procedure if exists [dbo].[export_ibex_patient_orders];
    drop procedure if exists [dbo].[export_ibex_patient_problems];
    drop procedure if exists [dbo].[export_ibex_patients];
    drop procedure if exists [dbo].[export_ibex_preferred_frequency_schedules];
    drop procedure if exists [dbo].[export_ibex_preferred_medication_doses];
    drop procedure if exists [dbo].[export_ibex_preferred_medication_routes];
    drop procedure if exists [dbo].[export_ibex_site_code_shares];
    drop procedure if exists [dbo].[export_ibex_site_formulary];
    drop procedure if exists [dbo].[export_ibex_site_formulary_match];
    drop procedure if exists [dbo].[export_ibex_site_options];
    drop procedure if exists [dbo].[export_ibex_sites];
    drop procedure if exists [dbo].[export_ibex_user_quick_list_items];
    drop procedure if exists [dbo].[export_ibex_users];
    ---- emar specific procedures with external references
    drop procedure if exists [dbo].[create_FDB_search];
    drop procedure if exists [dbo].[pc_fdb_get_drc_info];
    drop procedure if exists [dbo].[pc_fdb_meds_search];
end;

--- variables global to all diagram_ published scripts
declare
    @diagram_id      [int]
  , @version_current [int]
  , @version         [int]
  , @continue_update [bit];

declare @outputs table([Id] int not null);
--- https://docs.microsoft.com/en-us/sql/ssms/visual-db-tools/set-up-database-diagram-designer-visual-database-tools?view=sql-server-ver15
--- https://feedback.azure.com/forums/908035-sql-server/suggestions/37992649-ssms-18-1-crashes-when-opening-a-database-diagram
--- deploying these diagrams in having an issue at the moment.
--- it worked several times, but now causes ssms to crash. so removing for the moment.
---:r ..\Scripts\Post-Deployment\diagram_patients.sql
---:r ..\Scripts\Post-Deployment\diagram_security.sql
---
--- Create SQL Agent job for calculating user_quick_list most used
:r ..\Scripts\Post-Deployment\sql_agent_job.sql
