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
        from   [get_internal_id]('pulsecheck', 'users', 10087) [internal_id]
               inner join [dbo].[users] [users] on [internal_id].[id] = [users].[id];

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

        with SiteCounts
             as (select [site_id]
                      , count(*) as [cnt]
                 from   [user_quick_list_items]
                 group by [site_id]
                        , user_id)
             INSERT department_preferred_list_items
             select [q].[site_id]
                  , [department_code] = case
                                            when row_number() over(partition by [q].[site_id]
                                                 order by [q].[site_id]
                                                        , [brand_name]) < ([cnt].[cnt] / 2.0)
                                                then 'Main ED'
                                            else 'Fast Track'
                                        end
                  , [ndc]
                  , isnull([drug_id], 999999)
                  , [brand_name]
                  , [dose]
                  , [medication_unit_id]
                  , [medication_route_id]
                  , [frequency_schedule_id]
                  , [order_notes]
             from   [user_quick_list_items] as [q]
                    join [SiteCounts] as [cnt] on [q].[site_id] = [cnt].[site_id]
             order by [q].[site_id]
                    , [brand_name];
    end;