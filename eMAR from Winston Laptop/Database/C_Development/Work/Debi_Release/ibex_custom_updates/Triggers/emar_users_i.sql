print 'create trigger [ibex].[dbo].[drs].[emar_users_i];'

set @template = N'
create or alter trigger [dbo].[emar_users_i] on [dbo].[drs] after insert as
begin

    set nocount on;

    insert into [dbo].[emar_update_queue]
        ([entity]
       , [external_id]
       , [event_datetime]
        )
    select ''users''
         , [i].[num]
         , sysdatetimeoffset()
    from   [inserted] as [i];
end;
';

set @sql_cmd = @template;

execute [ibex].[dbo].[sp_executesql] 
    @statement = @sql_cmd;