use [master];
go
if exists(select null from sys.databases where name='emar_bacpac')
alter database [emar_bacpac] set single_user with rollback immediate;
go
drop database if exists [emar_bacpac];
go
if exists(select null from sys.databases where name='emar_clean')
alter database [emar_clean] set single_user with rollback immediate;
go
drop database if exists [emar_clean];
go






/*

USE master;
GO
ALTER DATABASE [fdb] SET SINGLE_USER WITH ROLLBACK IMMEDIATE
GO
ALTER DATABASE [fdb] MODIFY NAME = [fdbCopy] ;
GO
ALTER DATABASE [fdbCopy] SET MULTI_USER
GO

USE master;
GO
ALTER DATABASE [fdbCopy] SET SINGLE_USER WITH ROLLBACK IMMEDIATE
GO
ALTER DATABASE [fdbCopy] MODIFY NAME = [fdb] ;
GO
ALTER DATABASE [fdb] SET MULTI_USER
GO






USE master;
GO
ALTER DATABASE [ibex] SET SINGLE_USER WITH ROLLBACK IMMEDIATE
GO
ALTER DATABASE [ibex] MODIFY NAME = [ibexCopy] ;
GO
ALTER DATABASE [ibexCopy] SET MULTI_USER
GO

USE master;
GO
ALTER DATABASE [ibexCopy] SET SINGLE_USER WITH ROLLBACK IMMEDIATE
GO
ALTER DATABASE [ibexCopy] MODIFY NAME = [ibex] ;
GO
ALTER DATABASE [ibex] SET MULTI_USER
GO



*/
