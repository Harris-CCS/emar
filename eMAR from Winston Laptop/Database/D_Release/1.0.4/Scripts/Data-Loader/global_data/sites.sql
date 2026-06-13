print 'Loading Table: sites';

begin transaction;

/*************************************
        begin loading permanent tables
*************************************/

set identity_insert [dbo].[sites] on;

insert into [dbo].[sites]
(
    [id]
  , [name]
  , [is_active]
  , [time_zone_name]
)
select
    [val].[site_id]
  , [val].[name]
  , [val].[is_active]
  , [val].[time_zone_name]
from (
values
  ('-1', 'Dummy Site for Relational Integrity', '0', 'Central Standard Time')
, ('0', 'Dummy Site use up site_id 0', '0', 'Central Standard Time')
) as [val]
(
[site_id]
, [name]
, [is_active]
, [time_zone_name]
)
    left join [dbo].[sites] [site]
        on [site].[id] = [val].[site_id]
where [site].[id] is null;

set identity_insert [dbo].[sites] off;

/****************
        end table
****************/

commit transaction;