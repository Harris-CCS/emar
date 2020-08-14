print 'Loading Table: frequency_days';

drop table if exists [#frequency_days];

create table [#frequency_days]
    (
      [id]   [int] not null
    , [name] [nvarchar](40) not null);

/****************************************
        load temporary tables for staging
****************************************/

insert into [#frequency_days]
    ([id]
   , [name]
    )
values
    (0, 'Daily'),
    (1, 'Sunday'),
    (2, 'Monday'),
    (3, 'Tuesday'),
    (4, 'Wednesday'),
    (5, 'Thursday'),
    (6, 'Friday'),
    (7, 'Saturday');

/*************************************
        begin loading permanent tables
*************************************/

delete [target]
from [#frequency_days] as [source]
     right join [dbo].[frequency_days] as [target] on [target].[id] = [source].[id]
where  [source].[id] is null;

update [target] set    
    [name] = [source].[name]
from   [#frequency_days] as [source]
       inner join [dbo].[frequency_days] as [target] on [target].[id] = [source].[id]
where  [target].[name] <> [source].[name];

insert into [dbo].[frequency_days]
    ([id]
   , [name]
    )
select [source].[id]
     , [source].[name]
from   [#frequency_days] as [source]
       left join [dbo].[frequency_days] as [target] on [target].[id] = [source].[id]
where  [target].[id] is null;

/****************
        end table
****************/

drop table if exists [#frequency_days];

declare 
    @cte_frequency_calendar int = 0;

with cte_frequency_calendar
     as (select distinct 
                [the_day_of_week]
              , [the_day_name]
         from   [dbo].[frequency_calendar])
     select @cte_frequency_calendar = count(*)
     from   [dbo].[frequency_days] as [fd]
            inner join [cte_frequency_calendar] as [fc] on [fc].[the_day_of_week] = [fd].[id]
     where  [fc].[the_day_name] <> [fd].[name];

if @cte_frequency_calendar > 0
    begin
        raiserror('error: [frequency_days] and [frequency_calendar] days are out of synch. This could be casued by a first day of week sql server setting', 16, 1);
    end;