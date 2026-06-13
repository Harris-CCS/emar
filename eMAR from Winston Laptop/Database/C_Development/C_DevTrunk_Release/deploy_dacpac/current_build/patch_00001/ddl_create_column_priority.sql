-- test for
--1. ibex exists
--2. table exists
--3. column not exists

--execute ('alter table [dbo].[medication_routes] drop column if exists [priority];');
--execute ('alter table [dbo].[medication_units] drop column if exists [priority];');

declare
    @ibex_exists                       bit = 0
  , @medication_routes_exists          bit = 0
  , @medication_routes_priority_exists bit = 0
  , @medication_units_exists           bit = 0
  , @medication_units_priority_exists  bit = 0;

select
    @ibex_exists = 1
from [sys].[databases] [d]
where [d].[name] = 'ibex';

select
    @medication_routes_exists = 1
from sys.[tables] [t]
where [t].[name] = 'medication_routes';

select
    @medication_routes_priority_exists = 1
from [sys].[tables] [t]
    inner join [sys].[columns] [c]
        on [t].[object_id] = [c].[object_id]
where [t].[name] = 'medication_routes'
      and [c].[name] = 'priority';

select
    @medication_units_exists = 1
from [sys].[tables] [t]
where [t].[name] = 'medication_units';

select
    @medication_units_priority_exists = 1
from [sys].[tables] [t]
    inner join [sys].[columns] [c]
        on [t].[object_id] = [c].[object_id]
where [t].[name] = 'medication_units'
      and [c].[name] = 'priority';

drop table if exists [#ddl];

create table [#ddl]
    (
        table_name    sysname
      , ibex_exists   bit
      , table_exists  bit
      , column_exists bit

    );

insert into [#ddl]
(
    [table_name]
  , [ibex_exists]
  , [table_exists]
  , [column_exists]
)
values
    ('medication_routes', @ibex_exists, @medication_routes_exists, @medication_routes_priority_exists)
  , ('medication_units', @ibex_exists, @medication_units_exists, @medication_units_priority_exists)
;

if exists (
             select
                 null
             from [#ddl]
             where [table_name] = 'medication_routes'
                   and [ibex_exists] = 1
                   and [table_exists] = 1
                   and [column_exists] = 0
    )
    begin
        print 'create column: alter table [dbo].[medication_routes] add [priority] int null;';
        alter table [dbo].[medication_routes] add [priority] int null;
    end;
else
    begin
        print '**column exists: [dbo].[medication_routes].[priority] int null;';
    end;

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
        print 'create column: alter table [dbo].[medication_units] add [priority] int null;';
        alter table [dbo].[medication_units] add [priority] int null;
    end;
else
    begin
        print '**column exists: [dbo].[medication_units].[priority] int null;';
    end;

--- only use go when you need a new scope
--- such as ddl modification for a later update in a subsequent script

go