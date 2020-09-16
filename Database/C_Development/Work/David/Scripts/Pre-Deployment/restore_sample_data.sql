if exists
(
    select null
    from   [sys].[databases]
    where  [name] = 'ibex_sample'
)
    begin
        alter database [ibex_sample] set single_user with rollback immediate;
    end;

drop database if exists [ibex_sample];

print '~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~'
print 'Data Load Type: $(load_data)'
print '~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~'
if '$(load_data)' = 'live'
   or '$(load_data)' = 'sample'
   or '$(load_data)' = 'none'
    begin

        restore database [ibex_sample] from 
          disk = N'$(current_path)Scripts\Data-Loader\sample_data\ibex_sample.bak' 
          with file = 1, nounload, REPLACE, stats = 7;
    end;