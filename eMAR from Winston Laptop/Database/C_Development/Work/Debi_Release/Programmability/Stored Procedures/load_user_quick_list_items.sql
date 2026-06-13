create procedure [dbo].[load_user_quick_list_items]
as
begin

    set nocount on;

    print 'Loading Table: user_quick_list_items';

    create table [#user_quick_list_items]
        (
            [target_id]             [bigint]        null
          , [source_id]             [varchar](25)   null
          , [site]                  [varchar](25)   null
          , [user]                  [varchar](25)   null
          , [site_id]               [int]           null
          , [user_id]               [int]           null
          , [ndc]                   [varchar](32)   null
          , [drug_id]               [varchar](32)   null
          , [brand_name]            [nvarchar](255) null
          , [dose]                  [varchar](50)   null
          , [medication_unit]       [varchar](50)   null
          , [medication_route]      [varchar](50)   null
          , [medication_unit_id]    [int]           null
          , [medication_route_id]   [int]           null
          , [frequency_schedule_id] [varchar](50)   null
          , [order_notes]           [nvarchar](max) null
          , [medication_id]         [int]           null default 0
          , [priority_pick]         [smallint]      null
          , [existing_record]       [bit]           null default 0
        );

    insert into [#user_quick_list_items]
    (
        [source_id]
      , [site]
      , [user]
      , [ndc]
      , [drug_id]
      , [brand_name]
      , [dose]
      , [medication_unit]
      , [medication_route]
      , [frequency_schedule_id]
      , [order_notes]
    )
    execute ('execute dbo.export_ibex_user_quick_list_items');

    if (
                 select
                     count(*)
                 from [#user_quick_list_items]
        ) > 0
        begin

            create table [#medication_items]
                (
                    [medication_id] [int]           default 0
                  , [site_id]       [int]           not null default -1
                  , [ndc]           [varchar](32)   not null
                  , [drug_id]       [varchar](32)   not null
                  , [brand_name]    [nvarchar](255) not null
                  , [match]         [nvarchar](255) null --- Added for testing / debugging
                  , primary key clustered ([ndc] asc, [drug_id] asc, [brand_name] asc, [site_id] asc)
                );

            --get a distinct list of medications

            insert into [#medication_items]
            (
                [ndc]
              , [drug_id]
              , [brand_name]
            )
            select distinct
                isnull([ndc], '')
              , isnull([drug_id], '')
              , isnull([brand_name], '')
            from [#user_quick_list_items];

            create index [tmpdb_medication_items] on [#medication_items]
            ([medication_id] asc, [site_id] asc, [drug_id] asc);

            --set medication id's
            execute [dbo].[update_medication_id_list];

            update [target] set
                [medication_id] = [source].[medication_id]
            from [#medication_items] [source]
                inner join [#user_quick_list_items] [target]
                    on [source].[ndc] = [target].[ndc]
                        and [source].[brand_name] = [target].[brand_name]
                        and [source].[drug_id] = [target].[drug_id]
            where [source].[medication_id] > 0;

            -- get internal site_id
            update [source] set
                [site_id] = isnull([internal].[id], -1)
            from [#user_quick_list_items] as [source]
                outer apply [dbo].[get_internal_id]
                ('pulsecheck', 'sites', [source].[site]) as [internal];

            -- get internal user_id
            update [source] set
                [user_id] = isnull([internal].[id], -1)
            from [#user_quick_list_items] as [source]
                outer apply [dbo].[get_internal_id]
                ('pulsecheck', 'users', [source].[user]) as [internal];

            -- get medication_route_id
            update [source] set
                [medication_route_id] = [mr].[id]
            from [#user_quick_list_items] as [source]
                cross apply [dbo].[get_code_share_site]([source].[site_id], 'medication_routes') as [mr_site]
                inner join [dbo].[medication_routes] as [mr]
                    on [mr].[site_id] = [mr_site].[site_id]
                        and [mr].[code] = [source].[medication_route];

            -- get medication_unit_id
            update [source] set
                [medication_unit_id] = [mu].[id]
            from [#user_quick_list_items] as [source]
                cross apply [dbo].[get_code_share_site]([source].[site_id], 'medication_units') as [mr_site]
                inner join [dbo].[medication_units] as [mu]
                    on [mu].[site_id] = [mr_site].[site_id]
                        and [mu].[name] = [source].[medication_unit];

            /********************************
             synchornize internal / external id's
            ********************************/
            /*          PointOfView: [dbo].[external_ids] [ei]
    
                        source_id not null / target_id not null :: update in [TARGET]
    
                        ------------------------------------------
                        All other cases: Corrupt Record in [external_ids]
                        we want to generate a new [external_ids]
                        after inserting a new [TARGET] record
    
                      1.  source_id     null / target_id not null :: delete in [dbo].[external_ids] delete in [TARGET]
                               delete in [TARGET] makes [external_ids] qualify for item 2.
                      2.  source_id     null / target_id     null :: delete in [dbo].[external_ids]
                      3.  source_id not null / target_id     null :: delete in [dbo].[external_ids]
    
            */
            with cte_prioritry
                as (
                             select
                                 [source].[source_id]
                               , row_number() over (partition by
                                 [source].[site_id]
                                 , [source].[user_id]
                                 , [source].[medication_id]
                                 , isnull([source].[medication_route_id], -1)
                                 , isnull([source].[medication_unit_id], -1)
                                 , isnull([source].[dose], -1)
                                 , isnull([source].[order_notes], char(0))
                                 order by
                                 --padding varchar values with leading 00's ensures a better sort
                                 right('000000000000000000000' + [source].[source_id], 25) desc
                                 ) as [priority_pick]
                             from [#user_quick_list_items] as [source]
                    )
            update [source] set
                [priority_pick] = [pr].[priority_pick]
            from cte_prioritry [pr]
                inner join [#user_quick_list_items] as [source]
                    on [source].[source_id] = [pr].[source_id];

            -- delete any bad import records
            delete [source]
            from [#user_quick_list_items] as [source]
            where [source].[medication_id] = 0
                or [priority_pick] <> 1
                or [site_id] <= 0;

            -- transform remote data "external_id"
            update [source] set
                [source_id] = [site] + '|' + [source_id]
            from [#user_quick_list_items] as [source];

            --- delete duplicate data in [dbo].[user_quick_list_items] 
            with cte_prioritry
                as (
                             select
                                 [source].[id]
                               , row_number() over (partition by
                                 [source].[site_id]
                                 , [source].[user_id]
                                 , [source].[medication_id]
                                 , isnull([source].[medication_route_id], -1)
                                 , isnull([source].[medication_unit_id], -1)
                                 , isnull([source].[dose], -1)
                                 , isnull([source].[order_notes], char(0))
                                 order by
                                   [source].[id] desc
                                 ) as [priority_pick]
                             from [user_quick_list_items] as [source]
                    )
            delete [target]
            from cte_prioritry [pr]
                inner join [dbo].[user_quick_list_items] as [target]
                    on [target].[id] = [pr].[id]
            where [pr].[priority_pick] <> 1;

            --- Item 1.  Target Records for Delete
            delete [target]
            from [dbo].[external_ids] [ei]
                left join [#user_quick_list_items] as [source]
                    on [ei].[external_id] = [source].[source_id]
                left join [dbo].[user_quick_list_items] [target]
                    on [ei].[internal_id] = [target].[id]
            where [ei].[vendor] = 'pulsecheck'
                and [ei].[entity] = 'user_quick_list_items'
                and [source].[source_id] is null
                and [target].[id] is not null;

            --- Item 2. Delete Bad External ID Records
            delete [ei]
            from [dbo].[external_ids] [ei]
                left join [#user_quick_list_items] as [source]
                    on [ei].[external_id] = [source].[source_id]
                left join [dbo].[user_quick_list_items] [target]
                    on [ei].[internal_id] = [target].[id]
            where [ei].[vendor] = 'pulsecheck'
                and [ei].[entity] = 'user_quick_list_items'
                and [source].[source_id] is null
                and [target].[id] is null;

            --- Item 3. Delete Bad External ID Records
            delete [ei]
            from [dbo].[external_ids] [ei]
                left join [#user_quick_list_items] as [source]
                    on [ei].[external_id] = [source].[source_id]
                left join [dbo].[user_quick_list_items] [target]
                    on [ei].[internal_id] = [target].[id]
            where [ei].[vendor] = 'pulsecheck'
                and [ei].[entity] = 'user_quick_list_items'
                and [source].[source_id] is not null
                and [target].[id] is null;

            --- if emar unique index/constraint exists validate and create any missing [dbo].[external_ids] [ei]
            with cte_constraint_match
                as (
                             select
                                 [target].[id]           [internal_id]
                               , 'pulsecheck'            [vendor]
                               , 'user_quick_list_items' [entity]
                               , [source].[source_id]    [external_id]
                             from [#user_quick_list_items] as [source]
                                 inner join [dbo].[user_quick_list_items] [target]
                                     on [source].[site_id] = [target].[site_id]
                                         and [source].[user_id] = [target].[user_id]
                                         and [source].[medication_id] = [target].[medication_id]
                                         and isnull([source].[medication_route_id], -1) = isnull([target].[medication_route_id], -1)
                                         and isnull([source].[medication_unit_id], -1) = isnull([target].[medication_unit_id], -1)
                                         and isnull([source].[dose], -1) = isnull([target].[dose], -1)
                                         and isnull([source].[order_notes], char(0)) = isnull([target].[order_notes], char(0))
                    )
            insert into [dbo].[external_ids]
            (
                [internal_id]
              , [vendor]
              , [entity]
              , [external_id]
            )
            select
                [cm].[internal_id]
              , [cm].[vendor]
              , [cm].[entity]
              , [cm].[external_id]
            from cte_constraint_match [cm]
                left join [dbo].[external_ids] [ei]
                    on [ei].[internal_id] = [cm].[internal_id]
                        and [ei].[vendor] = [cm].[vendor]
                        and [ei].[entity] = [cm].[entity]
                        and [ei].[external_id] = [cm].[external_id]
            where [ei].[internal_id] is null;

            --- Match target_id's with external_ids table
            update [source] set
                [target_id]       = [internal_site].[id]
              , [existing_record] = 1
            from [#user_quick_list_items] as [source]
                cross apply [dbo].[get_internal_id]
                ('pulsecheck', 'user_quick_list_items', [source].[source_id]) as [internal_site];

            --- Update Matching Target Records
            update [target] set
                [site_id]               = [source].[site_id]
              , [user_id]               = [source].[user_id]
              , [dose]                  = [source].[dose]
              , [medication_unit_id]    = [source].[medication_unit_id]
              , [medication_route_id]   = [source].[medication_route_id]
              , [order_notes]           = [source].[order_notes]
              , [medication_id]         = [source].[medication_id]
            from [#user_quick_list_items] as [source]
                cross apply [dbo].[get_internal_id]('pulsecheck', 'user_quick_list_items', [source].[source_id]) [gii]
                inner join [dbo].[user_quick_list_items] [target]
                    on [gii].[id] = [target].[id];

            --- Generate New id's for insert records
            with cte_data
                as (
                             select
                                 ident_current('[dbo].[user_quick_list_items]') +
                                 row_number() over (order by [source].[source_id]) new_id
                               , [source].[target_id]
                             from [#user_quick_list_items] as [source]
                                 outer apply [dbo].[get_internal_id]('pulsecheck', 'user_quick_list_items', [source].[source_id]) [gii]
                                 left join [dbo].[user_quick_list_items] [target]
                                     on [gii].[id] = [target].[id]
                             where [target].[id] is null
                    )
            update [data] set
                [target_id] = [data].[new_id]
            from cte_data [data];

            /*************************************
                    begin loading permanent tables
                    delete / update / insert
            *************************************/
            set identity_insert [dbo].[user_quick_list_items] on;

            insert into [dbo].[user_quick_list_items]
            (
                [id]
              , [site_id]
              , [user_id]
              , [dose]
              , [medication_unit_id]
              , [medication_route_id]
              , [frequency_schedule_id]
              , [order_notes]
              , [medication_id]
            )
            select
                [target_id]
              , [site_id]
              , [user_id]
              , [dose]
              , [medication_unit_id]
              , [medication_route_id]
              , null [frequency_schedule_id]
              , [order_notes]
              , [medication_id]
            from [#user_quick_list_items] as [source]
            where [target_id] is not null
                  and [existing_record] = 0;

            set identity_insert [dbo].[user_quick_list_items] off;

            /***************************************
                    loading [external_ids] reference
            ***************************************/

            insert into [dbo].[external_ids]
            (
                [internal_id]
              , [vendor]
              , [entity]
              , [external_id]
            )
            select
                [target_id]
              , 'pulsecheck'
              , 'user_quick_list_items'
              , [source_id]
            from [#user_quick_list_items] as [source]
            where [site_id] > 0
                  and [target_id] is not null
                  and [existing_record] = 0;

        /****************
                end table
        ****************/

            --
            -- check for emar orphans
            -- the way to deal with orphans has to be determined for each table
            --
            --  user_quick_list_items: orphans will be deleted
            --
            delete [target]
            from [dbo].[user_quick_list_items] [target]
                outer apply [dbo].[get_external_id]('pulsecheck', 'user_quick_list_items', [target].[id]) [gii]
                left join [#user_quick_list_items] as [source]
                    on [gii].[id] = [source].[source_id]
            where [source].[source_id] is null;

        end;

    drop table if exists [#medication_items];
    drop table if exists [#user_quick_list_items];

end;
go