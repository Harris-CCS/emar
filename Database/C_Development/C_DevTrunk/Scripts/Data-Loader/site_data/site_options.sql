/*******************************
Default Values for Site Options 
  when populating a system
  or adding new options
*******************************/

declare 
    @LONG_DATE_FORMAT      varchar(25) = 'MM/dd/yyyy'
  , @SHORT_DATE_FORMAT     varchar(25) = 'MM/dd/yy'
  , @SCHEDULE_FUTURE_ITEMS varchar(25) = '3'
  , @RXALERT               varchar(25) = '0'
  , @MEDINPAT              varchar(25) = 'N'
  , @MEDOUTPAT             varchar(25) = 'N'
  , @MEDPYXIS              varchar(25) = 'N'
  , @MEDEXACTMATCH         varchar(25) = 'N';

drop table if exists [#site_options];

create table [#site_options]
    (
      [site_id]      [int] not null
    , [option_id]    [int] not null
    , [option_value] [varchar](255) not null);

/****************************************
        load temporary tables for staging
****************************************/

insert into [#site_options]
    ([site_id]
   , [option_id]
   , [option_value]
    )
select [site].[id] as   [site_id]
     , [option].[id] as [option_id]
     , case
           when [option].[name] = 'LONG_DATE_FORMAT'
               then @LONG_DATE_FORMAT
           when [option].[name] = 'SHORT_DATE_FORMAT'
               then @SHORT_DATE_FORMAT
           when [option].[name] = 'SCHEDULE_FUTURE_ITEMS'
               then @SCHEDULE_FUTURE_ITEMS
           when [option].[name] = 'RXALERT'
               then @RXALERT
           when [option].[name] = 'MEDINPAT'
               then @MEDINPAT
           when [option].[name] = 'MEDOUTPAT'
               then @MEDOUTPAT
           when [option].[name] = 'MEDPYXIS'
               then @MEDPYXIS
           when [option].[name] = 'MEDEXACTMATCH'
               then @MEDEXACTMATCH
           else 'DEFAULT_NOT_DEFINED'
       end as           [option_value]
from   [dbo].[sites] as [site]
       cross join [dbo].[options] as [option];

/*************************************
        begin loading permanent tables
*************************************/

delete [target]
from [#site_options] as [source]
     right join [dbo].[site_options] as [target] on [target].[site_id] = [source].[site_id]
                                                    and [target].[option_id] = [source].[option_id]
where  [source].[site_id] is null;

/************************
update goes here
but this script requires 
no update statement
************************/

insert into [dbo].[site_options]
    ([site_id]
   , [option_id]
   , [option_value]
    )
select [source].[site_id]
     , [source].[option_id]
     , [source].[option_value]
from   [#site_options] as [source]
       left join [dbo].[site_options] as [target] on [target].[site_id] = [source].[site_id]
                                                     and [target].[option_id] = [source].[option_id]
where  [target].[site_id] is null;

/****************
        end table
****************/

drop table if exists [#site_options];