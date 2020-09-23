print 'Loading Table: user_patients';

drop table if exists [#user_patients];

create table [#user_patients]
    (
      [source_id]                      [varchar](40) not null
    , [target_id]                      [int] null
    , [site_id]                        [varchar](25) not null
    , [doctor]                         [varchar](15) null
    , [resident]                       [varchar](15) null
    , [drextender]                     [varchar](15) null
    , [primarynurse]                   [varchar](15) null
    , [extender]                       [varchar](15) null
    , [firstdoctor]                    [varchar](15) null);

if '$(load_data)' = 'sample'
   or ('$(load_data)' = 'live'
       and @does_ibex_exist = 1)
    begin

        insert into [#user_patients]
            ([source_id]
           , [site_id]
           , [doctor]
           , [resident]
           , [drextender]
           , [primarynurse]
           , [extender]
           , [firstdoctor]
            )
        execute ('execute dbo.export_ibex_user_patients');
    end;

if
(
    select count(*)
    from   [#user_patients]
) > 0
    begin

        begin transaction;

/****************************************
        load temporary tables for staging
****************************************/

        update [source] set    
            [source_id] = [source].[site_id] + '|' + [source].[source_id]
        from   [#user_patients] as [source];

        update [source] set    
            [target_id] = [internal_patient].[id]
        from   [#user_patients] as [source]
               outer apply [dbo].[get_internal_id]
            ('pulsecheck', 'patients', [source].[source_id]) as [internal_patient]

/********************************
        get max id for seed value
********************************/

/*************************************
        begin loading permanent tables
*************************************/

        --DOCTOR1 = [pat].[firstdoctor]
        --DOCTOR2 = [pat].[doctor]
        --DOCTOR3 = [pat].[resident]
        --DOCTOR4 = [pat].[drextender]
        --NURSE1  = [pat].[primarynurse]
        --NURSE2  = [pat].[extender]

        insert into [dbo].[user_patients]
            ([user_id]
           , [patient_id]
           , [role_name]
            )
        select [internal_user].[id] as [users_id]
             , [source].[target_id]
             , 'DOCTOR1'
        from     [#user_patients] as [source]
                 cross apply [dbo].[get_internal_id]
            ('pulsecheck', 'users', [source].[firstdoctor]) as [internal_user]
        where   [internal_user].[id] > 0
        union all
        select [internal_user].[id] as [users_id]
             , [source].[target_id]
             , 'DOCTOR2'
        from     [#user_patients] as [source]
                 cross apply [dbo].[get_internal_id]
            ('pulsecheck', 'users', [source].[doctor]) as [internal_user]
        where   [internal_user].[id] > 0
        union all
        select [internal_user].[id] as [users_id]
             , [source].[target_id]
             , 'DOCTOR3'
        from     [#user_patients] as [source]
                 cross apply [dbo].[get_internal_id]
            ('pulsecheck', 'users', [source].[resident]) as [internal_user]
        where   [internal_user].[id] > 0
        union all
        select [internal_user].[id] as [users_id]
             , [source].[target_id]
             , 'DOCTOR4'
        from     [#user_patients] as [source]
                 cross apply [dbo].[get_internal_id]
            ('pulsecheck', 'users', [source].[drextender]) as [internal_user]
        where   [internal_user].[id] > 0
        union all
        select [internal_user].[id] as [users_id]
             , [source].[target_id]
             , 'NURSE1'
        from     [#user_patients] as [source]
                 cross apply [dbo].[get_internal_id]
            ('pulsecheck', 'users', [source].[primarynurse]) as [internal_user]
        where   [internal_user].[id] > 0
        union all
        select [internal_user].[id] as [users_id]
             , [source].[target_id]
             , 'NURSE2'
        from   [#user_patients] as [source]
               cross apply [dbo].[get_internal_id]
            ('pulsecheck', 'users', [source].[extender]) as [internal_user]
        where  [internal_user].[id] > 0;
        print @@rowcount;
        print '^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^'
        drop table if exists [#user_patients];

/***************************************
        loading [external_ids] reference
***************************************/

/****************
        end table
****************/

        commit transaction;
    end;

drop table if exists [#user_patients];