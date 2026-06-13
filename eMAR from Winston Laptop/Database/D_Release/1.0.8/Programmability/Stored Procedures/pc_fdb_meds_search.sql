create procedure [dbo].[pc_fdb_meds_search] 
      @med_name   varchar(max) = null
    , @drug_ids   varchar(max) = null
    , @limit      int          = null
    , @erx_search int          = 0
as
    begin
        set nocount on;

        declare 
            @RC1 int;
        declare 
            @med_name_escaped varchar(max);

        -- Temp table. If the rowcount of the first query is less than @limit, return the second query instead.
        create table [#meds]
            (
              [name] varchar(70) null
            , [ct]   int null);

        -- This first query selects meds based on the brand name.
        -- Choose the correct query to run
        if @med_name is not null
           and @drug_ids is null
            begin
                select @med_name_escaped = [escaped]
                from   [dbo].[escape_sql]
                    (@med_name);

                insert into [#meds]
                select distinct top (@limit) [brand_name] as                                       [name]
                                           , (charindex(@med_name, [brand_name]) - 1000) % 1000 as [ct]
                from                         [fdb_brand_name] as [fbn]
                                             left join [fdb_ndc_info] as [fni] on [fni].[medid] = [fbn].[MEDID]
                                                                                  and [fni].[ndc] = [fni].[base_ndc]
                where                        [brand_name] like '%' + @med_name_escaped + '%' escape '\'   -- '
                                             and (@erx_search = 0
                                                  or ([erx_search] = 1
                                                      and [repackaged] = 0))
                order by [ct]
                       , [brand_name];

                -- Get the rowcount for comparison.
                set @RC1 = @@ROWCOUNT;
                -- If the first query on brand name didn't return @limit rows,
                -- then fill out the remainder with rows with a matching active ingredient.
                if(@RC1 < @limit)
                    begin
                        -- Update the limit with the remaining rowcount after getting the first query.
                        set @limit = @limit - @rc1;
                        -- This query selects meds based on the active ingredient.
                        insert into [#meds]
                        select distinct top (@limit) [brand_name] as [name]
                                                   , null as         [ct]
                        from                         [dbo].[fdb_brand_name] as [fbn]
                                                     left join [fdb]..[RMIID1_MED] on [RMIID1_MED].[MEDID] = [fbn].[MEDID]
                                                     left join [fdb]..[RGCNSEQ4_GCNSEQNO_MSTR] on [RGCNSEQ4_GCNSEQNO_MSTR].[GCN_SEQNO] = [RMIID1_MED].[GCN_SEQNO]
                                                     left join [fdb]..[RHICL1_HIC_HICLSEQNO_LINK] on [RHICL1_HIC_HICLSEQNO_LINK].[HICL_SEQNO] = [RGCNSEQ4_GCNSEQNO_MSTR].[HICL_SEQNO]
                                                     left join [fdb]..[RHICD5_HIC_DESC] on [RHICD5_HIC_DESC].[HIC_SEQN] = [RHICL1_HIC_HICLSEQNO_LINK].[HIC_SEQN]
                                                     left join [fdb_ndc_info] as [fni] on [fni].[medid] = [fbn].[MEDID]
                                                                                          and [fni].[ndc] = [fni].[base_ndc]
                        where                        [RHICD5_HIC_DESC].[HIC_DESC] like @med_name_escaped + '%' escape '\'   -- '
                                                     and not [brand_name] in
                        (
                            select [name]
                            from   [#meds]
                        )
                                                     and (@erx_search = 0
                                                          or ([erx_search] = 1
                                                              and [repackaged] = 0))
                        order by [fbn].[brand_name];
                    end;
                select *
                from   [#meds];
            end

                -- Query for searching by drug ids;
            else
            begin
                if @drug_ids is not null
                    begin
                        -- Populate temp table with drug ids
                        select d.Item id
                        into [#drug_ids]
                        from   [dbo].[delimited_split_8k](@drug_ids, ',') d;

                        -- Get drugs with matching drug ids
                        select distinct top (@limit) [brand_name] as                                       [name]
                                                   , (charindex(@med_name, [brand_name]) - 1000) % 1000 as [ct]
                        from                         [fdb_brand_name] as [f]
                                                     join [#drug_ids] as [d] on [f].[PC_ROUTED_GEN_ID] = [d].[id]
                        order by [ct]
                               , [brand_name];
                    end;
            end;
    end;


go

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'Pulsecheck procdure used to search medications'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'PROCEDURE'
  , @level1name = N'pc_fdb_meds_search';
go
