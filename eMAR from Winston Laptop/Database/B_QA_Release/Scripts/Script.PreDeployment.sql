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
declare 
    @db_build varchar(50);--yymmddhh (purpose to stamp a unique number in the emar version)
select @db_build=CONVERT (VARCHAR(50), GETDATE(), 120);
select @db_build=REPLACE(REPLACE(REPLACE(@db_build, '-', ''), ' ', ''), ':', '');

set @db_build = '$(deploy_version)' + '.' + @db_build;

if not exists
(
    select null
    from   [sys].[tables]
    where  [name] = 'emar_version'
)
    begin
        create table [emar_version]
            (
              [id]              [int] identity(1, 1) not null
            , [version_number]  [varchar](50) not null
            , [update_type]     [varchar](10) not null
            , [update_start]    [datetimeoffset](7) not null
            , [update_complete] [datetimeoffset](7) null);
    end;

insert into [dbo].[emar_version]
    ([version_number]
   , [update_type]
   , [update_start]
   , [update_complete]
    )
values
    (@db_build, 'SQL', sysdatetimeoffset(), null);