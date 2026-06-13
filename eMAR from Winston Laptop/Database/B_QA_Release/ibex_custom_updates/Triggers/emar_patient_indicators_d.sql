print 'create trigger [ibex].[dbo].[pat_indicators].[emar_patient_indicators_d];'

set @template = N'
create or alter trigger [dbo].[emar_patient_indicators_d] on [dbo].[pat_indicators] after delete as
begin

    set nocount on;

    insert into [dbo].[emar_update_queue]
        ([entity]
       , [external_id]
       , [event_datetime]
        )
    select ''indicators''
         , cast([d].[site] as varchar(15))+''|''+[d].[ibex]
         , sysdatetimeoffset()
    from   [deleted] as [d]
    inner join [dbo].[pat] as [patients] on [patients].[site] = [d].[site]
                                        and [patients].[ibex] = [d].[ibex]
    where [patients].[emar_pat] = ''Y'';
end;
';

set @sql_cmd = @template;

execute [ibex].[dbo].[sp_executesql]
    @statement = @sql_cmd;