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
        public string FileName => $"{Name.ToLower().Replace(" ", "-")}.xhtml".ToValidFilePath();

        public string Name { get; set; }
        
        public string Content { get; set; }
    }
}
