print 'Loading Table: patient_allergies';

drop table if exists [#patient_allergies];

create table [#patient_allergies]
    (
      [patient_id]          [varchar](50) null
    , [class]               [varchar](32) null
    , [category]            [varchar](32) null
    , [internal_drug_id]    [varchar](32) null
    , [ndc]                 [varchar](32) null
    , [drug_id]             [varchar](32) null
    , [name]                [nvarchar](255) null
    , [alternate_name]      [nvarchar](255) null
    , [dose]                [varchar](50) null
    , [dose_unit]           [varchar](20) null
    , [medication_route_id] [varchar](50) null
    , [allergy_drug_id]     [varchar](32) null
    , [is_active]           [bit] not null
    , [comment]             [varchar](255) null
    , [schedule]            [varchar](40) null
    , [reaction]            [varchar](80) null
    , [severity]            [varchar](80) null
    , [parent_drug_id]      [varchar](32) null
    , [parent_drug_name]    [nvarchar](255) null
    , [add_user_id]         [varchar](50) null
    , [add_datetime]        [varchar](50) null
    , [change_user_id]      [varchar](50) null
    , [change_datetime]     [varchar](50) null);

if '$(load_data)' = 'live'
   and exists
(
    select null
    from   [master].[sys].[databases]
    where  [name] = 'ibex'
)
    begin

        insert into [#patient_allergies]
            ([patient_id]
           , [class]
           , [category]
           , [internal_drug_id]
           , [ndc]
           , [drug_id]
           , [name]
           , [alternate_name]
           , [dose]
           , [dose_unit]
           , [medication_route_id]
           , [allergy_drug_id]
           , [is_active]
           , [comment]
           , [schedule]
           , [reaction]
           , [severity]
           , [parent_drug_id]
           , [parent_drug_name]
           , [add_user_id]
           , [add_datetime]
           , [change_user_id]
           , [change_datetime]
            )
        execute ('execute dbo.export_ibex_patient_allergies');
    end;

if '$(load_data)' = 'sample'
    begin

        bulk insert [#patient_allergies] from '$(current_path)Scripts\Data-Loader\sample_data\patient_allergies.bcp' with(fieldterminator = '|~', rowterminator = '\n');
    end;

if
(
    select count(*)
    from   [#patient_allergies]
) > 0
    begin

        begin transaction;

/****************************************
        load temporary tables for staging
****************************************/

        alter table [#patient_allergies]
        add [id]        [bigint] identity(1, 1)
          , [target_id] [bigint];

/********************************
        get max id for seed value
********************************/

        set @max_id = null;

        select @max_id = max([id])
        from   [dbo].[patient_allergies];

        set @max_id = isnull(@max_id, 0);

        update [source] set    
            [target_id] = [source].[id] + @max_id
        from   [#patient_allergies] as [source];

/*************************************
        begin loading permanent tables
*************************************/

        -- set identity_insert [dbo].[patient_allergies] on;

        insert into [dbo].[patient_allergies]
            ([patient_id]
           , [class]
           , [category]
           , [internal_drug_id]
           , [ndc]
           , [drug_id]
           , [name]
           , [dose]
           , [dose_unit]
           , [medication_route_id]
           , [allergy_drug_id]
           , [is_active]
           , [comment]
           , [schedule]
           , [reaction]
           , [severity]
           , [parent_drug_id]
           , [parent_drug_name]
           , [add_user_id]
           , [add_datetime]
           , [change_user_id]
           , [change_datetime]
            )
        select        isnull([internal_patient_id].[id], -1) as     [patient_id]
                    , [source].[class]
                    , [source].[category]
                    , [source].[internal_drug_id]
                    , [source].[ndc]
                    , [source].[drug_id]
                    , [source].[name]
                    , [source].[dose]
                    , [source].[dose_unit]
                    , [medication_routes].[id] as                   [medication_routes_id]
                    , [source].[allergy_drug_id]
                    , [source].[is_active]
                    , [source].[comment]
                    , [source].[schedule]
                    , [source].[reaction]
                    , [source].[severity]
                    , [source].[parent_drug_id]
                    , [source].[parent_drug_name]
                    , isnull([internal_add_user_id].[id], 0) as    [add_user_id]
                    , case
                          when len([source].[add_datetime]) >= 8
                              then
        (
            select [msdb].[dbo].[agent_datetime]
                (left([source].[add_datetime], 8), substring([source].[add_datetime] + '000000', 9, 6))
        )
                                                                 else null
                      end as                                        [dateadd]
                    , isnull([internal_change_user_id].[id], 0) as [change_user_id]
                    , case
                          when len([source].[change_datetime]) >= 8
                              then
        (
            select [msdb].[dbo].[agent_datetime]
                (left([source].[change_datetime], 8), substring([source].[change_datetime] + '000000', 9, 6))
        )
                                                                    else null
                      end as                                        [datechg]
        from          [#patient_allergies] as [source]
                      outer apply [dbo].[get_internal_id]
            ('pulsecheck', 'patients', [source].[patient_id]) as [internal_patient_id]
                      left join [dbo].[patients] as [patients] on [patients].[id] = [internal_patient_id].[id]
                      outer apply [dbo].[get_internal_id]
            ('pulsecheck', 'users', [source].[add_user_id]) as [internal_add_user_id]
                      outer apply [dbo].[get_internal_id]
            ('pulsecheck', 'users', [source].[change_user_id]) as [internal_change_user_id]
                      outer apply
        (
            select top 1 [mr_item].[id]
            from
            (
                select 1 as [type]
                     , [mr].[id]
                     , [patients].[site_id]
                from     [dbo].[medication_routes] as [mr]
                where      [mr].[name] = [source].[medication_route_id]
                           and [mr].[site_id] = [patients].[site_id]
                union
                select 2 as [type]
                     , [mr].[id]
                     , [patients].[site_id]
                from   [dbo].[medication_routes] as [mr]
                where  [mr].[name] = [source].[medication_route_id]
                       and [mr].[site_id] <> [patients].[site_id]
            ) as [mr_item]
            order by [mr_item].[type]
                   , [mr_item].[site_id]
        ) as [medication_routes]
        order by [patient_id];
        --select *
        --from   [#patient_allergies];
        -- set identity_insert [dbo].[patient_allergies] off;

/***************************************
        loading [external_ids] reference
***************************************/
/****************
        end table
****************/

        commit transaction;
    end;

drop table if exists [#patient_allergies];








--        select        isnull([internal_patient_id].[id], -1) as     [patient_id]
--		,[source].[patient_id]
--        from          [#patient_allergies] as [source]
--                      outer apply [dbo].[get_internal_id]
--            ('pulsecheck', 'patients', [source].[patient_id]) as [internal_patient_id]
----                      left join [dbo].[patients] as [patients] on [patients].[id] = [internal_patient_id].[id]

--select * from dbo.external_ids where entity='patients' and external_id like '%20070907122558%' order by 4
--select * from [#patient_allergies] as [source] where patient_id='1|20070907122558'
--select * from  [dbo].[get_internal_id] ('pulsecheck', 'patients', '1|20070907122558') 
--select * from ibex.dbo.external_ids where entity='patients' and external_id like '%20070907122558%' order by 4


--select *
--        from   [ibex].[dbo].[alg] as [source]
--		order by dateadd


--               inner join [ibex].[dbo].[org] as [sites] on [sites].[site] = [source].[site]
--               inner join [ibex].[dbo].[pat] as [patients] on [patients].[site] = [sites].[site] and  [patients].[ibex] = [source].[ibex]


--		,[source].[patient_id]

--execute dbo.export_ibex_patient_allergies

--select count(*),site
--from ibex.dbo.pat 
--group by site
--order by site

--select count(*),site
--from ibex.dbo.alg
--group by site
--order by site

