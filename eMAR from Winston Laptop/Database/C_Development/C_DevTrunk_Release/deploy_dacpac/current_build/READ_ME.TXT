This folder is used to contain pre-deployment scripts executed outside of the visual studio update process.
Executing these scripts are meant to deploy schema changes that would result in VS error message: data loss errors.
By matching the database schema before VS Deployment these errors are avoided.

Update scripts need to be written in a format where they are rerunnable.
That is done by performing:
   1. Condition Test
   2. Alter \ Update Schema when required

These scripts should be named and numbered in the sequence order to be executed:
  Example: patch.00001.sql
  Example: patch.00002.sql
  Example: patch.00003.sql

Each script will have a folder named:
  Example: patch_00001
  Example: patch_00002
  Example: patch_00003

The patch_nnnnn folder can contain multiple scripts to be run by a single patch.nnnnn.sql script

The DOS cmd file: all_patches.cmd
Will be setup to run all individual patch.nnnnn.sql scripts