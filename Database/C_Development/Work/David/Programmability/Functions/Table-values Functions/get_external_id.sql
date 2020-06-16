create function [dbo].[get_external_id]
    (
          @site_id     [int]
        , @vendor      [varchar](50)
        , @entity      [varchar](50)
        , @internal_id [bigint]
    )
returns table
as
return
    (
        select [external_id] [id]
        from [dbo].[external_ids]
        where [vendor] = @vendor
          and [entity] = @entity
          and [site_id] = @site_id
          and [internal_id] = @internal_id
    );
go
-- Data Dictionary
--    Procedure
execute [sys].[sp_addextendedproperty]
    @name = N'MS_Description'
  , @value = N'Procedure to get the Vendor External ID from the database Internal id'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'FUNCTION'
  , @level1name = N'get_external_id';
go
