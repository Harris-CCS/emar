CREATE view [dbo].[drug_interactions_view]
as
-- Union each possible combo to each other.
-- The previous version was one select with case, coalesce, etc...
-- This one is easier to read and lets me selectively apply the
-- logic to handle interactions to a medication within a combo med
-- when med 2 is a combo med but to not apply it when med 2 is not
-- a combo med.
-- Winston Murdock, 09/13/2022.  PC-27429
-- 1) patient cart order -> patient cart order combo med
-- 2) patient cart order -> patient cart order non combo med
-- 3) patient cart order -> patient order combo med
-- 4) patient cart order -> patient order non combo med
-- 5) patient cart order -> home medication
-- 6) patient order -> patient order combo med
-- 7) patient order -> patient order non combo med
-- 8) patient order -> home medication

-- Patient cart order -> patient cart order combo med
    select [mi].[id]
         , [mi].[interaction_drug_1]
         , [mi].[interaction_drug_2]
         , [mi].[severity]
         , [mi].[override_reason_id]
         , [mi].[override_reason_user_id]
         , [mi].[override_reason_datetime]
         , [oi_2].[patient_cart_order_id] as  [interaction_order_id]
         , 'patient_cart_orders'as [interaction_order_table]
 		 , [pco1].[patient_id] as [patient_id]
		 , dbo.get_medication_detail_name_for_pc_routed_gen_id(mi.interaction_drug_2, pco2.medication_id) as [interaction_order_name]
    from   [medication_interactions] as [mi]
           inner join [order_interactions] as [oi_1] on [oi_1].[medication_interaction_id] = [mi].[id]
                                                        and [oi_1].[drug_num] = 1
           inner join [order_interactions] as [oi_2] on [oi_2].[medication_interaction_id] = [mi].[id]
                                                        and [oi_2].[drug_num] = 2
           inner join [patient_cart_orders] as [pco1] on [oi_1].[patient_cart_order_id] = [pco1].[id]
           inner join [medication_details] as [pcom1] on [pco1].[medication_id] = [pcom1].[medication_id]
           inner join [patient_cart_orders] as [pco2] on [oi_2].[patient_cart_order_id] = [pco2].[id]
           inner join [medication_details] as [pcom2] on [pco2].[medication_id] = [pcom2].[medication_id]
		   -- Join to medication for cart order 2 so that we know if it's a combo med or not.
		   inner join medications as m2 on pco2.medication_id = m2.id
	WHERE m2.drug_id = 'COMBO'

	UNION

-- Patient cart order -> patient cart order non combo med
    select [mi].[id]
         , [mi].[interaction_drug_1]
         , [mi].[interaction_drug_2]
         , [mi].[severity]
         , [mi].[override_reason_id]
         , [mi].[override_reason_user_id]
         , [mi].[override_reason_datetime]
         , [oi_2].[patient_cart_order_id] as  [interaction_order_id]
         , 'patient_cart_orders'as [interaction_order_table]
 		 , [pco1].[patient_id] as [patient_id]
		 , [pcom2].[brand_name] as [interaction_order_name]
    from   [medication_interactions] as [mi]
           inner join [order_interactions] as [oi_1] on [oi_1].[medication_interaction_id] = [mi].[id]
                                                        and [oi_1].[drug_num] = 1
           inner join [order_interactions] as [oi_2] on [oi_2].[medication_interaction_id] = [mi].[id]
                                                        and [oi_2].[drug_num] = 2
           inner join [patient_cart_orders] as [pco1] on [oi_1].[patient_cart_order_id] = [pco1].[id]
           inner join [medication_details] as [pcom1] on [pco1].[medication_id] = [pcom1].[medication_id]
           inner join [patient_cart_orders] as [pco2] on [oi_2].[patient_cart_order_id] = [pco2].[id]
           inner join [medication_details] as [pcom2] on [pco2].[medication_id] = [pcom2].[medication_id]
		   -- Join to medication for cart order 2 so that we know if it's a combo med or not.
		   inner join medications as m2 on pco2.medication_id = m2.id
	WHERE m2.drug_id <> 'COMBO'

	UNION

	-- Patient cart order -> patient order combo med
    select [mi].[id]
         , [mi].[interaction_drug_1]
         , [mi].[interaction_drug_2]
         , [mi].[severity]
         , [mi].[override_reason_id]
         , [mi].[override_reason_user_id]
         , [mi].[override_reason_datetime]
         , [oi_2].[patient_cart_order_id] as  [interaction_order_id]
         , 'patient_orders'as [interaction_order_table]
 		 , [pco1].[patient_id] as [patient_id]
		 , dbo.get_medication_detail_name_for_pc_routed_gen_id(mi.interaction_drug_2, po2.medication_id) as [interaction_order_name]
    from   [medication_interactions] as [mi]
           inner join [order_interactions] as [oi_1] on [oi_1].[medication_interaction_id] = [mi].[id]
                                                        and [oi_1].[drug_num] = 1
           inner join [order_interactions] as [oi_2] on [oi_2].[medication_interaction_id] = [mi].[id]
                                                        and [oi_2].[drug_num] = 2
           inner join [patient_cart_orders] as [pco1] on [oi_1].[patient_cart_order_id] = [pco1].[id]
           inner join [medication_details] as [pcom1] on [pco1].[medication_id] = [pcom1].[medication_id]
           inner join [patient_orders] as [po2] on [oi_2].[patient_order_id] = [po2].[id]
           inner join [medication_details] as [pom2] on [po2].[medication_id] = [pom2].[medication_id]
		   -- Join to medication for patient order 2 so that we know if it's a combo med or not.
		   inner join medications as m2 on po2.medication_id = m2.id
	WHERE m2.drug_id = 'COMBO'

	UNION

		-- Patient cart order -> patient order non combo med
    select [mi].[id]
         , [mi].[interaction_drug_1]
         , [mi].[interaction_drug_2]
         , [mi].[severity]
         , [mi].[override_reason_id]
         , [mi].[override_reason_user_id]
         , [mi].[override_reason_datetime]
         , [oi_2].[patient_cart_order_id] as  [interaction_order_id]
         , 'patient_orders'as [interaction_order_table]
 		 , [pco1].[patient_id] as [patient_id]        
		 , [pom2].[brand_name] as [interaction_order_name]
    from   [medication_interactions] as [mi]
           inner join [order_interactions] as [oi_1] on [oi_1].[medication_interaction_id] = [mi].[id]
                                                        and [oi_1].[drug_num] = 1
           inner join [order_interactions] as [oi_2] on [oi_2].[medication_interaction_id] = [mi].[id]
                                                        and [oi_2].[drug_num] = 2
           inner join [patient_cart_orders] as [pco1] on [oi_1].[patient_cart_order_id] = [pco1].[id]
           inner join [medication_details] as [pcom1] on [pco1].[medication_id] = [pcom1].[medication_id]
           inner join [patient_orders] as [po2] on [oi_2].[patient_order_id] = [po2].[id]
           inner join [medication_details] as [pom2] on [po2].[medication_id] = [pom2].[medication_id]
		   -- Join to medication for patient order 2 so that we know if it's a combo med or not.
		   inner join medications as m2 on po2.medication_id = m2.id
	WHERE m2.drug_id <> 'COMBO'

	UNION

	-- Patient cart order -> current medication
    select [mi].[id]
         , [mi].[interaction_drug_1]
         , [mi].[interaction_drug_2]
         , [mi].[severity]
         , [mi].[override_reason_id]
         , [mi].[override_reason_user_id]
         , [mi].[override_reason_datetime]
         , [oi_2].[patient_cart_order_id] as  [interaction_order_id]
         , 'patient_home_medications'as [interaction_order_table]
 		 , [pco1].[patient_id] as [patient_id]        
		 , [phm2].[name] as [interaction_order_name]
    from   [medication_interactions] as [mi]
           inner join [order_interactions] as [oi_1] on [oi_1].[medication_interaction_id] = [mi].[id]
                                                        and [oi_1].[drug_num] = 1
           inner join [order_interactions] as [oi_2] on [oi_2].[medication_interaction_id] = [mi].[id]
                                                        and [oi_2].[drug_num] = 2
           inner join [patient_cart_orders] as [pco1] on [oi_1].[patient_cart_order_id] = [pco1].[id]
           inner join [medication_details] as [pcom1] on [pco1].[medication_id] = [pcom1].[medication_id]
           inner join [patient_home_medications] as [phm2] on [oi_2].[patient_home_medication_id] = [phm2].[id]

	UNION

	-- Patient order -> patient order combo med
	select [mi].[id]
         , [mi].[interaction_drug_1]
         , [mi].[interaction_drug_2]
         , [mi].[severity]
         , [mi].[override_reason_id]
         , [mi].[override_reason_user_id]
         , [mi].[override_reason_datetime]
         , [oi_2].[patient_cart_order_id] as  [interaction_order_id]
         , 'patient_orders'as [interaction_order_table]
 		 , [po1].[patient_id] as [patient_id]        
		 , dbo.get_medication_detail_name_for_pc_routed_gen_id(mi.interaction_drug_2, po2.medication_id) as [interaction_order_name]
    from   [medication_interactions] as [mi]
           inner join [order_interactions] as [oi_1] on [oi_1].[medication_interaction_id] = [mi].[id]
                                                        and [oi_1].[drug_num] = 1
           inner join [order_interactions] as [oi_2] on [oi_2].[medication_interaction_id] = [mi].[id]
                                                        and [oi_2].[drug_num] = 2
           inner join [patient_orders] as [po1] on [oi_1].[patient_order_id] = [po1].[id]
           inner join [medication_details] as [pom1] on [po1].[medication_id] = [pom1].[medication_id]
           inner join [patient_orders] as [po2] on [oi_2].[patient_order_id] = [po2].[id]
           inner join [medication_details] as [pom2] on [po2].[medication_id] = [pom2].[medication_id]
		   -- Join to medication for patient order 2 so that we know if it's a combo med or not.
		   inner join medications as m2 on po2.medication_id = m2.id
	WHERE m2.drug_id = 'COMBO'

	UNION

	-- Patient order -> patient order non combo med
	select [mi].[id]
         , [mi].[interaction_drug_1]
         , [mi].[interaction_drug_2]
         , [mi].[severity]
         , [mi].[override_reason_id]
         , [mi].[override_reason_user_id]
         , [mi].[override_reason_datetime]
         , [oi_2].[patient_cart_order_id] as  [interaction_order_id]
         , 'patient_orders'as [interaction_order_table]
 		 , [po1].[patient_id] as [patient_id]        
		 , [pom2].[brand_name] as [interaction_order_name]
    from   [medication_interactions] as [mi]
           inner join [order_interactions] as [oi_1] on [oi_1].[medication_interaction_id] = [mi].[id]
                                                        and [oi_1].[drug_num] = 1
           inner join [order_interactions] as [oi_2] on [oi_2].[medication_interaction_id] = [mi].[id]
                                                        and [oi_2].[drug_num] = 2
           inner join [patient_orders] as [po1] on [oi_1].[patient_order_id] = [po1].[id]
           inner join [medication_details] as [pom1] on [po1].[medication_id] = [pom1].[medication_id]
           inner join [patient_orders] as [po2] on [oi_2].[patient_order_id] = [po2].[id]
           inner join [medication_details] as [pom2] on [po2].[medication_id] = [pom2].[medication_id]
		   -- Join to medication for patient order 2 so that we know if it's a combo med or not.
		   inner join medications as m2 on po2.medication_id = m2.id
	WHERE m2.drug_id <> 'COMBO'

	UNION

	-- Patient order -> current medication
	select [mi].[id]
         , [mi].[interaction_drug_1]
         , [mi].[interaction_drug_2]
         , [mi].[severity]
         , [mi].[override_reason_id]
         , [mi].[override_reason_user_id]
         , [mi].[override_reason_datetime]
         , [oi_2].[patient_cart_order_id] as  [interaction_order_id]
         , 'patient_home_medications'as [interaction_order_table]
 		 , [po1].[patient_id] as [patient_id]        
		 , [phm2].[name] as [interaction_order_name]
    from   [medication_interactions] as [mi]
           inner join [order_interactions] as [oi_1] on [oi_1].[medication_interaction_id] = [mi].[id]
                                                        and [oi_1].[drug_num] = 1
           inner join [order_interactions] as [oi_2] on [oi_2].[medication_interaction_id] = [mi].[id]
                                                        and [oi_2].[drug_num] = 2
           inner join [patient_orders] as [po1] on [oi_1].[patient_order_id] = [po1].[id]
           inner join [medication_details] as [pom1] on [po1].[medication_id] = [pom1].[medication_id]
		   inner join [patient_home_medications] as [phm2] on [oi_2].[patient_home_medication_id] = [phm2].[id]

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
