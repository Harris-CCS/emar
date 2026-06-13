print 'create table [ibex].[dbo].[emar_update_queue];'

set @template = N'
if not exists
(
    select null
    from   [sys].[objects]
    where  object_id = object_id(N''[emar_update_queue]'')
           and [type] in(N''U'')
)
    begin
        create table [dbo].[emar_update_queue]
            (
              [id]                 [bigint] identity(1, 1) not null
            , [entity]             [varchar](50) not null
            , [external_id]        [varchar](50) not null
            , [event_datetime]     datetimeoffset(7) null
            , [inprocess_datetime] datetimeoffset(7) null
            , [complete_datetime]  datetimeoffset(7) null
            , constraint [pk__emar_update_queue] primary key nonclustered([id] asc));

        create clustered index [cl_emar_update_queue]
        on [emar_update_queue]
            ([external_id] asc, [entity] asc);
    end;
';

set @sql_cmd = @template;

execute [ibex].[dbo].[sp_executesql] 
    @statement = @sql_cmd;

set @template = N'
if exists
(
    select null
    from  [sys].[columns] as [c]
    inner join [sys].[tables] as [t] on [c].object_id = [t].object_id
    inner join [sys].[schemas] as [sc] on [t].schema_id = [sc].schema_id
    where [sc].[name] = ''dbo''
      and [t].[name] = ''emar_update_queue''
      and [c].[name] = ''event_datetime''
      and [c].[is_nullable] = 1
)
    begin
        update dbo.emar_update_queue set event_datetime = sysdatetimeoffset() where event_datetime is null;
        alter table dbo.emar_update_queue alter column event_datetime datetimeoffset(7) not null;
    end;
';

set @sql_cmd = @template;

execute [ibex].[dbo].[sp_executesql] 
    @statement = @sql_cmd;

set @template = N'
declare 
    @df_name sysname
  , @cmd     nvarchar(max);

select @df_name = [d].[name]
from   [sys].[all_columns] as [c]
       inner join [sys].[tables] as [t] on [t].object_id = [c].object_id
       inner join [sys].[schemas] as [s] on [s].schema_id = [t].schema_id
       inner join [sys].[default_constraints] as [d] on [c].[default_object_id] = [d].object_id
where  [s].[name] = ''dbo''
       and [t].[name] = ''emar_update_queue''
       and [c].[name] = ''event_datetime'';

if isnull(@df_name, '''') <> ''df__emar_update_queue__event_datetime''
    begin
        if @df_name is not null
            begin
                set @cmd = replace(''alter table [dbo].[emar_update_queue] drop constraint <@df_name>'', ''<@df_name>'', @df_name);
                execute (@cmd);
            end;
        alter table [dbo].[emar_update_queue]
        add constraint [df__emar_update_queue__event_datetime] default(sysdatetimeoffset()) for [event_datetime];
    end;
';

set @sql_cmd = @template;

execute [ibex].[dbo].[sp_executesql] 
    @statement = @sql_cmd;