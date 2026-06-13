print 'Loading Table: site_options';

drop table if exists [#site_options2];

create table [#site_options2]
    (
      [site_id]          [varchar](25) null
    , [option_id]    [varchar](25) not null
    , [option_value] [varchar](255) not null);

if '$(load_data)' = 'sample'
   or ('$(load_data)' = 'live'
       and @does_ibex_exist = 1)
    begin

        insert into [#site_options2]
            ([site_id]
           , [option_id]
           , [option_value]
            )
        execute ('execute dbo.export_ibex_site_options');
    end;

if(
  select count(*)
  from [#site_options2]) > 0
    begin

        begin transaction;

/*************************************
        begin loading permanent tables
*************************************/

        with cte_site_options
             as (select [internal_site].[id] as [site_id]
                      , [source].[option_id]
                      , [source].[option_value]
                 from   [#site_options2] as [source]
                        outer apply [dbo].[get_internal_id]
                     ('pulsecheck', 'sites', [source].[site_id]) as [internal_site]
                 where  [source].[site_id] > 0)
             update [target] set    
                 [option_value] = [source].[option_value]
             from   [cte_site_options] as [source]
                    inner join [dbo].[options] as [per] on [per].[name] = [source].[option_id]
                    inner join [dbo].[site_options] as [target] on [target].[site_id] = [source].[site_id]
                    and [target].[option_id] = [per].[id]
             where  [target].[option_value] <> [source].[option_value];
select @@ROWCOUNT

/****************
        end table
****************/

        commit transaction;
    end;

drop table if exists [#site_options2];