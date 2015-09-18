using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Epub.Net.Extensions
{
    public static class StringExtensions
    {
        public static readonly Regex ReplaceInvalidCharsRegex = new Regex("([?!0-9])*[^a-zA-Z0-9]", RegexOptions.Compiled);

        public static string ToValidFilePath(this string filePath)
        {
            StringBuilder newFilePath = new StringBuilder(filePath);
            Path.GetInvalidFileNameChars().ToList().ForEach(p => newFilePath.Replace(p, '-'));

            return newFilePath.ToString();
        }

        public static string ReplaceInvalidChars(this string str, string with = "-")
        {
            return ReplaceInvalidCharsRegex.Replace(str, with);
        }

        public static bool HasInvalidPathChars(this string filePath)
        {
            return Path.GetInvalidFileNameChars().Any(filePath.Contains);
        }
    }
}
