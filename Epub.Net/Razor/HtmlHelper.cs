using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Epub.Net.Extensions;
using RazorEngine;
using RazorEngine.Templating;
using RazorEngine.Text;

namespace Epub.Net.Razor
{
    public class HtmlHelper
    {
        private static readonly Assembly TemplateAssembly = typeof(EBook).Assembly;

        public IEncodedString Partial(string templatePath, object model = null)
        {
            Type modelType = model?.GetType();

            string key = Path.GetFileNameWithoutExtension(templatePath);

            if (!Engine.Razor.IsTemplateCached(key, modelType))
            {
                string template = TemplateAssembly.GetResourceString(templatePath);

                if (string.IsNullOrEmpty(template) && File.Exists(templatePath))
                    template = File.ReadAllText(templatePath);
                else if (string.IsNullOrEmpty(template))
                    throw new Exception($"Could not find template {templatePath}!");

                return Raw(Engine.Razor.RunCompile(template, key, modelType, model));
            }

            return Raw(Engine.Razor.Run(key, modelType, model));
        }

        public IEncodedString Raw(string html)
        {
            return new RawString(html);
        }
    }
}
