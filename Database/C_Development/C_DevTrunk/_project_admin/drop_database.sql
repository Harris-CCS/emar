use [master];
go

if exists
(
    select null
    from   [sys].[databases]
    where  [name] = 'emar_bacpac'
)
    begin
        alter database [emar_bacpac] set single_user with rollback immediate;
    end;
go

drop database if exists [emar_bacpac];
go

if exists
(
    select null
    from   [sys].[databases]
    where  [name] = 'emar_clean'
)
    begin
        alter database [emar_clean] set single_user with rollback immediate;
    end;
go

drop database if exists [emar_clean];
go