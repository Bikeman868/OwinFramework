using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OwinFramework.Interfaces.Builder
{
    /// <summary>
    /// When requests are being traced this provides access to the trace information that was gathered
    /// </summary>
    public interface ITraceContext
    {
        /// <summary>
        /// Returns the trace information gathered so far for this trace context
        /// </summary>
        /// <returns></returns>
        string GetTraceOutput();
    }
}
