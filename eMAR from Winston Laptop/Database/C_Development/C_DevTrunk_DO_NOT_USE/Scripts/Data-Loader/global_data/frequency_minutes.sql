print 'Loading Table: frequency_minutes';

/*******************************************************************
Only load if there is no data in the table
The table can be rebuilt but only when idendified as a requirement. 
    I.E. To be done as needed, but not automatically

        begin loading permanent tables
*******************************************************************/

if
(
    select count(*)
    from   [dbo].[frequency_minutes]
) = 0
    begin

        insert into [dbo].[frequency_minutes]
        execute [dbo].[frequency_minutes_build];
    end;

/****************
        end table
****************/