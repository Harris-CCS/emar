print 'create trigger [ibex].[dbo].[pat_indicators].[emar_patient_indicators_i];'

set @template = N'
create or alter trigger [dbo].[emar_patient_indicators_i] on [dbo].[pat_indicators] after insert as
begin

    set nocount on;

    insert into [dbo].[emar_update_queue]
        ([entity]
       , [external_id]
       , [event_datetime]
        )
    select ''indicators''
         , cast([i].[site] as varchar(15))+''|''+[i].[ibex]
         , sysdatetimeoffset()
    from   [inserted] as [i]
    inner join [dbo].[pat] as [patients] on [patients].[site] = [i].[site]
                                        and [patients].[ibex] = [i].[ibex]
    where [patients].[emar_pat] = ''Y'';
end;
';

set @sql_cmd = @template;

execute [ibex].[dbo].[sp_executesql]
    @statement = @sql_cmd;