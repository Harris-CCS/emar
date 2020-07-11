/****************************
delete all permanent data
    delete performed in hierarchal sequence
****************************/
set nocount on;

drop table if exists [#table_order];

declare 
    @load_level    int
  , @load_sequence int
  , @schema_name   sysname
  , @table_name    sysname
  , @sql_cmd       nvarchar(max);

create table [#table_order]
    (
      [load_level]    int
    , [load_sequence] int
    , [schema_name]   sysname
    , [table_name]    sysname);

insert into [#table_order] values(0,1,'dbo','actions')
insert into [#table_order] values(0,2,'dbo','medication_routes')
insert into [#table_order] values(0,3,'dbo','options')
insert into [#table_order] values(0,4,'dbo','permissions')
insert into [#table_order] values(0,5,'dbo','prompt_groups')
insert into [#table_order] values(0,6,'dbo','sites')
insert into [#table_order] values(0,7,'dbo','templates')
insert into [#table_order] values(1,1,'dbo','action_route_templates')
insert into [#table_order] values(1,2,'dbo','department_preferred_list')
insert into [#table_order] values(1,3,'dbo','override_reasons')
insert into [#table_order] values(1,4,'dbo','patients')
insert into [#table_order] values(1,5,'dbo','prompts')
insert into [#table_order] values(1,6,'dbo','site_code_shares')
insert into [#table_order] values(1,7,'dbo','site_formulary')
insert into [#table_order] values(1,8,'dbo','site_formulary_match')
insert into [#table_order] values(1,9,'dbo','site_options')
insert into [#table_order] values(1,10,'dbo','template_prompt_groups')
insert into [#table_order] values(1,11,'dbo','users')
insert into [#table_order] values(2,1,'dbo','patient_allergies')
insert into [#table_order] values(2,2,'dbo','patient_carts')
insert into [#table_order] values(2,3,'dbo','patient_home_medications')
insert into [#table_order] values(2,4,'dbo','patient_indicators')
insert into [#table_order] values(2,5,'dbo','patient_orders')
insert into [#table_order] values(2,6,'dbo','prompt_choices')
insert into [#table_order] values(2,7,'dbo','user_permissions')
insert into [#table_order] values(2,8,'dbo','user_quick_list')
insert into [#table_order] values(3,1,'dbo','order_administrations')
insert into [#table_order] values(3,2,'dbo','patient_cart_details')
insert into [#table_order] values(4,1,'dbo','order_administration_notes')
insert into [#table_order] values(4,2,'dbo','order_events')
insert into [#table_order] values(5,1,'dbo','order_event_details')
insert into [#table_order] values(99,1,'dbo','external_ids')

declare csr cursor local fast_forward
for select [tbl].[load_level]
         , [tbl].[load_sequence]
         , [tbl].[schema_name]
         , [tbl].[table_name]
    from   [#table_order] as [tbl]
    order by [load_level] desc
           , [load_sequence];

open csr;

fetch next from csr into 
    @load_level
  , @load_sequence
  , @schema_name
  , @table_name;

while @@FETCH_STATUS = 0
    begin

        set @sql_cmd = '    Reset LVL: ' + right('000' + cast(@load_level as varchar(3)), 3) + ' SEQ: ' + right('000' + cast(@load_sequence as varchar(3)), 3) + ' TBL: ' + @schema_name + '.' + @table_name + '';
        print @sql_cmd;

        set @sql_cmd = N'if exists (select null from sys.tables where name=''' + @table_name + ''') delete [' + @schema_name + '].[' + @table_name + '];';
        execute [sp_executeSQL] @sql_cmd;

        if @schema_name + '.' + @table_name = 'dbo.external_ids'
            begin
                -- do not reseed this table
                set @sql_cmd = '';
            end;
        else
            begin
                set @sql_cmd = N'if exists (select null from sys.tables where name=''' + @table_name + ''') dbcc checkident(''[' + @schema_name + '].[' + @table_name + ']'',reseed,1) with no_infomsgs;';
                execute [sp_executeSQL] @sql_cmd;
            end;

        fetch next from csr into 
            @load_level
          , @load_sequence
          , @schema_name
          , @table_name;
    end;

close csr;
deallocate csr;

    print 'COMPLETE: delete_emar_data.sql'