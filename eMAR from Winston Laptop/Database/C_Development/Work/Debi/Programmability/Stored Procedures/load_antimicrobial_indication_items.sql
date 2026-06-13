create procedure [dbo].[load_antimicrobial_indication_items]
as
begin

    set nocount on;

    create table [#antimicrobial_indication_items]
        (
            [target_id]       [int]         null
          , [source_id]       [varchar](25) null
          , [site]            [varchar](25) not null
          , [sub_category]    [varchar](25) not null
          , [site_id]         [int]         null
          , [existing_record] [bit]         default 0
        );

    insert into [#antimicrobial_indication_items]
    (
        [source_id]
      , [site]
      , [sub_category]
    )
    execute ('execute dbo.export_ibex_antimicrobial_indication_items');

    if (
                 select
                     count(*)
                 from [#antimicrobial_indication_items]
        ) > 0
        begin

            begin transaction;

            /****************************************
                    load temporary tables for staging
            ****************************************/

            update [source] set
                [source_id] = [source].[site] + '|' + [source].[source_id]
            from [#antimicrobial_indication_items] as [source];

            update [source] set
                [site_id] = [internal_site].[id]
            from [#antimicrobial_indication_items] as [source]
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
                                 [target].[id]                    [internal_id]
                               , 'pulsecheck'                     [vendor]
                               , 'antimicrobial_indication_items' [entity]
                               , [source].[source_id]             [external_id]
                             from [#antimicrobial_indication_items] as [source]
                                 inner join [dbo].[antimicrobial_indication_items] [target]
                                     on [source].[site_id] = [target].[site_id]
                                         and [source].[sub_category] = [target].[sub_category]
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
            delete [target]
            from [dbo].[external_ids] [ei]
                left join [#antimicrobial_indication_items] as [source]
                    on [ei].[external_id] = [source].[source_id]
                left join [dbo].[antimicrobial_indication_items] [target]
                    on [ei].[internal_id] = [target].[id]
            where [ei].[vendor] = 'pulsecheck'
                and [ei].[entity] = 'antimicrobial_indication_items'
                and [source].[source_id] is null
                and [target].[id] is not null;

            --- Delete Bad External ID Records
            delete [ei]
            from [dbo].[external_ids] [ei]
                left join [#antimicrobial_indication_items] as [source]
                    on [ei].[external_id] = [source].[source_id]
                left join [dbo].[antimicrobial_indication_items] [target]
                    on [ei].[internal_id] = [target].[id]
            where [ei].[vendor] = 'pulsecheck'
                and [ei].[entity] = 'antimicrobial_indication_items'
                and ([source].[source_id] is null
                    or [target].[id] is null);

            --- Match target_id's with external_ids table
            update [source] set
                [target_id]       = [internal_site].[id]
              , [existing_record] = 1
            from [#antimicrobial_indication_items] as [source]
                cross apply [dbo].[get_internal_id]
                ('pulsecheck', 'antimicrobial_indication_items', [source].[source_id]) as [internal_site];

            --- Update Matching Target Records
            update [target] set
                [site_id]      = [source].[site_id]
              , [sub_category] = [source].[sub_category]
            from [#antimicrobial_indication_items] as [source]
                cross apply [dbo].[get_internal_id]('pulsecheck', 'antimicrobial_indication_items', [source].[source_id]) [gii]
                inner join [dbo].[antimicrobial_indication_items] [target]
                    on [gii].[id] = [target].[id];

            --- Generate New id's for insert records
            with cte_data
                as (
                             select
                                 ident_current('[dbo].[antimicrobial_indication_items]') +
                                 row_number() over (order by [source].[source_id]) new_id
                               , [source].[target_id]
                             from [#antimicrobial_indication_items] as [source]
                                 outer apply [dbo].[get_internal_id]('pulsecheck', 'antimicrobial_indication_items', [source].[source_id]) [gii]
                                 left join [dbo].[antimicrobial_indication_items] [target]
                                     on [gii].[id] = [target].[id]
                             where [target].[id] is null
                    )
            update [data] set
                [target_id] = [data].[new_id]
            from cte_data [data];

            /*************************************
                    begin loading permanent tables
            *************************************/

            set identity_insert [dbo].[antimicrobial_indication_items] on;

            insert into [dbo].[antimicrobial_indication_items]
            (
                [id]
              , [site_id]
              , [sub_category]
            )
            select
                [source].[target_id]
              , [source].[site_id]
              , [source].[sub_category]
            from [#antimicrobial_indication_items] as [source]
            where [site_id] > 0
                  and [target_id] is not null
                  and [existing_record] = 0;

            set identity_insert [dbo].[antimicrobial_indication_items] off;

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
              , 'antimicrobial_indication_items'
              , [source].[source_id]
            from [#antimicrobial_indication_items] as [source]
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
            --  sites: antimicrobial_indication_items will be deleted
            --
            delete [target]
            from [dbo].[antimicrobial_indication_items] [target]
                outer apply [dbo].[get_external_id]('pulsecheck', 'antimicrobial_indication_items', [target].[id]) [gii]
                left join [#antimicrobial_indication_items] as [source]
                    on [gii].[id] = [source].[source_id]
            where [source].[source_id] is null;

            commit transaction;
        end;

    drop table if exists [#antimicrobial_indication_items];
end;
go
/*

begin transaction;
set rowcount 200;
delete [antimicrobial_indication_items] where id>1000;
delete [ei]
from [dbo].[external_ids] [ei]
where [ei].[entity] = 'antimicrobial_indication_items'
set rowcount 0;

go

--select
--    *
--from [dbo].[external_ids] [ei]
--where [ei].[entity] = 'antimicrobial_indication_items'
--order by 1;

execute [dbo].[load_antimicrobial_indication_items];
select
    *
from [antimicrobial_indication_items] as [source]
order by 1;


select
    *
from [dbo].[external_ids] [ei]
where [ei].[entity] = 'antimicrobial_indication_items'
order by 1;

rollback transaction;
go

--rollback transaction;

*/