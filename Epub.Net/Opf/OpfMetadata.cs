using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Epub.Net.Utils;

namespace Epub.Net.Opf
{
    public class OpfMetadata
    {
        public string Identifier { get; set; } = StringUtilities.GenerateRandomString();

        public string Title { get; set; }

        public string Language { get; set; }

        public OpfMetadataElement Contributor { get; set; } = new OpfMetadataElement("contributor");

        public OpfMetadataElement Coverage { get; set; } = new OpfMetadataElement("coverage");

        public OpfMetadataElement Creator { get; set; } = new OpfMetadataElement("creator");

        public OpfMetadataElement Date { get; set; } = new OpfMetadataElement("date");

        public OpfMetadataElement Description { get; set; } = new OpfMetadataElement("description");

        public OpfMetadataElement Format { get; set; } = new OpfMetadataElement("format");

        public OpfMetadataElement Publisher { get; set; } = new OpfMetadataElement("publisher");

        public OpfMetadataElement Relation { get; set; } = new OpfMetadataElement("relation");

        public OpfMetadataElement Rights { get; set; } = new OpfMetadataElement("rights");

        public OpfMetadataElement Source { get; set; } = new OpfMetadataElement("source");

        public OpfMetadataElement Subject { get; set; } = new OpfMetadataElement("subject");

        public OpfMetadataElement Type { get; set; } = new OpfMetadataElement("type");

        public List<OpfMeta> Meta { get; set; } = new List<OpfMeta>();   
    }
}
