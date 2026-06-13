print 'create trigger [ibex].[dbo].[trx].[emar_patient_problems_i];'

set @template = N'
create or alter trigger [dbo].[emar_patient_problems_i] on [dbo].[trx] after insert as
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
    from   [inserted] as [i]
    where   [i].[type] = ''Q''
            and [i].status = ''A''
            and [i].[service] in(203, 200, 201);
end;
';

set @sql_cmd = @template;

execute [ibex].[dbo].[sp_executesql]
    @statement = @sql_cmd;



print 'create trigger [ibex].[dbo].[problem_episode].[emar_patient_problems2_i];'

set @template = N'
create or alter trigger [dbo].[emar_patient_problems2_i] on [dbo].[problem_episode] after insert as
begin

    set nocount on;

    insert into [dbo].[emar_update_queue]
        ([entity]
       , [external_id]
       , [event_datetime]
        )
    select ''patients''
         , cast([patients].[site] as varchar(15))+''|''+[i].[ibex]
         , sysdatetimeoffset()
    from   [inserted] as [i]
    inner join [pat] as [patients]
           on [patients].[ibex] = [i].[ibex];
end;
';

set @sql_cmd = @template;

execute [ibex].[dbo].[sp_executesql]
    @statement = @sql_cmd;