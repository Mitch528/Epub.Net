using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Epub.Net
{
    public class Language
    {
        public static readonly Language English = new Language("en");

        public string Abbreviation { get; set; }

        public Language(string abbrv)
        {
            Abbreviation = abbrv;
        }

        public override string ToString()
        {
            return Abbreviation;
        }

        public static implicit operator string (Language language)
        {
            return language.ToString();
        }
    }
}
