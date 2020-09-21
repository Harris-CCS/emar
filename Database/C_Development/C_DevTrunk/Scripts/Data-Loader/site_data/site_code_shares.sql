print 'Loading Table: site_code_shares';

drop table if exists [#site_code_shares];

create table [#site_code_shares]
    (
      [source_site_id] [int] not null
    , [target_site_id] [int] not null
    , [entity]         sysname not null);

if '$(load_data)' = 'sample'
   or ('$(load_data)' = 'live'
       and @does_ibex_exist = 1)
    begin
        insert into [#site_code_shares]
            ([source_site_id]
           , [target_site_id]
           , [entity]
            )
        execute ('execute dbo.export_ibex_site_code_shares');
    end;

begin transaction;

if
(
    select count(*)
    from   [#site_code_shares]
) > 0
    begin

/****************************************
        load temporary tables for staging
****************************************/
/********************************
        get max id for seed value
********************************/
/*************************************
        begin loading permanent tables
*************************************/

        --set identity_insert [dbo].[site_code_shares] on;

        insert into [dbo].[site_code_shares]
            ([source_site_id]
           , [target_site_id]
           , [entity]
            )
        select isnull([source_site].[id], -1) as [source_site_id]
             , isnull([target_site].[id], -1) as [target_site_id]
             , [source].[entity]
        from   [#site_code_shares] as [source]
               outer apply [dbo].[get_internal_id]
            ('pulsecheck', 'sites', [source].[source_site_id]) as [source_site]
               outer apply [dbo].[get_internal_id]
            ('pulsecheck', 'sites', [source].[target_site_id]) as [target_site];

        --set identity_insert [dbo].[site_code_shares] off;

/***************************************
        loading [external_ids] reference
***************************************/
/****************
        end table
****************/
    end;

/****************************************************
  set default code share site as source=target
  this ensures every entity has a default association
****************************************************/

with cte_source
     as (select [sites].[id] as        [source_site_id]
              , [sites].[id] as        [target_site_id]
              , [entities].[entity] as [entity]
         from   [dbo].[sites] as [sites]
                cross apply
         (
             select 'medication_units' as [entity]
             union all
             select 'medication_routes' as [entity]
             union all
             select 'frequency_schedules' as [entity]
             union all
             select 'order_instructions' as [entity]
         ) as [entities])
     insert into [dbo].[site_code_shares]
         ([source_site_id]
        , [target_site_id]
        , [entity]
         )
     select [source].[source_site_id]
          , [source].[target_site_id]
          , [source].[entity]
     from   [cte_source] as [source]
            left join [dbo].[site_code_shares] as [target] on [target].[source_site_id] = [source].[source_site_id]
                                                              and [target].[entity] = [source].[entity]
     where  [target].[entity] is null
            and [source].[source_site_id] > 0;

commit transaction;

drop table if exists [#site_code_shares];