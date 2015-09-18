using System.Collections.Generic;
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

        private readonly object _locker = new object();

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
            _metadata = CreateMetadata(metadata);
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

        public bool AddItem(OpfItem item, bool addToSpine = true)
        {
            lock(_locker)
            {
                if (_manifest.Descendants().Any(p => p.Attribute("id")?.Value == item.Id)
                        || _manifest.Descendants().Any(p => p.Attribute("href")?.Value == item.Href))
                    return false;

                _manifest.Add(item.ItemElement);

                if (addToSpine)
                    _spine.Add(item.SpineElement);
            }

            return true;
        }

        public void RemoveItem(OpfItem item)
        {
            lock(_locker)
            {
                _manifest.Descendants().SingleOrDefault(p => p.Name == "item" && p.Attribute("id")?.Value == item.Id)?.Remove();
                _spine.Descendants().SingleOrDefault(p => p.Name == "itemref" && p.Attribute("idref")?.Value == item.Id)?.Remove();
            }
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

        private XElement CreateMetadata(OpfMetadata metadata)
        {
            XElement element = new XElement(XMLNS + "metadata",
                            new XAttribute(XNamespace.Xmlns + "dc", DC),
                            new XElement(DC + "identifier",
                                new XAttribute("id", "uid"),
                                new XText(metadata.Identifier)
                            ),
                            new XElement(DC + "title",
                                new XText(metadata.Title)
                            ),
                            new XElement(DC + "language",
                                new XText(metadata.Language)
                            )
                        );

            element.Add(CreateMetadataElements(
                metadata.Contributor, metadata.Coverage, metadata.Creator, metadata.Date, metadata.Description, 
                metadata.Format, metadata.Publisher, metadata.Relation, metadata.Rights, metadata.Source, metadata.Subject,
                metadata.Type
            ).Cast<object>().ToArray());

            element.Add(metadata.Meta.Select(p =>
                new XElement("meta",
                    new XAttribute("property", p.Property),
                    new XAttribute("refines", p.Refines),
                    new XAttribute("id", p.Id),
                    new XAttribute("scheme", p.Scheme),
                    new XText(p.Text)
                )
            ));

            return element;
        }

        private static IEnumerable<XElement> CreateMetadataElements(params OpfMetadataElement[] elements)
        {
            foreach (OpfMetadataElement element in elements)
            {
                if (string.IsNullOrEmpty(element.Text))
                    continue;

                XElement mElement = new XElement(DC + element.Name, new XText(element.Text));

                if (!string.IsNullOrEmpty(element.Id))
                    mElement.Add(new XAttribute("id", element.Id));
                if (!string.IsNullOrEmpty(element.Language))
                    mElement.Add(new XAttribute("language", element.Language));
                if (element.Direction != null)
                    mElement.Add(new XAttribute("direction", element.Direction));

                yield return mElement;
            }
        }
    }
}
