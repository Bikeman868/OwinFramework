namespace OwinFramework.InterfacesV1.Middleware
{
    /// <summary>
    /// Indicates that the middleware buffers the output from downstream middleware and 
    /// alters it in some way before returning it to the browser.
    /// Examples of this type of middleware are:
    /// * Compression middleware
    /// * Encryption middleware
    /// * Image resizing middleware
    /// * Middleware that appends version numbers to URLs in HTML
    /// If the application developer adds multiple middleware of this type then they
    /// may have to add extra dependencies to make the pipeline work. For example if
    /// you have encryption, compression and image resizing then the image resizing
    /// can't work after the response has been encrypted or compressed. In most
    /// scenarios the application developer probably wants compression after encryption
    /// but this should not be enforced by the framework.
    /// Note that Output Cache is a special case of IResponseRewriter because it needs
    /// to communicate with downstream middleware.
    /// </summary>
    public interface IResponseRewriter
    {
        /// <summary>
        /// When the OWIN pipeline contains mutiple middleware that buffer and modify
        /// the response, this property allows them to share the same buffer rather than
        /// each middleware capturing the response from the previous middleware.
        /// If IResponseRewriter is in the OWIN context then the OutputBuffer property
        /// can not be null. If the middleware did not buffer the output from the 
        /// current request then it should not add IResponseRewriter to the context.
        /// </summary>
        byte[] OutputBuffer { get; set; }
    }
}
