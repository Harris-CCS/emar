print 'create trigger [ibex].[dbo].[hie_alg].[emar_patient_allergies_u];';

set @template = N'
create or alter trigger [dbo].[emar_patient_allergies_u] on [dbo].[hie_alg] after update as
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
    from   [inserted] as [i]
           inner join [deleted] as [d]          on [d].[num] = [i].[num]
           inner join [dbo].[pat] as [patients] on [patients].[site] = [i].[site]
                                               and [patients].[person] = [i].[person]
                                               and [patients].[acctnum] = [i].[acctnum]
    where ([i].[person] > char(1) and [i].[acctnum] > char(1)) and [patients].[emar_pat] = ''Y''
      and (
          isnull([i].[site]         ,char(1)) <> isnull([d].[site]         ,char(1))
       or isnull([i].[class]        ,char(1)) <> isnull([d].[class]        ,char(1))
       or isnull([i].[cat]          ,char(1)) <> isnull([d].[cat]          ,char(1))
       or isnull([i].[drug]         ,char(1)) <> isnull([d].[drug]         ,char(1))
       or isnull([i].[ndc]          ,char(1)) <> isnull([d].[ndc]          ,char(1))
       or isnull([i].[name]         ,char(1)) <> isnull([d].[name]         ,char(1))
       or isnull([i].[alg_drug_id]  ,char(1)) <> isnull([d].[alg_drug_id]  ,char(1))
       or isnull([i].[status]       ,char(1)) <> isnull([d].[status]       ,char(1))
       or isnull([i].[comment]      ,char(1)) <> isnull([d].[comment]      ,char(1))
       or isnull([i].[severity]     ,char(1)) <> isnull([d].[severity]     ,char(1))
       or isnull([i].[actionstatus] ,char(1)) <> isnull([d].[actionstatus] ,char(1))
          )
    union
    select [patients].[ibex]
         , [i].[site]
    from   [inserted] as [i]
           inner join [deleted] as [d]          on [d].[num] = [i].[num]
           inner join [dbo].[pat] as [patients] on [patients].[site] = [i].[site]
                                               and [patients].[person] = [i].[person]
    where ([i].[person] > char(1)) and [patients].[emar_pat] = ''Y''
      and (
          isnull([i].[site]         ,char(1)) <> isnull([d].[site]         ,char(1))
       or isnull([i].[class]        ,char(1)) <> isnull([d].[class]        ,char(1))
       or isnull([i].[cat]          ,char(1)) <> isnull([d].[cat]          ,char(1))
       or isnull([i].[drug]         ,char(1)) <> isnull([d].[drug]         ,char(1))
       or isnull([i].[ndc]          ,char(1)) <> isnull([d].[ndc]          ,char(1))
       or isnull([i].[name]         ,char(1)) <> isnull([d].[name]         ,char(1))
       or isnull([i].[alg_drug_id]  ,char(1)) <> isnull([d].[alg_drug_id]  ,char(1))
       or isnull([i].[status]       ,char(1)) <> isnull([d].[status]       ,char(1))
       or isnull([i].[comment]      ,char(1)) <> isnull([d].[comment]      ,char(1))
       or isnull([i].[severity]     ,char(1)) <> isnull([d].[severity]     ,char(1))
       or isnull([i].[actionstatus] ,char(1)) <> isnull([d].[actionstatus] ,char(1))
          )
    union
    select [patients].[ibex]
         , [i].[site]
    from   [inserted] as [i]
           inner join [deleted] as [d]          on [d].[num] = [i].[num]
           inner join [dbo].[pat] as [patients] on [patients].[site] = [i].[site]
                                               and [patients].[acctnum] = [i].[acctnum]
    where ([i].[acctnum] > char(1)) and [patients].[emar_pat] = ''Y''
      and (
          isnull([i].[site]         ,char(1)) <> isnull([d].[site]         ,char(1))
       or isnull([i].[class]        ,char(1)) <> isnull([d].[class]        ,char(1))
       or isnull([i].[cat]          ,char(1)) <> isnull([d].[cat]          ,char(1))
       or isnull([i].[drug]         ,char(1)) <> isnull([d].[drug]         ,char(1))
       or isnull([i].[ndc]          ,char(1)) <> isnull([d].[ndc]          ,char(1))
       or isnull([i].[name]         ,char(1)) <> isnull([d].[name]         ,char(1))
       or isnull([i].[alg_drug_id]  ,char(1)) <> isnull([d].[alg_drug_id]  ,char(1))
       or isnull([i].[status]       ,char(1)) <> isnull([d].[status]       ,char(1))
       or isnull([i].[comment]      ,char(1)) <> isnull([d].[comment]      ,char(1))
       or isnull([i].[severity]     ,char(1)) <> isnull([d].[severity]     ,char(1))
       or isnull([i].[actionstatus] ,char(1)) <> isnull([d].[actionstatus] ,char(1))
          )
    ) [patient]

end;
';

set @sql_cmd = @template;

execute [ibex].[dbo].[sp_executesql]
    @statement = @sql_cmd;

print 'create trigger [ibex].[dbo].[alg].[emar_patient_allergies2_u];';

set @template = N'
create or alter trigger [dbo].[emar_patient_allergies2_u] on [dbo].[alg] after update as
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
    inner join [deleted] as [d] on [d].[num] = [i].[num]
    inner join [dbo].[pat] as [patients] on [patients].[site] = [i].[site]
                                        and [patients].[ibex] = [i].[ibex]
    where [i].[type] = ''A'' and [patients].[emar_pat] = ''Y''
      and (
          isnull([i].[class]       ,char(1)) <> isnull([d].[class]       ,char(1))
       or isnull([i].[cat]         ,char(1)) <> isnull([d].[cat]         ,char(1))
       or isnull([i].[drug]        ,char(1)) <> isnull([d].[drug]        ,char(1))
       or isnull([i].[ndc]         ,char(1)) <> isnull([d].[ndc]         ,char(1))
       or isnull([i].[name]        ,char(1)) <> isnull([d].[name]        ,char(1))
       or isnull([i].[alt_name]    ,char(1)) <> isnull([d].[alt_name]    ,char(1))
       or isnull([i].[alg_drug_id] ,char(1)) <> isnull([d].[alg_drug_id] ,char(1))
       or isnull([i].[status]      ,char(1)) <> isnull([d].[status]      ,char(1))
       or isnull([i].[cmt]         ,char(1)) <> isnull([d].[cmt]         ,char(1))
       or isnull([i].[sched]       ,char(1)) <> isnull([d].[sched]       ,char(1))
       or isnull([i].[reaction]    ,char(1)) <> isnull([d].[reaction]    ,char(1))
       or isnull([i].[severity]    ,char(1)) <> isnull([d].[severity]    ,char(1))
       or isnull([i].[parent_id]   ,char(1)) <> isnull([d].[parent_id]   ,char(1))
       or isnull([i].[parent_name] ,char(1)) <> isnull([d].[parent_name] ,char(1))
       or isnull([i].[usr]         ,char(1)) <> isnull([d].[usr]         ,char(1))
       or isnull([i].[dateadd]     ,char(1)) <> isnull([d].[dateadd]     ,char(1))
       or isnull([i].[usrchg]      ,char(1)) <> isnull([d].[usrchg]      ,char(1))
       or isnull([i].[datechg]     ,char(1)) <> isnull([d].[datechg]     ,char(1))
       or isnull([i].[actionstatus],char(1)) <> isnull([d].[actionstatus],char(1))
       or isnull([i].[provider]    ,char(1)) <> isnull([d].[provider]    ,char(1))
          );
end;
';

set @sql_cmd = @template;

execute [ibex].[dbo].[sp_executesql]
    @statement = @sql_cmd;