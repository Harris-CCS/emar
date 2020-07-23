create procedure [dbo].[export_ibex_users]
as
    begin

        select [source].[num] as                    [id]
             , [source].[site] as                   [site_id]
             , rtrim(ltrim([source].[type])) as     [type]
             , case
                   when [source].status = 'Y'
                       then 1
                   else 0
               end as                               [is_active]
             , rtrim(ltrim([source].[init])) as     [initials_display]
             , rtrim(ltrim([source].[first])) as    [first_name]
             , rtrim(ltrim([source].[last])) as     [last_name]
             , '' as                                [middle_name]
             , '' as                                [name_suffix]
             , case
                   when [source].[ordonly] = 'Y'
                       then 1
                   else 0
               end as                               [ordering_only_physician]
             , 0 as                                 [name_display_initials]
             , rtrim(ltrim([source].[loginid])) as  [login_name]
             , rtrim(ltrim([source].[password])) as [login_password]
             , 0x00 as                              [salt]
             , case
                   when isdate([source].[datestamp]) = 1
                       then cast([source].[datestamp] as [datetimeoffset](7))
                   else null
               end as                               [last_login_time]
             , 0 as                                 [failed_login_attempts]
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