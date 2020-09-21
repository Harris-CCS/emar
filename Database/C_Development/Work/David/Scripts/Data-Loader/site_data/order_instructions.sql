print 'Loading Table: order_instructions';

drop table if exists [#order_instructions];

create table [#order_instructions]
    (
      [site_id]     [varchar](25) not null
    , [description] [varchar](80) not null
    , [is_active]   [bit] not null);

if '$(load_data)' = 'sample'
   or ('$(load_data)' = 'live'
       and @does_ibex_exist = 1)
    begin

        insert into [#order_instructions]
            ([site_id]
           , [description]
           , [is_active]
            )
        execute ('execute dbo.export_ibex_order_instructions');
    end;

if
(
    select count(*)
    from   [#order_instructions]
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

        -- set identity_insert [dbo].[order_instructions] on;

        insert into [dbo].[order_instructions]
            ([site_id]
           , [description]
           , [is_active]
            )
        select isnull([internal_site].[id], -1) as [site_id]
             , [source].[description]
             , [source].[is_active]
        from   [#order_instructions] as [source]
               outer apply [dbo].[get_internal_id]('pulsecheck', 'sites', [source].[site_id]) as [internal_site];

        -- set identity_insert [dbo].[order_instructions] off;

/***************************************
        loading [external_ids] reference
***************************************/
/****************
        end table
****************/

        commit transaction;
    end;

drop table if exists [#order_instructions];