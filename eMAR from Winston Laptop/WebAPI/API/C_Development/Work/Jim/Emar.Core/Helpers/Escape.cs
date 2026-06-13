using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Emar.Core.Helpers
{
    /// <summary>
    /// Library to handle escaping of data for storage or display purposes
    /// </summary>
    public static class Escape
    {
        /// <summary>
        /// Chart escape/unescape replacements
        /// </summary>
        private static List<Tuple<string, string>> CHART_ESCAPE = new List<Tuple<string, string>>
        {
            new Tuple<string, string>("<", "<LT>"),
            new Tuple<string, string>("&", "<AMP>"),
            new Tuple<string, string>("|", "<PIPE>"),
            new Tuple<string, string>("^", "<CARET>"),
            new Tuple<string, string>("\n", "<LF>")
        };

        /// <summary>
        /// HTML escape/unescape replacements
        /// </summary>
        private static List<Tuple<string, string>> HTML_ESCAPE = new List<Tuple<string, string>>
        {
            new Tuple<string, string>("&", "&amp;"),
            new Tuple<string, string>("\"", "&quot;"),
            new Tuple<string, string>(">", "&gt;"),
            new Tuple<string, string>("<", "&lt;"),
            new Tuple<string, string>("'", "&#39;"),
            new Tuple<string, string>("`", "&#96;")
        };

        /// <summary>
        /// Escape data for writing to the chart
        /// </summary>
        /// <param name="chartData">Piece of chart data to escape</param>
        /// <returns>Escaped chart data</returns>
        public static string ChartEscape(string chartData)
        {
            return DoReplacement(chartData, CHART_ESCAPE);
        }

        /// <summary>
        /// Unescape data written to the chart
        /// </summary>
        /// <param name="chartData">Piece of escaped chart data to unescape</param>
        /// <returns>Unescaped chart data</returns>
        public static string ChartUnescape(string chartData)
        {
            return DoReplacement(chartData, CHART_ESCAPE, true);
        }

        /// <summary>
        /// Escape a string being displayed in HTML
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string HtmlEscape(string str)
        {
            return DoReplacement(str, HTML_ESCAPE);
        }

        /// <summary>
        /// Unescape a string displayed in HTML
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string HtmlUnescape(string str)
        {
            return DoReplacement(str, HTML_ESCAPE, true);
        }

        /// <summary>
        /// Perform the replacement operations on a provided string
        /// </summary>
        /// <param name="data">String to perform replacements on</param>
        /// <param name="replacements">Replacements to perform</param>
        /// <param name="reverseReplacements">Flag for whether replacements should be applied in reverse order</param>
        /// <returns>String resulting from replacements</returns>
        private static string DoReplacement(string data, List<Tuple<string, string>> replacements, bool reverseReplacements = false)
        {
            if (String.IsNullOrEmpty(data))
            {
                return data;
            }

            if (reverseReplacements)
            {
                for (int i = replacements.Count - 1; i >= 0; i--)
                {
                    var t = replacements[i];
                    var find = t.Item2;
                    var replacement = t.Item1;

                    Regex re = new Regex(Regex.Escape(find));
                    data = re.Replace(data, replacement);
                }
            }
            else
            {
                for (int i = 0; i < replacements.Count; i++)
                {
                    var t = replacements[i];
                    var find = t.Item1;
                    var replacement = t.Item2;

                    Regex re = new Regex(Regex.Escape(find));
                    data = re.Replace(data, replacement);
                }
            }

            return data;
        }
    }
}