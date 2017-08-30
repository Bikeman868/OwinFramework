using System;
using OwinFramework.InterfacesV1.Facilities;

namespace OwinFramework.MiddlewareHelpers.EmbeddedResources
{
    /// <summary>
    /// This is an implementation of IMimeTypeEvaluator that you can register with IoC
    /// or you can provide your own if this one does not meet your needs.
    /// </summary>
    public class MimeTypeEvaluator : IMimeTypeEvaluator
    {
        public string MimeTypeFromExtension(string fileExtension)
        {
            switch (fileExtension.ToLower())
            {
                case ".html":
                case ".htm":
                    return "text/html";
                case ".css":
                case ".less":
                    return "text/css";
                case ".js":
                    return "application/javascript";
                case ".gif":
                    return "image/gif";
                case ".ico":
                    return "image/ico";
                case ".png":
                    return "image/png";
                case ".jpg":
                case ".jpeg":
                    return "image/jpeg";
            }
            return "application/octet-stream";
        }

        public string MimeTypeFromContent(string fileContent)
        {
            var docTypeIndex = fileContent.IndexOf("<!DOCTYPE html", 0, 256, StringComparison.InvariantCultureIgnoreCase);
            if (docTypeIndex >= 0 && docTypeIndex < 256) return "text/html";

            var htmlIndex = fileContent.IndexOf("<html", 0, 256, StringComparison.InvariantCultureIgnoreCase);
            if (htmlIndex >= 0 && htmlIndex < 256) return "text/html";

            return "text/plain";
        }

        public string MimeTypeFromContent(byte[] content)
        {
            return "application/octct-stream";
        }
    }
}
