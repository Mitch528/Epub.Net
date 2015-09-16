using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Epub.Net.Opf
{
    public class OpfMetadata
    {
        public string Identifier { get; set; }

        public string Title { get; set; }

        public string Creator { get; set; }

        public string Language { get; set; }

        public List<OpfMeta> Meta { get; set; }

        public OpfMetadata()
        {
            Identifier = Guid.NewGuid().ToString();
            Title = string.Empty;
            Creator = string.Empty;
            Language = string.Empty;
            Meta = new List<OpfMeta>();
        }
    }
}
