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
-- Table Renamed dbo.site_preferred_list dbo.department_preferred_list
drop table if exists dbo.site_preferred_list;
-- Table Renamed dbo.department_preferred_list dbo.department_preferred_list_items
drop table if exists dbo.department_preferred_list;
-- Table Renamed dbo.user_quick_list dbo.user_quick_list_items
drop table if exists dbo.user_quick_list;
-- Table Renamed dbo.patient_cart_details dbo.patient_cart_orders
drop table if exists dbo.patient_cart_details
-- Table no longer used
drop table if exists dbo.patient_carts

declare
    @max_id bigint;

/* Insert table order
LVL: 000 SEQ: 001 TBL: dbo.actions
LVL: 000 SEQ: 002 TBL: dbo.fdb_allergy_name
LVL: 000 SEQ: 003 TBL: dbo.fdb_brand_name
LVL: 000 SEQ: 004 TBL: dbo.fdb_ndc_info
LVL: 000 SEQ: 005 TBL: dbo.medication_routes
LVL: 000 SEQ: 006 TBL: dbo.options
LVL: 000 SEQ: 007 TBL: dbo.permissions
LVL: 000 SEQ: 008 TBL: dbo.prompt_groups
LVL: 000 SEQ: 009 TBL: dbo.sites
LVL: 000 SEQ: 010 TBL: dbo.templates
LVL: 001 SEQ: 001 TBL: dbo.action_route_templates
LVL: 001 SEQ: 002 TBL: dbo.department_preferred_list_items
LVL: 001 SEQ: 003 TBL: dbo.group_list_items
LVL: 001 SEQ: 004 TBL: dbo.override_reasons
LVL: 001 SEQ: 005 TBL: dbo.patient_cart_orders
LVL: 001 SEQ: 006 TBL: dbo.patients
LVL: 001 SEQ: 007 TBL: dbo.prompts
LVL: 001 SEQ: 008 TBL: dbo.site_code_shares
LVL: 001 SEQ: 009 TBL: dbo.site_formulary
LVL: 001 SEQ: 010 TBL: dbo.site_formulary_match
LVL: 001 SEQ: 011 TBL: dbo.site_options
LVL: 001 SEQ: 012 TBL: dbo.template_prompt_groups
LVL: 001 SEQ: 013 TBL: dbo.users
LVL: 002 SEQ: 001 TBL: dbo.cart_order_administrations
LVL: 002 SEQ: 002 TBL: dbo.patient_allergies
LVL: 002 SEQ: 003 TBL: dbo.patient_home_medications
LVL: 002 SEQ: 004 TBL: dbo.patient_indicators
LVL: 002 SEQ: 005 TBL: dbo.patient_orders
LVL: 002 SEQ: 006 TBL: dbo.prompt_choices
LVL: 002 SEQ: 007 TBL: dbo.user_permissions
LVL: 002 SEQ: 008 TBL: dbo.user_quick_list_items
LVL: 003 SEQ: 001 TBL: dbo.order_administrations
LVL: 004 SEQ: 001 TBL: dbo.order_administration_notes
LVL: 004 SEQ: 002 TBL: dbo.order_events
LVL: 005 SEQ: 001 TBL: dbo.order_event_details
*/
-- https://stackoverflow.com/questions/23923366/specifying-a-relative-path-in-post-deployment-sql-files
:r ..\Scripts\Data-Loader\global_data\fdb_allergy_name.sql
:r ..\Scripts\Data-Loader\global_data\fdb_brand_name.sql
:r ..\Scripts\Data-Loader\global_data\fdb_ndc_info.sql
:r ..\Scripts\Data-Loader\site_data\medication_routes.sql
:r ..\Scripts\Data-Loader\global_data\options.sql
:r ..\Scripts\Data-Loader\site_data\sites.sql
:r ..\Scripts\Data-Loader\site_data\group_list_items.sql
:r ..\Scripts\Data-Loader\phi_data\patients.sql
:r ..\Scripts\Data-Loader\site_data\site_formulary.sql
:r ..\Scripts\Data-Loader\site_data\site_formulary_match.sql
:r ..\Scripts\Data-Loader\site_data\site_options.sql
:r ..\Scripts\Data-Loader\user_data\users.sql
:r ..\Scripts\Data-Loader\phi_data\patient_allergies.sql
:r ..\Scripts\Data-Loader\phi_data\patient_home_medications.sql
--- BEGIN: custom data deployments for development
:r ..\Scripts\Data-Loader\development_data\antoni_data.sql
--- END: custom data deployments for development
:r ..\Scripts\Data-Loader\phi_data\patient_orders.sql
:r ..\Scripts\Data-Loader\user_data\user_quick_list_items.sql
--- BEGIN: custom data deployments for development
:r ..\Scripts\Data-Loader\development_data\bradley_data.sql
--- END: custom data deployments for development



-- procedures were only needed for data import process and are no longer needed.
drop procedure if exists [dbo].[export_ibex_fdb_allergy_name];
drop procedure if exists [dbo].[export_ibex_fdb_brand_name];
drop procedure if exists [dbo].[export_ibex_fdb_ndc_info];
drop procedure if exists [dbo].[export_ibex_medication_routes];
drop procedure if exists [dbo].[export_ibex_sites];
drop procedure if exists [dbo].[export_ibex_group_list_items];
drop procedure if exists [dbo].[export_ibex_patients];
drop procedure if exists [dbo].[export_ibex_site_formulary];
drop procedure if exists [dbo].[export_ibex_site_formulary_match];
drop procedure if exists [dbo].[export_ibex_users];
drop procedure if exists [dbo].[export_ibex_patient_allergies];
drop procedure if exists [dbo].[export_ibex_patient_home_medications];
drop procedure if exists [dbo].[export_ibex_patient_orders];
drop procedure if exists [dbo].[export_ibex_user_quick_list_items];

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
