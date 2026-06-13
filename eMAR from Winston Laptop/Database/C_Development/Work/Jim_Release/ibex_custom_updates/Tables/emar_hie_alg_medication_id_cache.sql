print 'create table [ibex].[dbo].[emar_hie_alg_medication_id_cache];'

set @template = N'
if not exists
(
    select null
    from   [sys].[objects]
    where  object_id = object_id(N''[emar_hie_alg_medication_id_cache]'')
           and [type] in(N''U'')
)
    begin
        CREATE TABLE [dbo].[emar_hie_alg_medication_id_cache]
        (
            [num] int NOT NULL PRIMARY KEY CLUSTERED
            ,[medication_id] int NOT NULL DEFAULT (0)
            ,[match] nvarchar(255) NULL
            ,FOREIGN KEY ([num]) REFERENCES hie_alg ([num]) ON DELETE CASCADE
        )
    end;
';

set @sql_cmd = @template;

execute [ibex].[dbo].[sp_executesql] 
    @statement = @sql_cmd;
/*
EXEC sys.sp_addextendedproperty @name=N'MS_Description'
                               ,@value=N'Cache table to store the calculated [medication_id] and [source] for [hie_alg] records so it doesn''t have to be recalculated repeatedly'
			                   ,@level0type=N'SCHEMA'
                               ,@level0name=N'dbo'
			                   ,@level1type=N'TABLE'
                               ,@level1name=N'emar_hie_alg_medication_id_cache';

EXEC sys.sp_addextendedproperty @name=N'MS_Description'
                               ,@value=N'Primary Key, and foreign key to the ibex..hie_alg table' 
			                   ,@level0type=N'SCHEMA'
                               ,@level0name=N'dbo'
			                   ,@level1type=N'TABLE'
                               ,@level1name=N'emar_hie_alg_medication_id_cache'
			                   ,@level2type=N'COLUMN'
                               ,@level2name=N'num';

EXEC sys.sp_addextendedproperty @name=N'MS_Description'
                               ,@value=N'Calculated medication_id (defaults to "0" if no match can be made on the [ndc], [drug_id], [name] combination)' 
			                   ,@level0type=N'SCHEMA'
                               ,@level0name=N'dbo'
			                   ,@level1type=N'TABLE'
                               ,@level1name=N'emar_hie_alg_medication_id_cache'
			                   ,@level2type=N'COLUMN'
                               ,@level2name=N'medication_id';

EXEC sys.sp_addextendedproperty @name=N'MS_Description'
                               ,@value=N'Method used to find the calculated medication_id (defaults to NULL if no match can be made on the [ndc], [drug_id], [name] combination)' 
			                   ,@level0type=N'SCHEMA'
                               ,@level0name=N'dbo'
			                   ,@level1type=N'TABLE'
                               ,@level1name=N'emar_hie_alg_medication_id_cache'
			                   ,@level2type=N'COLUMN'
                               ,@level2name=N'match';
*/