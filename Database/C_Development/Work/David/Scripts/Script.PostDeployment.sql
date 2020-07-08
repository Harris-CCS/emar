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

if '$(load_data)' in('sample','live')
begin
   :r ..\Scripts\Data-Loader\delete_emar_data.sql
   :r ..\Scripts\Data-Loader\data_loader_ddl.sql
end

/* Insert table order 
LVL: 000 SEQ: 001 TBL: dbo.medication_routes
LVL: 000 SEQ: 002 TBL: dbo.options
LVL: 000 SEQ: 003 TBL: dbo.permissions
LVL: 000 SEQ: 004 TBL: dbo.sites
LVL: 001 SEQ: 001 TBL: dbo.patients
LVL: 001 SEQ: 002 TBL: dbo.site_code_shares
LVL: 001 SEQ: 003 TBL: dbo.site_formulary
LVL: 001 SEQ: 004 TBL: dbo.site_formulary_match
LVL: 001 SEQ: 005 TBL: dbo.site_options
LVL: 001 SEQ: 006 TBL: dbo.site_preferred_list
LVL: 001 SEQ: 007 TBL: dbo.users
LVL: 002 SEQ: 001 TBL: dbo.patient_allergies
LVL: 002 SEQ: 002 TBL: dbo.patient_carts
LVL: 002 SEQ: 003 TBL: dbo.patient_home_medications
LVL: 002 SEQ: 004 TBL: dbo.patient_indicators
LVL: 002 SEQ: 005 TBL: dbo.user_permissions
LVL: 002 SEQ: 006 TBL: dbo.user_quick_list
LVL: 003 SEQ: 001 TBL: dbo.patient_cart_details
*/
if '$(load_data)' = 'sample'
begin
   :r ..\Scripts\Data-Loader\sample_data\sites.sql
   :r ..\Scripts\Data-Loader\sample_data\patients.sql
   :r ..\Scripts\Data-Loader\sample_data\users.sql
end

if '$(load_data)' = 'live'
and exists(select null from master.sys.databases where name = 'ibex')
begin
   :r ..\Scripts\Data-Loader\ibex_live_data\sites.sql
   :r ..\Scripts\Data-Loader\ibex_live_data\patients.sql
   :r ..\Scripts\Data-Loader\ibex_live_data\users.sql
end

--- variables global to all diagram_ published scripts
declare
    @diagram_id      [int]
  , @version_current [int]
  , @version         [int]
  , @continue_update [bit];

declare @outputs table([Id] int not null);

--- deploying these diagrams in having an issue at the moment.
--- it worked several times, but now causes ssms to crash. so removing for the moment.
---:r ..\Scripts\Post-Deployment\diagram_patients.sql
---:r ..\Scripts\Post-Deployment\diagram_security.sql
