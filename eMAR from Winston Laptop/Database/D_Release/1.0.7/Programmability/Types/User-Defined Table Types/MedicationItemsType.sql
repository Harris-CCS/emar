CREATE TYPE [dbo].[MedicationItemsType] AS TABLE(
	[ndc] [varchar](11) NULL,
	[drug_id] [varchar](32) NULL,
	[name] [nvarchar](255) NULL
)
GO