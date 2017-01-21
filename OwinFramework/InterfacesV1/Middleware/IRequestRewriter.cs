namespace OwinFramework.InterfacesV1.Middleware
{
    /// <summary>
    /// Indicates that the middleware modifies the request URL. Any other middleware
    /// that needs the final url (for example because it maps the path to a physical file)
    /// should declare an optional dependency on this middleware to ensure that all
    /// request modifications are done before the request is interpreted.
    /// Examples of this type of middleware are:
    /// * URL rewriting rules to map legacy urls onto the current sitemap
    /// * Asset versioning middleware that appends version numbers to URLs in
    ///   outgoing HTML, then removes these version numbers from incomming requests.
    /// * Middleware that redirects different browsers to different locations within
    ///   the web site, for example browsers that natively support the Dart programming
    ///   language can be served different files than browsers that do not support it.
    /// </summary>
    public interface IRequestRewriter
    {
    }
}
