print 'Loading Table: patient_indicators';

drop table if exists [#patient_indicators];

create table [#patient_indicators]
    (
      [patient_id]       [varchar](50) not null
    , [ordinal_position] [smallint] not null
    , [code]             [varchar](10) not null
    , [type]             [varchar](10) not null
    , [description]      [varchar](255) not null
	, [image_name]       [nvarchar](255) not null
    , [type_description] [nvarchar](255) not null);

if '$(load_data)' = 'sample'
   or ('$(load_data)' = 'live'
       and @does_ibex_exist = 1)
    begin

        insert into [#patient_indicators]
            ([patient_id]
           , [ordinal_position]
           , [code]
           , [type]
           , [description]
           , [image_name]
		   , [type_description]
            )
        execute ('execute dbo.export_ibex_patient_indicators');
    end;

if
(
    select count(*)
    from   [#patient_indicators]
) > 0
    begin

        begin transaction;

/****************************************
        load temporary tables for staging
****************************************/
/********************************
        get max id for seed value
********************************/
/*************************************
        begin loading permanent tables
*************************************/

        -- set identity_insert [dbo].[patient_indicators] on;

        insert into [dbo].[patient_indicators]
            ([patient_id]
           , [ordinal_position]
           , [code]
           , [type]
           , [description]
           , [image_name]
		   , [type_description]
            )
        select isnull([internal_patient_id].[id], -1) as [patient_id]
             , [source].[ordinal_position]
             , [source].[code]
             , [source].[type]
             , [source].[description]
             , [source].[image_name]
			 , [source].[type_description]
        from   [#patient_indicators] as [source]
               outer apply [dbo].[get_internal_id]
            ('pulsecheck', 'patients', [source].[patient_id]) as [internal_patient_id];

        -- set identity_insert [dbo].[patient_indicators] off;

/***************************************
        loading [external_ids] reference
***************************************/
/****************
        end table
****************/

        commit transaction;
    end;

drop table if exists [#patient_indicators];