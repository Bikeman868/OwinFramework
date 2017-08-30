namespace OwinFramework.MiddlewareHelpers.EmbeddedResources
{
    /// <summary>
    /// This is a cached version of a resource that is embedded into the middleware assembly
    /// </summary>
    public class EmbeddedResource
    {
        /// <summary>
        /// The name of the file that was embedded into the assembly
        /// </summary>
        public string FileName;

        /// <summary>
        /// The mime type to return to the browser when this file is served
        /// </summary>
        public string MimeType;

        /// <summary>
        /// The contents of this file
        /// </summary>
        public byte[] Content;
    }
}
