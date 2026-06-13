create view [dbo].[drug_interactions_view]
as
	-- You can't declare a table variable in a SQL view.
	-- But you can use create a common table expression and then select from that.
	-- Thus, I am selecting the interactions into the CTE, and then doing a select
	-- distinct from it.
	-- Also, I've changed this to call my function that takes the pc_routed_gen_id
	-- and medication_id as parameters and returns medication_details.brand_name.
	-- We need this because anything that has an interaction with one detail inside
	-- a combo med was returning whatever detail happened to be listed first for the
	-- combo med and was also duplicating that interaction as many time as there
	-- were details in the combo med.
	-- Winston Murdock, 01/26/2023.  PC-27720
	with  ret_table
	AS
	(
		select [mi].[id]
			, [mi].[interaction_drug_1]
			, [mi].[interaction_drug_2]
			, [mi].[severity]
			, [mi].[override_reason_id]
			, [mi].[override_reason_user_id]
			, [mi].[override_reason_datetime]
			, coalesce([oi_2].[patient_order_id], [oi_2].[patient_cart_order_id], [oi_2].[patient_home_medication_id]) as  [interaction_order_id]
			, case
				when [oi_2].[patient_order_id] is not null
					then 'patient_orders'
				when [oi_2].[patient_cart_order_id] is not null
					then 'patient_cart_orders'
				when [oi_2].[patient_home_medication_id] is not null
					then 'patient_home_medications'
			end as                                                                                                       [interaction_order_table]
 			, case
				when [po1].[patient_id] is not null
					then [po1].[patient_id]
				when [pco1].[patient_id] is not null
					then [pco1].[patient_id]
				when [phm1].[patient_id] is not null
					then [phm1].[patient_id]
			end as                                                                                                       [patient_id]        
			-- If this is a combo med, then we need to go out and get the exact medication detail within the medication.
			-- Else, just use the name from the coalesce statement comemnted out below.
			-- Make calls to my function in the "then" section of the case/when/else section below.
			-- Winston Murdock, 01/24/2023.
			,  case
			when [pcomed2].[drug_id] = 'COMBO'
				--Cart order for a combo med.
				then dbo.get_medication_detail_name_for_pc_routed_gen_id(mi.interaction_drug_2, pco2.medication_id)
			when [pomed2].[drug_id] = 'COMBO'
				-- Order for a combo med.
				then dbo.get_medication_detail_name_for_pc_routed_gen_id(mi.interaction_drug_2, po2.medication_id)
			-- Current meds cannot be a combo med.
			-- They would enter the individual meds (fenatNYL, Donnatal, Maalox, Lidocaine, etc...) individually.
			-- That's why there's no case for current medication here.
			else
				-- Not a combo med.  Do the same coalesce logic we use throughout this view to
				-- get the name for which one this one joins to.
				coalesce([phm2].[name], [pcom2].[brand_name], [pom2].[brand_name])
			end as                                                                                                       [interaction_order_name]
		from   [medication_interactions] as [mi]
				inner join [order_interactions] as [oi_1] on [oi_1].[medication_interaction_id] = [mi].[id]
															and [oi_1].[drug_num] = 1
				inner join [order_interactions] as [oi_2] on [oi_2].[medication_interaction_id] = [mi].[id]
															and [oi_2].[drug_num] = 2
				left outer join [patient_orders] as [po1] on [oi_1].[patient_order_id] = [po1].[id]
				left outer join [medication_details] as [pom1] on [po1].[medication_id] = [pom1].[medication_id]
				left outer join [patient_orders] as [po2] on [oi_2].[patient_order_id] = [po2].[id]
				left outer join [medication_details] as [pom2] on [po2].[medication_id] = [pom2].[medication_id]
				left outer join [patient_cart_orders] as [pco1] on [oi_1].[patient_cart_order_id] = [pco1].[id]
				left outer join [medication_details] as [pcom1] on [pco1].[medication_id] = [pcom1].[medication_id]
				left outer join [patient_cart_orders] as [pco2] on [oi_2].[patient_cart_order_id] = [pco2].[id]
				left outer join [medication_details] as [pcom2] on [pco2].[medication_id] = [pcom2].[medication_id]
				left outer join [patient_home_medications] as [phm1] on [oi_1].[patient_home_medication_id] = [phm1].[id]
				left outer join [patient_home_medications] as [phm2] on [oi_2].[patient_home_medication_id] = [phm2].[id]

				-- Add joins to the medication table so that we can pass along the medication id to the
				-- scalar function that gets the detail's brand name for a specific item inside a combo med.
				-- There is no need for a join to the medications table for any current medications.
				-- Current medications can never be combo meds (per Romel).
				left outer join [medications] as [pcomed2] on [pco2].[medication_id] = [pcomed2].id
				left outer join [medications] as [pomed2] on [po2].[medication_id] = [pomed2].id
	) -- end creation of the ret_table CTE

	-- Remove duplicate entries for interactions to combo meds.
	-- Listing the individual columns is faster than doing select *.
	-- The order on these columns matters.  If it's not in this exact order,
	-- then Entity Framework will barf on it when this gets into the API.
	--select distinct *
	SELECT DISTINCT
		id, interaction_drug_1, interaction_drug_2,
		severity, override_reason_id, override_reason_user_id,
		override_reason_datetime, interaction_order_id, interaction_order_name,
		interaction_order_table, patient_id
	FROM ret_table;
go
-- Data Dictionary
--    View

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'View to display drug interactions'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'VIEW'
  , @level1name = N'drug_interactions_view';
go
