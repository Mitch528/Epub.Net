using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Epub.Net.Opf
{
    public class OpfMeta
    {
        public string Property { get; set; }

        public string Refines { get; set; }

        public string Id { get; set; }

        public string Scheme { get; set; }

        public string Text { get; set; }

        public OpfMeta()
        {
            Property = string.Empty;
            Refines = string.Empty;
            Id = string.Empty;
            Scheme = string.Empty;
        }
    }
}
