using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RazorEngine;
using RazorEngine.Configuration;
using RazorEngine.Templating;

namespace Epub.Net.Razor
{
    public static class RazorCompiler
    {
        static RazorCompiler()
        {
            var config = new TemplateServiceConfiguration { DisableTempFileLocking = true };

            Engine.Razor = RazorEngineService.Create(config);
        }

        public static string Get(string template, string key, object model = null)
        {
            Type modelType = model?.GetType();

            if (!Engine.Razor.IsTemplateCached(key, modelType))
                return Engine.Razor.RunCompile(template, key, modelType, model);

            return Engine.Razor.Run(key, modelType, model);
        }
    }
}
