create view [dbo].[allergy_reactions_view]
as 
   select [reactions].[id]
        , [reactions].[patient_allergy_id]
        , [pa].[name] as                                                                   [patient_allergy_name]
        , case
              when [reactions].[patient_order_id] is not null
                  then 'patient_orders'
              when [reactions].[patient_cart_order_id] is not null
                  then 'patient_cart_orders'
          end as                                                                           [order_table]
        , coalesce([reactions].[patient_order_id], [reactions].[patient_cart_order_id]) as [order_id]
        , coalesce([md_po].[brand_name], [md_pco].[brand_name]) as                         [order_brand_name]
        , [reactions].[override_reason_id]
        , [reactions].[override_reason_user_id]
        , [reactions].[override_reason_datetime]
   from   [order_reactions] as [reactions]
          left outer join [patient_cart_orders] as [pco] on [reactions].[patient_cart_order_id] = [pco].[id]
          left outer join [medications] as [m_pco] on [pco].[medication_id] = [m_pco].[id]
          left outer join [medication_details] as [md_pco] on [m_pco].[id] = [md_pco].[medication_id]
          left outer join [patient_orders] as [po] on [reactions].[patient_order_id] = [po].[id]
          left outer join [medications] as [m_po] on [po].[medication_id] = [m_po].[id]
          left outer join [medication_details] as [md_po] on [po].[medication_id] = [m_po].[id]
          left outer join [patient_allergies] as [pa] on [reactions].[patient_allergy_id] = [pa].[id];
go