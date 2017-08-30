using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using OwinFramework.Interfaces.Utility;
using OwinFramework.InterfacesV1.Facilities;

namespace OwinFramework.MiddlewareHelpers.EmbeddedResources
{
    /// <summary>
    /// This class is useful if your middleware has embedded resources that
    /// you want to serve to the client. For example if your middleware comes
    /// with a user interface comprising html, css JavaScript etc and you want
    /// to embed those files into your middleware DLL and serve them to the
    /// browser to present the UI, then this class will help you with that.
    /// </summary>
    public class ResourceManager
    {
        private readonly IHostingEnvironment _hostingEnvironment;
        private readonly IMimeTypeEvaluator _mimeTypeEvaluator;

        private readonly IDictionary<string, EmbeddedResource> _resources 
            = new Dictionary<string, EmbeddedResource>(StringComparer.InvariantCultureIgnoreCase);

        private string _localResourcePath;

        /// <summary>
        /// Set this property to the a relative path within your website folder structure.
        /// Any files placed in this folder will override the files embedded into the assembly.
        /// For example if you embedded a UI that includes CSS, the consumer of your middleware
        /// can place a CSS file into this folder to customize the look and feel of your UI.
        /// </summary>
        public string LocalResourcePath
        {
            get { return _localResourcePath; }
            set 
            { 
                _localResourcePath = value;
                Clear();
            }
        }

        /// <summary>
        /// Constructs a new resource manager
        /// </summary>
        public ResourceManager(
            IHostingEnvironment hostingEnvironment, 
            IMimeTypeEvaluator mimeTypeEvaluator)
        {
            _hostingEnvironment = hostingEnvironment;
            _mimeTypeEvaluator = mimeTypeEvaluator;
        }

        /// <summary>
        /// Retrieves a cached resource from the specified assembly. If the resource exists
        /// on disk this will override the one that is embedded into the assembly - set the
        /// LocalResourcePath property to enable this override feature.
        /// </summary>
        public EmbeddedResource GetResource(Assembly assembly, string filename)
        {
            EmbeddedResource resource;
            lock (_resources)
                if (_resources.TryGetValue(filename, out resource))
                    return resource;

            var extension = Path.GetExtension(filename);
            resource = new EmbeddedResource
            {
                FileName = filename,
                MimeType = _mimeTypeEvaluator.MimeTypeFromExtension(extension)
            };

            if (!string.IsNullOrEmpty(LocalResourcePath))
            {
                var physicalFile = new FileInfo(_hostingEnvironment.MapPath(LocalResourcePath + filename));
                if (physicalFile.Exists)
                {
                    if (IsBinaryMimeType(resource.MimeType))
                    {
                        resource.Content = new byte[physicalFile.Length];
                        using (var stream = physicalFile.Open(FileMode.Open, FileAccess.Read, FileShare.Read))
                        {
                            ReadBinaryResource((int) physicalFile.Length, stream, resource);
                        }
                    }
                    else
                    {
                        using (var streamReader = physicalFile.OpenText())
                        {
                            ReadTextResource(filename, streamReader, resource);
                        }
                    }
                }
            }
            
            if (resource.Content == null)
            {
                var resourceStream = FindEmbeddedResource(assembly, filename);
                if (resourceStream != null)
                {
                    using (resourceStream)
                    {
                        if (IsBinaryMimeType(resource.MimeType))
                        {
                            ReadBinaryResource((int) resourceStream.Length, resourceStream, resource);
                        }
                        else
                        {
                            using (var reader = new StreamReader(resourceStream, Encoding.UTF8))
                            {
                                ReadTextResource(filename, reader, resource);
                            }
                        }
                    }
                }
            }

            lock (_resources)
                _resources[filename] = resource;

            return resource;
        }

        /// <summary>
        /// Deletes all cached resources forcing them to be reloaded on next access
        /// </summary>
        public void Clear()
        {
            lock (_resources) _resources.Clear();
        }

        /// <summary>
        /// Override this method to apply any transformations to text resources. These
        /// transformations happen only once for each file, the results are cached and reused.
        /// Examples of transformations are converting LESS to CSS.
        /// </summary>
        /// <param name="filename">The name of the requested resource file</param>
        /// <param name="content">The contents of the file</param>
        /// <returns>Transformed text content to serve to the user agent</returns>
        protected virtual string TransformTextResource(string filename, string content)
        {
            return content;
        }

        /// <summary>
        /// Override this method to provide logic to determine which mime types should be
        /// handled as binary files and which ones should be handled as text.
        /// </summary>
        protected virtual bool IsBinaryMimeType(string mimeType)
        {
            return mimeType.StartsWith("image/");
        }

        private Stream FindEmbeddedResource(Assembly assembly, string filename)
        {
            var resources = assembly.GetManifestResourceNames();

            filename = filename.Replace("/", ".");

            var resourceName = resources.FirstOrDefault(n => n.ToLower().Contains(filename));

            return resourceName == null ? null : assembly.GetManifestResourceStream(resourceName);
        }

        private void ReadTextResource(string filename, TextReader reader, EmbeddedResource resource)
        {
            var text = reader.ReadToEnd();
            text = TransformTextResource(filename, text);
            resource.Content = Encoding.UTF8.GetBytes(text);
        }

        private void ReadBinaryResource(int length, Stream stream, EmbeddedResource resource)
        {
            resource.Content = new byte[length];
            var offset = 0;
            while (true)
            {
                var bytesRead = stream.Read(resource.Content, offset, length - offset);
                if (bytesRead == 0) return;
                offset += bytesRead;
            }
        }
    }
}
