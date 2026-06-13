create view [dbo].[drug_interactions_view]
as
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
		, coalesce([phm2].[name], [pcom2].[brand_name], [pom2].[brand_name]) as                                        [interaction_order_name]
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
           left outer join [patient_home_medications] as [phm2] on [oi_2].[patient_home_medication_id] = [phm2].[id];
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
