using System.Linq;
using System.IO;
using System.Xml;
using System.Xml.Linq;

namespace Epub.Net.Opf
{
    public class OpfFile
    {
        internal static readonly XNamespace XMLNS = "http://www.idpf.org/2007/opf";
        internal static readonly XNamespace DC = "http://purl.org/dc/elements/1.1/";

        private static readonly XmlWriterSettings XmlSettings = new XmlWriterSettings
        {
            Indent = true
        };

        private XElement _metadata;
        private XElement _manifest;
        private XElement _spine;

        public XDocument Document { get; private set; }

        public OpfFile(OpfMetadata metadata)
        {
            Init(metadata);
        }

        internal OpfFile(string fileName)
        {
            Document = XDocument.Load(fileName);

            _metadata = Document.Element("metadata");
            _manifest = Document.Element("manifest");
            _spine = Document.Element("spine");
        }

        private void Init(OpfMetadata metadata)
        {
            _metadata = new XElement(XMLNS + "metadata",
                            new XAttribute(XNamespace.Xmlns + "dc", DC),
                            new XElement(DC + "identifier",
                                new XAttribute("id", "uid"),
                                new XText(metadata.Identifier)
                            ),
                            new XElement(DC + "title",
                                new XText(metadata.Title)
                            ),
                            new XElement(DC + "creator",
                                new XText(metadata.Creator)
                            ),
                            new XElement(DC + "language",
                                new XText(metadata.Language)
                            )
                        );

            _metadata.Add(metadata.Meta.Select(p =>
                new XElement("meta",
                    new XAttribute("property", p.Property),
                    new XAttribute("refines", p.Refines),
                    new XAttribute("id", p.Id),
                    new XAttribute("scheme", p.Scheme),
                    new XText(p.Text)
                )
            ));

            _manifest = new XElement(XMLNS + "manifest");
            _spine = new XElement(XMLNS + "spine");

            Document = new XDocument(
                new XElement(XMLNS + "package",
                    new XAttribute("version", "3.0"),
                    new XAttribute("unique-identifier", "uid"),
                    _metadata,
                    _manifest,
                    _spine
                )
            );
        }

        public void AddItem(OpfItem item, bool addToSpine = true)
        {
            _manifest.Add(item.ItemElement);

            if (addToSpine)
                _spine.Add(item.SpineElement);
        }

        public void RemoveItem(OpfItem item)
        {
            _manifest.Descendants().SingleOrDefault(p => p.Name == "item" && p.Attribute("id")?.Value == item.Id)?.Remove();
            _spine.Descendants().SingleOrDefault(p => p.Name == "itemref" && p.Attribute("idref")?.Value == item.Id)?.Remove();
        }

        public void Save(string dest)
        {
            Document.Save(dest);
        }

        public static OpfFile FromFile(string fileName)
        {
            return new OpfFile(fileName);
        }

        public override string ToString()
        {
            using (StringWriter sWriter = new StringWriter())
            using (XmlWriter writer = XmlWriter.Create(sWriter, XmlSettings))
            {
                Document.WriteTo(writer);
                writer.Flush();

                return sWriter.ToString();
            }
        }
    }
}
