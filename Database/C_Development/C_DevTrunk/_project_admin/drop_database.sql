use [master];
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
    where  [name] = 'emar_bacpac2'
)
    begin
        alter database [emar_bacpac2] set single_user with rollback immediate;
    end;
go

drop database if exists [emar_bacpac2];
go

if exists
(
    select null
    from   [sys].[databases]
    where  [name] = 'emar_bacpac_final'
)
    begin
        alter database [emar_bacpac_final] set single_user with rollback immediate;
    end;
go

drop database if exists [emar_bacpac_final];
go

if exists
(
    select null
    from   [sys].[databases]
    where  [name] = 'emar_dacpac_live'
)
    begin
        alter database [emar_dacpac_live] set single_user with rollback immediate;
    end;
go

drop database if exists [emar_dacpac_live];
go

if exists
(
    select null
    from   [sys].[databases]
    where  [name] = 'emar_dacpac_sample'
)
    begin
        alter database [emar_dacpac_sample] set single_user with rollback immediate;
    end;
go

drop database if exists [emar_dacpac_sample];
go