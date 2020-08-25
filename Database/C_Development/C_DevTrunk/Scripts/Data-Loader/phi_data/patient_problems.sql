print 'Loading Table: patient_problems';

drop table if exists [#patient_problems];

create table [#patient_problems]
    (
      [site_id]        [varchar](25) null
    , [patient_id]     [varchar](25) null
    , [code_set_name]  [varchar](25) null
    , [code_set_value] [varchar](25) null
    , [problem_name]   [nvarchar](255) not null
    , [diagnosis_type] [varchar](25) null);

if '$(load_data)' = 'live'
   and exists
(
    select null
    from   [master].[sys].[databases]
    where  [name] = 'ibex'
)
    begin

        insert into [#patient_problems]
            ([site_id]
           , [patient_id]
           , [code_set_name]
           , [code_set_value]
           , [problem_name]
           , [diagnosis_type]
            )
        execute ('execute dbo.export_ibex_patient_problems');
    end;

if '$(load_data)' = 'sample'
    begin

        bulk insert [#patient_problems] from '$(current_path)Scripts\Data-Loader\sample_data\patient_problems.bcp' with(fieldterminator = '|~', rowterminator = '\n');
    end;

if
(
    select count(*)
    from   [#patient_problems]
) > 0
    begin

        begin transaction;

/****************************************
        load temporary tables for staging
****************************************/

        alter table [#patient_problems]
        add [id]        [bigint] identity(1, 1)
          , [target_id] [bigint];

/********************************
        get max id for seed value
********************************/

        set @max_id = null;

        select @max_id = max([id])
        from   [dbo].[patient_problems];

        set @max_id = isnull(@max_id, 0);

/*************************************
        begin loading permanent tables
*************************************/

        -- set identity_insert [dbo].[patient_problems] on;

        insert into [dbo].[patient_problems]
            ([patient_id]
           , [code_set_name]
           , [code_set_value]
           , [problem_name]
           , [diagnosis_type]
            )
        select isnull([internal_patient_id].[id], -1) as [patient_id]
             , [source].[code_set_name]
             , [source].[code_set_value]
             , [source].[problem_name]
             , [source].[diagnosis_type]
        from   [#patient_problems] as [source]
               outer apply [dbo].[get_internal_id]
            ('pulsecheck', 'patients', [source].[patient_id]) as [internal_patient_id];

        -- set identity_insert [dbo].[patient_problems] off;

/***************************************
        loading [external_ids] reference
***************************************/
/****************
        end table
****************/

        commit transaction;
    end;

drop table if exists [#patient_problems];