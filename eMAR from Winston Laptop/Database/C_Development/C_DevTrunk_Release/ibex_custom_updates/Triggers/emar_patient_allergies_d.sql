print 'create trigger [ibex].[dbo].[hie_alg].[emar_patient_allergies_d];';

set @template = N'
create or alter trigger [dbo].[emar_patient_allergies_d] on [dbo].[hie_alg] after delete as
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
         , [d].[site]
    from     [deleted] as [d]
             inner join [dbo].[pat] as [patients] on [patients].[site] = [d].[site]
                                                 and [patients].[person] = [d].[person]
                                                 and [patients].[acctnum] = [d].[acctnum]
    where   [d].[person] > '''' and [d].[acctnum] > '''' and [patients].[emar_pat] = ''Y''
    union
    select [patients].[ibex]
         , [d].[site]
    from     [deleted] as [d]
             inner join [dbo].[pat] as [patients] on [patients].[site] = [d].[site]
                                                 and [patients].[person] = [d].[person]
    where   [d].[person] > '''' and [patients].[emar_pat] = ''Y''
    union
    select [patients].[ibex]
         , [d].[site]
    from   [deleted] as [d]
           inner join [dbo].[pat] as [patients] on [patients].[site] = [d].[site]
                                               and [patients].[acctnum] = [d].[acctnum]
    where  [d].[acctnum] > '''' and [patients].[emar_pat] = ''Y''
    ) [patient]

end;
';

set @sql_cmd = @template;

execute [ibex].[dbo].[sp_executesql] 
    @statement = @sql_cmd;

print 'create trigger [ibex].[dbo].[alg].[emar_patient_allergies2_d];';

set @template = N'
create or alter trigger [dbo].[emar_patient_allergies2_d] on [dbo].[alg] after delete as
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
    from  [deleted] as [d]
    inner join [dbo].[pat] as [patients] on [patients].[site] = [d].[site]
                                        and [patients].[ibex] = [d].[ibex]
   where  [d].[type] = ''A'' and [patients].[emar_pat] = ''Y'';
end;
';

set @sql_cmd = @template;

execute [ibex].[dbo].[sp_executesql] 
    @statement = @sql_cmd;