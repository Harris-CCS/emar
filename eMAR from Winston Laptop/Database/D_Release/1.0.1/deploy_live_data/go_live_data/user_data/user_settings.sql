print 'Loading Table: user_settings (DEFAULTS) Part 1';

/**********************************
Default Values for User settings
  when populating a system
  or adding new settings
**********************************/

declare 
    @default_values_up table
    (
      [name]          [nvarchar](40) not null
    , [default_value] [varchar](25) not null);

insert into @default_values_up
    ([name]
   , [default_value]
    )
select [name]
     , [value]
from   (values
    ('MEDICATION_SERVICES'      , 'E'),
    ('PATIENT_NAME_DISPLAY'     , 'Y'),
    ('PATIENT_PAGE_SORT'        , 'A'),
    ('DEPARTMENT_PAGE_SORT'     , 'A'),
    ('DEPARTMENT_PAGE_FILTERING', 'P')) as [val]([name], [value]);

drop table if exists [#user_settings];

create table [#user_settings]
    (
      [site_id]          [int] not null
    , [user_id]          [int] not null
    , [setting_id]    [int] not null
    , [setting_value] [varchar](255) not null);

/****************************************
        load temporary tables for staging
****************************************/

insert into [#user_settings]
    ([site_id]
   , [user_id]
   , [setting_id]
   , [setting_value]
    )
select [users].[site_id] as              [site_id]
     , [users].[id] as                   [user_id]
     , [setting].[id] as              [setting_id]
     , isnull([user_default].[default_value],'DEFAULT_NOT_DEFINED') as [user_default_value]
from   [dbo].[users] as [users]
       cross join [dbo].[settings] as [setting]
       left join @default_values_up as [user_default] on [user_default].[name] = [setting].[name];

create nonclustered index [temp_user_settings_01] on [#user_settings]
    ([user_id] asc, [site_id] asc, [setting_id] asc) 
      include
    ([setting_value]);

/*************************************
        begin loading permanent tables
*************************************/

delete [target]
from [#user_settings] as [source]
     right join [dbo].[user_settings] as [target] on [target].[site_id] = [source].[site_id]
                                                        and [target].[user_id] = [source].[user_id]
                                                        and [target].[setting_id] = [source].[setting_id]
where  [source].[site_id] is null;

/************************
update goes here
but this script requires 
no update statement
************************/

insert into [dbo].[user_settings]
    ([site_id]
   , [user_id]
   , [setting_id]
   , [setting_value]
    )
select [source].[site_id]
     , [source].[user_id]
     , [source].[setting_id]
     , [source].[setting_value]
from   [#user_settings] as [source]
       left join [dbo].[user_settings] as [target] on [target].[site_id] = [source].[site_id]
                                                         and [target].[user_id] = [source].[user_id]
                                                         and [target].[setting_id] = [source].[setting_id]
where  [target].[site_id] is null;

/****************
        end table
****************/

drop table if exists [#user_settings];




print 'Loading Table: user_settings (IMPORT VALUES) Part 2';

drop table if exists [#user_settings2];

create table [#user_settings2]
    (
        [ext_site_id]    [varchar](25)  null
      , [ext_user_id]    [varchar](25)  null
      , [ext_setting_id] [nvarchar](40) not null
      , [site_id]        int            null
      , [user_id]        int            null
      , [setting_id]     int            null
      , [setting_value]  [varchar](255) not null
    );

if '$(load_data)' = 'sample'
    or ('$(load_data)' = 'live'
        and @does_ibex_exist = 1)
    begin

        insert into [#user_settings2]
        (
            [ext_site_id]
          , [ext_user_id]
          , [ext_setting_id]
          , [setting_value]
        )
        execute ('execute dbo.export_ibex_user_settings');

        create nonclustered index [tmp_user_settings1] on [dbo].[#user_settings2]
        ([ext_setting_id]);

        create nonclustered index [tmp_user_settings2] on [dbo].[#user_settings2]
        ([ext_site_id]);
    end;

if (
             select
                 count(*)
             from [#user_settings2]
    ) > 0
    begin

        /*************************************
                begin loading permanent tables
        *************************************/

        begin transaction;

        update [target] set
            [site_id] = [source].[id]
        from [#user_settings2] as [target]
            outer apply [dbo].[get_internal_id]
            ('pulsecheck', 'sites', [target].[ext_site_id]) as [source];

        update [target] set
            [user_id] = [source].[id]
        from [#user_settings2] as [target]
            cross apply [dbo].[get_internal_id]
            ('pulsecheck', 'users', [target].[ext_user_id]) as [source];

        update [target] set
            [setting_id] = [source].[id]
        from [#user_settings2] as [target]
            inner join [dbo].[settings] as [source]
                on [source].[name] = [target].[ext_setting_id];

        update [source] set
            [setting_value] = [internal_device].[id]
        from [#user_settings2] as [source]
            outer apply [dbo].[get_internal_id]('pulsecheck', 'devices', [source].[setting_value]) as [internal_device]
        where [source].[ext_setting_id] = 'LAST_USED_PRINTER'
            and [source].[site_id] is not null;

        update [target] set
            [setting_value] = [source].[setting_value]
        from [#user_settings2] as [source]
            inner join [dbo].[user_settings] as [target]
                on [target].[site_id] = [source].[site_id]
                    and [target].[user_id] = [source].[user_id]
                    and [target].[setting_id] = [source].[setting_id]
        where [target].[setting_value] <> [source].[setting_value]
            and [source].[site_id] is not null;


        /****************
                end table
        ****************/

        commit transaction;
    end;

drop table if exists [#user_settings2];

