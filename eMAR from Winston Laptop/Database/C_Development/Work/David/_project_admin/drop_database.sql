use [master];
go

print 'deleteing database: $(emar_base)';

if exists
(
    select null
    from   [sys].[databases]
    where  [name] = '$(emar_base)'
)
    begin
        alter database [$(emar_base)] set single_user with rollback immediate;
    end;
go

drop database if exists [$(emar_base)];
go

print 'deleteing database: $(emar_load)';

if exists
(
    select null
    from   [sys].[databases]
    where  [name] = '$(emar_load)'
)
    begin
        alter database [$(emar_load)] set single_user with rollback immediate;
    end;
go

drop database if exists [$(emar_load)];
go

print 'deleteing database: $(emar_deploy)';

if exists
(
    select null
    from   [sys].[databases]
    where  [name] = '$(emar_deploy)'
)
    begin
        alter database [$(emar_deploy)] set single_user with rollback immediate;
    end;
go

drop database if exists [$(emar_deploy)];
go

print 'deleteing database: $(emar_load)2';

if exists
(
    select null
    from   [sys].[databases]
    where  [name] = '$(emar_load)2'
)
    begin
        alter database [$(emar_load)2] set single_user with rollback immediate;
    end;
go

drop database if exists [$(emar_load)2];
go

print 'deleteing database: $(emar_load)_sample';

if exists
(
    select null
    from   [sys].[databases]
    where  [name] = '$(emar_load)_sample'
)
    begin
        alter database [$(emar_load)_sample] set single_user with rollback immediate;
    end;
go

drop database if exists [$(emar_load)_sample];
go

print 'deleteing database: $(emar_load)_live';

if exists
(
    select null
    from   [sys].[databases]
    where  [name] = '$(emar_load)_live'
)
    begin
        alter database [$(emar_load)_live] set single_user with rollback immediate;
    end;
go

drop database if exists [$(emar_load)_live];
go

print 'deleteing database: $(emar_load)_final';

if exists
(
    select null
    from   [sys].[databases]
    where  [name] = '$(emar_load)_final'
)
    begin
        alter database [$(emar_load)_final] set single_user with rollback immediate;
    end;
go

drop database if exists [$(emar_load)_final];
go