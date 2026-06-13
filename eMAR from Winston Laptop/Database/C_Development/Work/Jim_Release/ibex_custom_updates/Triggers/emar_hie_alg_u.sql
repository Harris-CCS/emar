print 'create trigger [ibex].[dbo].[hie_alg].[emar_hie_alg_u];'

set @template = N'
CREATE OR ALTER TRIGGER emar_hie_alg_u ON dbo.hie_alg
FOR UPDATE
AS

DELETE	c
FROM	dbo.emar_hie_alg_medication_id_cache c
JOIN	inserted i
		ON c.num = i.num
JOIN	deleted d
		ON i.num = d.num
		AND (
				ISNULL(i.ndc, CHAR(0)) != ISNULL(d.ndc, CHAR(0))
				OR ISNULL(i.name, CHAR(0)) != ISNULL(d.name, CHAR(0))
		)
';

set @sql_cmd = @template;

execute [ibex].[dbo].[sp_executesql]
    @statement = @sql_cmd;