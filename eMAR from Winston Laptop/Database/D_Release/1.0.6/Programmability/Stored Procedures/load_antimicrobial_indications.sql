create procedure [dbo].[load_antimicrobial_indications]
as
begin

    set nocount on;

    create table [#antimicrobial_indications]
        (
            [target_id]        [int]           null
          , [source_id]        [varchar](25)   null
          , [site]             [varchar](25)   not null
          , [code]             [varchar](10)   null
          , [description]      [nvarchar](255) null
          , [status]           [char](1)       null
          , [position]         [varchar](25)   null
          , [is_active]        [bit]           null
          , [site_id]          [int]           null
          , [ordinal_position] [int]           null
          , [existing_record] [bit]         default 0
        );


    insert into [#antimicrobial_indications]
    (
        [source_id]
      , [site]
      , [code]
      , [description]
      , [status]
      , [position]
    )
    execute ('execute dbo.export_ibex_antimicrobial_indications');

    if (
                 select
                     count(*)
                 from [#antimicrobial_indications]
        ) > 0
        begin

            begin transaction;

            /****************************************
                    load temporary tables for staging
            ****************************************/
            update [source] set
                [source_id]        = [source].[site] + '|' + [source].[source_id]
              , [is_active]        =
                    case [status]
                        when 'A' then 1
                        else 0
                    end
              , [ordinal_position] =
                    case
                        when isnumeric([position]) = 1 then [position]
                        else 0
                    end
            from [#antimicrobial_indications] as [source];

            update [source] set
                [site_id] = [internal_site].[id]
            from [#antimicrobial_indications] as [source]
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
                                 [target].[id]               [internal_id]
                               , 'pulsecheck'                [vendor]
                               , 'antimicrobial_indications' [entity]
                               , [source].[source_id]        [external_id]
                             from [#antimicrobial_indications] as [source]
                                 inner join [dbo].[antimicrobial_indications] [target]
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
            delete [target]
            from [dbo].[external_ids] [ei]
                left join [#antimicrobial_indications] as [source]
                    on [ei].[external_id] = [source].[source_id]
                left join [dbo].[antimicrobial_indications] [target]
                    on [ei].[internal_id] = [target].[id]
            where [ei].[vendor] = 'pulsecheck'
                and [ei].[entity] = 'antimicrobial_indications'
                and [source].[source_id] is null
                and [target].[id] is not null;

            --- Delete Bad External ID Records
            delete [ei]
            from [dbo].[external_ids] [ei]
                left join [#antimicrobial_indications] as [source]
                    on [ei].[external_id] = [source].[source_id]
                left join [dbo].[antimicrobial_indications] [target]
                    on [ei].[internal_id] = [target].[id]
            where [ei].[vendor] = 'pulsecheck'
                and [ei].[entity] = 'antimicrobial_indications'
                and ([source].[source_id] is null
                    or [target].[id] is null);

            --- Match target_id's with external_ids table
            update [source] set
                [target_id]       = [internal_site].[id]
              , [existing_record] = 1
            from [#antimicrobial_indications] as [source]
                cross apply [dbo].[get_internal_id]
                ('pulsecheck', 'antimicrobial_indications', [source].[source_id]) as [internal_site];

            --- Update Matching Target Records
            update [target] set
                [site_id]          = [source].[site_id]
              , [code]             = [source].[code]
              , [description]      = [source].[description]
              , [is_active]        = [source].[is_active]
              , [ordinal_position] = [source].[ordinal_position]
            from [#antimicrobial_indications] as [source]
                cross apply [dbo].[get_internal_id]('pulsecheck', 'antimicrobial_indications', [source].[source_id]) [gii]
                inner join [dbo].[antimicrobial_indications] [target]
                    on [gii].[id] = [target].[id];

            --- Generate New id's for insert records
            with cte_data
                as (
                             select
                                 ident_current('[dbo].[antimicrobial_indications]') +
                                 row_number() over (order by [source].[source_id]) new_id
                               , [source].[target_id]
                             from [#antimicrobial_indications] as [source]
                                 outer apply [dbo].[get_internal_id]('pulsecheck', 'antimicrobial_indications', [source].[source_id]) [gii]
                                 left join [dbo].[antimicrobial_indications] [target]
                                     on [gii].[id] = [target].[id]
                             where [target].[id] is null
                    )
            update [data] set
                [target_id] = [data].[new_id]
            from cte_data [data];

            /*************************************
                    begin loading permanent tables
            *************************************/

            set identity_insert [dbo].[antimicrobial_indications] on;

            insert into [dbo].[antimicrobial_indications]
            (
                [id]
              , [site_id]
              , [code]
              , [description]
              , [is_active]
              , [ordinal_position]
            )
            select
                [target_id]
              , [site_id]
              , [code]
              , [description]
              , [is_active]
              , [ordinal_position]
            from [#antimicrobial_indications] as [source]
            where [site_id] > 0
                  and [target_id] is not null
                  and [existing_record] = 0;

            set identity_insert [dbo].[antimicrobial_indications] off;

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
              , 'antimicrobial_indications'
              , [source].[source_id]
            from [#antimicrobial_indications] as [source]
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
            --  sites: antimicrobial_indications will be deleted
            --
            delete [target]
            from [dbo].[antimicrobial_indications] [target]
                outer apply [dbo].[get_external_id]('pulsecheck', 'antimicrobial_indications', [target].[id]) [gii]
                left join [#antimicrobial_indications] as [source]
                    on [gii].[id] = [source].[source_id]
            where [source].[source_id] is null;

            commit transaction;
        end;

    drop table if exists [#antimicrobial_indications];
end;
go
/*
begin transaction;
--set rowcount 10;
--delete [antimicrobial_indications];
--set rowcount 0;

go



begin transaction;
set rowcount 0;
--delete [antimicrobial_indications];
delete [ei]
from [dbo].[external_ids] [ei]
where [ei].[entity] = 'antimicrobial_indications'
set rowcount 0;

go
select
    *
from [dbo].[external_ids] [ei]
where [ei].[entity] = 'antimicrobial_indications'
order by 1;

execute [dbo].[load_antimicrobial_indications];
select
    *
from [antimicrobial_indications] as [source]
order by 1;


select
    *
from [dbo].[external_ids] [ei]
where [ei].[entity] = 'antimicrobial_indications'
order by 1;

rollback transaction;
*/