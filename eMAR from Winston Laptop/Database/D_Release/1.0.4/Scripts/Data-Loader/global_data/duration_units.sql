print 'Loading Table: duration_units';

drop table if exists [#duration_units];

create table [#duration_units]
    (
      [id]                  [int] not null
    , [duration_in_minutes] [int] not null
    , [name]                [varchar](40) not null);

/****************************************
        load temporary tables for staging
****************************************/

insert into [#duration_units]
    ([id]
   , [duration_in_minutes]
   , [name]
    )
values
    (1,     0, 'Doses'  ),
    (2,     1, 'Minutes'),
    (3,    60, 'Hours'  ),
    (4,  1440, 'Days'   ),
    (5, 10080, 'Weeks'  ),
    (6, 43200, 'Months' );

/*************************************
        begin loading permanent tables
*************************************/

delete [target]
from [#duration_units] as [source]
     right join [dbo].[duration_units] as [target] on [target].[id] = [source].[id]
where  [source].[id] is null;

update [target] set    
    [duration_in_minutes] = [source].[duration_in_minutes]
  , [name] = [source].[name]
from   [#duration_units] as [source]
       inner join [dbo].[duration_units] as [target] on [target].[id] = [source].[id]
where  [target].[name] <> [source].[name]
       or [target].[duration_in_minutes] <> [source].[duration_in_minutes];

insert into [dbo].[duration_units]
    ([id]
   , [duration_in_minutes]
   , [name]
    )
select [source].[id]
     , [source].[duration_in_minutes]
     , [source].[name]
from   [#duration_units] as [source]
       left join [dbo].[duration_units] as [target] on [target].[id] = [source].[id]
where  [target].[id] is null;

/****************
        end table
****************/

drop table if exists [#duration_units];