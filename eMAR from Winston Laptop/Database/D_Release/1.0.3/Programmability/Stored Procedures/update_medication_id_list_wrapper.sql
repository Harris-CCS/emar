CREATE   procedure [dbo].[update_medication_id_list_wrapper]
	@MedicationItems MedicationItemsType READONLY
as
    begin
		-- Wrapper procedure around update_medication_id_list, which allows us to pass in a MedicationItemsType table
		-- from the API, update the match information for the drugs provided in that table, and send back the results.
        set nocount on;

		IF OBJECT_ID('tempdb..#medication_items') IS NOT NULL DROP TABLE #medication_items;
		CREATE TABLE #medication_items
		(
			medication_id int not null default(0),
			site_id int not null default(-1),
			ndc varchar(11),
			drug_id varchar(32),
			brand_name nvarchar(255),
			match nvarchar(255)
		);

		INSERT INTO #medication_items (ndc, drug_id, brand_name) SELECT ndc, drug_id, name FROM @MedicationItems;

		EXEC update_medication_id_list;

		SELECT * FROM #medication_items;

		DROP TABLE #medication_items;
	end
GO