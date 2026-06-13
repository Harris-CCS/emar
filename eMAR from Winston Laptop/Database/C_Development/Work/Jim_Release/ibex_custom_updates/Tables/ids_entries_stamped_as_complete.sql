print 'create table [ibex].[dbo].[ids_entries_stamped_as_complete];'

set @template = N'
if not exists
(
    select null
    from   [sys].[objects]
    where  object_id = object_id(N''[ids_entries_stamped_as_complete]'')
           and [type] in(N''U'')
)
    begin
        create table [dbo].[ids_entries_stamped_as_complete](
	        [id] [int] identity(1,1) not null,
	        [timestamp] [datetime] null,
	        [entity] [varchar](50) null,
	        [external_id] [varchar](100) null,
            constraint [PK_ids_entries_stamped_as_complete] primary key clustered ( [id] asc )
        );
    end;
';

set @sql_cmd = @template;

execute [ibex].[dbo].[sp_executesql] 
    @statement = @sql_cmd;
/*
EXEC [ibex].sys.sp_addextendedproperty @name=N'MS_Description'
                               ,@value=N''
			                   ,@level0type=N'SCHEMA'
                               ,@level0name=N'dbo'
			                   ,@level1type=N'TABLE'
                               ,@level1name=N'ids_entries_stamped_as_complete';

EXEC [ibex].sys.sp_addextendedproperty @name=N'MS_Description'
                               ,@value=N'Primary Key' 
			                   ,@level0type=N'SCHEMA'
                               ,@level0name=N'dbo'
			                   ,@level1type=N'TABLE'
                               ,@level1name=N'ids_entries_stamped_as_complete'
			                   ,@level2type=N'COLUMN'
                               ,@level2name=N'id';

EXEC [ibex].sys.sp_addextendedproperty @name=N'MS_Description'
                               ,@value=N'' 
			                   ,@level0type=N'SCHEMA'
                               ,@level0name=N'dbo'
			                   ,@level1type=N'TABLE'
                               ,@level1name=N'ids_entries_stamped_as_complete'
			                   ,@level2type=N'COLUMN'
                               ,@level2name=N'timestamp';

EXEC [ibex].sys.sp_addextendedproperty @name=N'MS_Description'
                               ,@value=N'' 
			                   ,@level0type=N'SCHEMA'
                               ,@level0name=N'dbo'
			                   ,@level1type=N'TABLE'
                               ,@level1name=N'ids_entries_stamped_as_complete'
			                   ,@level2type=N'COLUMN'
                               ,@level2name=N'entity';

EXEC [ibex].sys.sp_addextendedproperty @name=N'MS_Description'
                               ,@value=N'' 
			                   ,@level0type=N'SCHEMA'
                               ,@level0name=N'dbo'
			                   ,@level1type=N'TABLE'
                               ,@level1name=N'ids_entries_stamped_as_complete'
			                   ,@level2type=N'COLUMN'
                               ,@level2name=N'external_id';
*/