namespace OwinFramework.InterfacesV1.Upstream
{
    /// <summary>
    /// Allows middleware that is further down the pipeline to communicate upstream to
    /// the session middleware
    /// </summary>
    public interface IUpstreamSession
    {
        /// <summary>
        /// Signals to the session provider middleware that a session is needed to process this request.
        /// The session middleware should try to establish a session before returning, but if this
        /// is not possible it must return False;
        /// Imagine a situation where a request comes in for a user profile and the output cache
        /// has a cached version of it, but the rendering middleware needs to know the identity
        /// of the caller to decide if the cached data can be used or not, and this information
        /// can be persisted in session.
        /// </summary>
        bool EstablishSession();
    }
}
