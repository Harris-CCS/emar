create procedure [dbo].[export_ibex_patients]
as
    begin

        select [source].[ibex]
             , ltrim(rtrim([source].[site]))
             , ltrim(rtrim([source].[medrec]))
             , ltrim(rtrim([source].[acctnum]))
             , ltrim(rtrim([source].[lname]))
             , ltrim(rtrim([source].[fname]))
             , ltrim(rtrim([source].[mname]))
             , ltrim(rtrim([source].[suffix]))
             , ltrim(rtrim([source].[gender]))
             , case
                   when isdate([source].[dob]) = 1
                       then cast([source].[dob] as date)
                        else null
               end as [date_of_birth]
             , ltrim(rtrim([source].[age]))
             , ltrim(rtrim([source].[ageunits]))
             , ltrim(rtrim([source].[complaint]))
             , ltrim(rtrim([source].[height]))
             , ltrim(rtrim([source].[weight]))
             , ltrim(rtrim([source].[bed]))
             , ltrim(rtrim([source].[ward]))
             , ltrim(rtrim([source].[dept]))
             , ltrim(rtrim([source].[ord42]))
             , ltrim(rtrim([source].[ord23]))
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
             , ltrim(rtrim([source].[ord11]))
             , ltrim(rtrim([source].[vssys]))
             , ltrim(rtrim([source].[vsdia]))
             , ltrim(rtrim([source].[ord12]))
             , ltrim(rtrim([source].[vspulse]))
             , ltrim(rtrim([source].[vsmaplevel]))
             , ltrim(rtrim([source].[vsmap]))
             , ltrim(rtrim([source].[ord13]))
             , ltrim(rtrim([source].[vsresp]))
             , ltrim(rtrim([source].[ord14]))
             , ltrim(rtrim([source].[vstemp]))
             , ltrim(rtrim([source].[vsendtidallevel]))
             , ltrim(rtrim([source].[vsendtidal]))
             , ltrim(rtrim([source].[ord23]))
             , ltrim(rtrim([source].[vso2]))
             , ltrim(rtrim([source].[ord15]))
             , ltrim(rtrim([source].[vspain]))
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
execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'Procedure used to export ibex patients in emar format'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'PROCEDURE'
  , @level1name = N'export_ibex_patients';
go