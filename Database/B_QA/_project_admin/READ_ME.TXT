/******************************************************************************
*******************************************************************************
*******************************************************************************

WARNING, THIS IS FOR DEVELOPMENT DEPLOYMENT ONLY!!!!
        Do not try this in PRODUCTION

*******************************************************************************
*******************************************************************************
*******************************************************************************/


PURPOSE: This set of directions is how to build the project for check-in/release for TFS.


/********************************************/
Follow these steps to easily add a new table according to foreign key hierarchy
To build the project and prepare a bacpac to deploy with all test data.
These are the scripts to run 
    00_drop_build_databases.cmd
    01_command_line_project_build.cmd
    02_publish_emar_clean.cmd
    03_build_table_hierarchy_result.cmd
    04_edit_project_scripts.txt
    05_command_line_project_build.cmd
    06_publish_emar_clean.cmd
    07_export_emar_bacpac.cmd
    08_publish_emar_clean.cmd
    09_bcp.cmd
    10_extract_emar_dacpac.cmd
    11_import_emar_bacpac.cmd

If no new tables are being added preform script 00 then start at script 05

00 drops databases emar_clean and emar_bacpac
01 compile project checking for errors / there will be hundreds of warnings
02 publish update to emar_clean (no data load)
03 build a sorted list of commands for loading and deleting tables built on foreign key relations
      be sure to have previously added any export_ibex_table_name procedures (if export is required) and corresponding import procedures
      edit script (before running): table_hierarchy_list.sql if new export tables are added
04 use output from script 03 to edit scripts and adjust build order for inserting and deleting data
05 compile project checking for errors / there will be hundreds of warnings
06 publish update to emar_clean (data will be loaded from ibex database) and prepped for bacpac creation (** Need to correct load error... running a second time is a workaround)
07 create the bacpac file
08 publish code only update to emar_clean. This will be the procedures with external references
09 This script will build the sample export data files for a standalone update from the sample data files
10 extract a dacpac file (no data only code)
11 import the bacpac,dacpac to database emar_bacpac and review all changes are installed from deployment

A couple of the sample data bcp files report changes when there are none. (especially user_quick_list_items, the process changes case on the word TAB sometimes) I perform an undo on bcp files that are like that before check-in
