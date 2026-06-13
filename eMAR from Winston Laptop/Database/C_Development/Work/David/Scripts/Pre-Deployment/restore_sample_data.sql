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

declare 
    @default_data_path sysname = '$(DefaultDataPath)'
  , @default_log_path sysname  = '$(DefaultLogPath)';

set @default_data_path = @default_data_path + 'ibex_sample.mdf';
set @default_log_path = @default_log_path + 'ibex_sample_log.ldf';

print '~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~';
print 'Data Load Type: $(load_data)';
print @default_data_path
print @default_log_path
print '~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~';

if '$(load_data)' = 'sample'
    begin

        restore database [ibex_sample] 
            from disk = N'$(current_path)Scripts\Data-Loader\sample_data\ibex_sample.bak' 
            with file = 1
            , move N'ibex_sample' to @default_data_path
            , move N'ibex_sample_log' to @default_log_path
            , nounload
            , replace
            , stats = 7;
    end;


    
