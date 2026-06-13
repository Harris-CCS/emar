print 'Loading Table: user_settings';

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

