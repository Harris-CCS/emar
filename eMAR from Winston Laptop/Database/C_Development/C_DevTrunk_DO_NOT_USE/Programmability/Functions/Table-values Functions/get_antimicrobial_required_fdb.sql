print 'create function [dbo].[get_antimicrobial_required_fdb];';

drop function if exists [dbo].[get_antimicrobial_required_fdb];
/*
In visual studio, functions with external references appear to be treated different than procedures with external references
Added check to make sure fdb database exists before attempting to compile the function
*/
set @template = N'
create function [dbo].[get_antimicrobial_required_fdb]
(
      @site_id       int
    , @medication_id int
) returns table as return
(
    with cte_ndc
        as (select [ndc].[ndc]
            from   [dbo].[medications] as [m]
            inner join [dbo].[medication_details] as [md]
                    on [md].[medication_id] = [m].[id]
            inner join [dbo].[fdb_ndc_info] as [ndc]
                    on [md].[drug_id] = [ndc].[medid_string]
            where  [m].[id] = @medication_id
                    and [m].[drug_vendor] = ''F''),
        cte_fdb_list
        as (select distinct 
            --        ei1.ETC_ULTIMATE_PARENT_ETC_ID AS cat,
            --        ei2.ETC_NAME AS name,
                    [ei1].[ETC_ID] as [sub_cat]
            --        ei1.ETC_NAME as sub_cat_name
            from   [fdb].[dbo].[RETCNDC0_ETC_NDC] as [en]
            inner join [fdb].[dbo].[RETCTBL0_ETC_ID] as [ei1]
                    on [ei1].[ETC_ID] = [en].[ETC_ID]
            inner join [fdb].[dbo].[RETCTBL0_ETC_ID] as [ei2]
                    on [ei2].[ETC_ID] = [ei1].[ETC_ULTIMATE_PARENT_ETC_ID]
            inner join [cte_ndc] as [fni]
                    on [fni].[ndc] = [en].[NDC]
            where  [en].[ETC_COMMON_USE_IND] = ''1'')
        select case
                    when [mil].[sub_category] is null
                        then cast(0 as bit)
                    else cast(1 as bit)
                end as [antimicrobial_required]
        from   [cte_fdb_list] as [grp]
        left outer join [dbo].[antimicrobial_indication_items] as [mil]
                on [grp].[sub_cat] = [mil].[sub_category]
                    and [mil].[site_id] = @site_id
);
';

if exists(select null from sys.databases where name='fdb')
    begin
        set @sql_cmd = @template;

        execute [dbo].[sp_executesql] @statement = @sql_cmd;

        /***************
         Data Dictionary
            Function
        ***************/

        execute [sys].[sp_addextendedproperty] 
            @name = N'MS_Description'
          , @value = N'Function to determine if antimicrobial reason isrequired. RETURNS 1=True 0=False'
          , @level0type = N'SCHEMA'
          , @level0name = N'dbo'
          , @level1type = N'FUNCTION'
          , @level1name = N'get_antimicrobial_required_fdb';
    end;
