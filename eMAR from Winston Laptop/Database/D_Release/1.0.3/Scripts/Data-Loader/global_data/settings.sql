print 'Loading Table: settings';

drop table if exists [#settings];

create table [#settings]
    (
      [id]          [int] not null
    , [name]        [nvarchar](40) not null
    , [description] [varchar](1000) not null);

/****************************************
        load temporary tables for staging
****************************************/

insert into [#settings]
    ([id]
   , [name]
   , [description]
    )
values
    (1, 'MEDICATION_SERVICES' , 'Controls who has access to EMAR
allowable values Read (R) or Write (W) or Exclude (E)'),
    (2, 'PATIENT_NAME_DISPLAY'     , 'Y = "Full Name", I = "Last Name, First Initial", N = "Anonymous"'),
    (3, 'PATIENT_PAGE_SORT'        , 'E = "Entry Time", A = "Administration Time"'),
    (4, 'DEPARTMENT_PAGE_SORT'     , 'B = "Bed", P = "Patient Name", E ="Event Time"'),
    (5, 'DEPARTMENT_PAGE_FILTERING', 'A = "All Patients", M = "My Patients", V = "Pharmacy Verification Needed", U = "Upcoming Orders"'),
    (6, 'LAST_USED_PRINTER'        , 'Device ID from devices table');

/*************************************
        begin loading permanent tables
*************************************/

delete [target]
from [#settings] as [source]
     right join [dbo].[settings] as [target] on [target].[id] = [source].[id]
where  [source].[id] is null;

update [target] set    
    [name] = [source].[name]
  , [description] = [source].[description]
from   [#settings] as [source]
       inner join [dbo].[settings] as [target] on [target].[id] = [source].[id]
where  [target].[name] <> [source].[name]
       or [target].[description] <> [source].[description];

insert into [dbo].[settings]
    ([id]
   , [name]
   , [description]
    )
select [source].[id]
     , [source].[name]
     , [source].[description]
from   [#settings] as [source]
       left join [dbo].[settings] as [target] on [target].[id] = [source].[id]
where  [target].[id] is null;

/****************
        end table
****************/

drop table if exists [#settings];