create function [dbo].[escape_sql]
(
	@value varchar(max)
)
returns table 
as
return 
    (
        select
            replace
            (
                replace
                (
                    replace
                    (
                        replace
                        (
                            replace
                            (
                                @value, '%', '\%'
                            ), '_', '\_'
                        ), '^', '\^'
                    ), '[', '\['
                ), ']', '\]'
            ) as escaped
    );
go

-- Data Dictionary
--    Procedure

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'Procedure used for medication search to escape control characters in the like statement.'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'FUNCTION'
  , @level1name = N'escape_sql';
go