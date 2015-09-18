using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Epub.Net.Extensions;

namespace Epub.Net.Models
{
    public class Chapter
    {
        public string FileName => $"chapter_{Name.ReplaceInvalidChars()}.xhtml";

        public string Name { get; set; }

        public string Content { get; set; }
    }
}
