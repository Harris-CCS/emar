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
print 'Begin: ScriptPostDeployment.sql';

/* Insert table order
LVL: 000 SEQ: 001 TBL: dbo.actions
LVL: 000 SEQ: 002 TBL: dbo.duration_units
LVL: 000 SEQ: 003 TBL: dbo.fdb_allergy_name
LVL: 000 SEQ: 004 TBL: dbo.fdb_brand_name
LVL: 000 SEQ: 005 TBL: dbo.fdb_ndc_info
LVL: 000 SEQ: 006 TBL: dbo.frequency_calendar
LVL: 000 SEQ: 007 TBL: dbo.frequency_days
LVL: 000 SEQ: 008 TBL: dbo.frequency_interval_units
LVL: 000 SEQ: 009 TBL: dbo.frequency_minutes
LVL: 000 SEQ: 010 TBL: dbo.frequency_types
LVL: 000 SEQ: 011 TBL: dbo.global_options
LVL: 000 SEQ: 012 TBL: dbo.notification_categories
LVL: 000 SEQ: 013 TBL: dbo.options
LVL: 000 SEQ: 014 TBL: dbo.prompt_groups
LVL: 000 SEQ: 015 TBL: dbo.settings
LVL: 000 SEQ: 016 TBL: dbo.sites
LVL: 001 SEQ: 001 TBL: dbo.antimicrobial_indication_items
LVL: 001 SEQ: 002 TBL: dbo.antimicrobial_indications
LVL: 001 SEQ: 003 TBL: dbo.devices
LVL: 001 SEQ: 004 TBL: dbo.frequency_schedules
LVL: 001 SEQ: 005 TBL: dbo.medication_routes
LVL: 001 SEQ: 006 TBL: dbo.medication_units
LVL: 001 SEQ: 007 TBL: dbo.medications
LVL: 001 SEQ: 008 TBL: dbo.order_administration_available_actions
LVL: 001 SEQ: 009 TBL: dbo.order_available_actions
LVL: 001 SEQ: 010 TBL: dbo.order_instructions
LVL: 001 SEQ: 011 TBL: dbo.override_reasons
LVL: 001 SEQ: 012 TBL: dbo.patients
LVL: 001 SEQ: 013 TBL: dbo.prompts
LVL: 001 SEQ: 014 TBL: dbo.site_code_shares
LVL: 001 SEQ: 015 TBL: dbo.site_options
LVL: 001 SEQ: 016 TBL: dbo.template_response_rules
LVL: 001 SEQ: 017 TBL: dbo.users
LVL: 002 SEQ: 001 TBL: dbo.department_preferred_list_items
LVL: 002 SEQ: 002 TBL: dbo.frequency_interval_day_times
LVL: 002 SEQ: 003 TBL: dbo.group_list_items
LVL: 002 SEQ: 004 TBL: dbo.medication_details
LVL: 002 SEQ: 005 TBL: dbo.medication_interactions
LVL: 002 SEQ: 006 TBL: dbo.patient_allergies
LVL: 002 SEQ: 007 TBL: dbo.patient_home_medications
LVL: 002 SEQ: 008 TBL: dbo.patient_indicators
LVL: 002 SEQ: 009 TBL: dbo.patient_problems
LVL: 002 SEQ: 010 TBL: dbo.preferred_frequency_schedules
LVL: 002 SEQ: 011 TBL: dbo.preferred_medication_doses
LVL: 002 SEQ: 012 TBL: dbo.preferred_medication_routes
LVL: 002 SEQ: 013 TBL: dbo.print_history
LVL: 002 SEQ: 014 TBL: dbo.prompt_choices
LVL: 002 SEQ: 015 TBL: dbo.site_formulary
LVL: 002 SEQ: 016 TBL: dbo.site_formulary_match
LVL: 002 SEQ: 017 TBL: dbo.templates
LVL: 002 SEQ: 018 TBL: dbo.user_patients
LVL: 002 SEQ: 019 TBL: dbo.user_quick_list_items
LVL: 002 SEQ: 020 TBL: dbo.user_settings
LVL: 003 SEQ: 001 TBL: dbo.action_route_templates
LVL: 003 SEQ: 002 TBL: dbo.patient_cart_orders
LVL: 003 SEQ: 003 TBL: dbo.patient_orders
LVL: 003 SEQ: 004 TBL: dbo.template_prompt_groups
LVL: 004 SEQ: 001 TBL: dbo.cart_order_administrations
LVL: 004 SEQ: 002 TBL: dbo.order_administrations
LVL: 004 SEQ: 003 TBL: dbo.order_interactions
LVL: 004 SEQ: 004 TBL: dbo.order_reactions
LVL: 005 SEQ: 001 TBL: dbo.external_update_queue
LVL: 005 SEQ: 002 TBL: dbo.notifications
LVL: 005 SEQ: 003 TBL: dbo.order_administration_notes
LVL: 005 SEQ: 004 TBL: dbo.order_events
LVL: 006 SEQ: 001 TBL: dbo.order_event_details
*/
drop table if exists [#medication_items]; 

create table [#medication_items]
    (
      [medication_id] [int] default 0
    , [site_id]       [int] not null default-1
    , [ndc]           [varchar](32) not null
    , [drug_id]       [varchar](32) not null
    , [brand_name]    [nvarchar](255) not null
    , [match]         [nvarchar](255) null --- Added for testing / debugging
    , primary key clustered([ndc] asc, [drug_id] asc, [brand_name] asc, [site_id] asc));

declare 
      @export_database_name sysname = 'default_none'
    , @template nvarchar(max)
    , @sql_cmd nvarchar(max)
    , @does_ibex_exist bit = 0
    , @max_id bigint
    , @dev_custom_data_site_id int;

select @does_ibex_exist = 1
from   [master].[sys].[databases]
where  [name] = 'ibex';

if @does_ibex_exist = 1
    begin
        set @export_database_name = 'ibex';
    end;


print 'Begin: Data Load Process';
:r ..\Scripts\Data-Loader\export_procedures\export_ibex_antimicrobial_indication_items.sql
:r ..\Scripts\Data-Loader\export_procedures\export_ibex_antimicrobial_indications.sql
:r ..\Scripts\Data-Loader\export_procedures\export_ibex_devices.sql
:r ..\Scripts\Data-Loader\export_procedures\export_ibex_fdb_allergy_name.sql
:r ..\Scripts\Data-Loader\export_procedures\export_ibex_fdb_brand_name.sql
:r ..\Scripts\Data-Loader\export_procedures\export_ibex_fdb_ndc_info.sql
:r ..\Scripts\Data-Loader\export_procedures\export_ibex_group_list_items.sql
:r ..\Scripts\Data-Loader\export_procedures\export_ibex_medication_routes.sql
:r ..\Scripts\Data-Loader\export_procedures\export_ibex_medication_units.sql
:r ..\Scripts\Data-Loader\export_procedures\export_ibex_order_instructions.sql
:r ..\Scripts\Data-Loader\export_procedures\export_ibex_override_reasons.sql
:r ..\Scripts\Data-Loader\export_procedures\export_ibex_patient_allergies.sql
:r ..\Scripts\Data-Loader\export_procedures\export_ibex_patient_home_medications.sql
:r ..\Scripts\Data-Loader\export_procedures\export_ibex_patient_indicators.sql
:r ..\Scripts\Data-Loader\export_procedures\export_ibex_patient_orders.sql
:r ..\Scripts\Data-Loader\export_procedures\export_ibex_patient_problems.sql
:r ..\Scripts\Data-Loader\export_procedures\export_ibex_patients.sql
:r ..\Scripts\Data-Loader\export_procedures\export_ibex_site_code_shares.sql
:r ..\Scripts\Data-Loader\export_procedures\export_ibex_site_formulary.sql
:r ..\Scripts\Data-Loader\export_procedures\export_ibex_site_formulary_differences.sql
:r ..\Scripts\Data-Loader\export_procedures\export_ibex_site_formulary_match.sql
:r ..\Scripts\Data-Loader\export_procedures\export_ibex_site_options.sql
:r ..\Scripts\Data-Loader\export_procedures\export_ibex_sites.sql
:r ..\Scripts\Data-Loader\export_procedures\export_ibex_user_patients.sql
:r ..\Scripts\Data-Loader\export_procedures\export_ibex_user_quick_list_items.sql
:r ..\Scripts\Data-Loader\export_procedures\export_ibex_user_settings.sql
:r ..\Scripts\Data-Loader\export_procedures\export_ibex_users.sql

        -- https://stackoverflow.com/questions/23923366/specifying-a-relative-path-in-post-deployment-sql-files
        :r ..\Scripts\Data-Loader\global_data\actions.sql
        :r ..\Scripts\Data-Loader\global_data\duration_units.sql
        :r ..\Scripts\Data-Loader\global_data\frequency_calendar.sql
        :r ..\Scripts\Data-Loader\global_data\frequency_days.sql
        :r ..\Scripts\Data-Loader\global_data\frequency_interval_units.sql
        :r ..\Scripts\Data-Loader\global_data\frequency_minutes.sql
        :r ..\Scripts\Data-Loader\global_data\frequency_types.sql
        :r ..\Scripts\Data-Loader\global_data\global_options.sql
        :r ..\Scripts\Data-Loader\global_data\notification_categories.sql
        :r ..\Scripts\Data-Loader\global_data\options.sql
        --:r ..\Scripts\Data-Loader\global_data\prompt_groups.sql
        :r ..\Scripts\Data-Loader\global_data\settings.sql
        :r ..\Scripts\Data-Loader\global_data\sites.sql
        :r ..\Scripts\Data-Loader\global_data\order_administration_available_actions.sql
        :r ..\Scripts\Data-Loader\global_data\order_available_actions.sql
        --:r ..\Scripts\Data-Loader\global_data\prompts.sql
        --:r ..\Scripts\Data-Loader\global_data\prompt_choices.sql
        --:r ..\Scripts\Data-Loader\global_data\templates.sql
        --:r ..\Scripts\Data-Loader\global_data\action_route_templates.sql
        --:r ..\Scripts\Data-Loader\global_data\template_prompt_groups.sql

print 'End: Data Load Process';

drop table if exists [#medication_items]; 

:r "..\Programmability\Functions\Table-values Functions\get_antimicrobial_required_fdb.sql"
print 'End: get_antimicrobial_required_fdb.sql';


print 'End: Drop External Reference Procedures';

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
print 'Begin: Create SQL Agent jobs';
declare 
    @sql_agent_cmd                          nvarchar(max)
  , @sql_agent_job_id                       uniqueidentifier
  , @sql_agent_job_name                     nvarchar(128)
  , @sql_agent_category_name                nvarchar(128)
  , @sql_agent_schedule_name                nvarchar(128)
  , @sql_agent_template_job_category        nvarchar(max)
  , @sql_agent_template_job                 nvarchar(max)
  , @sql_agent_template_jobschedule         nvarchar(max)
  , @sql_agent_template_jobserver           nvarchar(max)
  , @sql_agent_template_job_category_delete nvarchar(max)
  , @sql_agent_template_job_delete          nvarchar(max)
  , @sql_agent_template_jobschedule_delete  nvarchar(max)
  , @sql_agent_template_job_step            nvarchar(max);

:r ..\Scripts\Post-Deployment\sql_agent_job.sql
:r ..\Scripts\Post-Deployment\sql_agent_job_order_administrations.sql
:r ..\Scripts\Post-Deployment\sql_agent_job_table_updater.sql
:r ..\Scripts\Post-Deployment\sql_agent_job_queue_cleanup.sql

print 'End: Create SQL Agent jobs';

---
--- Perform ibex customizations
---
if @does_ibex_exist = 1
    begin
        :r "..\ibex_custom_updates\Database_Configuration\enable_broker.sql"
        :r "..\ibex_custom_updates\Tables\emar_update_queue.sql"
        :r "..\ibex_custom_updates\Stored_Procedures\emar_update_queue_maintenance.sql"
        :r "..\ibex_custom_updates\Stored_Procedures\emar_patient_allergies_retrieve_sp.sql"
        :r "..\ibex_custom_updates\Stored_Procedures\emar_patient_medications_retrieve_sp.sql"
        :r "..\ibex_custom_updates\Triggers\emar_patients__hst_d.sql"
        :r "..\ibex_custom_updates\Triggers\emar_patients__hst_i.sql"
        :r "..\ibex_custom_updates\Triggers\emar_patients__hst_u.sql"
        :r "..\ibex_custom_updates\Triggers\emar_patients_d.sql"
        :r "..\ibex_custom_updates\Triggers\emar_patients_i.sql"
        :r "..\ibex_custom_updates\Triggers\emar_patients_u.sql"
        :r "..\ibex_custom_updates\Triggers\emar_patient_allergies_d.sql"
        :r "..\ibex_custom_updates\Triggers\emar_patient_allergies_i.sql"
        :r "..\ibex_custom_updates\Triggers\emar_patient_allergies_u.sql"
        :r "..\ibex_custom_updates\Triggers\emar_patient_indicators_d.sql"
        :r "..\ibex_custom_updates\Triggers\emar_patient_indicators_i.sql"
        :r "..\ibex_custom_updates\Triggers\emar_patient_indicators_u.sql"
        :r "..\ibex_custom_updates\Triggers\emar_patient_medications_d.sql"
        :r "..\ibex_custom_updates\Triggers\emar_patient_medications_i.sql"
        :r "..\ibex_custom_updates\Triggers\emar_patient_medications_u.sql"
        :r "..\ibex_custom_updates\Triggers\emar_patient_problems_d.sql"
        :r "..\ibex_custom_updates\Triggers\emar_patient_problems_i.sql"
        :r "..\ibex_custom_updates\Triggers\emar_patient_problems_u.sql"
        :r "..\ibex_custom_updates\Triggers\emar_sites_d.sql"
        :r "..\ibex_custom_updates\Triggers\emar_sites_i.sql"
        :r "..\ibex_custom_updates\Triggers\emar_sites_u.sql"
        :r "..\ibex_custom_updates\Triggers\emar_users_d.sql"
        :r "..\ibex_custom_updates\Triggers\emar_users_i.sql"
        :r "..\ibex_custom_updates\Triggers\emar_users_u.sql"
        :r "..\ibex_custom_updates\Views\emar_archived_patients_retrieve_view.sql"
        :r "..\ibex_custom_updates\Views\emar_patients_retrieve_view.sql"
        :r "..\ibex_custom_updates\Views\emar_users_retrieve_view.sql"
        :r "..\ibex_custom_updates\Views\emar_personnel_retrieve_view.sql"
        :r "..\ibex_custom_updates\Views\emar_patient_indicators_retrieve_view.sql"
    end;

print 'End: Perform ibex customizations';

-- Update the emar version table
update [target] set
    [update_complete] = sysdatetimeoffset()
from   [dbo].[emar_version] [target]
where  [target].[update_type] = 'SQL'
       and [target].[update_start] =
           (
               select top 1 [q1].[update_start]
               from [dbo].[emar_version] [q1]
               where [q1].[update_type] = 'SQL'
               order by [q1].[update_start] desc
           );

print 'COMPLETED: ScriptPostDeployment.sql';