namespace OwinFramework.Interfaces.Upstream
{
    /// <summary>
    /// Allows middleware that is further down the pipeline to communicate upstream to
    /// the identification middleware
    /// </summary>
    public interface IUpstreamIdentification
    {
        /// <summary>
        /// When this is false and the user can not be identified, the identification
        /// middleware should end the request processing and set back a not authorized
        /// response to the caller.
        /// </summary>
        bool AllowAnonymous { get; set; }
    }
}
