create procedure [dbo].[update_medication_id_list]
as
    begin

        set nocount on;

        update [target] set    
            [medication_id] = [md].[medication_id]
        from   [#medication_items] [target]
               inner join [dbo].[fdb_ndc_info] as [ndc] on [ndc].[ndc] = [target].[ndc]
               inner join [dbo].[medication_details] as [md] on [md].[drug_id] = [ndc].[medid]
        where  [target].[medication_id] = 0;

        update [target] set    
            [medication_id] = [md].[medication_id]
        from   [#medication_items] [target]
               inner join [dbo].[fdb_ndc_info] as [ndc] on [ndc].[base_ndc] = [target].[ndc]
               inner join [dbo].[medication_details] as [md] on [md].[drug_id] = [ndc].[medid]
        where  [target].[medication_id] = 0;

        update [target] set    
            [medication_id] = [source].[medication_id]
        from   [#medication_items] [target]
               inner join [dbo].[medication_details] as [source] on [source].[brand_name] = [target].[brand_name]
        where  [target].[medication_id] = 0;

        update [target] set    
            [target].[medication_id] = [source].[id]
        from   [#medication_items] [target]
               inner join [dbo].[medications] as [source] on [source].[display_name] = [target].[brand_name]
        where  [target].[medication_id] = 0;

        update [target] set    
            [target].[medication_id] = [source].[id]
        from   [#medication_items] [target]
               inner join [dbo].[medication_details] as [source] on [source].[brand_name] = [target].[brand_name]
               inner join [dbo].[medications] as [med] on [source].[medication_id] = [med].[id]
        where  [target].[medication_id] = 0
               and [med].[display_name] like '%' + [target].[brand_name] + '%';

        --- dev only statement
        --- not good for production
        update [target] set    
            [target].[medication_id] = [source].[medication_id]
        from   [#medication_items] [target]
               inner join [dbo].[medication_details] as [source] on [source].[active_list] = [target].[brand_name]
        where  [target].[medication_id] = 0;

        --- dev only statement
        --- not good for production
        update [target] set    
            [medication_id] = [source].[medication_id]
        from   [#medication_items] [target]
               inner join [#medication_items] as [source] on [source].[brand_name] = [target].[brand_name]
        where  [target].[medication_id] = 0
               and [source].[medication_id] > 0;

    end;
go
