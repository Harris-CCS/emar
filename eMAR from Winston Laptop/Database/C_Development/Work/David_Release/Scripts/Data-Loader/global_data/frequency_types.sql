print 'Loading Table: frequency_types';

drop table if exists [#frequency_types];

create table [#frequency_types]
    (
      [id]   [int] not null
    , [name] [sysname] not null);

/****************************************
        load temporary tables for staging
****************************************/

insert into [#frequency_types]
    ([id]
   , [name]
    )
values
    (0, ''),
    (1, 'Daily'),
    (2, 'Weekly'),
--  (3, 'Monthly'), Reserved for future use (maybe , maybe not)
    (4, 'Interval'),
    (5, 'One Time'),
    (6, 'STAT'),
    (7, 'PRN'),
    (8, 'Continuous');

/*************************************
        begin loading permanent tables
*************************************/

delete [target]
from [#frequency_types] as [source]
     right join [dbo].[frequency_types] as [target] on [target].[id] = [source].[id]
where  [source].[id] is null;

update [target] set    
    [name] = [source].[name]
from   [#frequency_types] as [source]
       inner join [dbo].[frequency_types] as [target] on [target].[id] = [source].[id]
where  [target].[name] <> [source].[name];

insert into [dbo].[frequency_types]
    ([id]
   , [name]
    )
select [source].[id]
     , [source].[name]
from   [#frequency_types] as [source]
       left join [dbo].[frequency_types] as [target] on [target].[id] = [source].[id]
where  [target].[id] is null;

/****************
        end table
****************/

drop table if exists [#frequency_types];