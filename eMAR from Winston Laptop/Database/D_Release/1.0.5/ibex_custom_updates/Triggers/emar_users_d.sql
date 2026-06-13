print 'create trigger [ibex].[dbo].[drs].[emar_users_d];'

set @template = N'
create or alter trigger [dbo].[emar_users_d] on [dbo].[drs] after delete as
begin

    set nocount on;

    insert into [dbo].[emar_update_queue]
        ([entity]
       , [external_id]
       , [event_datetime]
        )
    select ''users''
         , [d].[num]
         , sysdatetimeoffset()
    from   [deleted] as [d];
end;
';

set @sql_cmd = @template;

execute [ibex].[dbo].[sp_executesql] 
    @statement = @sql_cmd;