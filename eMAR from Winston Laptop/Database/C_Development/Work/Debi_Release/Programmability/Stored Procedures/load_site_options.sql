create procedure [dbo].[load_site_options]
as
begin

    set nocount on;

    create table [#site_options]
        (
            [site]         [varchar](25)   null
          , [site_id]      [int]           null
          , [option_name]  [nvarchar](40)  null
          , [option_id]    [int]           null
          , [option_value] [varchar](255)  null
        );

    /*******************************
    Procedure has 2 parts
        1. Load Defaults for all sites \ all options
        2. Import Ibex Specific values
    *******************************/

    print 'Loading Table: site_options (DEFAULTS) Part 1';

    /*******************************
    Default Values for Site Options 
      when populating a system
      or adding new options
    *******************************/

    declare
        @default_values_so table
            (
                [name]          [nvarchar](40) not null
              , [default_value] [varchar](25)  not null
            );

    insert into @default_values_so
    (
        [name]
      , [default_value]
    )
    select
        [name]
      , [value]
    from (values
    ('LONG_DATE_FORMAT'             , 'MM/dd/yyyy'),
    ('SHORT_DATE_FORMAT'            , 'MM/dd/yy'  ),
    ('SCHEDULE_FUTURE_ITEMS'        , '7'         ),
--  ('PATIENT_IMAGE_PATH', '')      , ''          ), no default defined
--  ('CUSTOM_INDICATORS_IMAGE_PATH' , ''          ), no default defined
    ('RXALERT'                      , '0'         ),
    ('MEDINPAT'                     , 'N'         ),
    ('MEDOUTPAT'                    , 'N'         ),
    ('MEDPYXIS'                     , 'N'         ),
    ('MEDEXACTMATCH'                , 'N'         ),
    ('DRUG_DB_VENDOR'               , 'F'         ),
    ('SESSION_TIMEOUT'              , '240'       ),
--  ('SESSION_TIMEOUT_URL'          , ''          ), no default defined
    ('SHOW_DOSE_FORM'               , 'Y'         ),
    ('SHOW_STRENGTH'                , 'Y'         ),
    ('POPUP_ON_GIVE'                , 'N'         )
--  ('DEFAULT_PRINTER_ID'           , ''          ), no default defined
    ) as [val] ([name], [value]);

    /****************************************
            load temporary tables for staging
    ****************************************/

    insert into [#site_options]
    (
        [site_id]
      , [option_id]
      , [option_value]
    )
    select
        [site].[id]                                                   as [site_id]
      , [option].[id]                                                 as [option_id]
      , isnull([user_default].[default_value], 'DEFAULT_NOT_DEFINED') as [user_default_value]
    from [dbo].[sites] as [site]
        cross join [dbo].[options] as [option]
        left join @default_values_so as [user_default]
            on [user_default].[name] = [option].[name]
    where [site].[id] > 0;

    /*************************************
            begin loading permanent tables
    *************************************/

    delete [target]
    from [#site_options] as [source]
        right join [dbo].[site_options] as [target]
            on [target].[site_id] = [source].[site_id]
                and [target].[option_id] = [source].[option_id]
    where [source].[site_id] is null;

    /************************
    update goes here
    but this script requires 
    no update statement
    ************************/

    insert into [dbo].[site_options]
    (
        [site_id]
      , [option_id]
      , [option_value]
    )
    select
        [source].[site_id]
      , [source].[option_id]
      , [source].[option_value]
    from [#site_options] as [source]
        left join [dbo].[site_options] as [target]
            on [target].[site_id] = [source].[site_id]
                and [target].[option_id] = [source].[option_id]
    where [target].[site_id] is null;

    /****************
            end table
    ****************/

    print 'Loading Table: site_options (IMPORT VALUES) Part 2';

    truncate table [#site_options];

    insert into [#site_options]
    (
        [site]
      , [option_name]
      , [option_value]
    )
    execute ('execute dbo.export_ibex_site_options');

    if (
                 select
                     count(*)
                 from [#site_options]
        ) > 0
        begin

            /*************************************
                    begin loading permanent tables
            *************************************/

            -- get internal site_id
            update [source] set
                [site_id] = isnull([internal_site].[id], -1)
            from [#site_options] as [source]
                outer apply [dbo].[get_internal_id]
                ('pulsecheck', 'sites', [source].[site]) as [internal_site];

            -- get internal option_id
            update [source] set
                [option_id] = [o].[id]
            from [#site_options] as [source]
                inner join [dbo].[options] [o]
                    on [o].[name] = [source].[option_name];

            -- update imported option values
            update [target] set
                [option_value] = [source].[option_value]
            from [#site_options] as [source]
                inner join [dbo].[site_options] as [target]
                    on [target].[site_id] = [source].[site_id]
                        and [target].[option_id] = [source].[option_id]
            where [target].[option_value] <> [source].[option_value];

        /****************
                end table
        ****************/
        end;

    drop table if exists [#site_options];
end;
go
/*
begin transaction;

execute [dbo].[load_site_options];
select
    [so].[id]
  , [so].[site_id]
  , [so].[option_id]
  , [so].[option_value]
  ,[o].[name]
from [dbo].[site_options] [so]
                inner join [dbo].[options] [o]
                    on [o].[id] = [so].[option_id]
order by 2
       , 3
       , 4;
go
rollback transaction;
*/