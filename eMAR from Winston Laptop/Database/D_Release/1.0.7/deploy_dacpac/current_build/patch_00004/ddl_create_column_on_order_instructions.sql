-- test for
--1. ibex exists
--2. table exists
--3. column not exists

--ddl add column null
--populate data
--ddl change column not null

declare
    @ibex_exists                    bit = 0
  , @order_instructions_exists      bit = 0
  , @order_instructions_code_exists bit = 0;

select
    @ibex_exists = 1
from [sys].[databases] [d]
where [d].[name] = 'ibex';

select
    @order_instructions_exists = 1
from sys.[tables] [t]
where [t].[name] = 'order_instructions';

select
    @order_instructions_code_exists = 1
from [sys].[tables] [t]
    inner join [sys].[columns] [c]
        on [t].[object_id] = [c].[object_id]
where [t].[name] = 'order_instructions'
      and [c].[name] = 'code';

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
        ('order_instructions', 'code', @ibex_exists, @order_instructions_exists, @order_instructions_code_exists);

--~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
if exists (
             select
                 null
             from [#ddl]
             where [table_name] = 'order_instructions'
                   and [column_name] = 'code'
                   and [ibex_exists] = 1
                   and [table_exists] = 1
                   and [column_exists] = 0
    )
    begin
        print 'create column: alter table [dbo].[order_instructions] add [code] varchar(25) not null;';
        alter table [dbo].[order_instructions] add [code] varchar(25) null;
    end;
else
    begin
        print '**column exists: [dbo].[order_instructions].[code] varchar(25) not null;';
    end;
--~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
--- only use go when you need a new scope
--- be aware using a "go" will create a "new sql batch" (local scope)

go

if exists (
             select
                 null
             from [#ddl]
             where [table_name] = 'order_instructions'
                   and [ibex_exists] = 1
                   and [table_exists] = 1
                   and [column_exists] = 0
    )
    begin

        --if this script loads data twice; data duplication can occur
        declare
            @order_instructions table
                (
                    [id]          [int]
                  , [description] [varchar](50) null
                  , [site_id]     [int]         null
                  , [code]        [varchar](50) null
                );

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

        -- query remote data

        insert into [#order_instructions]
        (
            [source_id]
          , [site]
          , [description]
          , [status]
          , [code]
        )
        execute ('execute dbo.export_ibex_order_instructions');


        -- transform remote data
        update [source] set
            [source_id] = [source].[site] + '|' + [source].[source_id]
          , [is_active] =
                case [status]
                    when 'Y' then 1
                    else 0
                end
        from [#order_instructions] as [source];

        -- get internal site_id
        update [source] set
            [site_id] = isnull([internal_site].[id], -1)
        from [#order_instructions] as [source]
            outer apply [dbo].[get_internal_id]
            ('pulsecheck', 'sites', [source].[site]) as [internal_site];

        /********************************
                set id's
        ********************************/

        -- match id's based on name
        -- by name was how the data was originally loaded
        update [source] set
            [target_id] = [target].[id]
        from [#order_instructions] as [source]
            inner join [dbo].[order_instructions] [target]
                on [source].[description] = [target].[description]
                    and [source].[site_id] = [target].[site_id]
        where [source].[target_id] is null;

        -- duplicates from import might be missing from original load
        -- mark duplicate as unmatched
        with cte_order_instructions
            as (
                         select
                             row_number() over (partition by [source].[description], [site] order by [description], [site]) primay_row
                           , [source].[target_id]
                         from [#order_instructions] as [source]
                )
        update mr set
            [target_id] = null
        from cte_order_instructions [mr]
        where [mr].[primay_row] <> 1;

        --insert duplicate names that were not originally loaded
        insert into [dbo].[order_instructions]
        (
            [site_id]
          , [description]
          , [code]
        )
        output [inserted].[id]
             , [inserted].[site_id]
             , [inserted].[description]
             , [inserted].[code]
               into @order_instructions (
               [id]
               , [site_id]
               , [description]
               , [code]
               )
        select
            [site_id]
          , [description]
            -- bit of a hack but for the first (go-live scenerio it works)
          , [source_id]
        from [#order_instructions] as [source]
        where [source].[target_id] is null;

        update [source] set
            [target_id] = [target].[id]
        from [#order_instructions] as [source]
            inner join @order_instructions [target]
                on [source].[source_id] = [target].[code]
                    and [source].[site_id] = [target].[site_id]
        where [source].[target_id] is null;

        --for this first go-live scenerio this table should already be empty for order_instructions
        delete [ei]
        from [dbo].[external_ids] [ei]
        where [vendor] = 'pulsecheck'
            and [entity] = 'order_instructions';

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
          , 'order_instructions'
          , [source_id]
        from [#order_instructions] as [source];

        -- update new column data
        update [target] set
            [site_id]     = [source].[site_id]
          , [description] = [source].[description]
          , [is_active]   = [source].[is_active]
          , [code]        = [source].[code]
        from [#order_instructions] [source]
            inner join [dbo].[order_instructions] [target]
                on [source].[target_id] = [target].[id];

    end;

-- change new columns to not null
if exists (
             select
                 null
             from [#ddl]
             where [table_name] = 'order_instructions'
                   and [column_name] = 'code'
                   and [ibex_exists] = 1
                   and [table_exists] = 1
                   and [column_exists] = 0
    )
    begin
        print 'modify: [dbo].[order_instructions] new columns not null;';
        alter table [dbo].[order_instructions] alter column [code] varchar(25) not null;
    end;
else
    begin
        print '**column exists: [dbo].[order_instructions] not null **previously completed;';
    end;
--~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
--- only use go when you need a new scope
--- be aware using a "go" will create a "new sql batch" (local scope)
go
drop table if exists [#ddl];
drop table if exists [#order_instructions];
go