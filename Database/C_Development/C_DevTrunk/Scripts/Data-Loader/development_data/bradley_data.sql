if '$(load_data)' in('sample', 'live')
    begin

        print 'Loading Table: bradley_data';

        update [dbo].[patients] set    
            [first_name] = '   ' + [first_name] + '   '
          , [middle_name] = '   ' + [middle_name] + '   '
          , [last_name] = '   ' + [last_name] + '   '
          , [name_suffix] = '   ' + 'MD' + '   '
        where  [first_name] = 'Lillian'
               and [last_name] = 'Infobutton';

        update [dbo].[patients] set    
            [middle_name] = 'A.'
        where  [first_name] = 'Chester'
               and [last_name] = 'Arthur';

        update [dbo].[users] set    
            [first_name] = 'Heather'
          , [last_name] = 'Abebe'
          , [middle_name] = 'L'
          , [name_suffix] = 'P.A.C.'
        from   [dbo].[get_internal_id]('pulsecheck', 'users', 10087) [internal_id]
               inner join [dbo].[users] [users] on [internal_id].[id] = [users].[id];

        declare 
            @scriptB_user_Id int =
        (
            select [internal_id]
            from   [dbo].[external_ids]
            where  [external_id] = '10147'
                   and [vendor] = 'pulsecheck'
                   and [entity] = 'users'
        );

        update [dbo].[user_quick_list_items] set    
            [weekly_usage_rolling_average] = -1
        where  [user_id] = @scriptB_user_Id;

        set rowcount 100;
        update [dbo].[user_quick_list_items] set    
            [weekly_usage_rolling_average] = 1.0
        where  [user_id] = @scriptB_user_Id;

        set rowcount 80;
        update [dbo].[user_quick_list_items] set    
            [weekly_usage_rolling_average]+=1.0
        where  [user_id] = @scriptB_user_Id;

        set rowcount 60;
        update [dbo].[user_quick_list_items] set    
            [weekly_usage_rolling_average]+=1.0
        where  [user_id] = @scriptB_user_Id;

        set rowcount 30;
        update [dbo].[user_quick_list_items] set    
            [weekly_usage_rolling_average]+=1.0
        where  [user_id] = @scriptB_user_Id;

        set rowcount 20;
        update [dbo].[user_quick_list_items] set    
            [weekly_usage_rolling_average]+=1.0
        where  [user_id] = @scriptB_user_Id;

        set rowcount 0;

        --~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        --~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~


    end;



