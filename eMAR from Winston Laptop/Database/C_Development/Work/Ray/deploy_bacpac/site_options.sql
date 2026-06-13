/**************************************************
Default Values for Site Option: DATABASESERVER_ROOT

  FORCE UPDATE of site_option to new value

**************************************************/

declare 
    @DATABASESERVER_ROOT varchar(25) = 'new value goes here';

/**********************
update permanent tables
**********************/

update [target] set    
    [option_value] = @DATABASESERVER_ROOT
from   [dbo].[site_options] as [target]
       inner join [dbo].[options] [options] on [target].[option_id] = [options].[id]
where  [options].[name] = 'DATABASESERVER_ROOT';
--and [target].[option_value] = 'VALUES TO CHANGE';

/********
end table
********/