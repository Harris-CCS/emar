-- test for
--1. ibex exists
--2. table exists
--3. column not exists

--ddl add column not null
--populate data
--ddl change column not null

declare
    @ibex_exists                        bit = 0
  , @medication_routes_exists           bit = 0
  , @medication_routes_is_active_exists bit = 0
  , @medication_routes_code_exists      bit = 0
  , @medication_routes_type_exists      bit = 0;

select
    @ibex_exists = 1
from [sys].[databases] [d]
where [d].[name] = 'ibex';

select
    @medication_routes_exists = 1
from sys.[tables] [t]
where [t].[name] = 'medication_routes';

select
    @medication_routes_is_active_exists = 1
from [sys].[tables] [t]
    inner join [sys].[columns] [c]
        on [t].[object_id] = [c].[object_id]
where [t].[name] = 'medication_routes'
      and [c].[name] = 'is_active';

select
    @medication_routes_code_exists = 1
from [sys].[tables] [t]
    inner join [sys].[columns] [c]
        on [t].[object_id] = [c].[object_id]
where [t].[name] = 'medication_routes'
      and [c].[name] = 'code';

select
    @medication_routes_type_exists = 1
from [sys].[tables] [t]
    inner join [sys].[columns] [c]
        on [t].[object_id] = [c].[object_id]
where [t].[name] = 'medication_routes'
      and [c].[name] = 'type';

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
    ('medication_routes', 'code', @ibex_exists, @medication_routes_exists, @medication_routes_code_exists)
  , ('medication_routes', 'type', @ibex_exists, @medication_routes_exists, @medication_routes_type_exists)
  , ('medication_routes', 'is_active', @ibex_exists, @medication_routes_exists, @medication_routes_is_active_exists)
;

--~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
if exists (
             select
                 null
             from [#ddl]
             where [table_name] = 'medication_routes'
                   and [column_name] = 'code'
                   and [ibex_exists] = 1
                   and [table_exists] = 1
                   and [column_exists] = 0
    )
    begin
        print 'create column: alter table [dbo].[medication_routes] add [code] varchar(25) not null;';
        alter table [dbo].[medication_routes] add [code] varchar(25) null;
    end;
else
    begin
        print '**column exists: [dbo].[medication_routes].[code] varchar(25) not null;';
    end;
--~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

--~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
if exists (
             select
                 null
             from [#ddl]
             where [table_name] = 'medication_routes'
                   and [column_name] = 'type'
                   and [ibex_exists] = 1
                   and [table_exists] = 1
                   and [column_exists] = 0
    )
    begin
        print 'create column: alter table [dbo].[medication_routes] add [type] varchar(25) not null;';
        alter table [dbo].[medication_routes] add [type] varchar(25) null;
    end;
else
    begin
        print '**column exists: [dbo].[medication_routes].[type] varchar(25) not null;';
    end;
--~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

--~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
if exists (
             select
                 null
             from [#ddl]
             where [table_name] = 'medication_routes'
                   and [column_name] = 'is_active'
                   and [ibex_exists] = 1
                   and [table_exists] = 1
                   and [column_exists] = 0
    )
    begin
        print 'create column: alter table [dbo].[medication_routes] add [is_active] bit not null;';
        alter table [dbo].[medication_routes] add [is_active] bit null;
    end;
else
    begin
        print '**column exists: [dbo].[medication_routes].[is_active] bit not null;';
    end;
--~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

--- only use go when you need a new scope
--- be aware using a "go" will create a "new sql batch" (local scope)
go
drop table if exists [#medication_routes];
go
create table [#medication_routes]
    (
        [target_id]       [int]          null
      , [source_id]       [varchar](25)  null
      , [site]            [varchar](25)  null
      , [code]            [varchar](25)  null
      , [type]            [varchar](25)  null
      , [name]            [nvarchar](50) null
      , [misc2]           [varchar](50)  null
      , [misc3]           [varchar](50)  null
      , [site_id]         [int]          null
      , [priority]        [int]          null
      , [status]          [varchar](25)  null
      , [is_active]       [bit]          null
      , [existing_record] [bit]          default 0
    );
go
-- query remote data
insert into [#medication_routes]
(
    [source_id]
  , [site]
  , [name]
  , [misc2]
  , [misc3]
  , [status]
  , [code]
)
execute ('execute dbo.export_ibex_medication_routes');

-- transform remote data
update [source] set
    [source_id] = [site] + '|' + [source_id]
  , [priority]  =
        case
            when isnumeric([misc2]) = 1 then cast([misc2] as [int])
            else 0
        end
  , [is_active] =
        case
            when [status] = 'A' then 1
            else 0
        end
  , [type] = isnull([misc3],'')
from [#medication_routes] as [source];

-- get internal site_id
update [source] set
    [site_id] = isnull([internal_site].[id], -1)
from [#medication_routes] as [source]
    outer apply [dbo].[get_internal_id]
    ('pulsecheck', 'sites', [source].[site]) as [internal_site];


--- Match target_id's with external_ids table
update [source] set
    [target_id]       = [internal_site].[id]
  , [existing_record] = 1
from [#medication_routes] as [source]
    cross apply [dbo].[get_internal_id]
    ('pulsecheck', 'medication_routes', [source].[source_id]) as [internal_site];


-- update new column data
update [target] set
    [site_id]   = [source].[site_id]
  , [name]      = [source].[name]
  , [priority]  = [source].[priority]
  , [is_active] = [source].[is_active]
  , [code]      = [source].[code]
  , [type]      = [source].[type]
from [#medication_routes] [source]
    inner join [dbo].[medication_routes] [target]
        on [source].[target_id] = [target].[id];


-- change new columns to not null
if exists (
             select
                 null
             from [#ddl]
             where [table_name] = 'medication_routes'
                   and [column_name] in('code','is_active')
                   and [ibex_exists] = 1
                   and [table_exists] = 1
                   and [column_exists] = 0
    )
    begin
        print 'modify: [dbo].[medication_routes] new columns not null;';
        alter table [dbo].[medication_routes] alter column [is_active] bit not null;
        alter table [dbo].[medication_routes] alter column [code] varchar(25) not null;
    end;
else
    begin
        print '**column exists: [dbo].[medication_routes] not null **previously completed;';
    end;

--- only use go when you need a new scope
--- be aware using a "go" will create a "new sql batch" (local scope)
go
drop table if exists [#ddl];
drop table if exists [#medication_routes];
go