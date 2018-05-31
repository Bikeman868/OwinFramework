using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OwinFramework.MiddlewareHelpers.Traceable
{
    /// <summary>
    /// Specifies the importance/severity of the trace information
    /// </summary>
    public enum TraceLevel
    {
        /// <summary>
        /// This is an error condition or very significant information and is always output unless tracing is
        /// turned off completely. For example exceptions, badly formed requests, authentication failures etc
        /// </summary>
        Error = 1,

        /// <summary>
        /// This is important information about the processing of the request.
        /// For example the identification of the caller, successful authorization, request matches the middleware
        /// path and method. Provides basic information about the processing of the request.
        /// </summary>
        Information = 2,

        /// <summary>
        /// Output only in the most detailed and verbose setting. This is can be used to track down very
        /// complicated problems where all of information needs to be analysed in detail to figure out
        /// what is going on.
        /// </summary>
        Debug = 3
    }
}
