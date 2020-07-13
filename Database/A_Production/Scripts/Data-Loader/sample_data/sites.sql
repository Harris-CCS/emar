begin transaction;

/*********************************
 load temporary tables for staging
*********************************/
insert into #sites values('19','19','0','Central Standard Time');
insert into #sites values('20','20','0','Central Standard Time');
insert into #sites values('21','21','0','Central Standard Time');
insert into #sites values('22','22','0','Central Standard Time');
insert into #sites values('23','23','0','Central Standard Time');
insert into #sites values('24','24','0','Central Standard Time');
insert into #sites values('25','25','0','Central Standard Time');
insert into #sites values('33','another test site','0','Central Standard Time');
insert into #sites values('12','Any','0','Central Standard Time');
insert into #sites values('8','Arnot Test Hospital','0','Central Standard Time');
insert into #sites values('38','Automation Test_FDB','1','Central Standard Time');
insert into #sites values('11','Automation Test_Multum','1','Central Standard Time');
insert into #sites values('39','Canadian FDB (39)','1','Central Standard Time');
insert into #sites values('3','Coach Clinic','1','Central Standard Time');
insert into #sites values('37','FDB  2TEST','1','Central Standard Time');
insert into #sites values('36','FDB (36)','1','Central Standard Time');
insert into #sites values('40','Medispan (40)','1','Central Standard Time');
insert into #sites values('5','Middle-earth','1','Central Standard Time');
insert into #sites values('27','New Site 27','0','Central Standard Time');
insert into #sites values('29','New Site 29','0','Central Standard Time');
insert into #sites values('30','New Site 30','0','Central Standard Time');
insert into #sites values('35','New Site 35','0','Central Standard Time');
insert into #sites values('42','New Testing Site','1','Central Standard Time');
insert into #sites values('28','Newer Site','0','Central Standard Time');
insert into #sites values('32','NewTestSite','0','Central Standard Time');
insert into #sites values('7','Performance Test','0','Central Standard Time');
insert into #sites values('6','Performance Test Data1','0','Central Standard Time');
insert into #sites values('1','Pulsecheck Hospital','1','Central Standard Time');
insert into #sites values('34','ResourceView','0','Central Standard Time');
insert into #sites values('4','Rules Site','0','Central Standard Time');
insert into #sites values('10','Site 10','0','Central Standard Time');
insert into #sites values('16','site 16','0','Central Standard Time');
insert into #sites values('9','Site 9','0','Central Standard Time');
insert into #sites values('14','site14-not good','0','Central Standard Time');
insert into #sites values('15','site15','0','Central Standard Time');
insert into #sites values('17','site17','0','Central Standard Time');
insert into #sites values('18','site18','0','Central Standard Time');
insert into #sites values('41','Test Site 41','1','Central Standard Time');
insert into #sites values('2','TRV Site','0','Central Standard Time');
insert into #sites values('31','UAT kona','0','Central Standard Time');
insert into #sites values('26','UAT''s other site','0','Central Standard Time');
insert into #sites values('13','UAT''s site','0','Central Standard Time');
insert into #sites values('255','Unit Test','1','Central Standard Time');

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
