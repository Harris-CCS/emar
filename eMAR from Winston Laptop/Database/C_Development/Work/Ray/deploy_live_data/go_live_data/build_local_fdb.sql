:listvar
 use [$(target_database)];
/******************************************
delete all permanent data
    delete performed in hierarchal sequence
******************************************/
execute [dbo].[create_FDB_search]

set nocount on;
set quoted_identifier on; -- needed to create fdb table computed columns

IF NOT EXISTS(SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'fdb_brand_name' AND COLUMN_NAME = 'MEDID_string') 
  alter table [dbo].[fdb_brand_name] add [MEDID_string]  AS (CONVERT([varchar](32),[MEDID])) persisted;
IF NOT EXISTS(SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'fdb_ndc_info' AND COLUMN_NAME = 'MEDID_string') 
  alter table [dbo].[fdb_ndc_info]   add [MEDID_string]  AS (CONVERT([varchar](32),[MEDID])) persisted;

--- fdb_medications_loader needs to have default site -1 to be able to function
if (
             select
                 count(*)
             from [dbo].[sites] [site]
             where [site].[id] in (0, -1)
    ) <> 2
    begin

        set identity_insert [dbo].[sites] on;

        insert into [dbo].[sites]
        (
            [id]
          , [name]
          , [is_active]
          , [time_zone_name]
        )
        select
            [val].[site_id]
          , [val].[name]
          , [val].[is_active]
          , [val].[time_zone_name]
        from (
        values
        ('-1', 'Dummy Site for Relational Integrity', '0', 'Central Standard Time')
        , ('0', 'Dummy Site use up site_id 0', '0', 'Central Standard Time')
        ) as [val]
        (
        [site_id]
        , [name]
        , [is_active]
        , [time_zone_name]
        )
            left join [dbo].[sites] [site]
                on [site].[id] = [val].[site_id]
        where [site].[id] is null;

        set identity_insert [dbo].[sites] off;

    end;

:r go_live_data\global_data\fdb_medications_loader.sql