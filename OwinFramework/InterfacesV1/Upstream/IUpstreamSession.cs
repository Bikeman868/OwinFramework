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
        /// </summary>
        /// <param name="sessionId">Only pass a session ID under very special circumstances. In
        /// almost all cases the session middleware is responsible for managing the session ID.</param>
        bool EstablishSession(string sessionId = null);

        /// <summary>
        /// Applications don't normally need to know the session ID, but there are some special
        /// circumstances where downstream middleware needs to establish a specific session, not
        /// the one associated with the request, and in these circumstances the session ID can be
        /// obtained here.
        /// </summary>
        string SessionId { get; }
    }
}
