create procedure [dbo].[load_override_reasons]
as
begin

    set nocount on;

    print 'Loading Table: override_reasons';

    drop table if exists [#override_reasons];

    create table [#override_reasons]
        (
            [target_id]       [int]         null
          , [source_id]       [varchar](25) null
          , [site]            [varchar](25) null
          , [site_id]         [varchar](25) null
          , [type]            [varchar](25) null
          , [status]          [char](1)     null
          , [is_medication]   [bit]         null
          , [description]     [varchar](80) null
          , [is_active]       [bit]         default 0
          , [existing_record] [bit]         default 0
        );

    -- query remote data

    insert into [#override_reasons]
    (
        [source_id]
      , [site]
      , [type]
      , [description]
      , [status]
    )
    execute ('execute dbo.export_ibex_override_reasons');

    if (
                 select
                     count(*)
                 from [#override_reasons]
        ) > 0
        begin

            begin transaction;

            /****************************************
                    load temporary tables for staging
            ****************************************/

            -- transform remote data
            update [source] set
                [source_id]     = [source].[site] + '|' + [source].[source_id]
              , [is_active]     =
                    case [status]
                        when 'A' then 1
                        else 0
                    end
              , [is_medication] =
                    case [type]
                        when 'M' then 1
                        else 0
                    end
            from [#override_reasons] as [source];

            -- get internal site_id
            update [source] set
                [site_id] = isnull([internal_site].[id], -1)
            from [#override_reasons] as [source]
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
                               , 'override_reasons'   [entity]
                               , [source].[source_id] [external_id]
                             from [#override_reasons] as [source]
                                 inner join [dbo].[override_reasons] [target]
                                     on [source].[site_id] = [target].[site_id]
                                         and [source].[description] = [target].[description]
                                         and [source].[is_medication] = [target].[is_medication]
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
            where [ei].[internal_id] is null
            order by 1;

            --- Delete Missing Target Records
            update [target] set
                [is_active] = 0
            from [dbo].[external_ids] [ei]
                left join [#override_reasons] as [source]
                    on [ei].[external_id] = [source].[source_id]
                left join [dbo].[override_reasons] [target]
                    on [ei].[internal_id] = [target].[id]
            where [ei].[vendor] = 'pulsecheck'
                and [ei].[entity] = 'override_reasons'
                and [source].[source_id] is null
                and [target].[id] is not null;

            --- Delete Bad External ID Records
            delete [ei]
            from [dbo].[external_ids] [ei]
                left join [#override_reasons] as [source]
                    on [ei].[external_id] = [source].[source_id]
                left join [dbo].[override_reasons] [target]
                    on [ei].[internal_id] = [target].[id]
            where [ei].[vendor] = 'pulsecheck'
                and [ei].[entity] = 'override_reasons'
                and ([source].[source_id] is null
                    or [target].[id] is null);

            --- Match target_id's with external_ids table
            update [source] set
                [target_id]       = [internal_site].[id]
              , [existing_record] = 1
            from [#override_reasons] as [source]
                cross apply [dbo].[get_internal_id]
                ('pulsecheck', 'override_reasons', [source].[source_id]) as [internal_site];

            --- Update Matching Target Records
            update [target] set
                [site_id]       = [source].[site_id]
              , [description]   = [source].[description]
              , [is_medication] = [source].[is_medication]
              , [is_active]     = [source].[is_active]
            from [#override_reasons] as [source]
                cross apply [dbo].[get_internal_id]('pulsecheck', 'override_reasons', [source].[source_id]) [gii]
                inner join [dbo].[override_reasons] [target]
                    on [gii].[id] = [target].[id];

            --- Generate New id's for insert records
            with cte_data
                as (
                             select
                                 ident_current('[dbo].[override_reasons]') +
                                 row_number() over (order by [source].[source_id]) new_id
                               , [source].[target_id]
                             from [#override_reasons] as [source]
                                 outer apply [dbo].[get_internal_id]('pulsecheck', 'override_reasons', [source].[source_id]) [gii]
                                 left join [dbo].[override_reasons] [target]
                                     on [gii].[id] = [target].[id]
                             where [target].[id] is null
                    )
            update [data] set
                [target_id] = [data].[new_id]
            from cte_data [data];
            /*************************************
                    begin loading permanent tables
            *************************************/

            set identity_insert [dbo].[override_reasons] on;

            insert into [dbo].[override_reasons]
            (
                [id]
              , [site_id]
              , [is_medication]
              , [description]
              , [is_active]
            )
            select
                [target_id]
              , [site_id]
              , [is_medication]
              , [description]
              , [is_active]
            from [#override_reasons] as [source]
            where [site_id] > 0
                  and [target_id] is not null
                  and [existing_record] = 0;

            set identity_insert [dbo].[override_reasons] off;

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
              , 'override_reasons'
              , [source_id]
            from [#override_reasons] as [source]
            where [site_id] > 0
                  and [target_id] is not null
                  and [existing_record] = 0;


            /****************
                    end table
            ****************/

            commit transaction;
        end;

    drop table if exists [#override_reasons];

end;
go