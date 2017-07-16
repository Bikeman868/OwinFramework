namespace OwinFramework.Interfaces.Builder
{
    /// <summary>
    ///  When request tracing is enabled, defines which requests will be traced
    /// </summary>
    public enum RequestsToTrace
    {
        /// <summary>
        /// No requests will produce any trace output
        /// </summary>
        None,

        /// <summary>
        /// Only requests that have trace in the query string will be traced
        /// </summary>
        QueryString,

        /// <summary>
        /// All requests will be traced
        /// </summary>
        All
    }
}
