using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Owin;
using OwinFramework.Interfaces.Builder;
using OwinFramework.InterfacesV1.Capability;

namespace OwinFramework.MiddlewareHelpers.Traceable
{
    /// <summary>
    /// Adds the ability to filter trace output according to level of severity
    /// </summary>
    public class TraceFilter: IDisposable
    {
        private readonly ITraceable _traceable;

        private IDisposable _configRegistration;
        private TraceFilterLevel _traceFilterLevel;

        /// <summary>
        /// Constructs a new trace filter. This filter can be used in place of
        /// calling the trace method directly to provide the ability to filter
        /// trace messages and configure which trace messages are output to the
        /// reace log
        /// </summary>
        /// <param name="configuration">The Owin Framework configuration mechanism</param>
        /// <param name="traceable">The middleware that implements ITraceable. It will
        /// be possible to filter trace messages by the class name of this middleware</param>
        public TraceFilter(
            IConfiguration configuration,
            ITraceable traceable)
        {
            _traceable = traceable;

            _traceFilterLevel = TraceFilterLevel.None;

            _configRegistration = configuration.Register(
                "/owinFramework/middleware/traceFilter",
                cfg => 
                {
                    if (cfg.MiddlewareClasses != null && cfg.MiddlewareClasses.Count > 0)
                    {
                        var name = _traceable.GetType().FullName;
                        if (!cfg.MiddlewareClasses.Any(c => name.EndsWith(c, StringComparison.OrdinalIgnoreCase)))
                        {
                            _traceFilterLevel = TraceFilterLevel.None;
                            return;
                        }
                    }

                    TraceFilterLevel traceFilterLevel;
                    if (Enum.TryParse(cfg.Level, true, out traceFilterLevel))
                        _traceFilterLevel = traceFilterLevel;
                },
                new Configuration());
        }

        public void Dispose()
        {
            if (_configRegistration != null)
                _configRegistration.Dispose();
            _configRegistration = null;
        }

        /// <summary>
        /// Outputs trace information only if tracing is turned on and the trace filter
        /// allows the message to go through
        /// </summary>
        /// <param name="context">The Owin context for the request</param>
        /// <param name="level">The level of severity/importance of this message</param>
        /// <param name="messageFunc">A lambda expression to execute only if tracing is enabled</param>
        public void Trace(IOwinContext context, TraceLevel level, Func<string> messageFunc)
        {
            if ((int)level > (int)_traceFilterLevel)
                return;

            _traceable.Trace(context, messageFunc);
        }

        /// <summary>
        /// Defines the configuration options available for the trace filter
        /// </summary>
        public class Configuration
        {
            /// <summary>
            /// Defines the maximum level of detail to output into the trace log
            /// </summary>
            public string Level { get; set; }

            /// <summary>
            /// When null or empty all middleware is traced.
            /// When this is a list of class names then only these middleware will output
            /// trace information
            /// </summary>
            public List<string> MiddlewareClasses { get; set; }

            /// <summary>
            /// Default public constructor initializes with default trace filter configuration
            /// </summary>
            public Configuration()
            {
                Level = TraceFilterLevel.None.ToString();
            }
        }

    }
}
