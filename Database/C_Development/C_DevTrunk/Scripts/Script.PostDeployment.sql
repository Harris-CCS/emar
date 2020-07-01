/*************************************************************************************
Post-Deployment Script Template
--------------------------------------------------------------------------------------
 This file contains SQL statements that will be appended to the build script.
 Use SQLCMD syntax to include a file in the post-deployment script.
 Example:      :r .\myfile.sql
 Use SQLCMD syntax to reference a variable in the post-deployment script.
 Example:      :setvar TableName MyTable
               SELECT * FROM [$(TableName)]
--------------------------------------------------------------------------------------
*************************************************************************************/

--- variables global to all diagram_ published scripts
declare
    @diagram_id      [int]
  , @version_current [int]
  , @version         [int]
  , @continue_update [bit];

declare @outputs table([Id] int not null);

--- deploying these diagrams in having an issue at the moment.
--- it worked several times, but now causes ssms to crash. so removing for the moment.
---:r ..\Scripts\Post-Deployment\diagram_patients.sql
---:r ..\Scripts\Post-Deployment\diagram_security.sql
