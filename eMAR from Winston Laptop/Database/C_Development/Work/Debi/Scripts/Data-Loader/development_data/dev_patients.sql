if '$(load_data)' in('sample', 'live')
    begin

        print 'Loading Table: development_data\dev_patients.sql';

    declare
        @tmp_degree table
        (
          [id]   int identity(1, 1)
        , [name] varchar(20));
        insert into @tmp_degree([name]) values('AN')
        insert into @tmp_degree([name]) values('ADN')
        insert into @tmp_degree([name]) values('ASN')
        insert into @tmp_degree([name]) values('BN')
        insert into @tmp_degree([name]) values('BP')
        insert into @tmp_degree([name]) values('BSN')
        insert into @tmp_degree([name]) values('MCM')
        insert into @tmp_degree([name]) values('MM')
        insert into @tmp_degree([name]) values('MMS')
        insert into @tmp_degree([name]) values('MN')
        insert into @tmp_degree([name]) values('MNA')
        insert into @tmp_degree([name]) values('MPAS')
        insert into @tmp_degree([name]) values('MS')
        insert into @tmp_degree([name]) values('MSM')
        insert into @tmp_degree([name]) values('MSN')
        insert into @tmp_degree([name]) values('MVSC')
        insert into @tmp_degree([name]) values('DCM')
        insert into @tmp_degree([name]) values('DDS')
        insert into @tmp_degree([name]) values('DMD')
        insert into @tmp_degree([name]) values('DMS')
        insert into @tmp_degree([name]) values('DNS')
        insert into @tmp_degree([name]) values('DS')
        insert into @tmp_degree([name]) values('DO')
        insert into @tmp_degree([name]) values('DPT')
        insert into @tmp_degree([name]) values('DSN')
        insert into @tmp_degree([name]) values('DScPT')
        insert into @tmp_degree([name]) values('DSS')
        insert into @tmp_degree([name]) values('DSW')
        insert into @tmp_degree([name]) values('DVM')
        insert into @tmp_degree([name]) values('MD')
        insert into @tmp_degree([name]) values('OD')
        insert into @tmp_degree([name]) values('PD')
        insert into @tmp_degree([name]) values('BSL')
        insert into @tmp_degree([name]) values('MJ')
        insert into @tmp_degree([name]) values('MSL')
        insert into @tmp_degree([name]) values('DCL')
        insert into @tmp_degree([name]) values('JCD')
        insert into @tmp_degree([name]) values('JD')
        insert into @tmp_degree([name]) values('JSD')
        insert into @tmp_degree([name]) values('SJD')
        insert into @tmp_degree([name]) values('LLD')
        insert into @tmp_degree([name]) values('LScD')
        insert into @tmp_degree([name]) values('BD')
        insert into @tmp_degree([name]) values('BRE')
        insert into @tmp_degree([name]) values('BRS')
        insert into @tmp_degree([name]) values('BTh')
        insert into @tmp_degree([name]) values('BTL')
        insert into @tmp_degree([name]) values('MDiv')
        insert into @tmp_degree([name]) values('MRb')
        insert into @tmp_degree([name]) values('MRE')
        insert into @tmp_degree([name]) values('MSM')
        insert into @tmp_degree([name]) values('MST')
        insert into @tmp_degree([name]) values('STM')
        insert into @tmp_degree([name]) values('MTh')
        insert into @tmp_degree([name]) values('ThM')
        insert into @tmp_degree([name]) values('MTS')
        insert into @tmp_degree([name]) values('DCM')
        insert into @tmp_degree([name]) values('DD')
        insert into @tmp_degree([name]) values('DHL')
        insert into @tmp_degree([name]) values('DHS')
        insert into @tmp_degree([name]) values('DTh')
        insert into @tmp_degree([name]) values('DMM')
        insert into @tmp_degree([name]) values('DMiss')
        insert into @tmp_degree([name]) values('DRE')
        insert into @tmp_degree([name]) values('DSM')
        insert into @tmp_degree([name]) values('DST');

        with cte_pat
             as (select row_number() over(
                        order by [patients].[id] desc) as [id]
                      , [patients].[id] as                [patients_id]
                 from   [dbo].[patients] as [patients]
                 where  [site_id] = @dev_custom_data_site_id
                        and [middle_name] = 'Chris')
             update [patients] set
                 [name_suffix] = [dg].[name]
               , [withdraw_consent] = case
                                          when [patients].[id] % 2 = 0
                                              then 1
                                          else 0
                                      end
               , [name_alert] = case
                                    when [patients].[id] % 3 = 0
                                        then 1
                                    else 0
                                end
               , [custom_number] = right(replace([vs_systolic] + [vs_diastolic] + convert(char(8), [date_of_birth], 112) + [vs_pulse] + [vs_pain_scale] + [vs_respiratory], ' ', ''), 9)
               , [person_number] = 'HL7_' + [medical_record_number]
             from   [dbo].[patients]
                    inner join [cte_pat] [target] on [target].[patients_id] = [patients].[id]
                    inner join @tmp_degree [dg] on [target].[id] = [dg].[id]
             where  [patients].[site_id] = @dev_custom_data_site_id;

    end;