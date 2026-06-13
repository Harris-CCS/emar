create procedure [dbo].[load_order_instructions]
as
begin

    set nocount on;

    print 'Loading Table: order_instructions';

    drop table if exists [#order_instructions];

    create table [#order_instructions]
        (
            [target_id]       [int]         null
          , [source_id]       [varchar](25) null
          , [site]            [varchar](25) null
          , [status]          [char](1)     null
          , [code]            [varchar](25) null
          , [site_id]         [varchar](25) null
          , [description]     [varchar](80) null
          , [is_active]       [bit]         default 0
          , [existing_record] [bit]         default 0
        );

    insert into [#order_instructions]
    (
        [source_id]
      , [site]
      , [description]
      , [status]
      , [code]
    )
    execute ('execute dbo.export_ibex_order_instructions');


    if (
                 select
                     count(*)
                 from [#order_instructions]
        ) > 0
        begin

            --begin transaction;

            /****************************************
                    load temporary tables for staging
            ****************************************/

            update [source] set
                [source_id] = [source].[site] + '|' + [source].[source_id]
              , [is_active] =
                    case [status]
                        when 'Y' then 1
                        else 0
                    end
            from [#order_instructions] as [source];

            update [source] set
                [site_id] = isnull([internal_site].[id], -1)
            from [#order_instructions] as [source]
                outer apply [dbo].[get_internal_id]
                ('pulsecheck', 'sites', [source].[site]) as [internal_site];

            /********************************
             synchornize internal / external id's
            ********************************/
            /*          PointOfView: [dbo].[external_ids] [ei]
                        
                        source_id not null / target_id not null :: update in [TARGET]

                        ------------------------------------------
                        All other cases: Corrupt Record in [external_ids] 
                        we want to generate a new [external_ids] 
                        after inserting a new [TARGET] record
            
                        source_id     null / target_id not null :: delete in [dbo].[external_ids] delete in [TARGET]
                        source_id     null / target_id     null :: delete in [dbo].[external_ids]
                        source_id not null / target_id     null :: delete in [dbo].[external_ids]

            */

            --- if emar unique index/constraint exists validate and create any missing [dbo].[external_ids] [ei]
            with cte_constraint_match
                as (
                             select
                                 [target].[id]        [internal_id]
                               , 'pulsecheck'         [vendor]
                               , 'order_instructions' [entity]
                               , [source].[source_id] [external_id]
                             from [#order_instructions] as [source]
                                 inner join [dbo].[order_instructions] [target]
                                     on [source].[site_id] = [target].[site_id]
                                         and [source].[code] = [target].[code]
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

            --- Delete Missing Target Records
            update [target] set
                [is_active] = 1
            from [dbo].[external_ids] [ei]
                left join [#order_instructions] as [source]
                    on [ei].[external_id] = [source].[source_id]
                left join [dbo].[order_instructions] [target]
                    on [ei].[internal_id] = [target].[id]
            where [ei].[vendor] = 'pulsecheck'
                and [ei].[entity] = 'order_instructions'
                and [source].[source_id] is null
                and [target].[id] is not null;

            --- Delete Bad External ID Records
            delete [ei]
            from [dbo].[external_ids] [ei]
                left join [#order_instructions] as [source]
                    on [ei].[external_id] = [source].[source_id]
                left join [dbo].[order_instructions] [target]
                    on [ei].[internal_id] = [target].[id]
            where [ei].[vendor] = 'pulsecheck'
                and [ei].[entity] = 'order_instructions'
                and ([source].[source_id] is null
                    or [target].[id] is null);

            --- Match target_id's with external_ids table
            update [source] set
                [target_id]       = [internal_site].[id]
              , [existing_record] = 1
            from [#order_instructions] as [source]
                cross apply [dbo].[get_internal_id]
                ('pulsecheck', 'order_instructions', [source].[source_id]) as [internal_site];

            --- Update Matching Target Records
            update [target] set
                [site_id]     = [source].[site_id]
              , [code]        = [source].[code]
              , [description] = [source].[description]
              , [is_active]   = [source].[is_active]
            from [#order_instructions] as [source]
                cross apply [dbo].[get_internal_id]('pulsecheck', 'order_instructions', [source].[source_id]) [gii]
                inner join [dbo].[order_instructions] [target]
                    on [gii].[id] = [target].[id];

            --- Generate New id's for insert records
            with cte_data
                as (
                             select
                                 ident_current('[dbo].[order_instructions]') +
                                 row_number() over (order by [source].[source_id]) new_id
                               , [source].[target_id]
                             from [#order_instructions] as [source]
                                 outer apply [dbo].[get_internal_id]('pulsecheck', 'order_instructions', [source].[source_id]) [gii]
                                 left join [dbo].[order_instructions] [target]
                                     on [gii].[id] = [target].[id]
                             where [target].[id] is null
                    )
            update [data] set
                [target_id] = [data].[new_id]
            from cte_data [data];

            /*************************************
                    begin loading permanent tables
            *************************************/

            set identity_insert [dbo].[order_instructions] on;

            insert into [dbo].[order_instructions]
            (
                [id]
              , [site_id]
              , [description]
              , [is_active]
              , [code]
            )
            select
                [source].[target_id]
              , [source].[site_id]
              , [source].[description]
              , [source].[is_active]
              , [source].[code]
            from [#order_instructions] as [source]
            where [site_id] > 0
                  and [target_id] is not null
                  and [existing_record] = 0;

            set identity_insert [dbo].[order_instructions] off;

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
                [source].[target_id]
              , 'pulsecheck'
              , 'order_instructions'
              , [source].[source_id]
            from [#order_instructions] as [source]
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
            --  sites: order_instructions will be deleted
            --
            update [target] set
                [is_active] = 1
            from [dbo].[order_instructions] [target]
                outer apply [dbo].[get_external_id]('pulsecheck', 'order_instructions', [target].[id]) [gii]
                left join [#order_instructions] as [source]
                    on [gii].[id] = [source].[source_id]
            where [source].[source_id] is null;

        --commit transaction;
        end;

    drop table if exists [#order_instructions];
end;
go
/*
    begin transaction;

    execute [dbo].[load_order_instructions];

    select
        *
    from [dbo].[order_instructions] [oi];
    select
        *
    from [dbo].[external_ids] [oi] where entity='order_instructions'
go

rollback transaction;

*/

