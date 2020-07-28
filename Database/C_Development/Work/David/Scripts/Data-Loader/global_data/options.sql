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
    (1, 'LONG_DATE_FORMAT', 'Long Date Format for user display'),
    (2, 'SHORT_DATE_FORMAT', 'Short Date Format for user display'),
    (3, 'SCHEDULE_FUTURE_ITEMS', 'How many days forward (including today) to generate future administration records');

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
       right join [dbo].[options] as [target] on [target].[id] = [source].[id]
where  [target].[name] = [source].[name]
       or [target].[description] = [source].[description];

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