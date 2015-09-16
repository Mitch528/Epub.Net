using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RazorEngine.Templating;

namespace Epub.Net.Razor
{
    public class HtmlSupportTemplateBase<T> : TemplateBase<T>
    {
        public HtmlHelper Html { get; set; }

        public HtmlSupportTemplateBase()
        {
            Html = new HtmlHelper();
        }
    }
}
