print 'create trigger [ibex].[dbo].[pat].[emar_patients_u];'

set @template = N'
create or alter trigger [dbo].[emar_patients_u] on [dbo].[pat] after update as
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
           on [i].[pat_id] = [d].[pat_id]
    where 
		  isnull([i].[emar_pat]            ,''N'')   <> isnull([d].emar_pat              ,''N'')
       or isnull([i].[ibex]                ,char(1)) <> isnull([d].[ibex]                ,char(1))
       or isnull([i].[person]              ,char(1)) <> isnull([d].[person]              ,char(1))
       or isnull([i].[acctnum]             ,char(1)) <> isnull([d].[acctnum]             ,char(1))
       or isnull([i].[site]                ,0)       <> isnull([d].[site]                ,0)
       or isnull([i].[medrec]              ,char(1)) <> isnull([d].[medrec]              ,char(1))
       or isnull([i].[lname]               ,char(1)) <> isnull([d].[lname]               ,char(1))
       or isnull([i].[fname]               ,char(1)) <> isnull([d].[fname]               ,char(1))
       or isnull([i].[mname]               ,char(1)) <> isnull([d].[mname]               ,char(1))
       or isnull([i].[suffix]              ,char(1)) <> isnull([d].[suffix]              ,char(1))
       or isnull([i].[gender]              ,char(1)) <> isnull([d].[gender]              ,char(1))
       or isnull([i].[dob]                 ,char(1)) <> isnull([d].[dob]                 ,char(1))
       or isnull([i].[age]                 ,0)       <> isnull([d].[age]                 ,0)
       or isnull([i].[ageunits]            ,char(1)) <> isnull([d].[ageunits]            ,char(1))
       or isnull([i].[complaint]           ,char(1)) <> isnull([d].[complaint]           ,char(1))
       or isnull([i].[height]              ,0.00)    <> isnull([d].[height]              ,0.00)
       or isnull([i].[weight]              ,0.00)    <> isnull([d].[weight]              ,0.00)
       or isnull([i].[bed]                 ,char(1)) <> isnull([d].[bed]                 ,char(1))
       or isnull([i].[ward]                ,char(1)) <> isnull([d].[ward]                ,char(1))
       or isnull([i].[dept]                ,char(1)) <> isnull([d].[dept]                ,char(1))
       or isnull([i].[ord42]               ,char(1)) <> isnull([d].[ord42]               ,char(1))
       or isnull([i].[naalert]             ,char(1)) <> isnull([d].[naalert]             ,char(1))
       or isnull([i].[withdraw]            ,char(1)) <> isnull([d].[withdraw]            ,char(1))
       or isnull([i].[vsdate]              ,char(1)) <> isnull([d].[vsdate]              ,char(1))
       or isnull([i].[ord11]               ,char(1)) <> isnull([d].[ord11]               ,char(1))
       or isnull([i].[vssys]               ,char(1)) <> isnull([d].[vssys]               ,char(1))
       or isnull([i].[vsdia]               ,char(1)) <> isnull([d].[vsdia]               ,char(1))
       or isnull([i].[ord12]               ,char(1)) <> isnull([d].[ord12]               ,char(1))
       or isnull([i].[vspulse]             ,char(1)) <> isnull([d].[vspulse]             ,char(1))
       or isnull([i].[vsmaplevel]          ,char(1)) <> isnull([d].[vsmaplevel]          ,char(1))
       or isnull([i].[vsmap]               ,char(1)) <> isnull([d].[vsmap]               ,char(1))
       or isnull([i].[ord13]               ,char(1)) <> isnull([d].[ord13]               ,char(1))
       or isnull([i].[vsresp]              ,char(1)) <> isnull([d].[vsresp]              ,char(1))
       or isnull([i].[ord14]               ,char(1)) <> isnull([d].[ord14]               ,char(1))
       or isnull([i].[vstemp]              ,char(1)) <> isnull([d].[vstemp]              ,char(1))
       or isnull([i].[vsendtidallevel]     ,char(1)) <> isnull([d].[vsendtidallevel]     ,char(1))
       or isnull([i].[vsendtidal]          ,char(1)) <> isnull([d].[vsendtidal]          ,char(1))
       or isnull([i].[ord23]               ,char(1)) <> isnull([d].[ord23]               ,char(1))
       or isnull([i].[vso2]                ,char(1)) <> isnull([d].[vso2]                ,char(1))
       or isnull([i].[ord15]               ,char(1)) <> isnull([d].[ord15]               ,char(1))
       or isnull([i].[vspain]              ,char(1)) <> isnull([d].[vspain]              ,char(1))
       or isnull([i].[custom_insurance_id] ,char(1)) <> isnull([d].[custom_insurance_id] ,char(1))
       or isnull([i].[eun]                 ,char(1)) <> isnull([d].[eun]                 ,char(1))
       or isnull([i].[gender_system]       ,char(1)) <> isnull([d].[gender_system]       ,char(1))
       or isnull([i].[doctor]              ,0)       <> isnull([d].[doctor]              ,0)
       or isnull([i].[resident]            ,0)       <> isnull([d].[resident]            ,0)
       or isnull([i].[drextender]          ,0)       <> isnull([d].[drextender]          ,0)
       or isnull([i].[primarynurse]        ,0)       <> isnull([d].[primarynurse]        ,0)
       or isnull([i].[extender]            ,0)       <> isnull([d].[extender]            ,0)
       or isnull([i].[firstdoctor]         ,0)       <> isnull([d].[firstdoctor]         ,0)
       or isnull([i].[exittype]            ,char(1)) <> isnull([d].[exittype]            ,char(1))
       or isnull([i].[exitcode]            ,char(1)) <> isnull([d].[exitcode]            ,char(1));
end;
';

set @sql_cmd = @template;

execute [ibex].[dbo].[sp_executesql]
    @statement = @sql_cmd;



