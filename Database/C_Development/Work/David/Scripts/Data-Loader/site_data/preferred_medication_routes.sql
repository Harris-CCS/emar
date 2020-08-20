print 'Loading Table: preferred_medication_routes';

if '$(load_data)' = 'live'
   or '$(load_data)' = 'sample'
    begin

        insert into [dbo].[preferred_medication_routes]
            ([drug_id]
           , [medication_route_id]
           , [site_id]
            )
        select    [drug_id]
                , [medication_route_id]
                , [site_id]
        from
        (
            select [drug_id]
                 , [medication_route_id]
                 , [site_id]
            from     [user_quick_list_items]
            where   [medication_route_id] is not null
            union
            select [drug_id]
                 , [medication_route_id]
                 , [site_id]
            from     [department_preferred_list_items]
            where   [medication_route_id] is not null
            union
            select [drug_id]
                 , [medication_route_id]
                 , [site_id]
            from   [group_list_items]
            where  [medication_route_id] is not null
        ) as [src]
        inner join [dbo].[fdb_brand_name] as [fdb] on [fdb].[MEDID] = [src].[drug_id]
        where len([drug_id]) > 0;
    end;