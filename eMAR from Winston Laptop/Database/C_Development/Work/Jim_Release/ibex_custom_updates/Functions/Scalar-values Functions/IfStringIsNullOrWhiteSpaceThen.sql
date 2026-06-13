print 'create function [dbo].[IfStringIsNullOrWhiteSpaceThen];';

/*
In visual studio, functions with external references appear to be treated different than procedures with external references
*/
set @template = N'
CREATE OR ALTER FUNCTION [dbo].[IfStringIsNullOrWhiteSpaceThen](@string varchar(200), @default varchar(10))
RETURNS varchar(200)
AS
BEGIN
	RETURN 
		CASE ISNULL(LTRIM(@string), '''') 
			WHEN ''''
				THEN @default
			ELSE @string
		END
END
';

set @sql_cmd = @template;

execute [ibex].[dbo].[sp_executesql] @statement = @sql_cmd;

/***************
 Data Dictionary
    Function
***************/
/*
execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'Function to determine if the string is a null or whitespace. RETURNS default=null or whitespace..string=not null or whitespace'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'FUNCTION'
  , @level1name = N'IfStringIsNullOrWhiteSpaceThen';
*/