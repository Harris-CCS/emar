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
insert into [#table_order] values(0,2,'dbo','medication_routes',0);
insert into [#table_order] values(0,3,'dbo','options',0);
insert into [#table_order] values(0,4,'dbo','permissions',0);
insert into [#table_order] values(0,5,'dbo','prompt_groups',0);
insert into [#table_order] values(0,6,'dbo','sites',0);
insert into [#table_order] values(0,7,'dbo','templates',0);
insert into [#table_order] values(1,1,'dbo','action_route_templates',0);
insert into [#table_order] values(1,2,'dbo','department_preferred_list_items',0);
insert into [#table_order] values(1,3,'dbo','override_reasons',0);
insert into [#table_order] values(1,4,'dbo','patient_cart_orders',0);
insert into [#table_order] values(1,5,'dbo','patients',0);
insert into [#table_order] values(1,6,'dbo','prompts',0);
insert into [#table_order] values(1,7,'dbo','site_code_shares',0);
insert into [#table_order] values(1,8,'dbo','site_formulary',0);
insert into [#table_order] values(1,9,'dbo','site_formulary_match',0);
insert into [#table_order] values(1,10,'dbo','site_options',0);
insert into [#table_order] values(1,11,'dbo','template_prompt_groups',0);
insert into [#table_order] values(1,12,'dbo','users',0);
insert into [#table_order] values(2,1,'dbo','patient_allergies',0);
insert into [#table_order] values(2,2,'dbo','patient_home_medications',0);
insert into [#table_order] values(2,3,'dbo','patient_indicators',0);
insert into [#table_order] values(2,4,'dbo','patient_orders',0);
insert into [#table_order] values(2,5,'dbo','prompt_choices',0);
insert into [#table_order] values(2,6,'dbo','user_permissions',0);
insert into [#table_order] values(2,7,'dbo','user_quick_list_items',0);
insert into [#table_order] values(3,1,'dbo','order_administrations',0);
insert into [#table_order] values(4,1,'dbo','order_administration_notes',0);
insert into [#table_order] values(4,2,'dbo','order_events',0);
insert into [#table_order] values(5,1,'dbo','order_event_details',0);
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
            inner join [cte_identity] as [id] on [tbl].[schema_name] = [id].[schema]
                                                 and [tbl].[table_name] = [id].[table];

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
                set @sql_cmd = N'if exists (select null from sys.tables where name=''' + @table_name + ''') dbcc checkident(''[' + @schema_name + '].[' + @table_name + ']'',reseed,0) with no_infomsgs;';
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
