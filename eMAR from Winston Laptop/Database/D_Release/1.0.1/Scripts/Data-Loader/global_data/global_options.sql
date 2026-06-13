print 'Loading Table: global_options';

drop table if exists [#global_options];

create table [#global_options]
    (
      [id]          [int] not null
    , [name]        [nvarchar](40) not null
    , [description] [varchar](1000) not null
    , [value]       [varchar](1000) not null);

/****************************************
        load temporary tables for staging
****************************************/

insert into [#global_options]
    ([id]
   , [name]
   , [description]
   , [value]
    )
select [source].[id]
     , [source].[name]
     , [source].[description]
     , [source].[value]
from   (values
    (1, 'HOST_SERVER_URL', 'The root url (or unc) for the primary application on this server', 'http://REPLACE_ME'),
    (2, 'ANTIMICROBIAL_DISPLAY', 'Choices: P = Present, R = Required, A = Absent', 'P')) as [source]([id], [name], [description], [value]);

/*************************************
        begin loading permanent tables
*************************************/

delete [target]
from [#global_options] as [source]
     right join [dbo].[global_options] as [target] on [target].[id] = [source].[id]
where  [source].[id] is null;

update [target] set    
    [name] = [source].[name]
  , [description] = [source].[description]
from   [#global_options] as [source]
       inner join [dbo].[global_options] as [target] on [target].[id] = [source].[id]
where  [target].[name] <> [source].[name]
       or [target].[description] <> [source].[description];

insert into [dbo].[global_options]
    ([id]
   , [name]
   , [description]
   , [value]
    )
select [source].[id]
     , [source].[name]
     , [source].[description]
     , [source].[value]
from   [#global_options] as [source]
       left join [dbo].[global_options] as [target] on [target].[id] = [source].[id]
where  [target].[id] is null;

/****************
        end table
****************/

drop table if exists [#global_options];