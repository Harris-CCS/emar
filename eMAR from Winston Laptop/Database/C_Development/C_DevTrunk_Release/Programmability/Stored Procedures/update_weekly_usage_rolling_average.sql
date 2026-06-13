create procedure [dbo].[update_weekly_usage_rolling_average]
as
    begin

        update [uqli] set    
            [weekly_usage_rolling_average] = case [weekly_usage_rolling_average]
                                                 when 0
                                                     then [usages_this_week]
                                                 when -1
                                                     then [usages_this_week]
                                                 else([weekly_usage_rolling_average] * 9 + [usages_this_week]) / 10
                                             end
          , [usages_this_week] = 0
        from   [user_quick_list_items] [uqli];
    end;