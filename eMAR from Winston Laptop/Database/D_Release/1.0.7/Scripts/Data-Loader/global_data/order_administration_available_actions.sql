print 'Loading Table: order_administration_available_actions GLOBAL';

declare 
    @order_administration_available_actions table
    (
      [site_id]               int not null
    , [order_status]          varchar(20) not null
    , [administration_status] varchar(20) not null
    , [point_in_time]         bit null
    , [available_action_id]   varchar(20) not null);

with cte_order_administration_available_actions
     as (select *
         from   (values
             ('Pending'           , 'Pending', null, 'Give'               ),
             ('Pending'           , 'Pending', null, 'Hold'               ),
             ('Pending'           , 'Pending', null, 'Acknowledge'        ),
             ('Pending'           , 'Pending', null, 'CoSign'             ),
             ('Pending'           , 'Pending', null, 'Reschedule'         ),
             ('Pending'           , 'Late'   , null, 'Give'               ),
             ('Pending'           , 'Late'   , null, 'Hold'               ),
             ('Pending'           , 'Late'   , null, 'Acknowledge'        ),
             ('Pending'           , 'Late'   , null, 'CoSign'             ),
             ('Pending'           , 'Late'   , 1   , 'MissedDose'         ),
             ('Pending'           , 'Late'   , null, 'Reschedule'         ),
             ('Pending'           , 'OnHold' , null   , 'Give'            ),
             ('Pending'           , 'OnHold' , null   , 'MissedDose'      ),
             ('Pending'           , 'OnHold' , null   , 'UnHold'          ),
             ('Pending'           , 'OnHold' , null   , 'Acknowledge'     ),
             ('Pending'           , 'OnHold' , null   , 'CoSign'          ),
             ('Pending'           , 'OnHold' , null   , 'Reschedule'      ),
             ('OnGoing'           , 'OnHold' , 1   , 'Give'               ),
             ('OnGoing'           , 'OnHold' , 1   , 'MissedDose'         ),
             ('OnGoing'           , 'OnHold' , 1   , 'UnHold'             ),
             ('OnGoing'           , 'OnHold' , 1   , 'Acknowledge'        ),
             ('OnGoing'           , 'OnHold' , 1   , 'CoSign'             ),
             ('OnGoing'           , 'OnHold' , 1   , 'Reschedule'         ),
             ('OnGoing'           , 'Pending', 1   , 'Give'               ),
             ('OnGoing'           , 'Pending', 1   , 'Hold'               ),
             ('OnGoing'           , 'Pending', 1   , 'Acknowledge'        ),
             ('OnGoing'           , 'Pending', 1   , 'CoSign'             ),
             ('OnGoing'           , 'Pending', 1   , 'Reschedule'         ),
             ('OnGoing'           , 'Late'   , 1   , 'Give'               ),
             ('OnGoing'           , 'Late'   , 1   , 'MissedDose'         ),
             ('OnGoing'           , 'Late'   , 1   , 'Hold'               ),
             ('OnGoing'           , 'Late'   , 1   , 'Acknowledge'        ),
             ('OnGoing'           , 'Late'   , 1   , 'CoSign'             ),
             ('OnGoing'           , 'Late'   , 1   , 'Reschedule'         ),
             ('OnGoing'           , 'OnGoing', null, 'FollowUp'           ),
             ('OnGoing'           , 'OnGoing', null, 'OrderDiscontinue'   ),
             --('OnGoing'           , 'OnGoing', null, 'Complete'           ), -- Removed 2/20/21  action not used
             ('OnGoing'           , 'OnGoing', null, 'CoSign'             ),
             ('OnGoing'           , 'Given'  , 1   , 'CoSign'             ),
             ('OnGoing'           , 'Given'  , 1   , 'FollowUp'           ),
             ('OnHold'            , 'OnHold' , null, 'Give'               ),
             ('OnHold'            , 'OnHold' , null, 'MissedDose'         ),
             ('OnHold'            , 'OnHold' , null, 'UnHold'             ),
             ('OnHold'            , 'OnHold' , null, 'Acknowledge'        ),
             ('OnHold'            , 'OnHold' , null, 'CoSign'             ),
             ('OnHold'            , 'OnHold' , null, 'Reschedule'         ),
             ('OnHold'            , 'Pending', null, 'Give'               ),
             ('OnHold'            , 'Pending', null, 'Hold'               ),
             ('OnHold'            , 'Pending', null, 'Acknowledge'        ),
             ('OnHold'            , 'Pending', null, 'CoSign'             ),
             ('OnHold'            , 'Pending', null, 'Reschedule'         ),
             ('OnHold'            , 'Late'   , null, 'Give'               ),
             ('OnHold'            , 'Late'   , null, 'MissedDose'         ),
             ('OnHold'            , 'Late'   , null, 'Hold'               ),
             ('OnHold'            , 'Late'   , null, 'Acknowledge'        ),
             ('OnHold'            , 'Late'   , null, 'CoSign'             ),
             ('OnHold'            , 'Late'   , null, 'Reschedule'         ),
             ('OnHold'            , 'Given'  , null, 'CoSign'             ),
             ('OnHold'            , 'Given'  , null, 'FollowUp'           ),
             ('PendingDiscontinue', 'OnHold' , null, 'Give'               ),
             ('PendingDiscontinue', 'OnHold' , null, 'MissedDose'         ),
             ('PendingDiscontinue', 'OnHold' , null, 'UnHold'             ),
             ('PendingDiscontinue', 'OnHold' , null, 'Acknowledge'        ),
             ('PendingDiscontinue', 'OnHold' , null, 'CoSign'             ),
             ('PendingDiscontinue', 'OnHold' , null, 'Reschedule'         ),
             ('PendingDiscontinue', 'Pending', null, 'Give'               ),
             ('PendingDiscontinue', 'Pending', null, 'Hold'               ),
             ('PendingDiscontinue', 'Pending', null, 'Acknowledge'        ),
             ('PendingDiscontinue', 'Pending', null, 'CoSign'             ),
             ('PendingDiscontinue', 'Pending', null, 'Reschedule'         ),
             ('PendingDiscontinue', 'Late'   , null, 'Give'               ),
             ('PendingDiscontinue', 'Late'   , null, 'MissedDose'         ),
             ('PendingDiscontinue', 'Late'   , null, 'Hold'               ),
             ('PendingDiscontinue', 'Late'   , null, 'Acknowledge'        ),
             ('PendingDiscontinue', 'Late'   , null, 'CoSign'             ),
             ('PendingDiscontinue', 'Late'   , null, 'Reschedule'         ),
             ('PendingDiscontinue', 'OnGoing', 1   , 'FollowUp'           ),
             ('PendingDiscontinue', 'OnGoing', 1   , 'CompleteDiscontinue'),
             ('PendingDiscontinue', 'OnGoing', 1   , 'CoSign'             ),
             ('PendingDiscontinue', 'Given'  , null, 'CoSign'             ),
             ('PendingDiscontinue', 'Given'  , null, 'FollowUp'           ),
             ('Discontinued'      , 'OnHold' , null, 'Give'               ),
             ('Discontinued'      , 'OnHold' , null, 'MissedDose'         ),
             ('Discontinued'      , 'OnHold' , null, 'UnHold'             ),
             ('Discontinued'      , 'OnHold' , null, 'Acknowledge'        ),
             ('Discontinued'      , 'OnHold' , null, 'CoSign'             ),
             ('Discontinued'      , 'OnHold' , null, 'Reschedule'         ),
             ('Discontinued'      , 'Given'  , null, 'CoSign'             ),
             ('Discontinued'      , 'Given'  , null, 'FollowUp'           ),
             ('Discontinued'      , 'OnGoing', null, 'CoSign'             ),
             ('Discontinued'      , 'OnGoing', null, 'FollowUp'           ),
             ('Completed'         , 'Given'  , null, 'CoSign'             ),
             ('Completed'         , 'Given'  , null, 'FollowUp'           )) as [t]
                    ( [order_status]                         
                    , [administration_status]                         
                    , [point_in_time]                         
                    , [available_action_id]))
insert into @order_administration_available_actions
    ([site_id]
   , [order_status]
   , [administration_status]
   , [point_in_time]
   , [available_action_id]
    )
select-1
    , [order_status]
    , [administration_status]
    , [point_in_time]
    , [available_action_id]
from  [cte_order_administration_available_actions];


declare 
    @order_administration_available_actions2 table
    (
      [site_id]               int not null
    , [order_status]          varchar(20) not null
    , [administration_status] varchar(20) not null
    , [point_in_time]         bit null
    , [available_action_id]   int not null);

insert into @order_administration_available_actions2
select [source].[site_id]
     , [source].[order_status]
     , [source].[administration_status]
     , [source].[point_in_time]
     , [action].[id] as [available_action_id]
from   @order_administration_available_actions as [source]
       inner join [dbo].[actions] as [action] on [action].[name] = [source].[available_action_id];

/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
/*--------------- Merge the Master Data (Site -1) --------------------------------------*/
/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/

merge into [dbo].[order_administration_available_actions] [target]
using @order_administration_available_actions2 [source]
on [source].[site_id] = [target].[site_id]
   and [source].[order_status]             = [target].[order_status]
   and [source].[administration_status]    = [target].[administration_status]
   and isnull([source].[point_in_time], 0) = isnull([target].[point_in_time], 0)
   and [source].[available_action_id]      = [target].[available_action_id]
    when not matched by target and [source].[site_id] = -1
        then
      insert([site_id]
           , [order_status]
           , [administration_status]
           , [point_in_time]
           , [available_action_id])
      values
    ([site_id], [order_status], [administration_status], [point_in_time], [available_action_id])
    when not matched by source and [target].[site_id] = -1
        then delete;

/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
/*--------------- Merge the Master Data with local sites -------------------------------*/
/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/

with cte_values
     as (select [site].[id] as [site_id]
              , [reference].[order_status]
              , [reference].[administration_status]
              , [reference].[point_in_time]
              , [reference].[available_action_id]
         from   [dbo].[sites] as [site]
                cross join [dbo].[order_administration_available_actions] as [reference]
         where  [reference].[site_id] = -1
         -- 2/20/21 only need these lines when a new site is added
         --       and [site].[id] not in
         --(
         --    select distinct 
         --           [site_id]
         --    from [dbo].[order_administration_available_actions]
         --)
		 )
     merge into [dbo].[order_administration_available_actions] [target]
     using [cte_values] [source]
     on [source].[site_id] = [target].[site_id]
        and [source].[order_status] = [target].[order_status]
        and [source].[administration_status] = [target].[administration_status]
        and isnull([source].[point_in_time], 0) = isnull([target].[point_in_time], 0)
        and [source].[available_action_id] = [target].[available_action_id]
         when not matched by target
             then
           insert([site_id]
                , [order_status]
                , [administration_status]
                , [point_in_time]
                , [available_action_id])
           values
         ([site_id], [order_status], [administration_status], [point_in_time], [available_action_id])
         -- 2/20/21 add delete to remove unmatched actions for each site
         when not matched by source 
           then delete;