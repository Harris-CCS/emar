begin transaction;

/*********************************
 load temporary tables for staging
*********************************/

insert into [#sites]
    ([source_id]
   , [name]
   , [is_active]
   , [time_zone_name]
    )
select [source].[site]
     , [source].[name]
     , case
           when [source].[status] = 'A'
               then 1
                else 0
       end
     , 'Central Standard Time'
from   [ibex].[dbo].[org] as [source];

alter table [#sites]
add [id]        [bigint] identity(1, 1)
  , [target_id] [bigint];

/*************************
 get max id for seed value
*************************/

set @max_id = null;

select @max_id = max([id])
from   [dbo].[sites];

set @max_id = isnull(@max_id, 0);

update [source] set    
    [target_id] = [source].[id] + @max_id
from   [#sites] as [source];

/******************************
 begin loading permanent tables
******************************/

set identity_insert [dbo].[sites] on;

insert into [dbo].[sites]
    ([id]
   , [name]
   , [is_active]
   , [time_zone_name]
    )
select [source].[target_id]
     , [source].[name]
     , [source].[is_active]
     , [source].[time_zone_name]
from   [#sites] as [source]
order by [name];

insert into [dbo].[sites]
    ([id]
   , [name]
   , [is_active]
   , [time_zone_name]
    )
values
    ('-1', 'Dummy Site for Relational Integrity', '0', 'Central Standard Time');

set identity_insert [dbo].[sites] off;

/********************************
 loading [external_ids] reference
********************************/

insert into [dbo].[external_ids]
    ([internal_id]
   , [vendor]
   , [entity]
   , [external_id]
    )
select [source].[target_id]
     , 'pulsecheck'
     , 'sites'
     , [source].[source_id]
from   [#sites] as [source];

/**********
 end table
**********/

commit transaction;

drop table if exists [#sites];