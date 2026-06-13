use [master];
go
if exists
(
    select null
    from   [sys].[databases]
    where  [name] = 'fdbCopy'
)
    begin
        alter database [fdbCopy] set single_user with rollback immediate;
        alter database [fdbCopy] modify name = [fdb];
        alter database [fdb] set multi_user;
    end;
go

if exists
(
    select null
    from   [sys].[databases]
    where  [name] = 'ibexCopy'
)
    begin
        alter database [ibexCopy] set single_user with rollback immediate;
        alter database [ibexCopy] modify name = [ibex];
        alter database [ibex] set multi_user;
    end;
go
