print 'Loading Table: preferred_medication_doses';

if '$(load_data)' = 'live'
   or '$(load_data)' = 'sample'
    begin

        insert into [dbo].[preferred_medication_doses]
            ([medication_id]
           , [dose]
           , [medication_unit_id]
           , [site_id]
            )
        select    [src].[medication_id]
                , [src].[dose]
                , [src].[medication_unit_id]
                , [src].[site_id]
        from
        (
            select [medication_id]
                 , [dose]
                 , [medication_unit_id]
                 , [site_id]
            from     [user_quick_list_items]
            where   [medication_unit_id] is not null
            union
            select [medication_id]
                 , [dose]
                 , [medication_unit_id]
                 , [site_id]
            from     [department_preferred_list_items]
            where   [medication_unit_id] is not null
            union
            select [medication_id]
                 , [dose]
                 , [medication_unit_id]
                 , [site_id]
            from   [group_list_items]
            where  [medication_unit_id] is not null
        ) as [src]
        inner join [dbo].[medications] as [med] on [med].[id] = [src].[medication_id]
        where [medication_id] > 0;
        select @@rowcount 'Loading Table: preferred_medication_doses';
    end;