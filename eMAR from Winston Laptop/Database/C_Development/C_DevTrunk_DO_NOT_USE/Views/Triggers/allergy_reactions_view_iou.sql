create or alter trigger [dbo].[allergy_reactions_view_no_update]
on [dbo].[allergy_reactions_view]
instead of update
as
begin
	return
end;
go
execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'Trigger is used to prevent attempts to update allergy_reactions_view, which is not an updatable view.'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'VIEW'
  , @level1name = N'allergy_reactions_view'
  , @level2type = N'TRIGGER'
  , @level2name = N'allergy_reactions_view_no_update';
go
