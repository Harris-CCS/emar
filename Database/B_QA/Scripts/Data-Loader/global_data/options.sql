print 'Loading Table: options';

drop table if exists [#options];

create table [#options]
    (
      [id]          [int] not null
    , [name]        [nvarchar](40) not null
    , [description] [varchar](1000) not null);

/****************************************
        load temporary tables for staging
****************************************/

insert into [#options]
    ([id]
   , [name]
   , [description]
    )
values
    (1, 'LONG_DATE_FORMAT'            , 'Long Date Format for user display
Allowable Chararters to use in the format value
d -> Represents the day of the month as a number from 1 through 31.
dd -> Represents the day of the month as a number from 01 through 31.
ddd-> Represents the abbreviated name of the day (Mon, Tues, Wed, etc).
dddd-> Represents the full name of the day (Monday, Tuesday, etc).
M-> Month number(eg.3)
MM-> Month number with leading zero(eg.04)
MMM-> Abbreviated Month Name (e.g. Dec)
MMMM-> Full month name (e.g. December)
y-> Year, no leading zero (e.g. 2015 would be 15)
yy-> Year, leading zero (e.g. 2015 would be 015)
yyy-> Year, (e.g. 2015)
yyyy-> Year, (e.g. 2015)'                                                                                                  ),
    (2, 'SHORT_DATE_FORMAT'           , 'Short Date Format for user display
Allowable Chararters to use in the format value
d -> Represents the day of the month as a number from 1 through 31.
dd -> Represents the day of the month as a number from 01 through 31.
ddd-> Represents the abbreviated name of the day (Mon, Tues, Wed, etc).
dddd-> Represents the full name of the day (Monday, Tuesday, etc).
M-> Month number(eg.3)
MM-> Month number with leading zero(eg.04)
MMM-> Abbreviated Month Name (e.g. Dec)
MMMM-> Full month name (e.g. December)
y-> Year, no leading zero (e.g. 2015 would be 15)
yy-> Year, leading zero (e.g. 2015 would be 015)
yyy-> Year, (e.g. 2015)
yyyy-> Year, (e.g. 2015)'                                                                                                  ),
    (3, 'SCHEDULE_FUTURE_ITEMS'       , 'How many days forward (including today) to generate future administration records'),
    (4, 'PATIENT_IMAGE_PATH'          , 'Path to patients image files'                                                     ),
    (5, 'CUSTOM_INDICATORS_IMAGE_PATH', 'Path to custom indicators image files'                                            ),
    (6, 'RXALERT'                     , 'Possible Values:
  - 0 = Show All
  - 5 = Show Moderate, Severe and Contraindicated Only
  - 6 = Show Severe and Contraindicated Only'                                                                              );

/*************************************
        begin loading permanent tables
*************************************/

delete [target]
from [#options] as [source]
     right join [dbo].[options] as [target] on [target].[id] = [source].[id]
where  [source].[id] is null;

update [target] set    
    [name] = [source].[name]
  , [description] = [source].[description]
from   [#options] as [source]
       inner join [dbo].[options] as [target] on [target].[id] = [source].[id]
where  [target].[name] <> [source].[name]
       or [target].[description] <> [source].[description];

insert into [dbo].[options]
    ([id]
   , [name]
   , [description]
    )
select [source].[id]
     , [source].[name]
     , [source].[description]
from   [#options] as [source]
       left join [dbo].[options] as [target] on [target].[id] = [source].[id]
where  [target].[id] is null;

/****************
        end table
****************/

drop table if exists [#options];