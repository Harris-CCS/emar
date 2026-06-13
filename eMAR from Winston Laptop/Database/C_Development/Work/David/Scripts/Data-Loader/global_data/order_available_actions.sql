print 'Loading Table: order_available_actions GLOBAL';

declare 
    @order_available_actions table
    (
      [site_id]             int not null
    , [order_status]        varchar(20) not null
    , [available_action_id] varchar(20) not null
    , [is_pit]              bit null
    , [is_prn_only]         bit null);

declare 
    @order_available_actions2 table
    (
      [site_id]             int not null
    , [order_status]        varchar(20) not null
    , [available_action_id] int not null
    , [is_pit]              bit null
    , [is_prn_only]         bit null);

insert into @order_available_actions
    ([site_id]
   , [order_status]
   , [available_action_id]
   , [is_pit]
   , [is_prn_only]
    )
select-1 as [site_id]
    , [order_status]
    , [available_action_id]
    , [is_pit]
    , [is_prn_only]
from  (values
    ('Pending'           , 'Cancel'             , null, 0),
    ('Pending'           , 'Delete'             , null, 0),
    ('Pending'           , 'OrderDiscontinue'   , null, 0),
    ('Pending'           , 'Repeat'             , null, 0),
    ('Pending'           , 'Cosign'             , null, 0),
    ('Pending'           , 'Give'               , null, 1),
    ('OnGoing'           , 'OrderDiscontinue'   , null, 0),
    ('OnGoing'           , 'Repeat'             , null, 0),
    ('OnGoing'           , 'Cosign'             , null, 0),
    ('OnGoing'           , 'Give'               , 1   , 1),
    ('OnHold'            , 'Cancel'             , null, 0),
    ('OnHold'            , 'Delete'             , null, 0),
    ('OnHold'            , 'OrderDiscontinue'   , null, 0),
    ('OnHold'            , 'Repeat'             , null, 0),
    ('OnHold'            , 'Cosign'             , null, 0),
    ('OnHold'            , 'Give'               , null, 1),
    ('PendingDiscontinue', 'CompleteDiscontinue', null, 0),
    ('PendingDiscontinue', 'Repeat'             , null, 0),
    ('PendingDiscontinue', 'Cosign'             , null, 0),
    ('Discontinued'      , 'Repeat'             , null, 0),
    ('Completed'         , 'Repeat'             , null, 0)) as [vals]
    (
      [order_status]
    , [available_action_id]
    , [is_pit]
    , [is_prn_only]
    );

with cte_source
     as (select [source].[site_id]
              , [source].[order_status]
              , [action].[id] as [available_action_id]
              , [source].[is_pit]
              , [source].[is_prn_only]
         from   @order_available_actions as [source]
                inner join [dbo].[actions] as [action] on [action].[name] = [source].[available_action_id])
     insert into @order_available_actions2
         ([site_id]
        , [order_status]
        , [available_action_id]
        , [is_pit]
        , [is_prn_only]
         )
     select [source].[site_id]
          , [source].[order_status]
          , [source].[available_action_id]
          , [source].[is_pit]
          , [source].[is_prn_only]
     from   [cte_source] as [source];

merge into [dbo].[order_available_actions] [target]
using @order_available_actions2 [source]
on [source].[site_id] = [target].[site_id]
   and [source].[order_status] = [target].[order_status]
   and [source].[available_action_id] = [target].[available_action_id]
    when not matched by target and [source].[site_id] = -1
        then
      insert([site_id]
           , [order_status]
           , [available_action_id]
           , [is_pit]
           , [is_prn_only])
      values
    ([site_id], [order_status], [available_action_id], [is_pit], [is_prn_only])
    when not matched by source and [target].[site_id] = -1
        then delete;

print 'Loading Table: order_available_actions LOCAL';

/******************************************************************
order_available_actions LOCAL
Loads Site Specific Defaults from the "GLOBAL" dataset (site_id=-1)
******************************************************************/

with cte_values
     as (select [site].[id] as [site_id]
              , [reference].[order_status]
              , [reference].[available_action_id]
              , [reference].[is_pit]
              , [reference].[is_prn_only]
         from   [dbo].[sites] as [site]
                cross join [dbo].[order_available_actions] as [reference]
         where  [reference].[site_id] = -1
                and [site].[id] not in
                 (
                     select distinct 
                            [site_id]
                     from [dbo].[order_available_actions]
                 )
        )
     merge into [dbo].[order_available_actions] [target]
     using [cte_values] [source]
     on [source].[site_id] = [target].[site_id]
        and [source].[order_status] = [target].[order_status]
        and [source].[available_action_id] = [target].[available_action_id]
         when not matched
             then
           insert([site_id]
                , [order_status]
                , [available_action_id]
                , [is_pit]
                , [is_prn_only])
           values
         ([site_id], [order_status], [available_action_id], [is_pit], [is_prn_only]);