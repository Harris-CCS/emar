print 'create trigger [ibex].[dbo].[hst].[emar_patients__hst_u];'

set @template = N'
create or alter trigger [dbo].[emar_patients__hst_u] on [dbo].[hst] after update as
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
           on [i].[hst_id] = [d].[hst_id]
    where [i].[emar_pat] = ''Y'' and
    (     isnull([i].[ibex]                ,char(1)) <> isnull([d].[ibex]                ,char(1))
       or isnull([i].[person]              ,char(1)) <> isnull([d].[person]              ,char(1))
       or isnull([i].[acctnum]             ,char(1)) <> isnull([d].[acctnum]             ,char(1))
       or isnull([i].[site]                ,char(1)) <> isnull([d].[site]                ,char(1))
       or isnull([i].[medrec]              ,char(1)) <> isnull([d].[medrec]              ,char(1))
       or isnull([i].[lname]               ,char(1)) <> isnull([d].[lname]               ,char(1))
       or isnull([i].[fname]               ,char(1)) <> isnull([d].[fname]               ,char(1))
       or isnull([i].[mname]               ,char(1)) <> isnull([d].[mname]               ,char(1))
       or isnull([i].[suffix]              ,char(1)) <> isnull([d].[suffix]              ,char(1))
       or isnull([i].[gender]              ,char(1)) <> isnull([d].[gender]              ,char(1))
       or isnull([i].[dob]                 ,char(1)) <> isnull([d].[dob]                 ,char(1))
       or isnull([i].[age]                 ,char(1)) <> isnull([d].[age]                 ,char(1))
       or isnull([i].[ageunits]            ,char(1)) <> isnull([d].[ageunits]            ,char(1))
       or isnull([i].[complaint]           ,char(1)) <> isnull([d].[complaint]           ,char(1))
       or isnull([i].[height]              ,char(1)) <> isnull([d].[height]              ,char(1))
       or isnull([i].[weight]              ,char(1)) <> isnull([d].[weight]              ,char(1))
       or isnull([i].[ward]                ,char(1)) <> isnull([d].[ward]                ,char(1))
       or isnull([i].[dept]                ,char(1)) <> isnull([d].[dept]                ,char(1))
       or isnull([i].[withdraw]            ,char(1)) <> isnull([d].[withdraw]            ,char(1))
       or isnull([i].[vsmaplevel]          ,char(1)) <> isnull([d].[vsmaplevel]          ,char(1))
       or isnull([i].[vsmap]               ,char(1)) <> isnull([d].[vsmap]               ,char(1))
       or isnull([i].[vsendtidallevel]     ,char(1)) <> isnull([d].[vsendtidallevel]     ,char(1))
       or isnull([i].[vsendtidal]          ,char(1)) <> isnull([d].[vsendtidal]          ,char(1))
       or isnull([i].[custom_insurance_id] ,char(1)) <> isnull([d].[custom_insurance_id] ,char(1))
       or isnull([i].[eun]                 ,char(1)) <> isnull([d].[eun]                 ,char(1))
       or isnull([i].[gender_system]       ,char(1)) <> isnull([d].[gender_system]       ,char(1))
       or isnull([i].[doctor]              ,char(1)) <> isnull([d].[doctor]              ,char(1))
       or isnull([i].[resident]            ,char(1)) <> isnull([d].[resident]            ,char(1))
       or isnull([i].[drextender]          ,char(1)) <> isnull([d].[drextender]          ,char(1))
       or isnull([i].[primarynurse]        ,char(1)) <> isnull([d].[primarynurse]        ,char(1))
       or isnull([i].[extender]            ,char(1)) <> isnull([d].[extender]            ,char(1))
       or isnull([i].[firstdoctor]         ,char(1)) <> isnull([d].[firstdoctor]         ,char(1)));
end;
';

set @sql_cmd = @template;

execute [ibex].[dbo].[sp_executesql]
    @statement = @sql_cmd;



