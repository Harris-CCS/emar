print 'Loading Table: patient_home_medications';

drop table if exists [#patient_home_medications];

create table [#patient_home_medications]
    (
      [patient_id]          [varchar](50) null
    , [class]               [varchar](32) null
    , [category]            [varchar](32) null
    , [internal_drug_id]    [varchar](32) null
    , [ndc]                 [varchar](32) null
    , [drug_id]             [varchar](32) null
    , [name]                [nvarchar](255) null
    , [alternate_name]      [nvarchar](255) null
    , [dose]                [varchar](50) null
    , [medication_unit_id]  [varchar](20) null
    , [medication_route_id] [varchar](50) null
    , [medication_drug_id]  [varchar](32) null
    , [is_active]           [bit] not null
    , [comment]             [varchar](255) null
    , [schedule]            [varchar](40) null
    , [reaction]            [varchar](80) null
    , [severity]            [varchar](80) null
    , [parent_drug_id]      [varchar](32) null
    , [parent_drug_name]    [nvarchar](255) null
    , [add_user_id]         [varchar](50) null
    , [add_datetime]        [varchar](50) null
    , [change_user_id]      [varchar](50) null
    , [change_datetime]     [varchar](50) null
    , [action_status]       [char](1) null);

if '$(load_data)' = 'live'
   and exists
(
    select null
    from   [master].[sys].[databases]
    where  [name] = 'ibex'
)
    begin

        insert into [#patient_home_medications]
            ([patient_id]
           , [class]
           , [category]
           , [internal_drug_id]
           , [ndc]
           , [drug_id]
           , [name]
           , [alternate_name]
           , [dose]
           , [medication_unit_id]
           , [medication_route_id]
           , [medication_drug_id]
           , [is_active]
           , [comment]
           , [schedule]
           , [reaction]
           , [severity]
           , [parent_drug_id]
           , [parent_drug_name]
           , [add_user_id]
           , [add_datetime]
           , [change_user_id]
           , [change_datetime]
           , [action_status]
            )
        execute ('execute dbo.export_ibex_patient_home_medications');
    end;

if '$(load_data)' = 'sample'
    begin

        bulk insert [#patient_home_medications] from '$(current_path)Scripts\Data-Loader\sample_data\patient_home_medications.bcp' with(fieldterminator = '|~', rowterminator = '\n');
    end;

if
(
    select count(*)
    from   [#patient_home_medications]
) > 0
    begin

        begin transaction;

/****************************************
        load temporary tables for staging
****************************************/

        alter table [#patient_home_medications]
        add [id]        [bigint] identity(1, 1)
          , [target_id] [bigint];

        update [source] set    
            [dose] = '0'
        from   [#patient_home_medications] as [source]
        where  rtrim(ltrim(isnull([source].[dose], ''))) = '';

        update [source] set    
            [dose] = '0'
        from   [#patient_home_medications] as [source]
        where  isnumeric([source].[dose]) = 0
               and charindex(' ', [source].[dose], 1) = 0;

        update [source] set    
            [dose] = left([source].[dose], charindex(' ', [source].[dose], 1) - 1)
          , [medication_unit_id] = substring([source].[dose], charindex(' ', [source].[dose], 1) + 1, len([source].[dose]))
        from   [#patient_home_medications] as [source]
        where  isnumeric([source].[dose]) = 0
               and charindex(' ', [source].[dose], 1) > 0
               and isnull([source].[medication_unit_id], '') = '';

        update [source] set    
            [dose] = '0'
        from   [#patient_home_medications] [source]
        where  isnumeric([source].[dose]) = 0;

/********************************
        get max id for seed value
********************************/

        set @max_id = null;

        select @max_id = max([id])
        from   [dbo].[patient_home_medications];

        set @max_id = isnull(@max_id, 0);

        update [source] set    
            [target_id] = [source].[id] + @max_id
        from   [#patient_home_medications] as [source];

/*************************************
        begin loading permanent tables
*************************************/

        -- set identity_insert [dbo].[patient_home_medications] on;

        insert into [dbo].[patient_home_medications]
            ([patient_id]
           , [class]
           , [category]
           , [internal_drug_id]
           , [ndc]
           , [drug_id]
           , [name]
           , [dose]
           , [medication_unit_id]
           , [medication_route_id]
           , [medication_drug_id]
           , [is_active]
           , [comment]
           , [schedule]
           , [reaction]
           , [severity]
           , [parent_drug_id]
           , [parent_drug_name]
           , [add_user_id]
           , [add_datetime]
           , [change_user_id]
           , [change_datetime]
           , [action_status]
            )
        select isnull([internal_patient_id].[id], -1) as                                                [patient_id]
             , [source].[class]
             , [source].[category]
             , [source].[internal_drug_id]
             , [source].[ndc]
             , [source].[drug_id]
             , [source].[name]
             , [source].[dose]
             , [mu].[id] as                                                                             [medication_unit_id]
             , [mr].[id] as                                                                             [medication_routes_id]
             , [source].[medication_drug_id]
             , [source].[is_active]
             , [source].[comment]
             , [source].[schedule]
             , [source].[reaction]
             , [source].[severity]
             , [source].[parent_drug_id]
             , [source].[parent_drug_name]
             , isnull([internal_add_user_id].[id], 0) as                                                [add_user_id]
             , [dbo].[ibex_date_to_offset_date]([source].[add_datetime], [site].[time_zone_name]) as    [add_datetime]
             , isnull([internal_change_user_id].[id], 0) as                                             [change_user_id]
             , [dbo].[ibex_date_to_offset_date]([source].[change_datetime], [site].[time_zone_name]) as [change_datetime]
             , [source].[action_status]
        from   [#patient_home_medications] as [source]
               outer apply [dbo].[get_internal_id]('pulsecheck', 'patients', [source].[patient_id]) as [internal_patient_id]
               left join [dbo].[patients] as [patients] on [patients].[id] = [internal_patient_id].[id]
               left join [dbo].[sites] as [site] on [site].[id] = [patients].[site_id]
               outer apply [dbo].[get_internal_id]('pulsecheck', 'users', [source].[add_user_id]) as [internal_add_user_id]
               outer apply [dbo].[get_internal_id]('pulsecheck', 'users', [source].[change_user_id]) as [internal_change_user_id]
               cross apply [dbo].[get_code_share_site]([site].[id], 'medication_units') as [mu_site]
               cross apply [dbo].[get_code_share_site]([site].[id], 'medication_routes') as [mr_site]
               left join [dbo].[medication_routes] as [mr] on [mr].[site_id] = [mr_site].[site_id]
                                                              and [mr].[name] = [source].[medication_route_id]
               left join [dbo].[medication_units] as [mu] on [mu].[site_id] = [mu_site].[site_id]
                                                             and [mu].[code] = [source].[medication_unit_id];

        -- set identity_insert [dbo].[patient_home_medications] off;

/***************************************
        loading [external_ids] reference
***************************************/
/****************
        end table
****************/

        commit transaction;
    end;

drop table if exists [#patient_home_medications];