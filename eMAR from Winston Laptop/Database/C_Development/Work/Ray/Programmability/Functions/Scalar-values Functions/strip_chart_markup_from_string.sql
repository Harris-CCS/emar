CREATE function [dbo].[strip_chart_markup_from_string]
	(
		@str varchar(max),
		@forComparison BIT = 0
	)
returns varchar(max)
as
begin

	IF (LEN(ISNULL(@str, '')) = 0)
		RETURN @str;

	IF (ISNUMERIC(@str) = 1)
		RETURN @str;
	
	SET @str = REPLACE(@str, '<LF>', '');

	-- This is only done for Emerus where a template changed. Probably shouldn't be used in other cases.
	SET @str = REPLACE(@str, '^CMedication infusion discontinued, on', '^Con');


	-- All sorts of strip/replace
	SET @str =
		replace(
			replace(
				replace(
					replace(
						replace(
							replace(
								replace(
									replace(
										replace(
											replace(
												replace(
													@str, '&^S^', '&'
												),
												'^U', '^'
											),
											'&^s=', '&'
										),
										'&^S', '&'
									),
									'^S^', ''
								),
								's=', ''
							),
							'<LT>', '<'
						),
						'.', ''
					),
					'<B>', ''
				),
				'</B>', ''
			),
			'&&', '&'
		);

	-- Remove possible weird leading characters
	WHILE (LEFT(@str, 1) = '&' AND LEN(@str) > 2)
	BEGIN
		SET @str = SUBSTRING(@str, 2, LEN(@str) - 1);
	END

	WHILE (LEFT(@str, 1) = '=' AND LEN(@str) > 2)
	BEGIN
		SET @str = SUBSTRING(@str, 2, LEN(@str) - 1);
	END

	WHILE (LEFT(@str, 1) = '^' AND LEN(@str) > 2)
	BEGIN
		SET @str = SUBSTRING(@str, 2, LEN(@str) - 1);
	END

	SET @str = REPLACE(@str, '^^^^', '');

	-- Remove QX codes...
	DECLARE @patPos INT = PATINDEX('%QX[0-9]%', @str);
	WHILE(@patPos > 0)
	BEGIN
		DECLARE @numLength INT = 1;
		DECLARE @numStart INT = @patPos + 2;
		WHILE(ISNUMERIC(SUBSTRING(@str, @numStart, @numLength)) = 1 AND (@numStart + @numLength - 1) <= LEN(@str)) 
		BEGIN
			SET @numLength = @numLength + 1;
		END

		SET @str = SUBSTRING(@str, 1, @patPos - 1) + SUBSTRING(@str, @patPos + @numLength + 1, LEN(@str) - (@patPos + @numLength));
	
		SET @patPos = PATINDEX('%QX[0-9]%', @str);
	END

	-- Any ^^\w+& strings should be removed...
	SET @patPos = PATINDEX('%^^[a-z ]%', @str);
	WHILE(@patPos > 0)
	BEGIN
		DECLARE @removeLength INT = 1;
		WHILE(SUBSTRING(@str, @patPos + 2 + @removeLength, 1) <> '&' AND SUBSTRING(@str, @patPos + 2 + @removeLength, 1) <> '=' AND (@patPos + @removeLength) <= LEN(@str)) 
		BEGIN
			SET @removeLength = @removeLength + 1;
		END

		SET @str = SUBSTRING(@str, 1, @patPos - 1) + SUBSTRING(@str, @patPos + @removeLength + 2, LEN(@str) - (@patPos + @removeLength - 1));
	
		SET @patPos = PATINDEX('%^^[a-z ]%', @str);
	END
	
	-- Remove leftover types
	SET @str = REPLACE(@str, '^C', '^');
	SET @str = REPLACE(@str, '^^', '');
	SET @str = REPLACE(@str, '^&', '&');
	SET @str = REPLACE(@str, '&^C', '&');
	SET @str = REPLACE(@str, '&^D=', '&');
	SET @str = REPLACE(@str, '&D=', '&');
	SET @str = REPLACE(@str, 'C=', '');
	SET @str = REPLACE(@str, ':=', '=');
	SET @str = REPLACE(@str, '<AMP>', '&');

	IF (@forComparison = 1)
	BEGIN
		-- Split apart the string, sort the fields into alphabetical order, and put them back together for comparison purposes
		DECLARE @data VARCHAR(8000);
		SELECT TOP 1000
			@data = COALESCE(@data + '&', '') + LTRIM(RTRIM(REPLACE(ISNULL(Item, ''), '^', ' ')))
		FROM [dbo].[delimited_split_8k](@str, '&')
		ORDER BY Item;

		SET @str = @data;
		SET @str = REPLACE(@str, ' ', '');
	END

	SET @str = REPLACE(@str, '&&', '&');
	SET @str = REPLACE(@str, '=&', '&');
	SET @str = REPLACE(@str, '  ', ' ');
	SET @str = LTRIM(RTRIM(@str));

	IF (RIGHT(@str, 1) = '=')
		SET @str = SUBSTRING(@str, 1, LEN(@str) - 1);

	return @str;
end;
go
/***************
 Data Dictionary
    Function
***************/

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'This function strips chart markup from a given string'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'FUNCTION'
  , @level1name = N'strip_chart_markup_from_string';
go