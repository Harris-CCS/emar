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