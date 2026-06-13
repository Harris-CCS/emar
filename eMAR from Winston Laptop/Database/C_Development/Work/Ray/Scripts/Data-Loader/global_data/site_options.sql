print 'Loading Table: site_options';

/*******************************
Default Values for Site Options 
  when populating a system
  or adding new options
*******************************/

declare 
    @default_values_so table
    (
      [name]          [nvarchar](40) not null
    , [default_value] [varchar](25) not null);

insert into @default_values_so
    ([name]
   , [default_value]
    )
select [name]
     , [value]
from   (values
    ('LONG_DATE_FORMAT'             , 'MM/dd/yyyy'),
    ('SHORT_DATE_FORMAT'            , 'MM/dd/yy'  ),
    ('SCHEDULE_FUTURE_ITEMS'        , '7'         ),
--  ('PATIENT_IMAGE_PATH', '')      , ''          ), no default defined
--  ('CUSTOM_INDICATORS_IMAGE_PATH' , ''          ), no default defined
    ('RXALERT'                      , '0'         ),
    ('MEDINPAT'                     , 'N'         ),
    ('MEDOUTPAT'                    , 'N'         ),
    ('MEDPYXIS'                     , 'N'         ),
    ('MEDEXACTMATCH'                , 'N'         ),
    ('DRUG_DB_VENDOR'               , 'F'         ),
    ('SESSION_TIMEOUT'              , '240'       ),
--  ('SESSION_TIMEOUT_URL'          , ''          ), no default defined
    ('SHOW_DOSE_FORM'               , 'Y'         ),
    ('SHOW_STRENGTH'                , 'Y'         ),
    ('POPUP_ON_GIVE'                , 'N'         )
    ) as [val]([name], [value]);

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
select [site].[id] as                                                   [site_id]
     , [option].[id] as                                                 [option_id]
     , isnull([user_default].[default_value], 'DEFAULT_NOT_DEFINED') as [user_default_value]
from   [dbo].[sites] as [site]
       cross join [dbo].[options] as [option]
       left join @default_values_so as [user_default] on [user_default].[name] = [option].[name];

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