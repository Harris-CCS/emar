use [master];
go
if exists
(
    select null
    from   [sys].[databases]
    where  [name] = 'fdb'
)
    begin
        alter database [fdb] set single_user with rollback immediate;
        alter database [fdb] modify name = [fdbCopy];
        alter database [fdbCopy] set multi_user;
    end;
go

if exists
(
    select null
    from   [sys].[databases]
    where  [name] = 'ibex'
)
    begin
        alter database [ibex] set single_user with rollback immediate;
        alter database [ibex] modify name = [ibexCopy];
        alter database [ibexCopy] set multi_user;
    end;
go