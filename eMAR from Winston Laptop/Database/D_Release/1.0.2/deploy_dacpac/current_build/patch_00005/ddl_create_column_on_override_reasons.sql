-- test for
--1. ibex exists
--2. table exists
--3. column not exists
--ddl add column null
--populate data
--ddl change column not null

declare
    @ibex_exists                       bit = 0
  , @override_reasons_exists           bit = 0
  , @override_reasons_is_active_exists bit = 0;

select
    @ibex_exists = 1
from [sys].[databases] [d]
where [d].[name] = 'ibex';

select
    @override_reasons_exists = 1
from sys.[tables] [t]
where [t].[name] = 'override_reasons';

select
    @override_reasons_is_active_exists = 1
from [sys].[tables] [t]
    inner join [sys].[columns] [c]
        on [t].[object_id] = [c].[object_id]
where [t].[name] = 'override_reasons'
      and [c].[name] = 'is_active';

drop table if exists [#ddl];

create table [#ddl]
    (
        table_name    sysname
      , column_name   sysname
      , ibex_exists   bit
      , table_exists  bit
      , column_exists bit

    );

insert into [#ddl]
(
    [table_name]
  , [column_name]
  , [ibex_exists]
  , [table_exists]
  , [column_exists]
)
values
        ('override_reasons', 'is_active', @ibex_exists, @override_reasons_exists, @override_reasons_is_active_exists);

--~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
if exists (
             select
                 null
             from [#ddl]
             where [table_name] = 'override_reasons'
                   and [column_name] = 'is_active'
                   and [ibex_exists] = 1
                   and [table_exists] = 1
                   and [column_exists] = 0
    )
    begin
        print 'create column: alter table [dbo].[override_reasons] add [is_active] varchar(25) not null;';
        alter table [dbo].[override_reasons] add [is_active] varchar(25) null;
    end;
else
    begin
        print '**column exists: [dbo].[override_reasons].[is_active] varchar(25) not null;';
    end;
--~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
--- only use go when you need a new scope
--- be aware using a "go" will create a "new sql batch" (local scope)

go

if exists (
             select
                 null
             from [#ddl]
             where [table_name] = 'override_reasons'
                   and [ibex_exists] = 1
                   and [table_exists] = 1
                   and [column_exists] = 0
    )
    begin

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
                set id's
        ********************************/

        -- match id's based on name
        -- by name was how the data was originally loaded
        update [source] set
            [target_id] = [target].[id]
        from [#override_reasons] as [source]
            inner join [dbo].[override_reasons] [target]
                on [source].[description] = [target].[description]
                    and [source].[site_id] = [target].[site_id]
                    and [source].[is_medication] = [target].[is_medication]
        where [source].[target_id] is null;

        --for this first go-live scenerio this table should already be empty for override_reasons
        delete [ei]
        from [dbo].[external_ids] [ei]
        where [vendor] = 'pulsecheck'
            and [entity] = 'override_reasons';

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
        from [#override_reasons] as [source];

        -- update new column data
        update [target] set
            [site_id]       = [source].[site_id]
          , [description]   = [source].[description]
          , [is_active]     = [source].[is_active]
          , [is_medication] = [source].[is_medication]
        from [#override_reasons] [source]
            inner join [dbo].[override_reasons] [target]
                on [source].[target_id] = [target].[id];

    end;

-- change new columns to not null
if exists (
             select
                 null
             from [#ddl]
             where [table_name] = 'override_reasons'
                   and [column_name] = 'is_active'
                   and [ibex_exists] = 1
                   and [table_exists] = 1
                   and [column_exists] = 0
    )
    begin
        print 'modify: [dbo].[override_reasons] new columns not null;';
        alter table [dbo].[override_reasons] alter column [is_active] varchar(25) not null;
    end;
else
    begin
        print '**column exists: [dbo].[override_reasons] not null **previously completed;';
    end;
--~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
--- only use go when you need a new scope
--- be aware using a "go" will create a "new sql batch" (local scope)
go
drop table if exists [#ddl];
drop table if exists [#override_reasons];
go