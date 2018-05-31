namespace OwinFramework.MiddlewareHelpers.Traceable
{
    /// <summary>
    /// Specifies the importance/severity of the trace information
    /// </summary>
    public enum TraceFilterLevel
    {
        /// <summary>
        /// Do not output any trace information
        /// </summary>
        None = 0,

        /// <summary>
        /// Only output error messages
        /// </summary>
        Error = 1,

        /// <summary>
        /// Output error and information messages
        /// </summary>
        Information = 2,

        /// <summary>
        /// Output all trace information
        /// </summary>
        All = 100
    }
}
