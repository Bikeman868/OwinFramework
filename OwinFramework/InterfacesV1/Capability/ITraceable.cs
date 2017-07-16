using System;
using System.Collections.Generic;
using Microsoft.Owin;

namespace OwinFramework.InterfacesV1.Capability
{
    /// <summary>
    /// Middleware that implements this interface can be configured to output trace information
    /// that can help in discovering why requests are not being handled in the way that
    /// the application developer was expecting. For example if an application developer
    /// configures static files middleware and no static files are served, maybe the 
    /// configuration is wrong, maybe it is looking in the wrong location on disk, maybe
    /// the web server does not have permission to access the files etc. This interface
    /// allows calls to be traced to help with tracking down these issues without the
    /// overhead of constantly tracing everything.
    /// </summary>
    public interface ITraceable
    {
        /// <summary>
        /// The builder will set this property immediately after constructing your
        /// middleware class. When tracing is disabled this action will do nothing
        /// very quickly. When tracing is enabled the function you pass to the 
        /// trace action will execute and the string it returns will be added to a
        /// log that is specific to the current request.
        /// </summary>
        /// <example>Trace(context, () => "File does not exist on disk");</example>
        Action<IOwinContext, Func<string>> Trace { get; set; }
    }
}
