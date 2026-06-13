print 'Loading Table: action_route_templates';

declare
    @action_route_templates table
        (
            [action_id]             [int]          null
          , [medication_route_id]   [int]          null
          , [template_id]           [int]          null
          , [site_id]               [int]          null
          , [action_name]           [varchar](20)  null
          , [template_name]         [nvarchar](20) null
          , [medication_route_name] [nvarchar](50) null
        );

insert into @action_route_templates
(
    [action_name]
  , [medication_route_name]
  , [site_id]
  , [template_name]
)
-- required: [action_name],[template_name]
-- optional: [site_id] [medication_route_name] note: site has priority over route
select
    [action_name]
  , [medication_route_name]
  , [site_id]
  , [template_name]
from (
--- give routes imported from client data
--- all other actions are published (pced) data 
values
('Cancel', null, null, 'CancelOrder')
, ('Reschedule', null, null, 'Reschedule')
, ('Delete', null, null, 'Delete')
, ('Hold', null, null, 'Hold')
, ('MissedDose', null, null, 'MissedDose')
, ('Unhold', null, null, 'Unhold')
, ('CompleteDiscontinue', null, null, 'Discontinued')
, ('Give', null, null, 'GenericGive')
) as [items] ([action_name], [medication_route_name], [site_id], [template_name]);

--Get Releated ID's for relational tables

update [target] set
    [action_id] = [source].[id]
from @action_route_templates [target]
    inner join [dbo].[actions] [source]
        on [source].[name] = [target].[action_name];

update [target] set
    [template_id] = [source].[id]
from @action_route_templates [target]
    inner join [dbo].[templates] [source]
        on [source].[name] = [target].[template_name];

insert into @action_route_templates
(
    [action_id]
  , [medication_route_id]
  , [template_id]
  , [site_id]
  , [action_name]
  , [template_name]
  , [medication_route_name]
)
select
    [target].[action_id]
  , [source].[id]
  , [target].[template_id]
  , [target].[site_id]
  , [target].[action_name]
  , [target].[template_name]
  , [target].[medication_route_name]
from @action_route_templates [target]
    inner join [dbo].[medication_routes] [source]
        on [target].[medication_route_name] = [source].[name];

delete [target]
from @action_route_templates [target]
where [target].[medication_route_name] is not null
    and [target].[medication_route_id] is null;

merge into [dbo].[action_route_templates] [target]
using @action_route_templates [source]
on [target].[action_id] = [source].[action_id]
    and [target].[template_id] = [source].[template_id]
    and isnull([target].[site_id], -86) = isnull([source].[site_id], -86)
    and isnull([target].[medication_route_id], -86) = isnull([source].[medication_route_id], -86)
    when not matched by target then
        insert
        (
            [action_id]
          , [medication_route_id]
          , [site_id]
          , [template_id]
        )
        values
            ([action_id], [medication_route_id], [site_id], [template_id])
    when not matched by source then
        delete;
