print 'Loading Table: notification_categories';

declare 
    @notification_categories table
    (
      [id]          [int] not null
    , [code]        [varchar](20) not null
    , [description] [nvarchar](150) not null
    , [priority]    [smallint] not null
    , [action_url]  [varchar](255));

/****************************************
        load temporary tables for staging
****************************************/

with cte_source
     as (select [id]
              , [code]
              , [description]
              , [priority]
              , [action_url]
         from   (values
             (1, 'PO', 'Possible Overdue', 1, 'external?patientId={PATIENT.ID}&userId={USER.ID}&dest=marpatient'),
             (2, 'PENDING', 'Pending', 5, 'external?patientId={PATIENT.ID}&userId={USER.ID}&dest=marpatient'),
             (3, 'FU', 'Follow-up', 10, 'external?patientId={PATIENT.ID}&userId={USER.ID}&dest=marpatient'),
             (4, 'IV1', 'IV Continuous Orders - Ending', 6, 'external?patientId={PATIENT.ID}&userId={USER.ID}&dest=marpatient'),
             (5, 'IV2', 'IV Continuous Orders - Ended', 2, 'external?patientId={PATIENT.ID}&userId={USER.ID}&dest=marpatient')
             ) as [a]([id], [code], [description], [priority], [action_url]))
     insert into @notification_categories
         ([id]
        , [code]
        , [description]
        , [priority]
        , [action_url]
         )
     select [id]
          , [code]
          , [description]
          , [priority]
          , [action_url]
     from   [cte_source];

/*************************************
        begin loading permanent tables
*************************************/

merge into [dbo].[notification_categories] [target]
using @notification_categories as [source]
on [target].[id] = [source].[id]
--    when not matched by source
--        then delete
    when matched
        then update set 
    [code] = [source].[code]
  , [description] = [source].[description]
  , [priority] = [source].[priority]
  , [action_url] = [source].[action_url]
    when not matched
        then
      insert([id]
           , [code]
           , [description]
           , [priority]
           , [action_url])
      values
    ([source].[id], [source].[code], [source].[description], [source].[priority], [source].[action_url]);

/****************
        end table
****************/