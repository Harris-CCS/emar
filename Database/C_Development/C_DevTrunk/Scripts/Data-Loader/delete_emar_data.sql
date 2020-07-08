/****************************
delete permanent all data
    delete performed in hierarchal sequence
****************************/
set nocount on;
/* Delete Hierarchal Order
LVL: 099 SEQ: 001 TBL: dbo.external_ids
LVL: 003 SEQ: 001 TBL: dbo.patient_cart_details
LVL: 002 SEQ: 001 TBL: dbo.patient_allergies
LVL: 002 SEQ: 002 TBL: dbo.patient_carts
LVL: 002 SEQ: 003 TBL: dbo.patient_home_medications
LVL: 002 SEQ: 004 TBL: dbo.patient_indicators
LVL: 002 SEQ: 005 TBL: dbo.user_permissions
LVL: 002 SEQ: 006 TBL: dbo.user_quick_list
LVL: 001 SEQ: 001 TBL: dbo.patients
LVL: 001 SEQ: 002 TBL: dbo.site_code_shares
LVL: 001 SEQ: 003 TBL: dbo.site_formulary
LVL: 001 SEQ: 004 TBL: dbo.site_formulary_match
LVL: 001 SEQ: 005 TBL: dbo.site_options
LVL: 001 SEQ: 006 TBL: dbo.site_preferred_list
LVL: 001 SEQ: 007 TBL: dbo.users
LVL: 000 SEQ: 001 TBL: dbo.medication_routes
LVL: 000 SEQ: 002 TBL: dbo.options
LVL: 000 SEQ: 003 TBL: dbo.permissions
LVL: 000 SEQ: 004 TBL: dbo.sites
*/

	print 'BEGIN: delete_emar_data.sql'

    delete [dbo].[external_ids];
    delete [dbo].[patient_cart_details];
    delete [dbo].[patient_allergies];
    delete [dbo].[patient_carts];
    delete [dbo].[patient_home_medications];
    delete [dbo].[patient_indicators];
    delete [dbo].[user_permissions];
    delete [dbo].[user_quick_list];
    delete [dbo].[patients];
    delete [dbo].[site_code_shares];
    delete [dbo].[site_formulary];
    delete [dbo].[site_formulary_match];
    delete [dbo].[site_options];
    delete [dbo].[site_preferred_list];
    delete [dbo].[users];
    delete [dbo].[medication_routes];
    delete [dbo].[options];
    delete [dbo].[permissions];
    delete [dbo].[sites];

    dbcc checkident('[dbo].[patient_cart_details]',reseed,1) with no_infomsgs;
    dbcc checkident('[dbo].[patient_allergies]',reseed,1) with no_infomsgs;
    dbcc checkident('[dbo].[patient_carts]',reseed,1) with no_infomsgs;
    dbcc checkident('[dbo].[patient_home_medications]',reseed,1) with no_infomsgs;
    dbcc checkident('[dbo].[patient_indicators]',reseed,1) with no_infomsgs;
    dbcc checkident('[dbo].[user_permissions]',reseed,1) with no_infomsgs;
    dbcc checkident('[dbo].[user_quick_list]',reseed,1) with no_infomsgs;
    dbcc checkident('[dbo].[patients]',reseed,1) with no_infomsgs;
    dbcc checkident('[dbo].[site_code_shares]',reseed,1) with no_infomsgs;
    dbcc checkident('[dbo].[site_formulary]',reseed,1) with no_infomsgs;
    dbcc checkident('[dbo].[site_formulary_match]',reseed,1) with no_infomsgs;
    dbcc checkident('[dbo].[site_options]',reseed,1) with no_infomsgs;
    dbcc checkident('[dbo].[site_preferred_list]',reseed,1) with no_infomsgs;
    dbcc checkident('[dbo].[users]',reseed,1) with no_infomsgs;
    dbcc checkident('[dbo].[medication_routes]',reseed,1) with no_infomsgs;
    dbcc checkident('[dbo].[options]',reseed,1) with no_infomsgs;
    dbcc checkident('[dbo].[permissions]',reseed,1) with no_infomsgs;
    dbcc checkident('[dbo].[sites]',reseed,1) with no_infomsgs;

	print 'COMPLETE: delete_emar_data.sql'
