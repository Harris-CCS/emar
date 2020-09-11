if '$(load_data)' in('sample', 'live')
    begin

        print 'Loading Table: hsi-an';

        update [users] set    
            [site_id] = [sites].[id]
        from   [dbo].[users]
               cross join [dbo].[sites]
        where  [users].[login_name] = 'dev_user'
               and [sites].[name] = 'Automation Test_Multum';

        insert into [dbo].[user_quick_list_items]
            ([site_id]
           , [user_id]
           , [medication_id]
           , [dose]
           , [medication_unit_id]
           , [medication_route_id]
           , [frequency_schedule_id]
           , [order_notes]
           , [usages_this_week]
           , [weekly_usage_rolling_average]
            )
        select [target_sites].[id] as [site_id]
             , [target_user].[id] as  [user_id]
             , [uqli].[medication_id]
             , [uqli].[dose]
             , [uqli].[medication_unit_id]
             , [uqli].[medication_route_id]
             , [uqli].[frequency_schedule_id]
             , [uqli].[order_notes]
             , [uqli].[usages_this_week]
             , [uqli].[weekly_usage_rolling_average]
        from   [dbo].[user_quick_list_items] as [uqli]
               cross join [dbo].[users] as [source_user]
               cross join [dbo].[users] as [target_user]
               cross join [dbo].[sites] as [source_sites]
               cross join [dbo].[sites] as [target_sites]
        where  [target_user].[login_name] = 'dev_user'
               and [target_user].[site_id] = [target_sites].[id]
               and [source_user].[login_name] = 'jedi'
               and [source_user].[site_id] = [source_sites].[id]
               and [target_sites].[name] = 'Automation Test_Multum'
               and [source_sites].[name] = 'Pulsecheck Hospital'
               and [uqli].user_id = [source_user].[id]
               and [uqli].[site_id] = [source_sites].[id];
    end;