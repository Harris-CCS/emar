print 'Loading Table: frequency_calendar';

/*******************************************************************
Only load if there is no data in the table
The table can be rebuilt but only when idendified as a requirement. 
    I.E. To be done as needed, but not automatically

        begin loading permanent tables
*******************************************************************/

if
(
    select count(*)
    from   [dbo].[frequency_calendar]
) = 0
    begin

        insert into [dbo].[frequency_calendar]
        execute [dbo].[frequency_calendar_build];
    end;

/****************
        end table
****************/
