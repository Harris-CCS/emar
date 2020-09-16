/*************************************************************************************
 Pre-Deployment Script Template							
--------------------------------------------------------------------------------------
 This file contains SQL statements that will be executed before the build script.	
 Use SQLCMD syntax to include a file in the pre-deployment script.			
 Example:      :r .\myfile.sql								
 Use SQLCMD syntax to reference a variable in the pre-deployment script.		
 Example:      :setvar TableName MyTable							
               SELECT * FROM [$(TableName)]					
--------------------------------------------------------------------------------------
*************************************************************************************/

if '$(load_data)' = 'sample'
    begin
        :r ..\Scripts\Pre-Deployment\restore_sample_data.sql
    end;

declare @does_ibex_exist bit = 0;
select @does_ibex_exist = 1
from   [master].[sys].[databases]
where  [name] = 'ibex';

if '$(load_data)' = 'sample'
   or  ('$(load_data)' = 'live'
         and @does_ibex_exist = 1
       )
    begin
        :r ..\Scripts\Data-Loader\delete_emar_data.sql
    end;
