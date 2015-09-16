using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Epub.Net
{
    public class MediaType
    {
        public static readonly MediaType GifType = new MediaType("image/gif");
        public static readonly MediaType JpegType = new MediaType("image/jpeg");
        public static readonly MediaType PngType = new MediaType("image/png");
        public static readonly MediaType SvgType = new MediaType("image/svg+xml");
        public static readonly MediaType XHtmlType = new MediaType("application/xhtml+xml");
        public static readonly MediaType Opf2Type = new MediaType("application/x-dtbncx+xml");
        public static readonly MediaType OpenTypeType = new MediaType("application/vnd.ms-opentype");
        public static readonly MediaType WOFFType = new MediaType("application/font-woff");
        public static readonly MediaType MediaOverlays30Type = new MediaType("application/smil+xml");
        public static readonly MediaType PLSType = new MediaType("application/pls+xml");
        public static readonly MediaType MP3Type = new MediaType("audio/mpeg");
        public static readonly MediaType MP4Type = new MediaType("audio/mp4");
        public static readonly MediaType CSSType = new MediaType("text/css");
        public static readonly MediaType RFC4329Type = new MediaType("text/javascript");

        public string Type { get; }

        public MediaType(string type)
        {
            Type = type;
        }

        public override string ToString()
        {
            return Type;
        }

        public static MediaType FromExtension(string ext)
        {
            string extension = ext;
            if (extension.StartsWith("."))
                extension = extension.Substring(1);

            MediaType mType = null;
            switch (extension)
            {
                case "gif":
                    mType = GifType;
                    break;
                case "png":
                    mType = PngType;
                    break;
                case "jpg":
                    mType = JpegType;
                    break;
                    //TODO: Finish
            };

            return mType;
        }

        public static implicit operator string (MediaType mType)
        {
            return mType.Type;
        }
    }
}
