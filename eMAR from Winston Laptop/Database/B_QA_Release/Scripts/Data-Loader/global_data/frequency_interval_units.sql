print 'Loading Table: frequency_interval_units';

drop table if exists [#frequency_interval_units];

create table [#frequency_interval_units]
    (
      [id]   [int] not null
    , [name] [sysname] not null);

/****************************************
        load temporary tables for staging
****************************************/

insert into [#frequency_interval_units]
    ([id]
   , [name]
    )
values
    (0, 'NA'),
--  (1, 'Seconds'), Reserved for future use (maybe , maybe not)
    (2, 'Minutes'),
    (3, 'Hours'),
    (4, 'Days');

/*************************************
        begin loading permanent tables
*************************************/

delete [target]
from [#frequency_interval_units] as [source]
     right join [dbo].[frequency_interval_units] as [target] on [target].[id] = [source].[id]
where  [source].[id] is null;

update [target] set    
    [name] = [source].[name]
from   [#frequency_interval_units] as [source]
       inner join [dbo].[frequency_interval_units] as [target] on [target].[id] = [source].[id]
where  [target].[name] <> [source].[name];

insert into [dbo].[frequency_interval_units]
    ([id]
   , [name]
    )
select [source].[id]
     , [source].[name]
from   [#frequency_interval_units] as [source]
       left join [dbo].[frequency_interval_units] as [target] on [target].[id] = [source].[id]
where  [target].[id] is null;

/****************
        end table
****************/

drop table if exists [#frequency_interval_units];