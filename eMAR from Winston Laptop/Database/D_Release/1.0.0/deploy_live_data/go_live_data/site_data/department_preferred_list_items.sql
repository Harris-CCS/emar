print 'Loading Table: department_preferred_list_items';

drop table if exists [#department_preferred_list_items];

create table [#department_preferred_list_items]
    (
      [site_id]               [varchar](25) not null
    , [group_name]            [nvarchar](255) not null
    , [group_type]            [varchar](5) not null
    , [ndc]                   [varchar](32) null
    , [drug_id]               [varchar](32) null
    , [brand_name]            [nvarchar](255) not null
    , [dose]                  [varchar](50) null
    , [medication_unit_id]    [varchar](40) null
    , [medication_route_id]   [varchar](50) null
    , [frequency_schedule_id] [varchar](50) null
    , [order_notes]           [nvarchar](max) null
    , [internal_site_id]      [int] null
    , [parent_id]             [int] null
    , [child_id]              [int] null
    , [medication_id]         [int] null -- existing medication_id of ndc/drug/brand
                                    default 0
    , [dept_pref_list] [varchar](50) null);

if '$(load_data)' = 'sample'
   or ('$(load_data)' = 'live'
       and @does_ibex_exist = 1)
    begin

        insert into [#department_preferred_list_items]
            ([site_id]
           , [group_name]
           , [group_type]
           , [ndc]
           , [drug_id]
           , [brand_name]
           , [dose]
           , [medication_unit_id]
           , [medication_route_id]
           , [frequency_schedule_id]
           , [order_notes]
           , [dept_pref_list]
            )
        execute ('execute dbo.export_ibex_group_list_items');
    end;

if
(
    select count(*)
    from   [#department_preferred_list_items]
) > 0
    begin

        begin transaction;

        delete [#department_preferred_list_items]
        where dept_pref_list is null

        truncate table [#medication_items];
        insert into [#medication_items]
            ([ndc]
           , [drug_id]
           , [brand_name]
            )
        select distinct
               isnull([#department_preferred_list_items].[ndc], '')
             , isnull([#department_preferred_list_items].[drug_id], '')
             , isnull([#department_preferred_list_items].[brand_name], '')
        from   [#department_preferred_list_items]
        where  [#department_preferred_list_items].[group_type] <> 'GX';

        --set medication id's
        execute [dbo].[update_medication_id_list];

        update [target] set
            [medication_id] = [source].[medication_id]
        from   [#medication_items] [source]
               inner join [#department_preferred_list_items] [target] on [source].[ndc] = [target].[ndc]
                                                          and [source].[brand_name] = [target].[brand_name]
                                                          and [source].[drug_id] = [target].[drug_id]
        where  [source].[medication_id] > 0;

        --- update internal site_id
        update [source] set
            [source].[internal_site_id] = [internal_site].[id]
        from   [#department_preferred_list_items] [source]
               outer apply [dbo].[get_internal_id]
            ('pulsecheck', 'sites', [source].[site_id]) as [internal_site];


        --- Get COMBO Medication Headers ID in Temp Table on CM Record
        update [source] set
            [source].[parent_id] = [target].[id]
        from   [#department_preferred_list_items] [source]
               inner join [dbo].[medications] [target] on [source].[group_name] = [target].[display_name]
                                                          and 'COMBO' = [target].[drug_id]
                                                          and [source].[internal_site_id] = [target].[site_id]
                                                          and 'F' = [target].[drug_vendor]
        where  [source].[group_type] = 'CM'
               and [source].[parent_id] is null;

        --- Get COMBO Medication Headers ID in Temp Table on GX Record
        update [source] set
            [source].[parent_id] = [target].[id]
        from   [#department_preferred_list_items] [source]
               inner join [dbo].[medications] [target] on [source].[brand_name] = [target].[display_name]
                                                          and 'COMBO' = [target].[drug_id]
                                                          and [source].[internal_site_id] = [target].[site_id]
                                                          and 'F' = [target].[drug_vendor]
        where  [source].[parent_id] is null;

/****************************************
        load temporary tables for staging
****************************************/

        alter table [#department_preferred_list_items]
        add [id]        [bigint] identity(1, 1)
          , [target_id] [bigint];

/********************************
        get max id for seed value
********************************/

        set @max_id = null;

        select @max_id = max([group_list_items].[id])
        from   [dbo].[group_list_items];

        set @max_id = isnull(@max_id, 0);

        update [source] set
            [source].[target_id] = [source].[id] + @max_id
        from   [#department_preferred_list_items] as [source];

/*************************************
        begin loading permanent tables
*************************************/

        -- set identity_insert [dbo].[group_list_items] on;

        insert into [dbo].[department_preferred_list_items]
            ([site_id]
           , [department_code]
--           , [group_name]
           , [dose]
           , [medication_unit_id]
           , [medication_route_id]
           , [frequency_schedule_id]
           , [order_notes]
           , [medication_id]
            )
        select [source].[internal_site_id] as [site_id]
             , '' as                          [department_code]
--             , [source].[group_name]
             , [source].[dose]
             , [mu].[id] as                   [medication_unit_id]
             , [mr].[id] as                   [medication_routes_id]
             , null as                        [frequency_schedule_id]
             , [source].[order_notes]
             , [source].[medication_id]
        from     [#department_preferred_list_items] as [source]
                 cross apply [dbo].[get_code_share_site]
            ([source].[internal_site_id], 'medication_units') as [mu_site]
                 cross apply [dbo].[get_code_share_site]
            ([source].[internal_site_id], 'medication_routes') as [mr_site]
                 left join [dbo].[medication_routes] as [mr] on [mr].[site_id] = [mr_site].[site_id]
                                                                and [mr].[name] = [source].[medication_route_id]
                 left join [dbo].[medication_units] as [mu] on [mu].[site_id] = [mu_site].[site_id]
                                                               and [mu].[code] = [source].[medication_unit_id]
        where   [source].[medication_id] > 0
                and [source].[group_type] not in('CM', 'GX')
        union
        select [source].[internal_site_id] as [site_id]
             , '' as                          [department_code]
--             , [source].[group_name]
             , null as                        [dose]
             , null as                        [medication_unit_id]
             , null as                        [medication_routes_id]
             , null as                        [frequency_schedule_id]
             , null as                        [order_notes]
             , [source].[parent_id] as        [medication_id]
        from   [#department_preferred_list_items] as [source]
               cross apply [dbo].[get_code_share_site]
            ([source].[internal_site_id], 'medication_units') as [mu_site]
               cross apply [dbo].[get_code_share_site]
            ([source].[internal_site_id], 'medication_routes') as [mr_site]
               left join [dbo].[medication_routes] as [mr] on [mr].[site_id] = [mr_site].[site_id]
                                                              and [mr].[name] = [source].[medication_route_id]
               left join [dbo].[medication_units] as [mu] on [mu].[site_id] = [mu_site].[site_id]
                                                             and [mu].[code] = [source].[medication_unit_id]
        where  [source].[parent_id] > 0
               and [source].[group_type] = 'GX';

        -- set identity_insert [dbo].[group_list_items] off;

/***************************************
        loading [external_ids] reference
***************************************/

/****************
        end table
****************/

        commit transaction;
    end;

drop table if exists [#department_preferred_list_items];