create function [dbo].[get_internal_id]
    (
          @site_id     [int]
        , @vendor      [varchar](50)
        , @entity      [varchar](50)
        , @external_id [varchar](50)
    )
returns table
as
return
    (
        select [internal_id] [id]
        from [dbo].[external_ids]
        where [vendor] = @vendor
          and [entity] = @entity
          and [site_id] = @site_id
          and [external_id] = @external_id
    );
go
-- Data Dictionary
--    Procedure
execute [sys].[sp_addextendedproperty]
    @name = N'MS_Description'
  , @value = N'Procedure to get the database Internal ID from the Vendor External id'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'FUNCTION'
  , @level1name = N'get_internal_id';
go
