use emar;
set nocount on;

declare
    @type_of_run varchar(6) = 'report';    ---(report / update)
/******************************************************************************************************************/
/******************************************************************************************************************/
/******************************************************************************************************************/
/******************************************************************************************************************/
/******************************************************************************************************************/
/******************************************************************************************************************/
/******************************************************************************************************************/
/******************************************************************************************************************/
/******************************************************************************************************************/
/******************************************************************************************************************/
/******************************************************************************************************************/
/******************************************************************************************************************/
/******************************************************************************************************************/
/******************************************************************************************************************/
/******************************************************************************************************************/
/******************************************************************************************************************/
/******************************************************************************************************************/
/******************************************************************************************************************/
/******************************************************************************************************************/
drop table if exists [#preferred_frequency_schedules_import_template];
drop table if exists [#preferred_frequency_schedules];

declare
    @error_update   varchar(50)   = 'preferred_frequency_schedules_update_script.sql'
  , @error_number   int           = 0
  , @error_severity int           = 0
  , @error_state    int           = 0
  , @error_line     int           = 0
  , @error_message  varchar(2048) = '';


create table [#preferred_frequency_schedules]
    (
        [id]                      [int]     null
      , [site_id]                 [int]     null
      , [medication_id]           [int]     null
      , [frequency_schedule_id]   [int]     null
      , [frequency_schedule_name] [sysname] null
    );

create table [#preferred_frequency_schedules_import_template]
    (
        [site_id]            [int]           null
      , [medication_id]      [int]           null
      , [medication_site_id] [int]           null
      , [drug_db_vendor]     [char](1)       null
      , [site_name]          [nvarchar](60)  null
      , [drug_id]            [varchar](32)   null
      , [display_name]       [nvarchar](100) null
      , [button01]           sysname         null
      , [button02]           sysname         null
      , [button03]           sysname         null
      , [button04]           sysname         null
      , [button05]           sysname         null
      , [button06]           sysname         null
      , [button07]           sysname         null
      , [button08]           sysname         null
      , [button09]           sysname         null
      , [button10]           sysname         null
    );

insert into [#preferred_frequency_schedules_import_template]
select
    cast(null as int)     as [site_id]
  , cast(null as int)     as [medication_id]
  , cast(null as int)     as [medication_site_id]
  , cast(null as char(1)) as [drug_db_vendor]
  , [source].[site_name]
  , [source].[drug_id]
  , [source].[display_name]
  , [source].[button01]
  , [source].[button02]
  , [source].[button03]
  , [source].[button04]
  , [source].[button05]
  , [source].[button06]
  , [source].[button07]
  , [source].[button08]
  , [source].[button09]
  , [source].[button10]
from [dbo].[preferred_frequency_schedules_import_template] as [source];

-- Get Site ID from Name
update [target] set
    [site_id] = [source].[id]
from [#preferred_frequency_schedules_import_template] as [target]
    inner join [dbo].[sites] as [source]
        on [source].[name] = [target].[site_name];

-- Get Site drug db vendor
update [target] set
    [drug_db_vendor] = [so].[option_value]
from [#preferred_frequency_schedules_import_template] as [target]
    inner join [dbo].[site_options] [so]
        on [so].[site_id] = [target].[site_id]
    inner join [dbo].[options] [o]
        on [so].[option_id] = [o].[id]
where [o].[name] = 'DRUG_DB_VENDOR';

-- Get Medication ID for non combo drugs
update [target] set
    [medication_id]      = [source].[id]
  , [medication_site_id] = [source].[site_id]
from [#preferred_frequency_schedules_import_template] as [target]
    inner join [dbo].[medications] as [source]
        on [source].[drug_id] = [target].[drug_id]
            and [source].[site_id] = -1
            and [source].[drug_vendor] = [target].[drug_db_vendor]
where [target].[drug_id] <> 'COMBO';

-- Get Medication ID for site specific combo drugs
update [target] set
    [medication_id]      = [source].[id]
  , [medication_site_id] = [source].[site_id]
from [#preferred_frequency_schedules_import_template] as [target]
    inner join [dbo].[medications] as [source]
        on [source].[drug_id] = [target].[drug_id]
            and [source].[site_id] = [target].[site_id]
            and [source].[drug_vendor] = [target].[drug_db_vendor]
            and [source].[display_name] = [target].[display_name]
where [target].[drug_id] = 'COMBO';

-- load all frequency schedule buttons into single column using "UNION" to eliminate duplicates
with cte_all
    as (
                 select
                     [site_id]
                   , [medication_id]
                   , [button01] [frequency_schedule_name]
                 from [#preferred_frequency_schedules_import_template]
                 where [button01] is not null
                 union
                 select
                     [site_id]
                   , [medication_id]
                   , [button02] [frequency_schedule_name]
                 from [#preferred_frequency_schedules_import_template]
                 where [button02] is not null
                 union
                 select
                     [site_id]
                   , [medication_id]
                   , [button03] [frequency_schedule_name]
                 from [#preferred_frequency_schedules_import_template]
                 where [button03] is not null
                 union
                 select
                     [site_id]
                   , [medication_id]
                   , [button04] [frequency_schedule_name]
                 from [#preferred_frequency_schedules_import_template]
                 where [button04] is not null
                 union
                 select
                     [site_id]
                   , [medication_id]
                   , [button05] [frequency_schedule_name]
                 from [#preferred_frequency_schedules_import_template]
                 where [button05] is not null
                 union
                 select
                     [site_id]
                   , [medication_id]
                   , [button06] [frequency_schedule_name]
                 from [#preferred_frequency_schedules_import_template]
                 where [button06] is not null
                 union
                 select
                     [site_id]
                   , [medication_id]
                   , [button07] [frequency_schedule_name]
                 from [#preferred_frequency_schedules_import_template]
                 where [button07] is not null
                 union
                 select
                     [site_id]
                   , [medication_id]
                   , [button08] [frequency_schedule_name]
                 from [#preferred_frequency_schedules_import_template]
                 where [button08] is not null
                 union
                 select
                     [site_id]
                   , [medication_id]
                   , [button09] [frequency_schedule_name]
                 from [#preferred_frequency_schedules_import_template]
                 where [button09] is not null
                 union
                 select
                     [site_id]
                   , [medication_id]
                   , [button10] [frequency_schedule_name]
                 from [#preferred_frequency_schedules_import_template]
                 where [button10] is not null
        )
insert into [#preferred_frequency_schedules]
(
    [site_id]
  , [medication_id]
  , [frequency_schedule_name]
)
select
    [site_id]
  , [medication_id]
  , [frequency_schedule_name]
from cte_all;

-- Get frequency_schedule_id
update [target] set
    [frequency_schedule_id] = [source].[id]
from [#preferred_frequency_schedules] as [target]
    inner join [dbo].[frequency_schedules] as [source]
        on [source].[name] = [target].[frequency_schedule_name]
            and [source].[site_id] = [target].[site_id];

-- Match the Records in the database to the import file
update [target] set
    [id] = [source].[id]
from [#preferred_frequency_schedules] [target]
    inner join [dbo].[preferred_frequency_schedules] [source]
        on [source].[site_id] = [target].[site_id]
            and [source].[medication_id] = [target].[medication_id]
            and [source].[frequency_schedule_id] = [target].[frequency_schedule_id];

if lower(@type_of_run) = lower('report')
    begin
        with cte_report_delete
            as (
                         select
                             [m].[drug_id]
                           , [m].[display_name]
                           , [fs].[name]        [frequency_schedule_name]
                           , [target].[site_id] [preferred_frequency_schedule_site_id]
                           , case
                                 when [m].[site_id] = -1 then [target].[site_id]
                                 else [m].[site_id]
                             end                [medications_site_id]
                         from [dbo].[preferred_frequency_schedules] [target]
                             inner join [dbo].[medications] [m]
                                 on [m].[id] = [target].[medication_id]
                             inner join [dbo].[frequency_schedules] [fs]
                                 on [fs].[id] = [target].[frequency_schedule_id]
                             left join [#preferred_frequency_schedules] [source]
                                 on [source].[id] = [target].[id]
                         where [source].[id] is null
                               and [target].[site_id] in (select distinct [site_id] from [#preferred_frequency_schedules])
                ),
            cte_report_insert
            as (
                         select
                             [m].[drug_id]
                           , [m].[display_name]
                           , [fs].[name]        [frequency_schedule_name]
                           , [source].[site_id] [preferred_frequency_schedule_site_id]
                           , case
                                 when [m].[site_id] = -1 then [source].[site_id]
                                 else [m].[site_id]
                             end                [medications_site_id]
                         from [#preferred_frequency_schedules] as [source]
                             inner join [dbo].[medications] [m]
                                 on [m].[id] = [source].[medication_id]
                             inner join [dbo].[frequency_schedules] [fs]
                                 on [fs].[id] = [source].[frequency_schedule_id]
                         where [source].[id] is null
                )
        select
            case
                when [del].[preferred_frequency_schedule_site_id] <> [del].[medications_site_id] then 'DELETE Bad Site Record'
                else 'DELETE'
            end        [note]
          , [s].[name] [site_name]
          , [del].[drug_id]
          , [del].[display_name]
          , [del].[frequency_schedule_name]
        from cte_report_delete [del]
            inner join [dbo].[sites] [s]
                on [del].[preferred_frequency_schedule_site_id] = [s].[id]
        union all
        select
            'INSERT'   [note]
          , [s].[name] [site_name]
          , [ins].[drug_id]
          , [ins].[display_name]
          , [ins].[frequency_schedule_name]
        from cte_report_insert [ins]
            inner join [dbo].[sites] [s]
                on [ins].[preferred_frequency_schedule_site_id] = [s].[id]
        order by [note]
               , [site_name]
               , [drug_id]
               , [display_name]
               , [frequency_schedule_name];

    end;

if lower(@type_of_run) = lower('update')
    begin

    begin try
        begin transaction;

        declare
            @preferred_frequency_schedules_i int = 0
          , @preferred_frequency_schedules_u int = 0
          , @preferred_frequency_schedules_d int = 0;

        delete [target]
        from [dbo].[preferred_frequency_schedules] [target]
            left join [#preferred_frequency_schedules] [source]
                on [source].[id] = [target].[id]
        where [source].[id] is null
            and [target].[site_id] in (select distinct [site_id] from [#preferred_frequency_schedules]);

        set @preferred_frequency_schedules_d = @@rowcount;

        insert into [dbo].[preferred_frequency_schedules]
        (
            [site_id]
          , [medication_id]
          , [frequency_schedule_id]
        )
        select
            [source].[site_id]
          , [source].[medication_id]
          , [source].[frequency_schedule_id]
        from [#preferred_frequency_schedules] as [source]
        where [id] is null
              and [source].[site_id] is not null
              and [source].[medication_id] is not null
              and [source].[frequency_schedule_id] is not null;

        set @preferred_frequency_schedules_i = @@rowcount;

        select
            [table] =  'preferred_frequency_schedules'
          , [insert] = @preferred_frequency_schedules_i
          , [update] = @preferred_frequency_schedules_u
          , [delete] = @preferred_frequency_schedules_d;

        commit transaction;
    end try
    begin catch

        select
            @error_number = error_number()
          , @error_severity = error_severity()
          , @error_state = error_state()
          , @error_line = error_line()
          , @error_message = error_message();

        set @error_message = 'Location: ' + @error_update + '>>' +
        'errornumber:' + cast(@error_number as varchar(15)) + '>>' +
        'severty:' + cast(@error_severity as varchar(15)) + '>>' +
        'state:' + cast(@error_state as varchar(15)) + '>>' +
        'line:' + cast(@error_line as varchar(15)) + '>>' +
        @error_message;

        if @@trancount > 0
            begin
                rollback transaction;
            end;

        raiserror (@error_message, 16, 1);

    end catch;

    end;

go

if @@trancount > 0
    begin
        select
            'Emergency rollback transaction;';
        rollback transaction;
    end;

/*
select * from [dbo].[preferred_frequency_schedules_import_template] as [source] where [source].[drug_id] in ('436325', '436293') order by [source].[drug_id];
update source set [button03] = 'Every 4 hours while awake  --  (2)' from [dbo].[preferred_frequency_schedules_import_template] as [source] where [source].[drug_id] = '436293';
update source set [button03] = null                                 from [dbo].[preferred_frequency_schedules_import_template] as [source] where [source].[drug_id] = '436325';
--update source set [button03] = 'Every 4 hours while awake  --  (2)' from [dbo].[preferred_frequency_schedules_import_template] as [source] where [source].[drug_id] = '436325';
--update source set [button03] = null                                 from [dbo].[preferred_frequency_schedules_import_template] as [source] where [source].[drug_id] = '436293';
select * from [dbo].[preferred_frequency_schedules_import_template] as [source] where [source].[drug_id] in ('436325', '436293') order by [source].[drug_id];

select
    *
from dbo.[preferred_frequency_schedules_export_view] [pfsev]
where site_name = 'Middle-earth'
and [drug_id] in ('436325', '436293')
order by [drug_id]
*/
