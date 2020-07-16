create procedure [dbo].[export_ibex_patients]
as
    begin

        select [source].[ibex]
             , [source].[site]
             , [source].[medrec]
             , [source].[acctnum]
             , [source].[lname]
             , [source].[fname]
             , [source].[mname]
             , [source].[suffix]
             , [source].[gender]
             , case
                   when isdate([source].[dob]) = 1
                       then cast([source].[dob] as date)
                        else null
               end as [date_of_birth]
             , [source].[age]
             , [source].[ageunits]
             , [source].[complaint]
             , [source].[height]
             , [source].[weight]
             , [source].[bed]
             , [source].[ward]
             , [source].[dept]
             , [source].[ord42]
             , [source].[ord23]
             , case
                   when [source].[naalert] = 'Y'
                       then 1
                        else 0
               end
             , case
                   when [source].[withdraw] = 'Y'
                       then 1
               else 0
               end
             , case
                   when isdate([source].[vsdate]) = 1
                       then cast([source].[vsdate] as date)
               else null
               end as [vsdate]
             , [source].[ord11]
             , [source].[vssys]
             , [source].[vsdia]
             , [source].[ord12]
             , [source].[vspulse]
             , [source].[vsmaplevel]
             , [source].[vsmap]
             , [source].[ord13]
             , [source].[vsresp]
             , [source].[ord14]
             , [source].[vstemp]
             , [source].[vsendtidallevel]
             , [source].[vsendtidal]
             , [source].[ord23]
             , [source].[vso2]
             , [source].[ord15]
             , [source].[vspain]
        from   [ibex].[dbo].[pat] as [source]
        order by [source].[lname]
               , [source].[fname]
               , case
                     when isdate([source].[dob]) = 1
                         then cast([source].[dob] as date)
                          else null
                 end
               , [source].[gender];
    end;
go
