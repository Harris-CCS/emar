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
   or ('$(load_data)' = 'live'
       and exists
(
    select null
    from   [master].[sys].[databases]
    where  [name] = 'ibex'
))
    begin
        :r ..\Scripts\Data-Loader\delete_emar_data.sql
    end;