using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Epub.Net.Opf
{
    public class OpfItem
    {
        public XElement ItemElement =>
            new XElement(OpfFile.XMLNS + "item",
                new XAttribute("href", Href),
                new XAttribute("id", Id),
                new XAttribute("media-type", MediaType.ToString()),
                new XAttribute("properties", Properties)
            );

        public XElement SpineElement =>
            new XElement(OpfFile.XMLNS + "itemref",
                new XAttribute("idref", Id),
                new XAttribute("linear", Linear ? "yes" : "no")
            );

        public string Href { get; set; }

        public string Id { get; set; }

        public bool Linear { get; set; }

        public string Properties { get; set; }

        public MediaType MediaType { get; set; }

        public OpfItem(string href, string id, MediaType mediaType)
        {
            Href = href;
            Id = id;
            MediaType = mediaType;

            Linear = true;
            Properties = string.Empty;
        }
    }
}
