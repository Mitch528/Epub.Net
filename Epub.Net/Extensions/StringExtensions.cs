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
        private static readonly string InvalidChars = string.Join("|", Path.GetInvalidFileNameChars().Select(p => Regex.Escape(p.ToString())));

        public static readonly Regex InvalidCharsRegex = new Regex("^[?!0-9]|" + InvalidChars, RegexOptions.Compiled);

        public static string ToValidFilePath(this string filePath)
        {
            StringBuilder newFilePath = new StringBuilder(filePath);
            Path.GetInvalidFileNameChars().ToList().ForEach(p => newFilePath.Replace(p, '_'));

            return newFilePath.ToString();
        }

        public static string ReplaceInvalidChars(this string str, string with = "_")
        {
            return InvalidCharsRegex.Replace(str, with);
        }

        public static bool HasInvalidChars(this string str)
        {
            return InvalidCharsRegex.IsMatch(str);
        }

        public static bool HasInvalidPathChars(this string filePath)
        {
            return Path.GetInvalidFileNameChars().Any(filePath.Contains);
        }
    }
}
