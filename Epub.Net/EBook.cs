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
using AngleSharp.Parser.Html;
using AngleSharp.Xml;
using Epub.Net.Extensions;
using Epub.Net.Models;
using Epub.Net.Opf;
using Epub.Net.Razor;
using Epub.Net.Utils;

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
            GenerateEpubAsync(epubDest).Wait();
        }

        public void GenerateEpub(string epubDest, GenerateOptions options)
        {
            GenerateEpubAsync(epubDest, options).Wait();
        }

        public Task GenerateEpubAsync(string epubDest)
        {
            return GenerateEpubAsync(epubDest, DefaultGenerateOptions);
        }

        public async Task GenerateEpubAsync(string epubDest, GenerateOptions options)
        {
            OpfFile opf = new OpfFile(new OpfMetadata
            {
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
                    await EmbedImagesAsync(opf, chapter, Path.Combine(epub, "images"));

                OpfItem item = new OpfItem(chapter.FileName, chapter.Name.ReplaceInvalidChars(), MediaType.XHtmlType);
                opf.AddItem(item);

                var parser = new HtmlParser();
                var doc = parser.Parse(chapter.Content);
                chapter.Content = doc.QuerySelector("body").ChildNodes.ToHtml(new XmlMarkupFormatter());

                File.WriteAllText(Path.Combine(epub, chapter.FileName),
                        RazorCompiler.Get(Templates[EBookTemplate.Chapter], "chapter", chapter));
            }

            opf.Save(Path.Combine(epub, "package.opf"));

            if (File.Exists(epubDest))
                File.Delete(epubDest);

            using (FileStream fs = new FileStream(epubDest, FileMode.CreateNew))
            using (ZipArchive za = new ZipArchive(fs, ZipArchiveMode.Create))
            {
                za.CreateEntryFromFile(Path.Combine(tmpDir, "mimetype"), "mimetype", CompressionLevel.NoCompression);

                Zip(za, epub, "EPUB");
                Zip(za, metaInf, "META-INF");
            }

            Directory.Delete(tmpDir, true);
        }

        protected virtual async Task EmbedImagesAsync(OpfFile opfFile, Chapter chapter, string outputDir)
        {
            HtmlParser parser = new HtmlParser();
            var doc = await parser.ParseAsync(chapter.Content);
            var tasks = new List<Task>();

            foreach (var img in doc.QuerySelectorAll("img"))
            {
                tasks.Add(Task.Run(async () =>
                {
                    string src = img.GetAttribute("src");
                    string fileName = Path.GetFileNameWithoutExtension(src)?.ReplaceInvalidChars();

                    if (string.IsNullOrEmpty(fileName))
                        return;

                    string fileExt = Path.GetExtension(fileName);
                    fileName += fileExt;

                    if (fileName.HasInvalidPathChars())
                        fileName = fileName.ToValidFilePath();

                    string path = Path.Combine(outputDir, fileName);

                    if (File.Exists(path))
                        return;

                    if (!new Uri(src).IsFile)
                    {
                        try
                        {
                            using (HttpClient client = new HttpClient())
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
                                    return;

                                if (Path.GetExtension(path) != ext)
                                    path = path + ext;

                                if (File.Exists(path))
                                    return;

                                using (FileStream fs = new FileStream(path, FileMode.CreateNew))
                                    await resp.Content.CopyToAsync(fs);
                            }
                        }
                        catch (Exception)
                        {
                            return;
                        }
                    }
                    else if (File.Exists(src))
                    {
                        File.Copy(src, path);
                    }

                    MediaType mType = MediaType.FromExtension(Path.GetExtension(path));

                    if (mType == null)
                        return;

                    string filePath = Path.Combine(new DirectoryInfo(outputDir).Name, Path.GetFileName(path)).Replace(@"\", "/");
                    img.SetAttribute("src", filePath);

                    opfFile.AddItem(new OpfItem(filePath, StringUtilities.GenerateRandomString(),
                        mType), false);
                }));
            }

            await Task.WhenAll(tasks.ToArray());

            chapter.Content = doc.QuerySelector("body").ChildNodes.ToHtml(new XmlMarkupFormatter());
        }

        private static void Zip(ZipArchive archive, string dir, string dest)
        {
            foreach (FileInfo f in new DirectoryInfo(dir).GetFiles())
            {
                archive.CreateEntryFromFile(f.FullName, Path.Combine(dest, f.Name).Replace(@"\", "/"));
            }

            foreach (DirectoryInfo d in new DirectoryInfo(dir).GetDirectories())
            {
                Zip(archive, d.FullName, Path.Combine(dest, d.Name));
            }
        }
    }
}
