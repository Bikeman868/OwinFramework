using System;

namespace OwinFramework.Routing
{
    public class RoutingException: Exception
    {
        public RoutingException() { }

        public RoutingException(string message) 
            : base(message) { }

        public RoutingException(string message, Exception innerException) 
            : base(message, innerException) { }
    }
}
