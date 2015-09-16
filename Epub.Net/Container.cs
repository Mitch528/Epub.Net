using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Epub.Net
{
    public class Container
    {
        private static readonly XNamespace Namespace = "urn:oasis:names:tc:opendocument:xmlns:container";

        private XElement _container;
        private XElement _rootFiles;
        private XDocument _doc;

        public Container()
        {
            Init();
        }

        private void Init()
        {
            _rootFiles = new XElement(Namespace + "rootfiles");
            _container = new XElement(Namespace + "container",
                new XAttribute("version", "1.0"),
                _rootFiles
            );

            _doc = new XDocument(_container);
        }

        internal Container(string fileName)
        {
            _doc = XDocument.Load(fileName);
            _container = _doc.Element("container");
            _rootFiles = _doc.Element("rootfiles");
        }

        public void AddRootFile(RootFile rootFile)
        {
            _rootFiles.Add(new XElement(Namespace + "rootfile",
                new XAttribute("full-path", rootFile.FullPath),
                new XAttribute("media-type", rootFile.MediaType)
             ));
        }

        public void RemoveRootFile(RootFile rootFile)
        {
            _rootFiles.Descendants().SingleOrDefault(p => p.Name == "rootfile" && p.Attribute("full-path")?.Value == rootFile.FullPath
                && p.Attribute("media-type")?.Value == rootFile.MediaType)?.Remove();
        }

        public void Save(string dest)
        {
            _doc.Save(dest);
        }

        public static Container FromFile(string fileName)
        {
            return new Container(fileName);
        }
    }
}
