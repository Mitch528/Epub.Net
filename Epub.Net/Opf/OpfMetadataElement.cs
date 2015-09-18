using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Epub.Net.Opf
{
    public class OpfMetadataElement
    {
        public string Name { get; }

        public string Text { get; set; }

        public string Id { get; set; }

        public string Language { get; set; }

        public OpfMetadataDirection? Direction { get; set; }

        public OpfMetadataElement(string name)
        {
            Name = name;
        }
    }
}
