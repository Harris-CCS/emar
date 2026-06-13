create procedure [dbo].[frequency_minutes_build] 
      @max_minutes int = 525600
as
    begin
        with seq([sequence])
             as (select 0
                 union all
                 select [sequence] + 1
                 from   [seq]
                 where  [sequence] <= @max_minutes)
             select [sequence]
             from   [seq] option(maxrecursion 0);
    end;

go

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'Procedure used to build a sequence of numbers (minutes), to be used in schedule calculation / generation'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'PROCEDURE'
  , @level1name = N'frequency_minutes_build';
go