using System;

namespace OwinFramework.Routing
{
    /// <summary>
    /// This type of executopn is thrown by the routing compoennts in the OWIN Framework
    /// </summary>
    public class RoutingException: Exception
    {
        /// <summary>
        /// Default public constructor for RoutingException
        /// </summary>
        public RoutingException() { }

        /// <summary>
        /// Constructs a RoutingException containing an error message
        /// </summary>
        public RoutingException(string message) 
            : base(message) { }

        /// <summary>
        /// Constructs a RoutingException containing an error message and an inner exception
        /// </summary>
        public RoutingException(string message, Exception innerException) 
            : base(message, innerException) { }
    }
}
