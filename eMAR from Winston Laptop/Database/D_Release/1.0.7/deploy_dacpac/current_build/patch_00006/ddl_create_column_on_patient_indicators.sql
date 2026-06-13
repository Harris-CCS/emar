-- test for
--1. ibex exists
--2. table exists
--3. column not exists
--ddl add column null
--populate data
--ddl change column not null

declare
    @ibex_exists                               bit = 0
  , @patient_indicators_exists		           bit = 0
  , @patient_indicators_type_description_exists bit = 0;

select
    @ibex_exists = 1
from [sys].[databases] [d]
where [d].[name] = 'ibex';

select
    @patient_indicators_exists = 1
from sys.[tables] [t]
where [t].[name] = 'patient_indicators';

select
    @patient_indicators_type_description_exists = 1
from [sys].[tables] [t]
    inner join [sys].[columns] [c]
        on [t].[object_id] = [c].[object_id]
where [t].[name] = 'patient_indicators'
      and [c].[name] = 'type_description';

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
        ('patient_indicators', 'type_description', @ibex_exists, @patient_indicators_exists, @patient_indicators_type_description_exists);

--~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
if exists (
             select
                 null
             from [#ddl]
             where [table_name] = 'patient_indicators'
                   and [column_name] = 'type_description'
                   and [ibex_exists] = 1
                   and [table_exists] = 1
                   and [column_exists] = 0
    )
    begin
        print 'create column: alter table [dbo].[patient_indicators] add [type_description] nvarchar(255) null;';
        alter table [dbo].[patient_indicators] add [type_description] nvarchar(255) null;
    end;
else
    begin
        print '**column exists: [dbo].[patient_indicators].[type_description] nvarchar(255) null;';
    end;
--~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
--- only use go when you need a new scope
--- be aware using a "go" will create a "new sql batch" (local scope)

go

if exists (
             select
                 null
             from [#ddl]
             where [table_name] = 'patient_indicators'
                   and [ibex_exists] = 1
                   and [table_exists] = 1
                   and [column_exists] = 0
    )
    begin

        drop table if exists [#patient_indicators];

        create table [#patient_indicators]
            (
				   [target_id]           [bigint] null
				 , [source_id]           [varchar](25) not null
				 , [ordinal_position]    [smallint] not null
				 , [code]                [varchar](10) not null
				 , [type]                [varchar](10) not null
				 , [description]         [varchar](255) not null
				 , [image_name]          [nvarchar](255) not null
				 , [type_description]    [nvarchar](255) null
            );

        -- query remote data

        insert into [#patient_indicators]
        (
		    [source_id]
		  , [ordinal_position]
          , [code]
          , [type]
          , [description]
          , [image_name]
		  , [type_description]
        )
        execute ('execute dbo.export_ibex_patient_indicators');

        /********************************
                set id's
 
       ********************************/

	    -- get internal site_id
        update [source] set
            [target_id] = isnull([internal_id].[id], -1)
        from [#patient_indicators] as [source]
            outer apply [dbo].[get_internal_id]
            ('pulsecheck', 'patients', [source].[source_id]) as [internal_id];

        -- update new column data
        update [target] set
            [type_description]   = [source].[type_description]
		from [#patient_indicators] [source]
            inner join [dbo].[patient_indicators] [target]
                on [source].[target_id] = [target].[patient_id]
				and [source].[type] = [target].[type];

    end;

-- change new columns to not null
if exists (
             select
                null
             from [#ddl]
             where [table_name] = 'patient_indicators'
                   and [column_name] = 'type_description'
                   and [ibex_exists] = 1
                   and [table_exists] = 1
                   and [column_exists] = 0
    )
    begin
        print 'modify: [dbo].[patient_indicators] new columns not null;';
        alter table [dbo].[patient_indicators] alter column [type_description] varchar(255) not null;
    end;
else
    begin
        print '**column exists: [dbo].[patient_indicators] not null **previously completed;';
    end;
--~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
--- only use go when you need a new scope
--- be aware using a "go" will create a "new sql batch" (local scope)
go
drop table if exists [#ddl];
drop table if exists [#override_reasons];
go