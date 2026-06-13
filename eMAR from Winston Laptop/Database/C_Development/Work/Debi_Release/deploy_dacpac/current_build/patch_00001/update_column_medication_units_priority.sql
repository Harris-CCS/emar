-- test for
--1. ibex exists
--2. medication_units exists
--3. priority not exists

if exists (
             select
                 null
             from [#ddl]
             where [table_name] = 'medication_units'
                   and [ibex_exists] = 1
                   and [table_exists] = 1
                   and [column_exists] = 0
    )
    begin

        --if this script loads data twice; data duplication can occur
        declare
            @medication_units table
                (
                    [id]         [int]
                  , [site_id]    [int]         null
                  , [code]       [varchar](50) not null
                  , [name]       [varchar](50) null
                  , [print_name] [varchar](50) not null
                  , [is_active]  [bit]         not null
                  , [priority]   [int]         null
                );

        print 'Update Table: medication_units';

        drop table if exists [#medication_units];

        create table [#medication_units]
            (
                [id]         [int]         identity (1, 1)
              , [source_id]  [varchar](25) null
              , [target_id]  [int]         null
              , [site]       [varchar](25) not null
              , [code]       [varchar](50) not null
              , [name]       [varchar](50) not null
              , [print_name] [varchar](50) not null
              , [is_active]  [bit]         not null
              , [misc2]      [varchar](50) null
              , [site_id]    [int]         null
              , [priority]   [int]         null
            );


        insert into [#medication_units]
        (
            [source_id]
          , [site]
          , [code]
          , [name]
          , [print_name]
          , [is_active]
          , [misc2]
        )
        execute ('execute dbo.export_ibex_medication_units');


        if (
                     select
                         count(*)
                     from [#medication_units]
            ) > 0
            begin

                /****************************************
                        load temporary tables for staging
                ****************************************/

                update [source] set
                    [site_id]  = isnull([internal_site].[id], -1)
                  , [priority] =
                        case
                            when isnumeric([source].[misc2]) = 1 then cast([source].[misc2] as [int])
                            else 0
                        end
                from [#medication_units] as [source]
                    outer apply [dbo].[get_internal_id]
                    ('pulsecheck', 'sites', [source].[site]) as [internal_site];

                /********************************
                        set id's
                ********************************/

                -- match id's based on name
                -- by name was how the data was originally loaded
                update [source] set
                    [target_id] = [target].[id]
                from [#medication_units] as [source]
                    inner join [dbo].[medication_units] [target]
                        on [source].[code] = [target].[code]
                            and [source].[name] = [target].[name]
                            and [source].[site_id] = [target].[site_id];

                -- duplicates from import might be missing from original load
                -- mark duplicate as unmatched
                with cte_medication_units
                    as (
                                 select
                                     row_number() over (partition by [code], [name], [site] order by [code], [name], [site]) primay_row
                                   , [source].[id]
                                   , [source].[source_id]
                                   , [source].[target_id]
                                   , [source].[site]
                                   , [source].[name]
                                   , [source].[misc2]
                                   , [source].[site_id]
                                   , [source].[priority]
                                 from [#medication_units] as [source]
                        )
                update mr set
                    [target_id] = null
                from cte_medication_units [mr]
                where [mr].[primay_row] <> 1;


                --insert duplicate names that were not originally loaded
                insert into [dbo].[medication_units]
                (
                    [site_id]
                  , [code]
                  , [name]
                  , [print_name]
                  , [is_active]
                  , [priority]
                )
                output [inserted].[id]
                     , [inserted].[site_id]
                     , [inserted].[code]
                     , [inserted].[name]
                     , [inserted].[print_name]
                     , [inserted].[is_active]
                     , [inserted].[priority]
                       into @medication_units (
                       [id]
                       , [site_id]
                       , [code]
                       , [name]
                       , [print_name]
                       , [is_active]
                       , [priority]
                       )
                select
                    [source].[site_id]
                  , [source].[code]
                  , [source].[name]
                  , [source].[print_name]
                  , [source].[is_active]
                    -- bit of a hack but for the first (go-live scenerio it works)
                  , [source].[source_id]
                from [#medication_units] as [source]
                where [source].[target_id] is null;


                update [source] set
                    [target_id] = [target].[id]
                from [#medication_units] as [source]
                    inner join @medication_units [target]
                        on [source].[source_id] = [target].[priority]
                            and [source].[site_id] = [target].[site_id]
                where [source].[target_id] is null;

                --for this first go-live scenerio this table should already be empty for medication_units
                delete [ei]
                from [dbo].[external_ids] [ei]
                where [vendor] = 'pulsecheck'
                    and [entity] = 'medication_units';

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
                  , 'medication_units'
                  , [source].[site] + '|' + [source].[source_id]
                from [#medication_units] as [source];

                /*************************************
                        begin update permanent tables
                *************************************/

                update [target] set
                    [priority] =
                    [source].[priority]
                from [#medication_units] as [source]
                    inner join [dbo].[medication_units] as [target]
                        on [source].[target_id] = [target].[id]
                where [source].[site_id] > 0;

                declare
                    @medication_units_rows varchar(10);
                select
                    @medication_units_rows = @@rowcount;

                print 'medication_units Rows Updated: ' + @medication_units_rows;

            /****************
                    end table
            ****************/
            end;

        drop table if exists [#medication_units];

    end;
else
    begin
        print 'medication_units Rows Updated: Script has been run previously';
    end;
