print 'Loading Table: order_available_actions LOCAL';

/******************************************************************
order_available_actions LOCAL
Loads Site Specific Defaults from the "GLOBAL" dataset (site_id=-1)
******************************************************************/

with cte_values
     as (select [site].[id] as [site_id]
              , [reference].[order_status]
              , [reference].[available_action_id]
              , [reference].[is_pit]
              , [reference].[is_prn_only]
         from   [dbo].[sites] as [site]
                cross join [dbo].[order_available_actions] as [reference]
         where  [site].[id] not in
         (
             select distinct 
                    [site_id]
             from [dbo].[order_available_actions]
         )
                and [reference].[site_id] = -1)
     merge into [dbo].[order_available_actions] [target]
     using [cte_values] [source]
     on [source].[site_id] = [target].[site_id]
        and [source].[order_status] = [target].[order_status]
        and [source].[available_action_id] = [target].[available_action_id]
         when not matched
             then
           insert([site_id]
                , [order_status]
                , [available_action_id]
                , [is_pit]
                , [is_prn_only])
           values
         ([site_id], [order_status], [available_action_id], [is_pit], [is_prn_only]);