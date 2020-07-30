print 'Loading Table: user_quick_list_items';

drop table if exists [#user_quick_list_items];

create table [#user_quick_list_items]
    (
      [site_id]             [varchar](25) not null
    , [user_id]             [varchar](25) not null
    , [ndc]                 [varchar](32) null
    , [drug_id]             [varchar](32) null
    , [brand_name]          [nvarchar](255) not null
    , [dose]                [varchar](50) null
    , [medication_unit_id]  [varchar](50) null
    , [medication_route_id] [varchar](50) null
    , [frequency_id]        [varchar](50) null
    , [order_notes]         [nvarchar](max) null);

if '$(load_data)' = 'live'
   and exists
(
    select null
    from   [master].[sys].[databases]
    where  [name] = 'ibex'
)
    begin

        insert into [#user_quick_list_items]
            ([site_id]
           , [user_id]
           , [ndc]
           , [drug_id]
           , [brand_name]
           , [dose]
           , [medication_unit_id]
           , [medication_route_id]
           , [frequency_id]
           , [order_notes]
            )
        execute ('execute dbo.export_ibex_user_quick_list_items');
    end;

if '$(load_data)' = 'sample'
    begin

        bulk insert [#user_quick_list_items] from '$(current_path)Scripts\Data-Loader\sample_data\user_quick_list_items.bcp' with(fieldterminator = '|~', rowterminator = '\n');
    end;

if
(
    select count(*)
    from   [#user_quick_list_items]
) > 0
    begin

        begin transaction;

/****************************************
        load temporary tables for staging
****************************************/

        alter table [#user_quick_list_items]
        add [id]        [bigint] identity(1, 1)
          , [target_id] [bigint];

/********************************
        get max id for seed value
********************************/

        set @max_id = null;

        select @max_id = max([id])
        from   [dbo].[user_quick_list_items];

        set @max_id = isnull(@max_id, 0);

        update [source] set
            [target_id] = [source].[id] + @max_id
        from   [#user_quick_list_items] as [source];

/*************************************
        begin loading permanent tables
*************************************/

        -- set identity_insert [dbo].[user_quick_list_items] on;

        insert into [dbo].[user_quick_list_items]
            ([site_id]
           , [user_id]
           , [ndc]
           , [drug_id]
           , [brand_name]
           , [dose]
           , [medication_unit_id]
           , [medication_route_id]
           , [frequency_id]
           , [order_notes]
            )
        select isnull([internal_site_site].[id], -1) as [site_id]
             , isnull([internal_site_user].[id], -1) as [user_id]
             , [source].[ndc]
             , [source].[drug_id]
             , [source].[brand_name]
             , [source].[dose]
             , [mu].[id] as                             [medication_unit_id]
             , [mr].[id] as                             [medication_routes_id]
             , [source].[frequency_id]
             , [source].[order_notes]
        from   [#user_quick_list_items] as [source]
               outer apply [dbo].[get_internal_id]('pulsecheck', 'sites', [source].[site_id]) as [internal_site_site]
               outer apply [dbo].[get_internal_id]('pulsecheck', 'users', [source].[site_id]) as [internal_site_user]
               cross apply [dbo].[get_code_share_site]([internal_site_site].[id], 'medication_units') as [mu_site]
               cross apply [dbo].[get_code_share_site]([internal_site_site].[id], 'medication_routes') as [mr_site]
               left join [dbo].[medication_routes] as [mr] on [mr].[site_id] = [mr_site].[site_id]
                                                              and [mr].[name] = [source].[medication_route_id]
               left join [dbo].[medication_units] as [mu] on [mu].[site_id] = [mu_site].[site_id]
                                                             and [mu].[code] = [source].[medication_unit_id]
        order by [source].[brand_name];

        -- set identity_insert [dbo].[user_quick_list_items] off;

/***************************************
        loading [external_ids] reference
***************************************/
/****************
        end table
****************/

        commit transaction;
    end;

drop table if exists [#user_quick_list_items];