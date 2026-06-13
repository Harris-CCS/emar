/*************************************
This script loads only FDB Medications

delete from medication_details
delete from medications

select * from fdb_ndc_info
select * from fdb_brand_name
select * from fdb_allergy_name
select * from medications
select * from medication_details
*************************************/

declare 
    @medications_loader_fdb_i        int = 0
  , @medications_loader_fdb_u        int = 0
  , @medications_loader_fdb_d        int = 0
  , @medication_details_loader_fdb_i int = 0
  , @medication_details_loader_fdb_u int = 0
  , @medication_details_loader_fdb_d int = 0;

if '$(load_data)' = 'live'
   or '$(load_data)' = 'sample'
    begin

        drop table if exists [#medication_details];

        drop table if exists [#medications];

        create table [#medication_details]
            (
              [id]                  [int] null
            , [medication_id]       [int] null
            , [drug_id]             [varchar](32) null
            , [brand_name]          [nvarchar](255) null
            , [active_list]         [nvarchar](max) null
            , [dose]                [decimal](11, 2) null
            , [medication_unit_id]  [int] null
            , [medication_route_id] [int] null
            , [is_active]           [bit] not null
            , [drug_vendor]         [char](1) null);

        create table [#medications]
            (
              [id]           [int] null
            , [site_id]      [int] null
            , [drug_id]      [varchar](32) null
            , [display_name] [nvarchar](255) null
            , [drug_vendor]  [char](1) null);

        insert into [#medication_details]
            ([drug_id]
           , [drug_vendor]
           , [is_active]
           , [brand_name]
           , [active_list]
            )
        select [MEDID]
             , 'F'
             , 1
             , [brand_name]
             , [active]
        from   [dbo].[fdb_brand_name];

        insert into [#medications]
            ([site_id]
           , [drug_id]
           , [drug_vendor]
           , [display_name]
            )
        select-1
            , [MEDID]
            , 'F'
            , [long_brand_name]
        from  [dbo].[fdb_brand_name];

/*******************************************************************************
    TESTING LOAD
delete [#medications]        where drug_id= '435896'
delete [#medication_details] where drug_id= '435896'
update [medications] set drug_vendor='U',display_name='diff_vend: '+display_name
*******************************************************************************/

        -----~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        -----~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        -----~~~~~~~ Process: [medications] ~~~~~~~~~~~~~~
        -----~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        -----~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

        update [target] set    
            [display_name] = [source].[display_name]
        from   [#medications] as [source]
               inner join [dbo].[medications] as [target] on [source].[site_id] = [target].[site_id]
                                                             and [source].[drug_id] = [target].[drug_id]
                                                             and [source].[drug_vendor] = [target].[drug_vendor]
        where  [source].[display_name] <> [target].[display_name];

        set @medications_loader_fdb_u = @@rowcount;

/***********************************************************************************************
    No Delete Mode for this table

set @medications_loader_fdb_d = @@rowcount;
***********************************************************************************************/

        insert into [dbo].[medications]
            ([site_id]
           , [display_name]
           , [drug_vendor]
           , [drug_id]
            )
        select [source].[site_id]
             , [source].[display_name]
             , [source].[drug_vendor]
             , [source].[drug_id]
        from   [#medications] as [source]
               left join [dbo].[medications] as [target] on [source].[site_id] = [target].[site_id]
                                                            and [source].[drug_id] = [target].[drug_id]
                                                            and [source].[drug_vendor] = [target].[drug_vendor]
        where  [target].[drug_id] is null;

        set @medications_loader_fdb_i = @@rowcount;

        -----~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        -----~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        -----~~~~~~~ Set: [medication_id] ~~~~~~~~~~~~~~~~
        -----~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        -----~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

        update [target] set    
            [target].[medication_id] = [source].[id]
        from   [dbo].[medications] as [source]
               inner join [#medication_details] as [target] on [target].[drug_id] = [source].[drug_id]
        where  [source].[site_id] = -1
               and [source].[drug_vendor] = 'F';
        -----~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        -----~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        -----~~~~~~~ Process: [medication_details] ~~~~~~~
        -----~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        -----~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

        update [target] set    
            [drug_id] = [source].[drug_id]
          , [brand_name] = [source].[brand_name]
          , [is_active] = [source].[is_active]
        from   [#medication_details] as [source]
               inner join [dbo].[medication_details] as [target] on [source].[medication_id] = [target].[medication_id]
               inner join [dbo].[medications] as [med] on [target].[medication_id] = [med].[id]
        where  [med].[drug_vendor] = 'F'
               and ([source].[drug_id] <> [target].[drug_id]
                    or [source].[brand_name] <> [target].[brand_name]
                    or [source].[is_active] <> [target].[is_active]);

        set @medication_details_loader_fdb_u = @@rowcount;

        update [target] set    
            [is_active] = 0
        from   [#medication_details] as [source]
               right join [dbo].[medication_details] as [target] on [source].[medication_id] = [target].[medication_id]
               inner join [dbo].[medications] as [med] on [target].[medication_id] = [med].[id]
        where  [med].[drug_vendor] = 'F'
               and [source].[drug_id] is null
               and [target].[is_active] <> 0;

        set @medication_details_loader_fdb_d = @@rowcount;

        insert into [dbo].[medication_details]
            ([medication_id]
           , [drug_id]
           , [brand_name]
           , [active_list]
           , [is_active]
            )
        select [source].[medication_id]
             , [source].[drug_id]
             , [source].[brand_name]
             , [source].[active_list]
             , [source].[is_active]
        from   [#medication_details] as [source]
               left join [dbo].[medication_details] as [target] on [source].[medication_id] = [target].[medication_id]
        where  [target].[drug_id] is null;

        set @medication_details_loader_fdb_i = @@rowcount;
        -----~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        -----~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        -----~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        -----~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        -----~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

        select @medications_loader_fdb_i as        [medications_insert]
             , @medications_loader_fdb_u as        [medications_update]
             , @medications_loader_fdb_d as        [medications_delete]
             , @medication_details_loader_fdb_i as [medication_details_insert]
             , @medication_details_loader_fdb_u as [medication_details_update]
             , @medication_details_loader_fdb_d as [medication_details_delete];

/*************************************************************************************
-- queries from above testing
select *
from     [medications]
where   [drug_id] = '435896'
union
select *
from   [#medications]
where  [drug_id] = '435896';

select *
     , '' as [drug_vendor]
from     [medication_details]
where   [drug_id] = '435896'
union
select *
from   [#medication_details]
where  [drug_id] = '435896';

select [m].[id] as  [medication_id]
     , [md].[id] as [medication_detail_id]
     , [m].[drug_vendor]
     , [m].[drug_id]
     , [m].[display_name]
     , [md].[brand_name]
     , [md].[active_list]
     , [md].[is_active]
from   [dbo].[medications] as [m]
       left join [dbo].[medication_details] as [md] on [m].[id] = [md].[medication_id]
where  [m].[drug_id] = '435896';
--
*************************************************************************************/
    end;