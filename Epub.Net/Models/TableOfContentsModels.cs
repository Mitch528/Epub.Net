using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Epub.Net.Models
{
    public class TableOfContents
    {
        public string Title { get; set; }

        public List<Section> Sections { get; set; }

        public TableOfContents()
        {
            Sections = new List<Section>();
        }
    }

    public class Section
    {
        public string Name { get; set; }

        public string Href { get; set; }

        public List<Section> SubSections { get; set; }

        public Section()
        {
            SubSections = new List<Section>();
        }
    }
}
