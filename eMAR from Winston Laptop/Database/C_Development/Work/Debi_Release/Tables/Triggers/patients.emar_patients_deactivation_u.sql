create trigger [dbo].[emar_patients_deactivation_u]
on [dbo].[patients]
after update
as
begin

set nocount on;

update [d] set
    [deactivation_datetime] =
        case
            when [i].[is_active] = 1 then null
            else sysdatetimeoffset()
        end
from [inserted] as [i]
    inner join [dbo].[patients] as [d]
        on [i].[id] = [d].[id];

end;
go
execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'Trigger is used to set the patient deactivation_datetime when the bit column is_active is changed'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'TABLE'
  , @level1name = N'patients'
  , @level2type = N'TRIGGER'
  , @level2name = N'emar_patients_deactivation_u';
go
