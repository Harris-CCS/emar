create function [dbo].[get_prompt_sequence_from_template]
(
      @templateId      int
) returns table as return
(
WITH cte_first
AS
(
    SELECT t.id,t.[name],tpg.[sequence],tpg.prompt_group_id FROM templates t 
	JOIN template_prompt_groups tpg ON t.id=tpg.template_id 
)
SELECT p.id AS prompt_id,ROW_NUMBER() OVER(ORDER BY cf.id,cf.[sequence],p.[sequence]) AS row_num FROM prompts p
LEFT OUTER JOIN cte_first cf ON p.prompt_group_id=cf.prompt_group_id
JOIN prompt_groups pg ON cf.prompt_group_id=pg.id
WHERE cf.id=@templateId
);
GO
        /***************
         Data Dictionary
            Function
        ***************/

        execute [sys].[sp_addextendedproperty] 
            @name = N'MS_Description'
          , @value = N'Function to determine the sequential prompt order of a given template.'
          , @level0type = N'SCHEMA'
          , @level0name = N'dbo'
          , @level1type = N'FUNCTION'
          , @level1name = N'get_prompt_sequence_from_template';
GO
