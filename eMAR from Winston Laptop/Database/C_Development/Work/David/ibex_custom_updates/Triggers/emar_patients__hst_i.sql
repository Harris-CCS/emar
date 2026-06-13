print 'create trigger [ibex].[dbo].[hst].[emar_patients__hst_i];'

set @template = N'
create or alter trigger [dbo].[emar_patients__hst_i] on [dbo].[hst] after insert as
begin

    set nocount on;

    insert into [dbo].[emar_update_queue]
        ([entity]
       , [external_id]
       , [event_datetime]
        )
    select ''patients''
         , cast([i].[site] as varchar(15))+''|''+[i].[ibex]
         , sysdatetimeoffset()
    from   [inserted] as [i];
end;
';

set @sql_cmd = @template;

execute [ibex].[dbo].[sp_executesql]
    @statement = @sql_cmd;