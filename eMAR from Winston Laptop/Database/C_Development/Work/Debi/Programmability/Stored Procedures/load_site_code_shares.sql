create procedure [dbo].[load_site_code_shares]
as
begin

    set nocount on;

    print 'Loading Table: site_code_shares';

    declare
        @code_share_tables table
            (
                ibex_name sysname
              , emar_name sysname
            );

    insert into @code_share_tables
    (
        ibex_name
      , emar_name
    )
    select
        ibex_name
      , emar_name
    from (
    values
      (''              , 'frequency_schedules')
    , ('med_indication', 'antimicrobial_indications')
    , ('med_route'     , 'medication_routes')
    , ('med_unit'      , 'medication_units')
    , (''              , 'order_instructions')
    ) as [items] (ibex_name, emar_name);

    drop table if exists [#site_code_shares];

    create table [#site_code_shares]
        (
            [target_id]      [int]         null
          , [source_site]    [varchar](25) null
          , [target_site]    [varchar](25) null
          , [source_site_id] [int]         null
          , [target_site_id] [int]         null
          , [source_entity]  sysname       null
          , [target_entity]  sysname       null
        );

    -- get remote data

    insert into [#site_code_shares]
    (
        [source_site]
      , [target_site]
      , [source_entity]
    )
    execute ('execute dbo.export_ibex_site_code_shares');

    -- translate ibex share name to emar share name
    update [source] set
        [target_entity] = [ref].[emar_name]
    from [#site_code_shares] [source]
        inner join @code_share_tables [ref]
            on [ref].[ibex_name] = [source].[source_entity];

    --- delete records not configured above for emar code share
    delete [#site_code_shares]
    where [target_entity] is null;

    /****************************************
            load temporary tables for staging
    ****************************************/

    -- translate ibex site to emar site
    update [source] set
        [source_site_id] = isnull([internal_site].[id], -1)
    from [#site_code_shares] as [source]
        cross apply [dbo].[get_internal_id]
        ('pulsecheck', 'sites', [source].[source_site]) as [internal_site];

    -- translate ibex site to emar site
    update [source] set
        [target_site_id] = isnull([internal_site].[id], -1)
    from [#site_code_shares] as [source]
        cross apply [dbo].[get_internal_id]
        ('pulsecheck', 'sites', [source].[target_site]) as [internal_site];

    --- add default records for [source_site_id] site
    --- that way every code share has itself as a default when no other share exists
    with cte_defaults
        as (
                     select
                         [s].[id]          [source_site_id]
                       , [s].[id]          [target_site_id]
                       , [cst].[emar_name] [target_entity]
                     from @code_share_tables [cst]
                         cross join [dbo].[sites] [s]
                     where [s].[id] > 0
            )
    insert into [#site_code_shares]
    (
        [source_site_id]
      , [target_site_id]
      , [target_entity]
    )
    select
        [source].[source_site_id]
      , [source].[target_site_id]
      , [source].[target_entity]
    from cte_defaults [source]
        left join [#site_code_shares] [target]
            on [source].[source_site_id] = [target].[source_site_id]
                and [source].[target_entity] = [target].[target_entity]
    where [target].[source_site_id] is null;


    if (
                 select
                     count(*)
                 from [#site_code_shares]
        ) > 0
        begin

            /********************************
             synchornize internal / external id's
            ********************************/
            update [source] set
                [target_id] = [target].[id]
            from [#site_code_shares] as [source]
                inner join [dbo].[site_code_shares] [target]
                    on [source].[source_site_id] = [target].[source_site_id]
                        and [source].[target_site_id] = [target].[target_site_id]
                        and [source].[target_entity] = [target].[entity]
            where [source].[target_id] is null;

            /*************************************
                    begin loading permanent tables
            *************************************/
            insert into [dbo].[site_code_shares]
            (
                [source_site_id]
              , [target_site_id]
              , [entity]
            )
            select
                [source_site_id]
              , [target_site_id]
              , [source].[target_entity]
            from [#site_code_shares] as [source]
                inner join @code_share_tables as [entities]
                    on [source].[target_entity] = [entities].[emar_name]
            where [source].[target_id] is null;

            /********************************
             synchornize internal / external id's
            ********************************/
            update [source] set
                [target_id] = [target].[id]
            from [#site_code_shares] as [source]
                inner join [dbo].[site_code_shares] [target]
                    on [source].[source_site_id] = [target].[source_site_id]
                        and [source].[target_site_id] = [target].[target_site_id]
                        and [source].[target_entity] = [target].[entity]
            where [source].[target_id] is null;

            delete [target]
            from [#site_code_shares] as [source]
                right join [dbo].[site_code_shares] [target]
                    on [source].[source_site_id] = [target].[source_site_id]
                        and [source].[target_site_id] = [target].[target_site_id]
                        and [source].[target_entity] = [target].[entity]
            where [source].[target_id] is null;

        /****************
                end table
        ****************/
        end;

    drop table if exists [#site_code_shares];
end;

go