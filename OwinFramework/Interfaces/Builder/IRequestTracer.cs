using System;
using Microsoft.Owin;

namespace OwinFramework.Interfaces.Builder
{
    /// <summary>
    /// This interface is implemented by the builder and allows application
    /// developers to decide how tracing information is output. If this
    /// interface is not used, then trace output is output using the
    /// built in System.Diagnostics.Trace class.
    /// </summary>
    public interface IRequestTracer
    {

        /// <summary>
        /// Turns tracing on. This should only be used in a development environment.
        /// If your production environment is very low volume you could also choose
        /// to leave it on in production but this is not the use case it was designed 
        /// for. You can also trace specific requests by passing a traceOption of
        /// QueryString then appending ?trace=true the url when calling your service.
        /// </summary>
        IBuilder EnableTracing(RequestsToTrace traceOption = RequestsToTrace.All);

        /// <summary>
        /// A function that gets called at the end of processing each
        /// request that is being traced. This function should output
        /// the trace information so that developers can see it and
        /// use it to debug issues.
        /// The default function writes the trace information using
        /// the built-in System.Diagnostics.Trace class.
        /// </summary>
        Action<IOwinContext, string> TraceOutput { get; set; }
    }
}
