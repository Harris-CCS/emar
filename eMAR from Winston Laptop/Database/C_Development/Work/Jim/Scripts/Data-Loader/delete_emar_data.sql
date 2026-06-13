/******************************************
delete all permanent data
    delete performed in hierarchal sequence
******************************************/

set nocount on;

drop table if exists [#table_order];

declare
    @load_level    int
  , @load_sequence int
  , @schema_name   sysname
  , @table_name    sysname
  , @has_identity  bit
  , @sql_cmd       nvarchar(max);

create table [#table_order]
    (
      [load_level]    int
    , [load_sequence] int
    , [schema_name]   sysname
    , [table_name]    sysname
    , [has_identity]  bit);

insert into [#table_order] values(0,1,'dbo','actions',0);
insert into [#table_order] values(0,2,'dbo','duration_units',0);
insert into [#table_order] values(0,3,'dbo','fdb_allergy_name',0);
insert into [#table_order] values(0,4,'dbo','fdb_brand_name',0);
insert into [#table_order] values(0,5,'dbo','fdb_ndc_info',0);
insert into [#table_order] values(0,6,'dbo','frequency_calendar',0);
insert into [#table_order] values(0,7,'dbo','frequency_days',0);
insert into [#table_order] values(0,8,'dbo','frequency_interval_units',0);
insert into [#table_order] values(0,9,'dbo','frequency_minutes',0);
insert into [#table_order] values(0,10,'dbo','frequency_types',0);
insert into [#table_order] values(0,11,'dbo','global_options',0);
insert into [#table_order] values(0,12,'dbo','notification_categories',0);
insert into [#table_order] values(0,13,'dbo','options',0);
insert into [#table_order] values(0,14,'dbo','prompt_groups',0);
insert into [#table_order] values(0,15,'dbo','settings',0);
insert into [#table_order] values(0,16,'dbo','sites',0);
insert into [#table_order] values(1,1,'dbo','antimicrobial_indication_items',0);
insert into [#table_order] values(1,2,'dbo','antimicrobial_indications',0);
insert into [#table_order] values(1,3,'dbo','devices',0);
insert into [#table_order] values(1,4,'dbo','frequency_schedules',0);
insert into [#table_order] values(1,5,'dbo','medication_routes',0);
insert into [#table_order] values(1,6,'dbo','medication_units',0);
insert into [#table_order] values(1,7,'dbo','medications',0);
insert into [#table_order] values(1,8,'dbo','order_administration_available_actions',0);
insert into [#table_order] values(1,9,'dbo','order_available_actions',0);
insert into [#table_order] values(1,10,'dbo','order_instructions',0);
insert into [#table_order] values(1,11,'dbo','override_reasons',0);
insert into [#table_order] values(1,12,'dbo','patients',0);
insert into [#table_order] values(1,13,'dbo','prompts',0);
insert into [#table_order] values(1,14,'dbo','site_code_shares',0);
insert into [#table_order] values(1,15,'dbo','site_options',0);
insert into [#table_order] values(1,16,'dbo','template_response_rules',0);
insert into [#table_order] values(1,17,'dbo','users',0);
insert into [#table_order] values(2,1,'dbo','department_preferred_list_items',0);
insert into [#table_order] values(2,2,'dbo','frequency_interval_day_times',0);
insert into [#table_order] values(2,3,'dbo','group_list_items',0);
insert into [#table_order] values(2,4,'dbo','medication_details',0);
insert into [#table_order] values(2,5,'dbo','medication_interactions',0);
insert into [#table_order] values(2,6,'dbo','patient_allergies',0);
insert into [#table_order] values(2,7,'dbo','patient_home_medications',0);
insert into [#table_order] values(2,8,'dbo','patient_indicators',0);
insert into [#table_order] values(2,9,'dbo','patient_problems',0);
insert into [#table_order] values(2,10,'dbo','preferred_frequency_schedules',0);
insert into [#table_order] values(2,11,'dbo','preferred_medication_doses',0);
insert into [#table_order] values(2,12,'dbo','preferred_medication_routes',0);
insert into [#table_order] values(2,13,'dbo','print_history',0);
insert into [#table_order] values(2,14,'dbo','prompt_choices',0);
insert into [#table_order] values(2,15,'dbo','site_formulary',0);
insert into [#table_order] values(2,16,'dbo','site_formulary_match',0);
insert into [#table_order] values(2,17,'dbo','templates',0);
insert into [#table_order] values(2,18,'dbo','user_patients',0);
insert into [#table_order] values(2,19,'dbo','user_quick_list_items',0);
insert into [#table_order] values(2,20,'dbo','user_settings',0);
insert into [#table_order] values(3,1,'dbo','action_route_templates',0);
insert into [#table_order] values(3,2,'dbo','patient_cart_orders',0);
insert into [#table_order] values(3,3,'dbo','patient_orders',0);
insert into [#table_order] values(3,4,'dbo','template_prompt_groups',0);
insert into [#table_order] values(4,1,'dbo','cart_order_administrations',0);
insert into [#table_order] values(4,2,'dbo','order_administrations',0);
insert into [#table_order] values(4,3,'dbo','order_interactions',0);
insert into [#table_order] values(4,4,'dbo','order_reactions',0);
insert into [#table_order] values(5,1,'dbo','external_update_queue',0);
insert into [#table_order] values(5,2,'dbo','notifications',0);
insert into [#table_order] values(5,3,'dbo','order_administration_notes',0);
insert into [#table_order] values(5,4,'dbo','order_events',0);
insert into [#table_order] values(6,1,'dbo','order_event_details',0);
insert into [#table_order] values(99,1,'dbo','external_ids',0);

with cte_identity
     as (select [schema] = [s].[name]
              , [table] = [t].[name]
         from   [sys].[schemas] as [s]
                inner join [sys].[tables] as [t] on [s].[schema_id] = [t].[schema_id]
         where  exists
         (
             select 1
             from   [sys].[identity_columns]
             where  [object_id] = [t].[object_id]
         ))
     update [tbl] set
         [has_identity] = 1
     from   [#table_order] as [tbl]
            inner join [cte_identity] as [id] on [tbl].[schema_name] COLLATE DATABASE_DEFAULT = [id].[schema]
                                                 and [tbl].[table_name]  COLLATE DATABASE_DEFAULT = [id].[table];

declare csr cursor local fast_forward
for select [tbl].[load_level]
         , [tbl].[load_sequence]
         , [tbl].[schema_name]
         , [tbl].[table_name]
         , [tbl].[has_identity]
    from   [#table_order] as [tbl]
    order by [load_level] desc
           , [load_sequence];

open csr;

fetch next from csr into
    @load_level
  , @load_sequence
  , @schema_name
  , @table_name
  , @has_identity;

while @@FETCH_STATUS = 0
    begin

        set @sql_cmd = '    Reset LVL: ' + right('000' + cast(@load_level as varchar(3)), 3) + ' SEQ: ' + right('000' + cast(@load_sequence as varchar(3)), 3) + ' TBL: ' + @schema_name + '.' + @table_name + '';
        print @sql_cmd;

        set @sql_cmd = N'if exists (select null from sys.tables where name=''' + @table_name + ''') delete [' + @schema_name + '].[' + @table_name + '];';
        execute [sp_executeSQL] @sql_cmd;

        if @has_identity = 0
            begin
                -- do not reseed this table
                set @sql_cmd = '';
            end;
        else
            begin
                --- once a table seed value has been used the next ID will be seed+1
                --- truncate table will reset to seed
                --- delete table will remain seed+1
                set @sql_cmd = N'if exists (select null from sys.tables where name=''' + @table_name + ''')
    begin
        if (select IDENT_CURRENT(''[' + @schema_name + '].[' + @table_name + ']''))>1
            dbcc checkident(''[' + @schema_name + '].[' + @table_name + ']'',reseed,0) with no_infomsgs;
    end;';
                execute [sp_executeSQL] @sql_cmd;
            end;

        set @has_identity = 0;
        fetch next from csr into
            @load_level
          , @load_sequence
          , @schema_name
          , @table_name
          , @has_identity;
    end;

close csr;

deallocate csr;

print 'COMPLETE: delete_emar_data.sql';
