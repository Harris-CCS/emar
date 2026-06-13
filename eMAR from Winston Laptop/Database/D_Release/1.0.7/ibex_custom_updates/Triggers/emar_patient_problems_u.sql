print 'create trigger [ibex].[dbo].[trx].[emar_patient_problems_u];'

set @template = N'
create or alter trigger [dbo].[emar_patient_problems_u] on [dbo].[trx] after update as
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
    inner join [deleted] as [d]
           on [i].[trx_id] = [d].[trx_id]
    inner join [pat] as [patients]
           on [patients].[ibex] = [i].[ibex]
          and [patients].[site] = [i].[site]
    where   [i].[type] = ''Q''
            and [i].status = ''A''
            and [i].[service] in(203, 200, 201)
            and [patients].[emar_pat] = ''Y''
            and (isnull([i].[alienkey],char(1))  <> isnull([d].[alienkey],char(1))
             or  isnull([i].[name],char(1))      <> isnull([d].[name],char(1))
             or  isnull([i].[riskgreen],char(1)) <> isnull([d].[riskgreen],char(1))
             or  isnull([i].[service],0)      <> isnull([d].[service],0)
             or  isnull([i].[type],char(1))      <> isnull([d].[type],char(1))
             or  isnull([i].[status],char(1))    <> isnull([d].[status],char(1))
                );
end;
';

set @sql_cmd = @template;

execute [ibex].[dbo].[sp_executesql]
    @statement = @sql_cmd;



print 'create trigger [ibex].[dbo].[problem_episode].[emar_patient_problems2_u];'

set @template = N'
create or alter trigger [dbo].[emar_patient_problems2_u] on [dbo].[problem_episode] after update as
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
    inner join [deleted] as [d]
          on [i].[episode_id] = [d].[episode_id]
    inner join [pat] as [patients]
          on [patients].[ibex] = [i].[ibex]
    where [patients].[emar_pat] = ''Y'' and
          (  isnull([i].[problem_code],char(1))       <> isnull([d].[problem_code],char(1))
          or isnull([i].[problem_name],char(1))       <> isnull([d].[problem_name],char(1))
          or isnull([i].[problem_code_system],char(1))<> isnull([d].[problem_code_system],char(1))
          or isnull([i].[internal_status],char(1))    <> isnull([d].[internal_status],char(1)));

end;
';

set @sql_cmd = @template;

execute [ibex].[dbo].[sp_executesql]
    @statement = @sql_cmd;
