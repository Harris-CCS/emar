print 'create trigger [ibex].[dbo].[hie_meds].[emar_patient_medications_i];';

set @template = N'
create or alter trigger [dbo].[emar_patient_medications_i] on [dbo].[hie_meds] after insert as
begin

    set nocount on;

    insert into [dbo].[emar_update_queue]
        ([entity]
       , [external_id]
       , [event_datetime]
        )
    select ''patients''
         , cast([patient].[site] as varchar(15))+''|''+[patient].[ibex]
         , sysdatetimeoffset()
    from (
    select [patients].[ibex]
         , [i].[site]
    from     [inserted] as [i]
             inner join [dbo].[pat] as [patients] on [patients].[site] = [i].[site]
                                                     and [patients].[person] = [i].[person]
                                                     and [patients].[acctnum] = [i].[acctnum]
    where   [i].[person] > '''' and [i].[acctnum] > '''' and [patients].[emar_pat] = ''Y''
    union
    select [patients].[ibex]
         , [i].[site]
    from     [inserted] as [i]
             inner join [dbo].[pat] as [patients] on [patients].[site] = [i].[site]
                                                     and [patients].[person] = [i].[person]
    where   [i].[person] > '''' and [patients].[emar_pat] = ''Y''
    union
    select [patients].[ibex]
         , [i].[site]
    from   [inserted] as [i]
           inner join [dbo].[pat] as [patients] on [patients].[site] = [i].[site]
                                                   and [patients].[acctnum] = [i].[acctnum]
    where  [i].[acctnum] > '''' and [patients].[emar_pat] = ''Y''
    ) [patient]

end;
';

set @sql_cmd = @template;

execute [ibex].[dbo].[sp_executesql]
    @statement = @sql_cmd;

print 'create trigger [ibex].[dbo].[alg].[emar_patient_medications2_i];';

set @template = N'
create or alter trigger [dbo].[emar_patient_medications2_i] on [dbo].[alg] after insert as
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
    from  [inserted] as [i]
    inner join [dbo].[pat] as [patients] on [patients].[site] = [i].[site]
                                            and [patients].[ibex] = [i].[ibex]
   where  [i].[type] = ''M'' and [patients].[emar_pat] = ''Y'';
end;
';

set @sql_cmd = @template;

execute [ibex].[dbo].[sp_executesql] 
    @statement = @sql_cmd;