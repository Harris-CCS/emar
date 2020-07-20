create procedure [dbo].[export_ibex_users]
as
    begin

        select [source].[num]
             , [source].[site]
             , rtrim(ltrim([source].[type])) as    [type]
             , case
                   when [source].status = 'Y'
                       then 1
                                                else 0
               end as                              [status]
             , rtrim(ltrim([source].[init])) as    [init]
             , rtrim(ltrim([source].[first])) as   [first]
             , rtrim(ltrim([source].[last])) as    [last]
             , case
                   when [source].[ordonly] = 'Y'
                       then 1
                                                else 0
               end as                              [ordonly]
             , 0 as                                [name_display_initials]
             , rtrim(ltrim([source].[loginid])) as [loginid]
             , rtrim(ltrim([source].[password]))   [password]
             , 0x00 as                             [salt]
             , case
                   when isdate([source].[datestamp]) = 1
                       then cast([source].[datestamp] as [datetimeoffset](7))
                       else null
               end
             , 0 as                                [failed_login_attempts]
        from   [ibex].[dbo].[drs] as [source]
        order by [source].[last]
               , [source].[first]
               , [source].[site]
               , [source].[num];
    end;
go

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'Procedure used to export ibex users in emar format'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'PROCEDURE'
  , @level1name = N'export_ibex_users';
go
