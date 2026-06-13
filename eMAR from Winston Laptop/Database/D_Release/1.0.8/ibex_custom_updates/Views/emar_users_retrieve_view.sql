print 'create view [ibex].[dbo].[emar_users_retrieve_view];';

set @template = N'
create or alter view [dbo].[emar_users_retrieve_view]

as

select
    [source].[num]                     as [id]
  , [source].[site]                    as [site_id]
  , rtrim(ltrim([source].[type]))      as [type]
  , convert(int, case
        when [source].status = ''A'' then 1
        else 0
    end)                              as [is_active]
  , rtrim(ltrim([source].[init]))      as [initials_display]
  , rtrim(ltrim([source].[first]))     as [first_name]
  , rtrim(ltrim([source].[last]))      as [last_name]
  , ''''                                 as [middle_name]
  , ''''                                 as [name_suffix]
  , case
        when [source].[ordonly] = ''Y'' then 1
        else 0
    end                                as [ordering_only_physician]
  , 0                                  as [name_display_initials]
  , rtrim(ltrim([source].[loginid]))   as [login_name]
  , rtrim(ltrim([source].[password]))  as [login_password]
  , 0x00                               as [salt]
  , [source].[lastlogin]               as [last_login_time]
  , 0                                  as [failed_login_attempts]
  , case
        when substring([grid], 76, 1) in (''R'', ''W'') then substring([grid], 76, 1)
        else ''E''
    end                                as [medication_services_access]

	-- Add the medprn column to the view so that we can update the last used device for the user in emar.
	-- Winston Murdock, 07/14/2021.  EMAR-1087.
  --  , case
  --      when [source].[medprn] is null then 0
  --      when [source].[medprn] < 0 then 0
  --      else [source].[medprn]
  --  end as [medprn]
  , dbo.emar_is_device_pulled_in_from_ibex([source].[medPrn]) as [medprn]
from [dbo].[drs] as [source];

';

set @sql_cmd = @template;

execute [ibex].[dbo].[sp_executesql] 
    @statement = @sql_cmd;
