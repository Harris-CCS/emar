begin transaction;

/*********************************
 load temporary tables for staging
*********************************/

insert into [#users]
    ([source_id]
   , [site_id]
   , [type]
   , [is_active]
   , [initials_display]
   , [first_name]
   , [last_name]
   , [ordering_only_physician]
   , [name_display_preference]
   , [login_name]
   , [login_password]
   , [salt]
   , [last_login_time]
   , [failed_login_attempts]
    )
select [source].[num]
     , [source].[site]
     , [source].[type]
     , case
           when [source].status = 'Y'
               then 1
                else 0
       end
     , [source].[init]
     , [source].[first]
     , [source].[last]
     , case
           when [source].[ordonly] = 'Y'
               then 1
                else 0
       end
     , 0 as    [name_display_preference]
     , [source].[loginid]
     , [source].[password]
     , 0x00 as [salt]
     , case
           when isdate([source].[datestamp]) = 1
               then cast([source].[datestamp] as [datetimeoffset](7))
               else null
       end
     , 0 as    [failed_login_attempts]
from   [ibex].[dbo].[drs] as [source];

update [source] set    
    [site_id] = isnull([internal_site].[id], -1)
from   [#users] as [source]
       outer apply [dbo].[get_internal_id]
    ('pulsecheck', 'sites', [source].[site_id]) as [internal_site];

alter table [#users]
add [id]        [bigint] identity(1, 1)
  , [target_id] [bigint];

/*************************
 get max id for seed value
*************************/

set @max_id = null;

select @max_id = max([id])
from   [dbo].[users];

set @max_id = isnull(@max_id, 0);

update [source] set    
    [target_id] = [source].[id] + @max_id
from   [#users] as [source];

/******************************
 begin loading permanent tables
******************************/

set identity_insert [dbo].[users] on;

insert into [dbo].[users]
    ([id]
   , [site_id]
   , [type]
   , [is_active]
   , [initials_display]
   , [first_name]
   , [last_name]
   , [ordering_only_physician]
   , [name_display_preference]
   , [login_name]
   , [login_password]
   , [salt]
   , [last_login_time]
   , [failed_login_attempts]
    )
select [source].[target_id]
     , [source].[site_id]
     , [source].[type]
     , [source].[is_active]
     , [source].[initials_display]
     , [source].[first_name]
     , [source].[last_name]
     , [source].[ordering_only_physician]
     , [source].[name_display_preference]
     , [source].[login_name]
     , [source].[login_password]
     , [source].[salt]
     , [source].[last_login_time]
     , [source].[failed_login_attempts]
from   [#users] as [source]
order by [source].[last_name]
       , [source].[first_name];

set identity_insert [dbo].[users] off;

/********************************
 loading [external_ids] reference
********************************/

insert into [dbo].[external_ids]
    ([internal_id]
   , [vendor]
   , [entity]
   , [external_id]
    )
select [source].[target_id]
     , 'pulsecheck'
     , 'users'
     , [source].[source_id]
from   [#users] as [source];

/*********
 end table
*********/

commit transaction;

drop table if exists [#users];