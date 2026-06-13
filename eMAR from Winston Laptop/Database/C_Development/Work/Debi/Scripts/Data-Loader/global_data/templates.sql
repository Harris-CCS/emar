print 'Loading Table: templates';

declare
    @templates table
        (
            [id]                 [int]          not null
          , [name]               [nvarchar](20) not null
          , [is_active]          [bit]          not null
          , [title]              [varchar](50)  not null
          , [save_button_text]   [nvarchar](25) null
          , [cancel_button_text] [nvarchar](25) null
        );

insert into @templates
(
    [id]
  , [name]
  , [is_active]
  , [title]
  , [save_button_text]
  , [cancel_button_text]
)
select
    [id]
  , [name]
  , [is_active]
  , [title]
  , [save_button_text]
  , [cancel_button_text]
from (
values
(1, 'Ear', 1, 'Ear Give Template', 'Give', 'Cancel')
, (2, 'CancelOrder', 1, 'Cancel Order', 'Save Cancel', 'Cancel')
, (3, 'Reschedule', 1, 'Reschedule Order', 'Confirm Reschedule', 'Cancel')
, (4, 'Hold', 1, 'Hold Template', 'Hold', 'Cancel')
, (5, 'Delete', 1, 'Delete Template', 'Confirm Delete', 'Cancel')
, (6, 'MissedDose', 1, 'Missed Dose Template', 'Missed Dose', 'Cancel')
, (7, 'Unhold', 1, 'Unhold Template', 'Unhold', 'Cancel')
, (8, 'Discontinued', 1, 'Discontinued Template', 'Discontinued', 'Cancel')
, (9, 'Intramuscular', 1, 'Intramuscular Give Template', 'Give', 'Cancel')
, (10, 'Oral', 1, 'Oral Give Template', 'Give', 'Cancel')
, (11, 'Intravenous', 1, 'Intravenous Give Template', 'Give', 'Cancel')
, (12, 'Nasal', 1, 'Nasal Give Template', 'Give', 'Cancel')
, (13, 'Eye', 1, 'Eye Give Template', 'Give', 'Cancel')
, (14, 'Enteral', 1, 'Enteral Give Template', 'Give', 'Cancel')
, (15, 'Transdermal', 1, 'Transdermal Give Template', 'Give', 'Cancel')
, (16, 'Intradermal', 1, 'Intradermal Give Template', 'Give', 'Cancel')
, (17, 'Inhalation', 1, 'Inhalation Give Template', 'Give', 'Cancel')
, (18, 'IntravenousInI', 1, 'IntravenousInI Give Template', 'Give', 'Cancel')
, (19, 'Rectal', 1, 'Rectal Give Template', 'Give', 'Cancel')
, (20, 'Subcutaneous', 1, 'Subcutaneous Give Template', 'Give', 'Cancel')
, (21, 'Vaginal', 1, 'Vaginal Give Template', 'Give', 'Cancel')
,(22, 'GenericGive', 1, 'Generic Give Template', 'Give', 'Cancel')
,(23, 'Intraosseous', 1, 'Intraosseous Give Template', 'Give', 'Cancel')
) as [items] 
([id], [name], [is_active], [title], [save_button_text], [cancel_button_text]);

/******************
*** [templates] ***
******************/

merge into [dbo].[templates] [target]
using @templates [source]
on [target].[id] = [source].[id]
    when matched
        and ([target].[name] <> [source].[name]
            or [target].[is_active] <> [source].[is_active]
            or [target].[title] <> [source].[title]
            or [target].[save_button_text] <> [source].[save_button_text]
            or [target].[cancel_button_text] <> [source].[cancel_button_text]) then
        update set
            [name]               = [source].[name]
          , [is_active]          = [source].[is_active]
          , [title]              = [source].[title]
          , [save_button_text]   = [source].[save_button_text]
          , [cancel_button_text] = [source].[cancel_button_text]
    when not matched then
        insert
        (
            [id]
          , [name]
          , [is_active]
          , [title]
          , [save_button_text]
          , [cancel_button_text]
        )
        values
            ([id], [name], [is_active], [title], [save_button_text], [cancel_button_text])
    when not matched by source then
        update set
            [is_active] = 0;