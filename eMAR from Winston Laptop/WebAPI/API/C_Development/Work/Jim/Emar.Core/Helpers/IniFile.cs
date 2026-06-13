using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;

namespace Emar.Core.Helpers
{
    public class IniFile   // revision 11
    {
        //This was copied from Donald's ICD stuff.
        //The .ini file is at inetpub/wwwroot/eMARAPI/pharmacy_notification/pharmacy_notifications.ini.
        //The only changes I made were to the constructor.
        //If a path is passed in, then we'll use that.
        //If a path is not passed in, then we'll calculate the path to PharmacyNotificationService\pharmacy_notifications.ini.
        //Winston Murdock, 10/06/2021.

        string Path;
        
        //This will be EMAR.Core.
        //When we access it below, we'll add ".dll" to it.
        string EXE = System.Reflection.Assembly.GetExecutingAssembly().GetName().Name;

        [System.Runtime.InteropServices.DllImport("kernel32", CharSet = System.Runtime.InteropServices.CharSet.Unicode)]
        static extern long WritePrivateProfileString(string Section, string Key, string Value, string FilePath);

        [System.Runtime.InteropServices.DllImport("kernel32", CharSet = System.Runtime.InteropServices.CharSet.Unicode)]
        static extern int GetPrivateProfileString(string Section, string Key, string Default, StringBuilder RetVal, int Size, string FilePath);

        public IniFile(string sPath = null)
        {
            //If a path was passed in, then use that.
            //If a path is not passed in, then get the path to Emar.Core.dll and use that to
            //calculate the path to either
            //<Solution_Folder>\Emar.Services\PharmacyNotificationService\pharmacy_notifications.ini
            //or
            //<API_website_root>\pharmacy_notification/pharmacy_notifications.ini
            if (String.IsNullOrEmpty(sPath))
            {
                //No path was passed in.
                //Calculate the path to PharmacyNotificationService\pharmacy_notifications.ini relative to the location of Emar.Core.dll.
                if (System.Diagnostics.Debugger.IsAttached)
                {
                    //We are debugging in the IDE (F5).
                    //The path to this assembly is
                    //<Solution_Folder>\Emar.Api\bin\debug\netcareapp3.1\EmarCore.dll
                    //We want to point to the ini file within the solution folder structure.
                    //<Solution_Folder>\Emar.Services\PharmacyNotificationService\pharmacy_notifications.ini

                    //Get the the path to this assembly.
                    //<Solution_Folder>\Emar.Api\bin\debug\netcareapp3.1\EmarCore.dll
                    sPath = Assembly.GetExecutingAssembly().Location;

                    //Remove Emar.Api\bin\debug\netcareapp3.1\EmarCore.dll
                    //Then add Emar.Services\PharmacyNotificationService\pharmacy_notifications.ini
                    //Use @ to specify literal strings, so we don't have to encode the directory separators.
                    sPath = sPath.Replace(@"Emar.Api\bin\Debug\netcoreapp3.1\" + EXE + ".dll", @"Emar.Services\PharmacyNotificationService\pharmacy_notifications.ini");
                }
                else
                {
                    //We are not debugging.
                    //This assembly is at <API_website_root>\Emar.Core.Dll
                    //And the ini file is in
                    //<API_website_root>\pharmacy_notification/pharmacy_notifications.ini

                    //Get the path to this assembly.
                    //This assembly is at <API_website_root>\Emar.Core.Dll
                    sPath = Assembly.GetExecutingAssembly().Location;

                    //Chop off EmarCore.dll
                    sPath = sPath.Replace(EXE + ".dll", "");

                    //Now we should be at the root folder of the API website.
                    //Add pharmacy_notification/pharmacy_notifications.ini
                    //Use a literal string so we don't have to encode the directory separators.
                    sPath += @"pharmacy_notification\pharmacy_notifications.ini";
                } //end if
            } //end if (was the path passed in).

            //Now we've got the path to the ini file as a string.
            //We either take it straight from the parameter, or we calculate it above.
            //Get a FileInfo object for the path.
            Path = new FileInfo(sPath).FullName;
        }

        public string Read(string Key, string Section = null)
        {
            var RetVal = new StringBuilder(255);
            GetPrivateProfileString(Section ?? EXE, Key, "", RetVal, 255, Path);
            return RetVal.ToString();
        }

        public void Write(string Key, string Value, string Section = null)
        {
            WritePrivateProfileString(Section ?? EXE, Key, Value, Path);
        }

        public void DeleteKey(string Key, string Section = null)
        {
            Write(Key, null, Section ?? EXE);
        }

        public void DeleteSection(string Section = null)
        {
            Write(null, null, Section ?? EXE);
        }

        public bool KeyExists(string Key, string Section = null)
        {
            return Read(Key, Section).Length > 0;
        }
    }
}
