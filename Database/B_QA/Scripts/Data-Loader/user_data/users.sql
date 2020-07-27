print 'Loading Table: users';

drop table if exists [#users];

create table [#users]
    (
      [source_id]               [varchar](40) not null
    , [site_id]                 [varchar](25) not null
    , [type]                    [char](1) not null
    , [is_active]               [bit] not null
    , [initials_display]        [varchar](4) not null
    , [first_name]              [varchar](35) not null
    , [last_name]               [varchar](35) not null
    , [middle_name]             [varchar](35) not null
    , [name_suffix]             [varchar](35) not null
    , [ordering_only_physician] [bit] null
    , [name_display_initials]   [bit] null
    , [login_name]              [varchar](255) not null
    , [login_password]          [varchar](255) not null
    , [salt]                    [binary](16) not null
    , [last_login_time]         [varchar](50) null
    , [failed_login_attempts]   [int] not null);

if '$(load_data)' = 'live'
   and exists
(
    select null
    from   [master].[sys].[databases]
    where  [name] = 'ibex'
)
    begin

        insert into [#users]
            ([source_id]
           , [site_id]
           , [type]
           , [is_active]
           , [initials_display]
           , [first_name]
           , [last_name]
           , [middle_name]
           , [name_suffix]
           , [ordering_only_physician]
           , [name_display_initials]
           , [login_name]
           , [login_password]
           , [salt]
           , [last_login_time]
           , [failed_login_attempts]
            )
        execute ('execute dbo.export_ibex_users');
    end;

if '$(load_data)' = 'sample'
    begin

        bulk insert [#users] from '$(current_path)Scripts\Data-Loader\sample_data\users.bcp' with(fieldterminator = '|~', rowterminator = '\n');
    end;

if
(
    select count(*)
    from   [#users]
) > 0
    begin

        begin transaction;

/****************************************
        load temporary tables for staging
****************************************/

        alter table [#users]
        add [id]        [bigint] identity(1, 1)
          , [target_id] [bigint];

/********************************
        get max id for seed value
********************************/

        set @max_id = null;

        select @max_id = max([id])
        from   [dbo].[users];

        set @max_id = isnull(@max_id, 0);

        update [source] set    
            [target_id] = [source].[id] + @max_id
        from   [#users] as [source];

/*************************************
        begin loading permanent tables
*************************************/

        set identity_insert [dbo].[users] on;

        insert into [dbo].[users]
            ([id]
           , [site_id]
           , [type]
           , [is_active]
           , [initials_display]
           , [first_name]
           , [last_name]
           , [middle_name]
           , [name_suffix]
           , [ordering_only_physician]
           , [name_display_initials]
           , [login_name]
           , [login_password]
           , [salt]
           , [last_login_time]
           , [failed_login_attempts]
            )
        select [source].[target_id]
             , isnull([internal_site].[id], -1) as [site_id]
             , [source].[type]
             , [source].[is_active]
             , [source].[initials_display]
             , [source].[first_name]
             , [source].[last_name]
             , [source].[middle_name]
             , [source].[name_suffix]
             , [source].[ordering_only_physician]
             , [source].[name_display_initials]
             , [source].[login_name]
             , [source].[login_password]
             , [source].[salt]
             , case
                   when [source].[last_login_time] <= 100
                       then null
                   else dateadd(second,cast([source].[last_login_time] as bigint),'19700101') at time zone [site].[time_zone_name]
               end as                              [last_login_time]
             , [source].[failed_login_attempts]
        from   [#users] as [source]
               outer apply [dbo].[get_internal_id]
            ('pulsecheck', 'sites', [source].[site_id]) as [internal_site]
               left join [dbo].[sites] as [site] on [site].[id] = [internal_site].[id]
        order by [source].[last_name]
               , [source].[first_name];

        insert into [dbo].[users]
            ([id]
           , [site_id]
           , [type]
           , [is_active]
           , [initials_display]
           , [first_name]
           , [last_name]
           , [middle_name]
           , [name_suffix]
           , [ordering_only_physician]
           , [name_display_initials]
           , [login_name]
           , [login_password]
           , [salt]
           , [last_login_time]
           , [failed_login_attempts]
            )
        values
            ('0', '-1', '', 0, 0, 'Dummy User for Relational Integrity', 'Dummy User for Relational Integrity', '', '', '0', '0', '', '', 0x00, null, '0');

        set identity_insert [dbo].[users] off;

/***************************************
        loading [external_ids] reference
***************************************/

        insert into [dbo].[external_ids]
            ([internal_id]
           , [vendor]
           , [entity]
           , [external_id]
            )
        select [source].[target_id]
             , 'pulsecheck'
             , 'users'
             , [source].[source_id]
        from   [#users] as [source];

/****************
        end table
****************/

        commit transaction;
    end;

drop table if exists [#users];