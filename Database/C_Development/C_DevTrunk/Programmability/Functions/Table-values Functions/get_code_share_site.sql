create function [dbo].[get_code_share_site]
    (
      @site_id int
    , @entity  sysname)
returns table
as

/******************************************************************
Designed to always return a site.
If no site or entity is configured, return input parameter:@site_id
******************************************************************/
    return
(
    select    isnull([child].[site_id], [parent].[site_id]) as [site_id]
    from
    (
        select @site_id as [site_id]
    ) as [parent]
    outer apply
    (
        select [cs].[target_site_id] as [site_id]
        from   [dbo].[site_code_shares] as [cs]
        where  [cs].[source_site_id] = @site_id
               and [cs].[entity] = @entity
    ) as [child]
);
go

/***************
 Data Dictionary
    Function
***************/

execute [sys].[sp_addextendedproperty] 
    @name = N'MS_Description'
  , @value = N'Function to get the the code share site ID to use for table lookups.'
  , @level0type = N'SCHEMA'
  , @level0name = N'dbo'
  , @level1type = N'FUNCTION'
  , @level1name = N'get_code_share_site';
go