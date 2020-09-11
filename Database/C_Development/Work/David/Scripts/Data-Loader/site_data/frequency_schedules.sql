print 'Loading Table: frequency_schedules';
drop table if exists [#frequency_schedules];
drop table if exists [#frequency_interval_day_times];
drop table if exists [#frequency_interval_day_times_resolve];

if '$(load_data)' = 'live'
or '$(load_data)' = 'sample'
    begin

create table [#frequency_schedules]
    (
      [name]                       [sysname] null
    , [frequency_type_id]          [int] default 1  -- Daily, Weekly ...
    , [frequency_type_recurring]    [int] default 1  -- every n days (weeks... maybe future dev)
    , [frequency_interval]         [int] default 0  -- time interval default 0
    , [frequency_interval_unit_id] [int] default 0  -- interval unit Hours, Minutes
    , [interval_start_time]        [time](0) default '00:00'
    , [interval_end_minutes]       smallint default 0
    , [notes]                      varchar(1000) null);

create table [#frequency_interval_day_times]
    (
      [name]           [sysname] null
    , [frequency_schedule_id]   [int] null
    , [frequency_day]  [int] null
    , [frequency_time] [time](0) null);


insert into [#frequency_schedules] values('2 times daily'                                ,'1','1','0','0','00:00:00','0','0900, 1800');
insert into [#frequency_schedules] values('2 times daily  --  (2)'                       ,'1','1','0','0','00:00:00','0','0800, 2200 - use for insulin');
insert into [#frequency_schedules] values('2 times daily  --  (3)'                       ,'1','1','0','0','00:00:00','0','0300, 1800 - use for tacrolimus');
insert into [#frequency_schedules] values('2 times daily (Rl)'                           ,'1','1','0','0','00:00:00','0','respiratory use only - 0800, 2000');
insert into [#frequency_schedules] values('2 times daily after meals'                    ,'1','1','0','0','00:00:00','0','0830, 1900');
insert into [#frequency_schedules] values('2 times daily before meals'                   ,'1','1','0','0','00:00:00','0','0330, 1730');
insert into [#frequency_schedules] values('2 times daily PRN'                            ,'7','1','0','0','00:00:00','0','2 (two) times daily as needed');
insert into [#frequency_schedules] values('2 times daily with meals'                     ,'1','1','0','0','00:00:00','0','0730, 1800');
insert into [#frequency_schedules] values('2 times weekly'                               ,'7','1','0','0','00:00:00','0','2 (two) times a week');
insert into [#frequency_schedules] values('3 times daily'                                ,'1','1','0','0','00:00:00','0','0900, 1400, 2100');
insert into [#frequency_schedules] values('3 times daily  --  (2)'                       ,'1','1','0','0','00:00:00','0','respiratory use only - 0800, 1400, 2100');
insert into [#frequency_schedules] values('3 times daily after meals'                    ,'1','1','0','0','00:00:00','0','0830, 1300, 1900');
insert into [#frequency_schedules] values('3 times daily around food'                    ,'1','1','0','0','00:00:00','0','Medrol Dose Pack D2 use only - 0730, 1300, 1800');
insert into [#frequency_schedules] values('3 times daily before meals'                   ,'1','1','0','0','00:00:00','0','0330, 1130, 1730');
insert into [#frequency_schedules] values('3 times daily PRN'                            ,'7','1','0','0','00:00:00','0','3 (three) times daily as needed');
insert into [#frequency_schedules] values('3 times daily with meals'                     ,'1','1','0','0','00:00:00','0','0730, 1200, 1800');
insert into [#frequency_schedules] values('3 times weekly'                               ,'2','1','0','0','00:00:00','0','3 (three) times a week-T-Th-S');
insert into [#frequency_schedules] values('3 times weekly  --  (2)'                      ,'2','1','0','0','00:00:00','0','3 (three) times a week-M-WF-');
insert into [#frequency_schedules] values('30 min pre-op'                                ,'7','1','0','0','00:00:00','0','30 (thirty) minutes pre-op');
insert into [#frequency_schedules] values('4 times daily'                                ,'1','1','0','0','00:00:00','0','0800, 1200, 1800,2100');
insert into [#frequency_schedules] values('4 times daily  --  (2)'                       ,'1','1','0','0','00:00:00','0','respiratory use only - 0800, 1200, 1600, 2100');
insert into [#frequency_schedules] values('4 times daily after meals and at nightly'     ,'7','1','0','0','00:00:00','0','4X Daily (PC and at bedtime)');
insert into [#frequency_schedules] values('4 times daily after meals and nightly PRN'    ,'7','1','0','0','00:00:00','0','4 (four) times daily after meals and at bedtime as needed');
insert into [#frequency_schedules] values('4 times daily before meals and nightly'       ,'1','1','0','0','00:00:00','0','0330, 1130, 1730,2100');
insert into [#frequency_schedules] values('4 times daily PRN'                            ,'7','1','0','0','00:00:00','0','4 (four) times daily as needed');
insert into [#frequency_schedules] values('4 times daily tapering'                       ,'7','1','0','0','00:00:00','0','Medrol Dose Pack D3-D6 Taper');
insert into [#frequency_schedules] values('4 times daily with meals and nightly'         ,'1','1','0','0','00:00:00','0','0730, 1200, 1800,2200');
insert into [#frequency_schedules] values('4 times weekly'                               ,'7','1','0','0','00:00:00','0','4 (four) times a week');
insert into [#frequency_schedules] values('5 nmes Daily PRN'                             ,'7','1','0','0','00:00:00','0','5 (five) times daily a s needed');
insert into [#frequency_schedules] values('5 times daily'                                ,'1','1','0','0','00:00:00','0','0300, 1100, 1400, 1800,2200');
insert into [#frequency_schedules] values('5 times weekly'                               ,'7','1','0','0','00:00:00','0','5 (five) times a week');
insert into [#frequency_schedules] values('6 times daily'                                ,'1','1','0','0','00:00:00','0','0700, 1000, 1300, 1600, 1900,2200');
insert into [#frequency_schedules] values('6 times weekly'                               ,'7','1','0','0','00:00:00','0','6 (six) times a week');
insert into [#frequency_schedules] values('60 min pre-op'                                ,'5','1','0','0','00:00:00','0','60 (sixty) minutes pre-op');
insert into [#frequency_schedules] values('90 min pre-op'                                ,'5','1','0','0','00:00:00','0','90 (ninety) minutes pre-op');
insert into [#frequency_schedules] values('After dinner'                                 ,'1','1','0','0','00:00:00','0','Medrol Dose Pack D1 use only - 1900');
insert into [#frequency_schedules] values('After lunch'                                  ,'1','1','0','0','00:00:00','0','Medrol Dose Pack D1 use only - 1300');
insert into [#frequency_schedules] values('As needed'                                    ,'7','1','0','0','00:00:00','0','as needed');
insert into [#frequency_schedules] values('At bedtime'                                   ,'1','1','0','0','00:00:00','0','2200');
insert into [#frequency_schedules] values('Before breakfast'                             ,'1','1','0','0','00:00:00','0','0330');
insert into [#frequency_schedules] values('Before breakfast (Medrol Only)'               ,'1','1','0','0','00:00:00','0','Medrol Dose Pack D1 use only - 0730');
insert into [#frequency_schedules] values('Before breakfast weekly'                      ,'7','1','0','0','00:00:00','0','once a week before breakfast - use for alendronate/risedronate');
insert into [#frequency_schedules] values('Code/trauma/sedation continuous med'          ,'8','1','0','0','00:00:00','0','for a code, trauma, or sedation');
insert into [#frequency_schedules] values('Code/trauma/sedation medication'              ,'5','1','0','0','00:00:00','0','for a code, trauma, or sedation');
insert into [#frequency_schedules] values('Continuous'                                   ,'8','1','0','0','00:00:00','0','continuous');
insert into [#frequency_schedules] values('Continuous PAH'                               ,'1','1','0','0','00:00:00','0','Daily at 1300');
insert into [#frequency_schedules] values('Continuous PRN'                               ,'7','1','0','0','00:00:00','0','continuous pm');
insert into [#frequency_schedules] values('Continuous TPN'                               ,'1','1','0','0','00:00:00','0','Nightly at 1800');
insert into [#frequency_schedules] values('Continuous TPN - Neonatal'                    ,'1','1','0','0','00:00:00','0','Nightly at 1800');
insert into [#frequency_schedules] values('Cyclic TPN - see admin instructions'          ,'1','1','0','0','00:00:00','0','Nightly at 1800');
insert into [#frequency_schedules] values('Daily'                                        ,'1','1','0','0','00:00:00','0','0900');
insert into [#frequency_schedules] values('Daily  --  (2)'                               ,'1','1','0','0','00:00:00','0','use for warfarin - 1700');
insert into [#frequency_schedules] values('Daily  --  (3)'                               ,'1','1','0','0','00:00:00','0','use for digoxin- 1700');
insert into [#frequency_schedules] values('Daily  --  (4)'                               ,'1','1','0','0','00:00:00','0','0800 - use for insulin');
insert into [#frequency_schedules] values('Daily  --  (5)'                               ,'1','1','0','0','00:00:00','0','respiratory use only - 0800');
insert into [#frequency_schedules] values('Daily before lunch'                           ,'1','1','0','0','00:00:00','0','1130');
insert into [#frequency_schedules] values('Daily PRN'                                    ,'7','1','0','0','00:00:00','0','daily as needed');
insert into [#frequency_schedules] values('Daily with breakfast'                         ,'1','1','0','0','00:00:00','0','0730');
insert into [#frequency_schedules] values('Daily with dinner'                            ,'1','1','0','0','00:00:00','0','1800');
insert into [#frequency_schedules] values('Daily with lunch'                             ,'1','1','0','0','00:00:00','0','1200');
insert into [#frequency_schedules] values('During hospitalization'                       ,'5','1','0','0','00:00:00','0','to be given prior to discharge');
insert into [#frequency_schedules] values('Every 1 hour PRN'                             ,'7','1','0','0','00:00:00','0','every hour as needed');
insert into [#frequency_schedules] values('Every 1 hour scheduled'                       ,'4','1','1','3','00:00:00','0','every 1 (one) hour');
insert into [#frequency_schedules] values('Every 1 hour while awake'                     ,'1','1','1','3','03:00:00','1140','0300-2200 (Omit 2300-0500)');
insert into [#frequency_schedules] values('Every 10 hours'                               ,'4','1','10','3','00:00:00','0','every 10 (ten) hours');
insert into [#frequency_schedules] values('Every 10 min'                                 ,'4','1','10','2','00:00:00','0','every 10 (ten) minutes');
insert into [#frequency_schedules] values('Every 10 min PRN'                             ,'7','1','0','0','00:00:00','0','every 10 (ten) minutes as needed');
insert into [#frequency_schedules] values('Every 12 hours'                               ,'4','1','12','3','00:00:00','0','every 12 (twelve) hours');
insert into [#frequency_schedules] values('Every 12 hours for 3 doses per week'          ,'7','1','0','0','00:00:00','0','3 doses weeklyq- 12h. oral methotrexate optional scheduling');
insert into [#frequency_schedules] values('Every 12 hours PRN'                           ,'7','1','0','0','00:00:00','0','every 1 2 (twelve) hours as needed');
insert into [#frequency_schedules] values('Every 12 hours scheduled'                     ,'1','1','0','0','00:00:00','0','0900,2100');
insert into [#frequency_schedules] values('Every 12 hours scheduled (Rl)'                ,'1','1','0','0','00:00:00','0','respiratory use only - 0700, 1900');
insert into [#frequency_schedules] values('Every 14 days'                                ,'4','1','14','4','00:00:00','0','every 14 days');
insert into [#frequency_schedules] values('Every 15 min'                                 ,'4','1','15','2','00:00:00','0','every 1 5 (fifteen) minutes');
insert into [#frequency_schedules] values('Every 15 min PRN'                             ,'7','1','0','0','00:00:00','0','every 1 5 (fifteen) minutes as needed');
insert into [#frequency_schedules] values('Every 16 hours'                               ,'4','1','16','3','00:00:00','0','every 16 (sixteen) hours');
insert into [#frequency_schedules] values('Every 18 hours'                               ,'4','1','18','3','00:00:00','0','every 18 (eighteen) hours');
insert into [#frequency_schedules] values('Every 2 hour PRN'                             ,'7','1','0','0','00:00:00','0','every 2 (two) hours as needed');
insert into [#frequency_schedules] values('Every 2 hours'                                ,'4','1','2','3','00:00:00','0','respiratory use only - every 2 (two) hours');
insert into [#frequency_schedules] values('Every 2 hours scheduled'                      ,'4','1','2','3','00:00:00','0','every 2 (two) hours');
insert into [#frequency_schedules] values('Every 2 hours while awake'                    ,'1','1','2','3','06:00:00','960','0300-2200 (Omit 0000,0200,0400)');
insert into [#frequency_schedules] values('Every 20 min PRN'                             ,'7','1','0','0','00:00:00','0','every 20 (twenty) minutes');
insert into [#frequency_schedules] values('Every 21 days'                                ,'4','1','21','4','00:00:00','0','every 21 days');
insert into [#frequency_schedules] values('Every 24 hours'                               ,'4','1','24','3','00:00:00','0','every 24 hours');
insert into [#frequency_schedules] values('Every 24 hours  --  (2)'                      ,'4','1','24','3','00:00:00','0','Q24H - antibiotics/other - exclude from MAR hold');
insert into [#frequency_schedules] values('Every 24 hours scheduled'                     ,'1','1','0','0','00:00:00','0','0900');
insert into [#frequency_schedules] values('Every 28 days'                                ,'4','1','28','4','00:00:00','0','every 28 days');
insert into [#frequency_schedules] values('Every 3 days'                                 ,'4','1','3','4','00:00:00','0','every 3 days');
insert into [#frequency_schedules] values('Every 3 hours'                                ,'4','1','3','3','00:00:00','0','respiratory use only - every 3 (three) hours');
insert into [#frequency_schedules] values('Every 3 hours PRN'                            ,'7','1','0','0','00:00:00','0','every 3 (three) hours as needed');
insert into [#frequency_schedules] values('Every 3 hours scheduled'                      ,'4','1','3','3','00:00:00','0','Every 3 (three) hours');
insert into [#frequency_schedules] values('Every 3 months'                               ,'4','1','90','4','00:00:00','0','every 3 months');
insert into [#frequency_schedules] values('Every 30 days'                                ,'4','1','30','4','00:00:00','0','every 30 days');
insert into [#frequency_schedules] values('Every 30 min'                                 ,'4','1','30','2','00:00:00','0','every 30 (thirty) minutes');
insert into [#frequency_schedules] values('Every 30 min PRN'                             ,'7','1','0','0','00:00:00','0','every 30 (thirty) minutes as needed');
insert into [#frequency_schedules] values('Every 32 hours'                               ,'4','1','32','3','00:00:00','0','every 32 hours');
insert into [#frequency_schedules] values('Every 36 hours'                               ,'4','1','36','3','00:00:00','0','every 36 hours');
insert into [#frequency_schedules] values('Every 4 hours'                                ,'4','1','4','3','00:00:00','0','every 4 (four) hours');
insert into [#frequency_schedules] values('Every 4 hours  --  (2)'                       ,'4','1','4','3','00:00:00','0','respiratory use only - every 4 (four) hours');
insert into [#frequency_schedules] values('Every 4 hours PRN'                            ,'7','1','0','0','00:00:00','0','every 4 (four) hours as needed');
insert into [#frequency_schedules] values('Every 4 hours scheduled'                      ,'1','1','0','0','00:00:00','0','0000,0400,0800, 1200, 1600,2000');
insert into [#frequency_schedules] values('Every 4 hours while awake'                    ,'1','1','4','3','03:00:00','1140','0300-2200 (omit 0200)');
insert into [#frequency_schedules] values('Every 4 hours while awake  --  (2)'           ,'1','1','4','3','07:00:00','720','respiratory only - 0700-1900 (omit 2300, 0300)');
insert into [#frequency_schedules] values('Every 4 months'                               ,'4','1','120','4','00:00:00','0','every 4 months');
insert into [#frequency_schedules] values('Every 48 hours'                               ,'4','1','48','3','00:00:00','0','every 48 hours');
insert into [#frequency_schedules] values('Every 48 hours PRN'                           ,'7','1','0','0','00:00:00','0','every 48 hours as needed');
insert into [#frequency_schedules] values('Every 5 min'                                  ,'4','1','5','2','00:00:00','0','every 5 (five) minutes');
insert into [#frequency_schedules] values('Every 5 min PRN'                              ,'7','1','0','0','00:00:00','0','every 5 (five) minutes as needed');
insert into [#frequency_schedules] values('Every 6 hours'                                ,'4','1','6','3','00:00:00','0','every 6 (six) hours');
insert into [#frequency_schedules] values('Every 6 hours PRN'                            ,'7','1','0','0','00:00:00','0','every 6 (six) hours as needed');
insert into [#frequency_schedules] values('Every 6 hours scheduled'                      ,'1','1','0','0','00:00:00','0','0000,0300, 1200, 1800');
insert into [#frequency_schedules] values('Every 6 hours scheduled (respiratory)'        ,'1','1','0','0','00:00:00','0','respiratory use only - 0300, 0900, 1500, 2100');
insert into [#frequency_schedules] values('Every 6 hours while awake'                    ,'1','2','6','3','08:00:00','720','respiratory use only - 0800,1400, 2000 (Omit 0200)');
insert into [#frequency_schedules] values('Every 6 hours while asleep'                   ,'1','2','6','3','21:00:00','720','respiratory use only - 2100,0300, 0900');
insert into [#frequency_schedules] values('Every 6 months'                               ,'4','1','180','4','00:00:00','0','every 6 months');
insert into [#frequency_schedules] values('Every 7 days'                                 ,'4','1','7','4','00:00:00','0','every 7 days');
insert into [#frequency_schedules] values('Every 72 hours'                               ,'4','1','72','3','00:00:00','0','every 72 hours');
insert into [#frequency_schedules] values('Every 72 hours PRN'                           ,'7','1','0','0','00:00:00','0','every 72 hours as needed');
insert into [#frequency_schedules] values('Every 8 hours'                                ,'4','1','8','3','00:00:00','0','every 8 (eight) hours');
insert into [#frequency_schedules] values('Every 8 hours PRN'                            ,'7','1','0','0','00:00:00','0','every 8 (eight) hours as needed');
insert into [#frequency_schedules] values('Every 8 hours scheduled'                      ,'1','1','0','0','00:00:00','0','0300, 1400,2200');
insert into [#frequency_schedules] values('Every 8 hours scheduled (respiratory)'        ,'1','1','0','0','00:00:00','0','respiratory use only - 0000, 0800, 1600');
insert into [#frequency_schedules] values('Every evening'                                ,'1','1','0','0','00:00:00','0','1700');
insert into [#frequency_schedules] values('Every evening  --  (2)'                       ,'1','1','0','0','00:00:00','0','1800 - use for tacrolimus');
--~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
--~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
--~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
--~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
insert into [#frequency_interval_day_times] values('2 times daily'                                ,null,0,'09:00');
insert into [#frequency_interval_day_times] values('2 times daily'                                ,null,0,'18:00');
insert into [#frequency_interval_day_times] values('2 times daily  --  (2)'                       ,null,0,'08:00');
insert into [#frequency_interval_day_times] values('2 times daily  --  (2)'                       ,null,0,'22:00');
insert into [#frequency_interval_day_times] values('2 times daily  --  (3)'                       ,null,0,'03:00');
insert into [#frequency_interval_day_times] values('2 times daily  --  (3)'                       ,null,0,'18:00');
insert into [#frequency_interval_day_times] values('2 times daily (Rl)'                           ,null,0,'08:00');
insert into [#frequency_interval_day_times] values('2 times daily (Rl)'                           ,null,0,'20:00');
insert into [#frequency_interval_day_times] values('2 times daily after meals'                    ,null,0,'08:30');
insert into [#frequency_interval_day_times] values('2 times daily after meals'                    ,null,0,'19:00');
insert into [#frequency_interval_day_times] values('2 times daily before meals'                   ,null,0,'03:30');
insert into [#frequency_interval_day_times] values('2 times daily before meals'                   ,null,0,'17:00');
insert into [#frequency_interval_day_times] values('2 times daily with meals'                     ,null,0,'07:30');
insert into [#frequency_interval_day_times] values('2 times daily with meals'                     ,null,0,'18:00');
insert into [#frequency_interval_day_times] values('3 times daily'                                ,null,0,'09:00');
insert into [#frequency_interval_day_times] values('3 times daily'                                ,null,0,'14:00');
insert into [#frequency_interval_day_times] values('3 times daily'                                ,null,0,'21:00');
insert into [#frequency_interval_day_times] values('3 times daily  --  (2)'                       ,null,0,'08:00');
insert into [#frequency_interval_day_times] values('3 times daily  --  (2)'                       ,null,0,'14:00');
insert into [#frequency_interval_day_times] values('3 times daily  --  (2)'                       ,null,0,'21:00');
insert into [#frequency_interval_day_times] values('3 times daily after meals'                    ,null,0,'08:30');
insert into [#frequency_interval_day_times] values('3 times daily after meals'                    ,null,0,'13:00');
insert into [#frequency_interval_day_times] values('3 times daily after meals'                    ,null,0,'19:00');
insert into [#frequency_interval_day_times] values('3 times daily around food'                    ,null,0,'07:30');
insert into [#frequency_interval_day_times] values('3 times daily around food'                    ,null,0,'13:00');
insert into [#frequency_interval_day_times] values('3 times daily around food'                    ,null,0,'18:00');
insert into [#frequency_interval_day_times] values('3 times daily before meals'                   ,null,0,'03:30');
insert into [#frequency_interval_day_times] values('3 times daily before meals'                   ,null,0,'11:30');
insert into [#frequency_interval_day_times] values('3 times daily before meals'                   ,null,0,'17:30');
insert into [#frequency_interval_day_times] values('3 times daily with meals'                     ,null,0,'07:30');
insert into [#frequency_interval_day_times] values('3 times daily with meals'                     ,null,0,'12:00');
insert into [#frequency_interval_day_times] values('3 times daily with meals'                     ,null,0,'18:00');
insert into [#frequency_interval_day_times] values('3 times weekly'                               ,null,3,'12:00');
insert into [#frequency_interval_day_times] values('3 times weekly'                               ,null,5,'12:00');
insert into [#frequency_interval_day_times] values('3 times weekly'                               ,null,7,'12:00');
insert into [#frequency_interval_day_times] values('3 times weekly  --  (2)'                      ,null,2,'12:00');
insert into [#frequency_interval_day_times] values('3 times weekly  --  (2)'                      ,null,4,'12:00');
insert into [#frequency_interval_day_times] values('3 times weekly  --  (2)'                      ,null,6,'12:00');
insert into [#frequency_interval_day_times] values('4 times daily'                                ,null,0,'08:00');
insert into [#frequency_interval_day_times] values('4 times daily'                                ,null,0,'12:00');
insert into [#frequency_interval_day_times] values('4 times daily'                                ,null,0,'18:00');
insert into [#frequency_interval_day_times] values('4 times daily'                                ,null,0,'21:00');
insert into [#frequency_interval_day_times] values('4 times daily  --  (2)'                       ,null,0,'08:00');
insert into [#frequency_interval_day_times] values('4 times daily  --  (2)'                       ,null,0,'12:00');
insert into [#frequency_interval_day_times] values('4 times daily  --  (2)'                       ,null,0,'16:00');
insert into [#frequency_interval_day_times] values('4 times daily  --  (2)'                       ,null,0,'21:00');
insert into [#frequency_interval_day_times] values('4 times daily before meals and nightly'       ,null,0,'03:30');
insert into [#frequency_interval_day_times] values('4 times daily before meals and nightly'       ,null,0,'11:30');
insert into [#frequency_interval_day_times] values('4 times daily before meals and nightly'       ,null,0,'17:30');
insert into [#frequency_interval_day_times] values('4 times daily before meals and nightly'       ,null,0,'21:00');
insert into [#frequency_interval_day_times] values('4 times daily with meals and nightly'         ,null,0,'07:30');
insert into [#frequency_interval_day_times] values('4 times daily with meals and nightly'         ,null,0,'12:00');
insert into [#frequency_interval_day_times] values('4 times daily with meals and nightly'         ,null,0,'18:00');
insert into [#frequency_interval_day_times] values('4 times daily with meals and nightly'         ,null,0,'22:00');
insert into [#frequency_interval_day_times] values('5 times daily'                                ,null,0,'03:00');
insert into [#frequency_interval_day_times] values('5 times daily'                                ,null,0,'11:00');
insert into [#frequency_interval_day_times] values('5 times daily'                                ,null,0,'14:00');
insert into [#frequency_interval_day_times] values('5 times daily'                                ,null,0,'18:00');
insert into [#frequency_interval_day_times] values('5 times daily'                                ,null,0,'22:00');
insert into [#frequency_interval_day_times] values('6 times daily'                                ,null,0,'07:00');
insert into [#frequency_interval_day_times] values('6 times daily'                                ,null,0,'10:00');
insert into [#frequency_interval_day_times] values('6 times daily'                                ,null,0,'13:00');
insert into [#frequency_interval_day_times] values('6 times daily'                                ,null,0,'16:00');
insert into [#frequency_interval_day_times] values('6 times daily'                                ,null,0,'19:00');
insert into [#frequency_interval_day_times] values('6 times daily'                                ,null,0,'22:00');
insert into [#frequency_interval_day_times] values('After dinner'                                 ,null,0,'19:00');
insert into [#frequency_interval_day_times] values('After lunch'                                  ,null,0,'13:00');
insert into [#frequency_interval_day_times] values('At bedtime'                                   ,null,0,'22:00');
insert into [#frequency_interval_day_times] values('Before breakfast'                             ,null,0,'03:30');
insert into [#frequency_interval_day_times] values('Before breakfast (Medrol Only)'               ,null,0,'07:30');
insert into [#frequency_interval_day_times] values('Continuous PAH'                               ,null,0,'13:00');
insert into [#frequency_interval_day_times] values('Continuous TPN'                               ,null,0,'18:00');
insert into [#frequency_interval_day_times] values('Continuous TPN - Neonatal'                    ,null,0,'18:00');
insert into [#frequency_interval_day_times] values('Cyclic TPN - see admin instructions'          ,null,0,'18:00');
insert into [#frequency_interval_day_times] values('Daily'                                        ,null,0,'09:00');
insert into [#frequency_interval_day_times] values('Daily  --  (2)'                               ,null,0,'17:00');
insert into [#frequency_interval_day_times] values('Daily  --  (3)'                               ,null,0,'17:00');
insert into [#frequency_interval_day_times] values('Daily  --  (4)'                               ,null,0,'08:00');
insert into [#frequency_interval_day_times] values('Daily  --  (5)'                               ,null,0,'08:00');
insert into [#frequency_interval_day_times] values('Daily before lunch'                           ,null,0,'11:30');
insert into [#frequency_interval_day_times] values('Daily with breakfast'                         ,null,0,'07:30');
insert into [#frequency_interval_day_times] values('Daily with dinner'                            ,null,0,'18:00');
insert into [#frequency_interval_day_times] values('Daily with lunch'                             ,null,0,'12:00');
insert into [#frequency_interval_day_times] values('Every 1 hour while awake'                     ,null,0,null);
insert into [#frequency_interval_day_times] values('Every 12 hours scheduled'                     ,null,0,'09:00');
insert into [#frequency_interval_day_times] values('Every 12 hours scheduled'                     ,null,0,'21:00');
insert into [#frequency_interval_day_times] values('Every 12 hours scheduled (Rl)'                ,null,0,'07:00');
insert into [#frequency_interval_day_times] values('Every 12 hours scheduled (Rl)'                ,null,0,'19:00');
insert into [#frequency_interval_day_times] values('Every 2 hours while awake'                    ,null,0,null);
insert into [#frequency_interval_day_times] values('Every 24 hours scheduled'                     ,null,0,'09:00');
insert into [#frequency_interval_day_times] values('Every 4 hours scheduled'                      ,null,0,'00:00');
insert into [#frequency_interval_day_times] values('Every 4 hours scheduled'                      ,null,0,'04:00');
insert into [#frequency_interval_day_times] values('Every 4 hours scheduled'                      ,null,0,'08:00');
insert into [#frequency_interval_day_times] values('Every 4 hours scheduled'                      ,null,0,'12:00');
insert into [#frequency_interval_day_times] values('Every 4 hours scheduled'                      ,null,0,'16:00');
insert into [#frequency_interval_day_times] values('Every 4 hours scheduled'                      ,null,0,'20:00');
insert into [#frequency_interval_day_times] values('Every 4 hours while awake'                    ,null,0,null);
insert into [#frequency_interval_day_times] values('Every 4 hours while awake  --  (2)'           ,null,0,null);
insert into [#frequency_interval_day_times] values('Every 6 hours scheduled'                      ,null,0,'00:00');
insert into [#frequency_interval_day_times] values('Every 6 hours scheduled'                      ,null,0,'03:00');
insert into [#frequency_interval_day_times] values('Every 6 hours scheduled'                      ,null,0,'12:00');
insert into [#frequency_interval_day_times] values('Every 6 hours scheduled'                      ,null,0,'18:00');
insert into [#frequency_interval_day_times] values('Every 6 hours scheduled (respiratory)'        ,null,0,'03:00');
insert into [#frequency_interval_day_times] values('Every 6 hours scheduled (respiratory)'        ,null,0,'09:00');
insert into [#frequency_interval_day_times] values('Every 6 hours scheduled (respiratory)'        ,null,0,'15:00');
insert into [#frequency_interval_day_times] values('Every 6 hours scheduled (respiratory)'        ,null,0,'21:00');
insert into [#frequency_interval_day_times] values('Every 6 hours while awake'                    ,null,0,null);
insert into [#frequency_interval_day_times] values('Every 6 hours while asleep'                   ,null,0,null);
insert into [#frequency_interval_day_times] values('Every 8 hours scheduled'                      ,null,0,'03:00');
insert into [#frequency_interval_day_times] values('Every 8 hours scheduled'                      ,null,0,'14:00');
insert into [#frequency_interval_day_times] values('Every 8 hours scheduled'                      ,null,0,'22:00');
insert into [#frequency_interval_day_times] values('Every 8 hours scheduled (respiratory)'        ,null,0,'00:00');
insert into [#frequency_interval_day_times] values('Every 8 hours scheduled (respiratory)'        ,null,0,'08:00');
insert into [#frequency_interval_day_times] values('Every 8 hours scheduled (respiratory)'        ,null,0,'16:00');
insert into [#frequency_interval_day_times] values('Every evening'                                ,null,0,'17:00');
insert into [#frequency_interval_day_times] values('Every evening  --  (2)'                       ,null,0,'18:00');
set identity_insert [dbo].[frequency_schedules] on;
insert into [dbo].[frequency_schedules]
    ([id]
   , [site_id]
   , [name]
   , [frequency_type_id]
   , [frequency_type_recurring]
   , [frequency_interval]
   , [frequency_interval_unit_id]
   , [interval_start_time]
   , [interval_end_minutes]
   , [notes]
   , [point_in_time]
    )
values(0,-1,'',0,0,0,0,'00:00',0,'',0)
set identity_insert [dbo].[frequency_schedules] off;

insert into [dbo].[frequency_schedules]
    ([site_id]
   , [name]
   , [frequency_type_id]
   , [frequency_type_recurring]
   , [frequency_interval]
   , [frequency_interval_unit_id]
   , [interval_start_time]
   , [interval_end_minutes]
   , [notes]
   , [point_in_time]
    )
select [site].[site_id]
     , [fs].[name]
     , [fs].[frequency_type_id]
     , [fs].[frequency_type_recurring]
     , [fs].[frequency_interval]
     , [fs].[frequency_interval_unit_id]
     , [fs].[interval_start_time]
     , [fs].[interval_end_minutes]
     , [fs].[notes]
     , 1 [point_in_time]
from   [#frequency_schedules] as [fs]
       cross join
(
    select [id] as [site_id] from [get_internal_id]('pulsecheck', 'sites',  1) union all
    select [id] as [site_id] from [get_internal_id]('pulsecheck', 'sites',  5) union all
    select [id] as [site_id] from [get_internal_id]('pulsecheck', 'sites', 11) union all
    select [id] as [site_id] from [get_internal_id]('pulsecheck', 'sites', 36) union all
    select [id] as [site_id] from [get_internal_id]('pulsecheck', 'sites', 39) union all
    select [id] as [site_id] from [get_internal_id]('pulsecheck', 'sites', 40) union all
    select [id] as [site_id] from [get_internal_id]('pulsecheck', 'sites', 19) union all
    select [id] as [site_id] from [get_internal_id]('pulsecheck', 'sites', 23)
) as [site]
order by [site_id]
       , [frequency_type_id]
       , [frequency_interval_unit_id]
       , [name];

select [site].[site_id]
     , [fidt].[name]
     , [frequency_schedule_id]
     , [frequency_day]
     , [frequency_time]
into [#frequency_interval_day_times_resolve]
from   [#frequency_interval_day_times] as [fidt]
       cross join
(
    select [id] as [site_id] from [get_internal_id]('pulsecheck', 'sites',  1) union all
    select [id] as [site_id] from [get_internal_id]('pulsecheck', 'sites',  5) union all
    select [id] as [site_id] from [get_internal_id]('pulsecheck', 'sites', 11) union all
    select [id] as [site_id] from [get_internal_id]('pulsecheck', 'sites', 36) union all
    select [id] as [site_id] from [get_internal_id]('pulsecheck', 'sites', 39) union all
    select [id] as [site_id] from [get_internal_id]('pulsecheck', 'sites', 40) union all
    select [id] as [site_id] from [get_internal_id]('pulsecheck', 'sites', 19) union all
    select [id] as [site_id] from [get_internal_id]('pulsecheck', 'sites', 23)
) as [site]
order by [site_id]
       , [name]
       , [frequency_day]
       , [frequency_time];

update [res] set    
    [frequency_schedule_id] = [fs].[id]
from   [dbo].[frequency_schedules] [fs]
       inner join [#frequency_interval_day_times_resolve] [res] on [res].[site_id] = [fs].[site_id]
                                                                   and [res].[name] = [fs].[name];

insert into [dbo].[frequency_interval_day_times]
select [frequency_schedule_id]
     , [frequency_day]
     , [frequency_time]
from   [#frequency_interval_day_times_resolve];


update [dbo].[frequency_schedules] set    
    [point_in_time] = 0
where  [name] like '%contin%'
and [point_in_time] = 1;

end;

drop table if exists [#frequency_schedules];
drop table if exists [#frequency_interval_day_times];
drop table if exists [#frequency_interval_day_times_resolve];
