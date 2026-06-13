print 'create trigger [ibex].[dbo].[pat].[emar_patients_d];'

set @template = N'
create or alter trigger [dbo].[emar_patients_d] on [dbo].[pat] after delete as
begin

    set nocount on;

    insert into [dbo].[emar_update_queue]
        ([entity]
       , [external_id]
       , [event_datetime]
        )
    select ''patients''
         , cast([d].[site] as varchar(15))+''|''+[d].[ibex]
         , sysdatetimeoffset()
    from   [deleted] as [d];
end;
';

set @sql_cmd = @template;

execute [ibex].[dbo].[sp_executesql]
    @statement = @sql_cmd;