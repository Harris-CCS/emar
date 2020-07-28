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

        update [users] set    
            [first_name] = 'Heather'
          , [last_name] = 'Abebe'
          , [middle_name] = 'L'
          , [name_suffix] = 'P.A.C.'
        where  [id] = 240;

        declare 
            @userId int =
        (
            select [internal_id]
            from   [external_ids]
            where  [external_id] = '36'
                   and [vendor] = 'pulsecheck'
                   and [entity] = 'users'
        );

        update [user_quick_list_items] set    
            [weekly_usage_rolling_average] = -1
        where  user_id = @userId;

        set rowcount 100;
        update [user_quick_list_items] set    
            [weekly_usage_rolling_average] = 1.0
        where  user_id = @userId;

        set rowcount 80;
        update [user_quick_list_items] set    
            [weekly_usage_rolling_average]+=1.0
        where  user_id = @userId;

        set rowcount 60;
        update [user_quick_list_items] set    
            [weekly_usage_rolling_average]+=1.0
        where  user_id = @userId;

        set rowcount 30;
        update [user_quick_list_items] set    
            [weekly_usage_rolling_average]+=1.0
        where  user_id = @userId;

        set rowcount 20;
        update [user_quick_list_items] set    
            [weekly_usage_rolling_average]+=1.0
        where  user_id = @userId;

        set rowcount 0;
    end;