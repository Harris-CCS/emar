print 'Loading Table: actions';

declare 
    @actions table
    (
      [id]          [int] not null
    , [name]        [varchar](20) not null
    , [description] [varchar](100) not null);

/****************************************
        load temporary tables for staging
****************************************/

with cte_source
     as (select [id]
              , action
              , [description]
         from   (values
             (1, 'Acknowledge', 'Acknowledge'),
             (2, 'Cancel', 'Cancel'),
             (3, 'Complete', 'Complete'),
             (4, 'CompleteDiscontinue', 'Complete Discontinue'),
             (5, 'CoSign', 'Co-Sign'),
             (6, 'Delete', 'Delete'),
             (7, 'FollowUp', 'Follow Up'),
             (8, 'Give', 'Give'),
             (9, 'Hold', 'Hold'),
             (10, 'MissedDose', 'Missed Dose'),
             (11, 'OrderDiscontinue', 'Order Discontinue'),
             (12, 'Repeat', 'Repeat'),
             (13, 'Reschedule', 'Reschedule'),
			 (14, 'UnHold', 'Un-Hold'),
             (15, 'Modify', 'Modify'),
             (16, 'PharmVerification', 'Pharmacy Verification')) as [a]([id], action, [description]))
     insert into @actions
         ([id]
        , [name]
        , [description]
         )
     select [id]
          , action
          , [description]
     from   [cte_source];

/*************************************
        begin loading permanent tables
*************************************/

merge into [dbo].[actions] [target]
using @actions as [source]
on [target].[id] = [source].[id]
--    when not matched by source
--        then delete
    when matched
        then update set 
    [name] = [source].[name]
  , [description] = [source].[description]
    when not matched
        then
      insert([id]
           , [name]
           , [description])
      values
    ([source].[id], [source].[name], [source].[description]);

/****************
        end table
****************/