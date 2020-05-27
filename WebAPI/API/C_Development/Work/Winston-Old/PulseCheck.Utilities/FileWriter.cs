using System.IO;

namespace PulseCheck.Utilities
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