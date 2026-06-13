create function [dbo].[ibex_date_to_offset_date]
	(
		  @ibex_date varchar(14)
		, @timezone sysname
	)
returns datetimeoffset(7)
as
begin
	
return 
	case 
		when isnull(nullif(rtrim(ltrim(@ibex_date)),''),'')='' 
		        then null 
		when len(isnull(nullif(rtrim(ltrim(@ibex_date)),''),''))<8 
		        then null 
		when len(isnull(nullif(rtrim(ltrim(@ibex_date)),''),''))=8 
				then cast((left(@ibex_date,8)+' 00:00:00') as datetime) at time zone @timezone
		when charindex(' ',rtrim(ltrim(@ibex_date)))>0 
				then
				case 
					when isdate(left(@ibex_date,charindex(' ',rtrim(ltrim(@ibex_date)))))=0
							then null
					else cast(left(@ibex_date,charindex(' ',rtrim(ltrim(@ibex_date)))) as datetime) at time zone @timezone
				end
		else cast(left(@ibex_date,8)+' '+ stuff(stuff(substring(@ibex_date+'000000',9,6),5,0,':'),3,0,':') as datetime) at time zone @timezone
	end
end
go
/***************
 Data Dictionary
    Function
***************/

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'This function converts IBEX date format to datetimeoffset format
IBEX date format is a 12 to 14 digit string thea represents the date and time concatonated with no special characters'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'FUNCTION'
  , @level1name = N'ibex_date_to_offset_date';
go

