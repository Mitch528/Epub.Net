using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Epub.Net.Extensions
{
    public static class AssemblyExtensions
    {
        public static string GetResourceString(this Assembly assembly, string name)
        {
            byte[] resource = assembly.GetResourceByteArray(name);

            if (resource == null)
                return null;
            
            return Encoding.UTF8.GetString(resource);
        }

        public static byte[] GetResourceByteArray(this Assembly assembly, string name)
        {
            using (MemoryStream ms = new MemoryStream())
            using (Stream stream = assembly.GetManifestResourceStream(name))
            {
                if (stream == null)
                    return null;

                stream.CopyTo(ms);

                return ms.ToArray();
            }
        }
    }
}
