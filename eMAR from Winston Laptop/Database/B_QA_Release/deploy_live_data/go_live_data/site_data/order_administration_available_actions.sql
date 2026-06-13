print 'Loading Table: order_administration_available_actions Local';

/******************************************************************
order_administration_available_actions LOCAL
Loads Site Specific Defaults from the "GLOBAL" dataset (site_id=-1)
******************************************************************/

with cte_values
     as (select [site].[id] as [site_id]
              , [reference].[order_status]
              , [reference].[administration_status]
              , [reference].[point_in_time]
              , [reference].[available_action_id]
         from   [dbo].[sites] as [site]
                cross join [dbo].[order_administration_available_actions] as [reference]
         where  [site].[id] not in
         (
             select distinct 
                    [site_id]
             from [dbo].[order_administration_available_actions]
         )
                and [reference].[site_id] = -1)
     merge into [dbo].[order_administration_available_actions] [target]
     using [cte_values] [source]
     on [source].[site_id] = [target].[site_id]
        and [source].[order_status] = [target].[order_status]
        and [source].[administration_status] = [target].[administration_status]
        and isnull([source].[point_in_time], 0) = isnull([target].[point_in_time], 0)
        and [source].[available_action_id] = [target].[available_action_id]
         when not matched
             then
           insert([site_id]
                , [order_status]
                , [administration_status]
                , [point_in_time]
                , [available_action_id])
           values
         ([site_id], [order_status], [administration_status], [point_in_time], [available_action_id]);