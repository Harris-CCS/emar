print 'create trigger [ibex].[dbo].[trx].[emar_patient_problems_d];'

set @template = N'
create or alter trigger [dbo].[emar_patient_problems_d] on [dbo].[trx] after delete as
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
    from   [deleted] as [d]
    inner join [dbo].[pat] as [patients] on [patients].[site] = [d].[site]
                                        and [patients].[ibex] = [d].[ibex]
    where   [d].[type] = ''Q''
            and [d].status = ''A''
            and [d].[service] in(203, 200, 201)
            and [patients].[emar_pat] = ''Y'';

end;
';

set @sql_cmd = @template;

execute [ibex].[dbo].[sp_executesql]
    @statement = @sql_cmd;


print 'create trigger [ibex].[dbo].[problem_episode].[emar_patient_problems2_d];'

set @template = N'
create or alter trigger [dbo].[emar_patient_problems2_d] on [dbo].[problem_episode] after delete as
begin

    set nocount on;

    insert into [dbo].[emar_update_queue]
        ([entity]
       , [external_id]
       , [event_datetime]
        )
    select ''patients''
         , cast([patients].[site] as varchar(15))+''|''+[d].[ibex]
         , sysdatetimeoffset()
    from   [deleted] as [d]
    inner join [pat] as [patients]
           on [patients].[ibex] = [d].[ibex]
    where [patients].[emar_pat] = ''Y'';

end;
';

set @sql_cmd = @template;

execute [ibex].[dbo].[sp_executesql]
    @statement = @sql_cmd;