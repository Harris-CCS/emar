create procedure [dbo].[update_medication_id_list]
as
    begin

        set nocount on;

        --- ndc match
        update [target] set    
            [medication_id] = [md].[medication_id]
          , [match] = '--- ndc match'
        from   [#medication_items] [target]
               inner join [dbo].[fdb_ndc_info] as [ndc] on [ndc].[ndc] = [target].[ndc]
               inner join [dbo].[medication_details] as [md] on [md].[drug_id] = [ndc].[medid]
               inner join [dbo].[medications] as [m] on [m].[id] = [md].[medication_id]
                                                        and [target].[site_id] = [m].[site_id]
        where  [m].[site_id] = -1
               and [target].[medication_id] = 0
               and [target].[ndc] > '';

        --- base ndc match
        update [target] set    
            [medication_id] = [md].[medication_id]
          , [match] = '--- base ndc match'
        from   [#medication_items] [target]
               inner join [dbo].[fdb_ndc_info] as [ndc] on [ndc].[base_ndc] = [target].[ndc]
               inner join [dbo].[medication_details] as [md] on [md].[drug_id] = [ndc].[medid]
               inner join [dbo].[medications] as [m] on [m].[id] = [md].[medication_id]
                                                        and [target].[site_id] = [m].[site_id]
        where  [m].[site_id] = -1
               and [target].[medication_id] = 0
               and [target].[ndc] > '';

        --- drug_id match
        update [target] set    
            [medication_id] = [md].[medication_id]
          , [match] = '--- drug_id match'
        from   [#medication_items] [target]
               inner join [dbo].[medication_details] as [md] on [md].[drug_id] = [target].[drug_id]
               inner join [dbo].[medications] as [m] on [m].[id] = [md].[medication_id]
                                                        and [target].[site_id] = [m].[site_id]
        where  [m].[site_id] = -1
               and [target].[medication_id] = 0
               and [target].[drug_id] > '';

        --- brand_name match
        update [target] set    
            [medication_id] = [md].[medication_id]
          , [match] = '--- brand_name match'
        from   [#medication_items] [target]
               inner join [dbo].[medication_details] as [md] on [md].[brand_name] = [target].[brand_name]
               inner join [dbo].[medications] as [m] on [m].[id] = [md].[medication_id]
                                                        and [target].[site_id] = [m].[site_id]
        where  [m].[site_id] = -1
               and [target].[medication_id] = 0
               and [target].[brand_name] > '';

        --- long_brand_name match (display_name)
        update [target] set    
            [target].[medication_id] = [m].[id]
          , [match] = '--- long_brand_name match (display_name)'
        from   [#medication_items] [target]
               inner join [dbo].[medications] as [m] on [m].[display_name] = [target].[brand_name]
                                                        and [target].[site_id] = [m].[site_id]
        where  [m].[site_id] = -1
               and [target].[medication_id] = 0
               and [target].[brand_name] > '';

        --- long_brand_name wildcard contains match (display_name)
        update [target] set    
            [target].[medication_id] = [m].[id]
          , [match] = '--- long_brand_name wildcard contains match (display_name)'
        from   [#medication_items] [target]
               inner join [dbo].[medication_details] as [md] on [md].[brand_name] = [target].[brand_name]
               inner join [dbo].[medications] as [m] on [md].[medication_id] = [m].[id]
                                                        and [target].[site_id] = [m].[site_id]
        where  [m].[site_id] = -1
               and [target].[medication_id] = 0
               and [target].[brand_name] > ''
               and [m].[display_name] like '%' + [target].[brand_name] + '%';

        --- dev only statement
        --- not good for production
        --- not good for dev either
        --- active_list match
        --update [target] set    
        --    [target].[medication_id] = [md].[medication_id]
        --  , [match] = '--- active_list match'
        --from   [#medication_items] [target]
        --       inner join [dbo].[medication_details] as [md] on [md].[active_list] = [target].[brand_name]
        --       inner join [dbo].[medications] as [m] on [md].[medication_id] = [m].[id] and [target].[site_id] = [m].[site_id]
        --where  [m].[site_id] = -1
        --       and [target].[medication_id] = 0
        --       and [target].[brand_name] > '';
        ---
        --- dev only statement
        --- not good for production??
        --- self match on brand_name
        update    [target] set       
            [medication_id] = [source].[medication_id]
          , [match] = '--- self match on brand_name'
        from      [#medication_items] [target]
                  cross apply
        (
            select top 1 [internal].[medication_id]
            from         [#medication_items] as [internal]
            where        [internal].[brand_name] = [target].[brand_name]
                         and [target].[brand_name] > ''
                         and [internal].[medication_id] > 0
            order by [internal].[medication_id]
        ) [source]
        where [target].[medication_id] = 0
              and [source].[medication_id] > 0
              and [target].[brand_name] > '';
    end;
go