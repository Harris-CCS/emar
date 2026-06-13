using System;
using System.IO;
using System.Diagnostics;

namespace Emar.Core.Helpers
{
    /// <summary>
    /// Library to handle simple file writing
    /// </summary>
    public static class FileWriter
    {
        /// <summary>
        /// Write a single line to a file at the path specified
        /// </summary>
        /// <param name="path">Path to file</param>
        /// <param name="text">Text to write to file</param>
        /// <param name="append">Boolean flag for whether an existing file should have lines appended</param>
        /// <remarks>File will be created if it does not already exist</remarks>
        public static void Write(string path, string text, bool append = true)
        {
            //This was hitting errors when trying to checkout a patient's cart and move an order from
            //patient_cart_order to patient_order.  I added this try/catch block
            //to trap for errors.  That let the cart checkout go through, but I still think
            //some of Jim's trigger files were not being written.
            //I wwas seeing errors in ibex\mail, so I granted "everyone" permissions to that folder on 57c.
            //That stopped the errors there.
            //I'm thinking the mail errors were the ones breaking things.
            //Winston Murdock, 02/17/2021.  EMAR-692.
            try
            {

                FileInfo file = new FileInfo(path);
                file.Directory.Create();

                if (!string.IsNullOrWhiteSpace(text))
                {
                    using (StreamWriter sw = new StreamWriter(path, append))
                    {
                        sw.WriteLine(text);
                    }
                }
            }
            catch (Exception ex)
            {
                //Log the exception in writing to a file.
                using (EventLog eventLog = new EventLog("Application"))
                {
                    string sException = ex.Message + "\n";
                    sException += "source = " + ex.Source + "\n";
                    sException += text + "\n";
                    sException += ex.StackTrace + "\n";

                    eventLog.Source = "PulseCheck EMAR API";
                    eventLog.WriteEntry(sException, EventLogEntryType.Error, 101, 1);
                } //end using.
            } //end try/catch
        }

        /// <summary>
        /// Write multiple lines to a file at the path specified
        /// </summary>
        /// <param name="path">Path to file</param>
        /// <param name="lines">Lines of text to write to file</param>
        /// <param name="append">Boolean flag for whether an existing file should have lines appended</param>
        /// <remarks>File will be created if it does not already exist</remarks>
        public static void Write(string path, string[] lines, bool append = true)
        {
            Write(path, string.Join("", lines), append);
        }
    }
}