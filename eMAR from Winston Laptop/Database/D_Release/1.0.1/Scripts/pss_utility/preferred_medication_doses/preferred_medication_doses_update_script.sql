use emar;
set nocount on;

declare
    @type_of_run varchar(6) = 'update';    ---(report / update)
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
drop table if exists [#preferred_medication_doses_import_template];
drop table if exists [#preferred_medication_doses];

declare
    @error_update   varchar(50)   = 'preferred_medication_doses_update_script.sql'
  , @error_number   int           = 0
  , @error_severity int           = 0
  , @error_state    int           = 0
  , @error_line     int           = 0
  , @error_message  varchar(2048) = '';


create table [#preferred_medication_doses]
    (
        [id]                   [int]            null
      , [site_id]              [int]            null
      , [medication_id]        [int]            null
      , [medication_unit_id]   [int]            null
      , [medication_unit_name] [nvarchar](50)   null
      , [dose]                 [decimal](11, 2) null
    );

create table [#preferred_medication_doses_import_template]
    (
        [site_id]            [int]           null
      , [medication_id]      [int]           null
      , [medication_site_id] [int]           null
      , [drug_db_vendor]     [char](1)       null
      , [site_name]          [nvarchar](60)  null
      , [drug_id]            [varchar](32)   null
      , [display_name]       [nvarchar](100) null
      , [dose01]             [nvarchar](50)  null
      , [unit_name01]        [nvarchar](50)  null
      , [dose02]             [nvarchar](50)  null
      , [unit_name02]        [nvarchar](50)  null
      , [dose03]             [nvarchar](50)  null
      , [unit_name03]        [nvarchar](50)  null
      , [dose04]             [nvarchar](50)  null
      , [unit_name04]        [nvarchar](50)  null
      , [dose05]             [nvarchar](50)  null
      , [unit_name05]        [nvarchar](50)  null
      , [dose06]             [nvarchar](50)  null
      , [unit_name06]        [nvarchar](50)  null
      , [dose07]             [nvarchar](50)  null
      , [unit_name07]        [nvarchar](50)  null
      , [dose08]             [nvarchar](50)  null
      , [unit_name08]        [nvarchar](50)  null
      , [dose09]             [nvarchar](50)  null
      , [unit_name09]        [nvarchar](50)  null
      , [dose10]             [nvarchar](50)  null
      , [unit_name10]        [nvarchar](50)  null
    );

insert into [#preferred_medication_doses_import_template]
select
    cast(null as int)     as [site_id]
  , cast(null as int)     as [medication_id]
  , cast(null as int)     as [medication_site_id]
  , cast(null as char(1)) as [drug_db_vendor]
  , [source].[site_name]
  , [source].[drug_id]
  , [source].[display_name]
  , [source].[dose01]
  , [source].[unit_name01]
  , [source].[dose02]
  , [source].[unit_name02]
  , [source].[dose03]
  , [source].[unit_name03]
  , [source].[dose04]
  , [source].[unit_name04]
  , [source].[dose05]
  , [source].[unit_name05]
  , [source].[dose06]
  , [source].[unit_name06]
  , [source].[dose07]
  , [source].[unit_name07]
  , [source].[dose08]
  , [source].[unit_name08]
  , [source].[dose09]
  , [source].[unit_name09]
  , [source].[dose10]
  , [source].[unit_name10]
from [dbo].[preferred_medication_doses_import_template] as [source];

-- Get Site ID from Name
update [target] set
    [site_id] = [source].[id]
from [#preferred_medication_doses_import_template] as [target]
    inner join [dbo].[sites] as [source]
        on [source].[name] = [target].[site_name];

-- Get Site drug db vendor
update [target] set
    [drug_db_vendor] = [so].[option_value]
from [#preferred_medication_doses_import_template] as [target]
    inner join [dbo].[site_options] [so]
        on [so].[site_id] = [target].[site_id]
    inner join [dbo].[options] [o]
        on [so].[option_id] = [o].[id]
where [o].[name] = 'DRUG_DB_VENDOR';

-- Get Medication ID for non combo drugs
update [target] set
    [medication_id]      = [source].[id]
  , [medication_site_id] = [source].[site_id]
from [#preferred_medication_doses_import_template] as [target]
    inner join [dbo].[medications] as [source]
        on [source].[drug_id] = [target].[drug_id]
            and [source].[site_id] = -1
            and [source].[drug_vendor] = [target].[drug_db_vendor]
where [target].[drug_id] <> 'COMBO';

-- Get Medication ID for site specific combo drugs
update [target] set
    [medication_id]      = [source].[id]
  , [medication_site_id] = [source].[site_id]
from [#preferred_medication_doses_import_template] as [target]
    inner join [dbo].[medications] as [source]
        on [source].[drug_id] = [target].[drug_id]
            and [source].[site_id] = [target].[site_id]
            and [source].[drug_vendor] = [target].[drug_db_vendor]
            and [source].[display_name] = [target].[display_name]
where [target].[drug_id] = 'COMBO';

-- load all medication routes buttons into single column using "UNION" to eliminate duplicates
with cte_all
    as (
                 select
                     [site_id]
                   , [medication_id]
                   , [dose01]      [dose]
                   , [unit_name01] [medication_unit_name]
                 from [#preferred_medication_doses_import_template]
                 where [unit_name01] is not null
                 union
                 select
                     [site_id]
                   , [medication_id]
                   , [dose02]      [dose]
                   , [unit_name02] [medication_unit_name]
                 from [#preferred_medication_doses_import_template]
                 where [unit_name02] is not null
                 union
                 select
                     [site_id]
                   , [medication_id]
                   , [dose03]      [dose]
                   , [unit_name03] [medication_unit_name]
                 from [#preferred_medication_doses_import_template]
                 where [unit_name03] is not null
                 union
                 select
                     [site_id]
                   , [medication_id]
                   , [dose04]      [dose]
                   , [unit_name04] [medication_unit_name]
                 from [#preferred_medication_doses_import_template]
                 where [unit_name04] is not null
                 union
                 select
                     [site_id]
                   , [medication_id]
                   , [dose05]      [dose]
                   , [unit_name05] [medication_unit_name]
                 from [#preferred_medication_doses_import_template]
                 where [unit_name05] is not null
                 union
                 select
                     [site_id]
                   , [medication_id]
                   , [dose06]      [dose]
                   , [unit_name06] [medication_unit_name]
                 from [#preferred_medication_doses_import_template]
                 where [unit_name06] is not null
                 union
                 select
                     [site_id]
                   , [medication_id]
                   , [dose07]      [dose]
                   , [unit_name07] [medication_unit_name]
                 from [#preferred_medication_doses_import_template]
                 where [unit_name07] is not null
                 union
                 select
                     [site_id]
                   , [medication_id]
                   , [dose08]      [dose]
                   , [unit_name08] [medication_unit_name]
                 from [#preferred_medication_doses_import_template]
                 where [unit_name08] is not null
                 union
                 select
                     [site_id]
                   , [medication_id]
                   , [dose09]      [dose]
                   , [unit_name09] [medication_unit_name]
                 from [#preferred_medication_doses_import_template]
                 where [unit_name09] is not null
                 union
                 select
                     [site_id]
                   , [medication_id]
                   , [dose10]      [dose]
                   , [unit_name10] [medication_unit_name]
                 from [#preferred_medication_doses_import_template]
                 where [unit_name10] is not null
        )
insert into [#preferred_medication_doses]
(
    [site_id]
  , [medication_id]
  , [dose]
  , [medication_unit_name]
)
select
    [site_id]
  , [medication_id]
  , [dose]
  , [medication_unit_name]
from cte_all;

-- Get medication_unit_id
update [target] set
    [medication_unit_id] = [source].[id]
from [#preferred_medication_doses] as [target]
    inner join [dbo].[medication_units] as [source]
        on [source].[name] = [target].[medication_unit_name]
            and [source].[site_id] = [target].[site_id];

-- Match the Records in the database to the import file
update [target] set
    [id] = [source].[id]
from [#preferred_medication_doses] [target]
    inner join [dbo].[preferred_medication_doses] [source]
        on [source].[site_id] = [target].[site_id]
            and [source].[medication_id] = [target].[medication_id]
            and [source].[medication_unit_id] = [target].[medication_unit_id]
            and [source].[dose] = [target].[dose];

if lower(@type_of_run) = lower('report')
    begin
        with cte_report_delete
            as (
                         select
                             [m].[drug_id]
                           , [m].[display_name]
                           , [fs].[name]        [medication_unit_name]
                           , [target].[site_id] [preferred_medication_unit_site_id]
                           , case
                                 when [m].[site_id] = -1 then [target].[site_id]
                                 else [m].[site_id]
                             end                [medications_site_id]
                             ,[target].[dose]
                         from [dbo].[preferred_medication_doses] [target]
                             inner join [dbo].[medications] [m]
                                 on [m].[id] = [target].[medication_id]
                             inner join [dbo].[medication_units] [fs]
                                 on [fs].[id] = [target].[medication_unit_id]
                             left join [#preferred_medication_doses] [source]
                                 on [source].[id] = [target].[id]
                         where [source].[id] is null
                               and [target].[site_id] in (select distinct [site_id] from [#preferred_medication_doses])
                ),
            cte_report_insert
            as (
                         select
                             [m].[drug_id]
                           , [m].[display_name]
                           , [fs].[name]        [medication_unit_name]
                           , [source].[site_id] [preferred_medication_unit_site_id]
                           , case
                                 when [m].[site_id] = -1 then [source].[site_id]
                                 else [m].[site_id]
                             end                [medications_site_id]
                             ,[source].[dose]
                         from [#preferred_medication_doses] as [source]
                             inner join [dbo].[medications] [m]
                                 on [m].[id] = [source].[medication_id]
                             inner join [dbo].[medication_units] [fs]
                                 on [fs].[id] = [source].[medication_unit_id]
                         where [source].[id] is null
                )
        select
            case
                when [del].[preferred_medication_unit_site_id] <> [del].[medications_site_id] then 'DELETE Bad Site Record'
                else 'DELETE'
            end        [note]
          , [s].[name] [site_name]
          , [del].[drug_id]
          , [del].[display_name]
                             ,[del].[dose]
          , [del].[medication_unit_name]
        from cte_report_delete [del]
            inner join [dbo].[sites] [s]
                on [del].[preferred_medication_unit_site_id] = [s].[id]
        union all
        select
            'INSERT'   [note]
          , [s].[name] [site_name]
          , [ins].[drug_id]
          , [ins].[display_name]
                             ,[ins].[dose]
          , [ins].[medication_unit_name]
        from cte_report_insert [ins]
            inner join [dbo].[sites] [s]
                on [ins].[preferred_medication_unit_site_id] = [s].[id]
        order by [drug_id]
               , [note]
               , [site_name]
               , [display_name]
               , [medication_unit_name];

    end;

if lower(@type_of_run) = lower('update')
    begin

    begin try
        begin transaction;

        declare
            @preferred_medication_doses_i int = 0
          , @preferred_medication_doses_u int = 0
          , @preferred_medication_doses_d int = 0;

        delete [target]
        from [dbo].[preferred_medication_doses] [target]
            left join [#preferred_medication_doses] [source]
                on [source].[id] = [target].[id]
        where [source].[id] is null
            and [target].[site_id] in (select distinct [site_id] from [#preferred_medication_doses]);

        set @preferred_medication_doses_d = @@rowcount;

        insert into [dbo].[preferred_medication_doses]
        (
            [site_id]
          , [medication_id]
          , [dose]
          , [medication_unit_id]
        )
        select
            [source].[site_id]
          , [source].[medication_id]
          , [source].[dose]
          , [source].[medication_unit_id]
        from [#preferred_medication_doses] as [source]
        where [id] is null
              and [source].[site_id] is not null
              and [source].[medication_id] is not null
              and [source].[medication_unit_id] is not null;

        set @preferred_medication_doses_i = @@rowcount;

        select
            [table] =  'preferred_medication_doses'
          , [insert] = @preferred_medication_doses_i
          , [update] = @preferred_medication_doses_u
          , [delete] = @preferred_medication_doses_d;


        --rollback transaction;
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
