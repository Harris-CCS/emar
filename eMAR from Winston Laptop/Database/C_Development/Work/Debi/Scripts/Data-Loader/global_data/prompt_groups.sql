print 'Loading Table: prompt_groups';

declare
    @prompt_groups table
        (
            [id]    [int]         not null
          , [name]  [varchar](20) not null
          , [title] [varchar](50) not null
        );

insert into @prompt_groups
(
    [id]
  , [name]
  , [title]
)
select
    [id]
  , [name]
  , [title]
from (
values
(1, 'Medication', 'MEDICATION')
, (2, 'Emotional', '')
, (3, 'Safety', 'SAFETY INTERVENTIONS')
, (4, 'CancelReason', 'Cancellation Reasons')
, (7, 'RescheduleDetails', '')
, (9, 'Notes_At_Notify', '')
, (10, 'HoldAndMissedDose', 'Reasons')
, (11, 'Delete', 'Confirm Delete')
, (12, 'DeleteGeneric', '')
, (13, 'Unhold', 'Unhold Reasons')
, (15, 'IntraMuscMedication', 'Medication')
, (16, 'Assessment', 'Pre-Administration Assessment')
, (17, 'GenericGive', '')
, (18, 'OralMedication', 'MEDICATION')
, (19, 'DefaultGive', '')
, (20, 'EnteralMedication', 'Medication')
, (21, 'AmbulateSafety', 'Safety Interventions')
, (22, 'NasalMedication', 'Medication')
, (23, 'InhalationMedication', 'Medication')
, (24, 'InhalationAssessment', 'Pre-Administration Assessment')
, (25, 'IntraDermMedication', 'Medication')
, (26, 'IntraOssMedication', 'Medication')
, (27, 'IntraOssAssessment', 'Pre-Administration Assessment')
, (28, 'RectalMedication', 'Medication')
, (29, 'TransDermMedication', 'Medication')
, (30, 'VaginalMedication', 'Medication')
, (31, 'SubcutanMedication', 'Medication')
, (32, 'IVMedication', 'Medication')
, (33, 'IVAssessment', 'Pre-Administration Assessment')
) as [items] 
([id], [name], [title]);

/**********************
*** [prompt_groups] ***
**********************/

merge into [dbo].[prompt_groups] [target]
using @prompt_groups [source]
on [target].[id] = [source].[id]
    when matched
        and ([target].[name] <> [source].[name]
            or [target].[title] <> [source].[title]) then
        update set
            [name]  = [source].[name]
          , [title] = [source].[title]
    when not matched by target then
        insert
        (
            [id]
          , [name]
          , [title]
        )
        values
            ([id], [name], [title]);