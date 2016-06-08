using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OwinFramework.Interfaces
{
    /// <summary>
    /// Allows middleware that is further down the pipeline to communicate upstream to
    /// the session middleware
    /// </summary>
    public interface IUpstreamSession
    {
        /// <summary>
        /// Signals to the session provider middleware that a session is needed to process this request.
        /// You can not set this property to false. If any middleware sets it to true for the request
        /// this can't be overriden to false by another middleware component
        /// </summary>
        bool SessionRequired { get; set; }
    }
}
