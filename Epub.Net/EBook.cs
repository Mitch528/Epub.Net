using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Epub.Net.Extensions;
using Epub.Net.Models;
using Epub.Net.Opf;
using Epub.Net.Razor;
using HtmlAgilityPack;

namespace Epub.Net
{
    public class EBook
    {
        public static readonly GenerateOptions DefaultGenerateOptions = new GenerateOptions
        {
            EmbedImages = true
        };

        public static readonly Assembly TemplateAsssembly = typeof(EBook).Assembly;

        public static readonly IReadOnlyDictionary<EBookTemplate, string> DefaultTemplates = new Dictionary<EBookTemplate, string>
        {
            { EBookTemplate.Cover, TemplateAsssembly.GetResourceString("Epub.Net.Templates.Cover.cshtml") },
            { EBookTemplate.TableOfContents, TemplateAsssembly.GetResourceString("Epub.Net.Templates.TableOfContents.cshtml") },
            { EBookTemplate.Chapter, TemplateAsssembly.GetResourceString("Epub.Net.Templates.Chapter.cshtml") }
        };

        public string Title { get; set; }

        public string CoverImage { get; set; }

        public List<Chapter> Chapters { get; }

        public Dictionary<EBookTemplate, string> Templates { get; } = DefaultTemplates.ToDictionary(p => p.Key, p => p.Value);

        public Language Language { get; set; } = Language.English;


        public EBook()
        {
            Chapters = new List<Chapter>();
        }

        public void GenerateEpub(string epubDest)
        {
            GenerateEpub(epubDest, DefaultGenerateOptions);
        }

        public void GenerateEpub(string epubDest, GenerateOptions options)
        {
            OpfFile opf = new OpfFile(new OpfMetadata
            {
                Identifier = Title,
                Title = Title,
                Language = Language
            });

            string tmpDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            string epub = Path.Combine(tmpDir, "EPUB");
            string metaInf = Path.Combine(tmpDir, "META-INF");
            Directory.CreateDirectory(tmpDir);
            Directory.CreateDirectory(epub);
            Directory.CreateDirectory(Path.Combine(epub, "covers"));
            Directory.CreateDirectory(Path.Combine(epub, "css"));
            Directory.CreateDirectory(Path.Combine(epub, "fonts"));
            Directory.CreateDirectory(Path.Combine(epub, "images"));

            Directory.CreateDirectory(metaInf);

            File.WriteAllText(Path.Combine(tmpDir, "mimetype"), "application/epub+zip");

            Container container = new Container();
            container.AddRootFile(new RootFile { FullPath = "EPUB/package.opf", MediaType = "application/oebps-package+xml" });
            container.Save(Path.Combine(metaInf, "container.xml"));

            if (!string.IsNullOrEmpty(CoverImage))
            {
                string coverExt = Path.GetExtension(CoverImage);
                MediaType mType = MediaType.FromExtension(coverExt);

                if (mType != MediaType.PngType && mType != MediaType.JpegType)
                    throw new Exception("Invalid cover image extension!");

                string coverImgFile = Path.GetFileName(CoverImage);
                string coverImg = Path.Combine("covers", coverImgFile);

                if (!new Uri(CoverImage).IsFile)
                    using (WebClient wc = new WebClient())
                        wc.DownloadFile(CoverImage, Path.Combine(epub, coverImg));
                else if (File.Exists(CoverImage))
                    File.Copy(CoverImage, Path.Combine(epub, coverImg));

                OpfItem coverImageItem = new OpfItem(coverImg.Replace(@"\", "/"), Path.GetFileNameWithoutExtension(coverImg), mType)
                {
                    Linear = false,
                    Properties = "cover-image"
                };

                OpfItem coverItem = new OpfItem("cover.xhtml", "cover", MediaType.XHtmlType);
                File.WriteAllText(Path.Combine(epub, "cover.xhtml"),
                    RazorCompiler.Get(Templates[EBookTemplate.Cover], "cover", $"covers/{coverImgFile}"));

                opf.AddItem(coverItem);
                opf.AddItem(coverImageItem, false);
            }

            TableOfContents toc = new TableOfContents { Title = Title };
            toc.Sections.AddRange(Chapters.Select(p => new Section { Name = p.Name, Href = p.FileName }));

            string tocFile = Path.Combine(epub, "toc.xhtml");
            File.WriteAllText(tocFile, RazorCompiler.Get(Templates[EBookTemplate.TableOfContents], "toc", toc));

            OpfItem navItem = new OpfItem("toc.xhtml", "toc", MediaType.XHtmlType) { Properties = "nav" };
            opf.AddItem(navItem);

            foreach (Chapter chapter in Chapters)
            {
                if (options.EmbedImages)
                    EmbedImages(opf, chapter, Path.Combine(epub, "images"));

                OpfItem item = new OpfItem(chapter.FileName, chapter.Name.ToLower().Replace(" ", "-"), MediaType.XHtmlType);
                opf.AddItem(item);

                File.WriteAllText(Path.Combine(epub, chapter.FileName),
                    RazorCompiler.Get(Templates[EBookTemplate.Chapter], "chapter", chapter));
            }

            opf.Save(Path.Combine(epub, "package.opf"));

            if (File.Exists(epubDest))
                File.Delete(epubDest);

            ZipFile.CreateFromDirectory(tmpDir, epubDest);

            Directory.Delete(tmpDir, true);
        }

        protected virtual void EmbedImages(OpfFile opfFile, Chapter chapter, string outputDir)
        {
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(chapter.Content);

            using (HttpClient client = new HttpClient())
            {
                var imageNodes = doc.DocumentNode.SelectNodes("//img");

                if (imageNodes == null)
                    return;

                foreach (HtmlNode imgNode in imageNodes)
                {
                    string src = imgNode.Attributes["src"].Value;
                    string fileName = Path.GetFileName(src);

                    if (string.IsNullOrEmpty(fileName))
                        continue;

                    if (fileName.HasInvalidPathChars())
                        fileName = Path.GetRandomFileName();

                    string path = Path.Combine(outputDir, fileName);

                    if (File.Exists(path))
                        continue;

                    if (!new Uri(src).IsFile)
                    {
                        try
                        {
                            HttpResponseMessage resp = client.GetAsync(src).Result;   
                            resp.EnsureSuccessStatusCode();

                            string mediaType = resp.Content.Headers.ContentType.MediaType.ToLower();
                            string ext;

                            if (mediaType == MediaType.JpegType)
                                ext = ".jpg";
                            else if (mediaType == MediaType.PngType)
                                ext = ".png";
                            else
                                continue;

                            if (Path.GetExtension(path) != ext)
                                path = path + ext;

                            byte[] fileData = resp.Content.ReadAsByteArrayAsync().Result;
                            File.WriteAllBytes(path, fileData);
                        }
                        catch (Exception)
                        {
                            continue;
                        }
                    }
                    else if (File.Exists(src))
                    {
                        File.Copy(src, path);
                    }

                    MediaType mType = MediaType.FromExtension(Path.GetExtension(path));

                    if (mType == null)
                        continue;

                    string filePath = Path.Combine(new DirectoryInfo(outputDir).Name, Path.GetFileName(path)).Replace(@"\", "/");
                    imgNode.SetAttributeValue("src", filePath);

                    opfFile.AddItem(new OpfItem(filePath, Path.GetFileNameWithoutExtension(path),
                        mType), false);
                }
            }

            chapter.Content = doc.DocumentNode.OuterHtml;
        }
    }
}
