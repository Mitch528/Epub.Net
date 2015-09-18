using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Epub.Net.Utils
{
    public static class StringUtilities
    {
        private static readonly Random Rand = new Random();

        private static readonly string Chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";

        public static string GenerateRandomString(int length = 8)
        {
            return string.Join(string.Empty, Enumerable.Repeat(Chars, length)
              .Select(s => s[Rand.Next(s.Length)])
              .ToArray());
        }
    }
}
