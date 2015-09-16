using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Epub.Net.Extensions
{
    public static class StringExtensions
    {
        public static string ToValidFilePath(this string filePath)
        {
            StringBuilder newFilePath = new StringBuilder(filePath);
            Path.GetInvalidFileNameChars().ToList().ForEach(p => newFilePath.Replace(p, '-'));

            return newFilePath.ToString();
        }

        public static bool HasInvalidPathChars(this string filePath)
        {
            return Path.GetInvalidFileNameChars().Any(filePath.Contains);
        }
    }
}
