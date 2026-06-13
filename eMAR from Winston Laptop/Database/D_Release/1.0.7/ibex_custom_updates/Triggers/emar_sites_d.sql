print 'create trigger [ibex].[dbo].[org].[emar_sites_d];'

set @template = N'
create or alter trigger [dbo].[emar_sites_d] on [dbo].[org] after delete as
begin

    set nocount on;

    insert into [dbo].[emar_update_queue]
        ([entity]
       , [external_id]
       , [event_datetime]
        )
    select ''sites''
         , [d].[site]
         , sysdatetimeoffset()
    from   [deleted] as [d];
end;
';

set @sql_cmd = @template;

execute [ibex].[dbo].[sp_executesql]
    @statement = @sql_cmd;