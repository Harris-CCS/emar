print 'create trigger [ibex].[dbo].[org].[emar_sites_i];'

set @template = N'
create or alter trigger [dbo].[emar_sites_i] on [dbo].[org] after insert as
begin

    set nocount on;

    insert into [dbo].[emar_update_queue]
        ([entity]
       , [external_id]
       , [event_datetime]
        )
    select ''sites''
         , [i].[site]
         , sysdatetimeoffset()
    from   [inserted] as [i];
end;
';

set @sql_cmd = @template;

execute [ibex].[dbo].[sp_executesql]
    @statement = @sql_cmd;