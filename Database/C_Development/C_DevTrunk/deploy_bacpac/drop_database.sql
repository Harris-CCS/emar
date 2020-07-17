use [master];
alter database [emar] set single_user with rollback immediate;
drop database [emar];