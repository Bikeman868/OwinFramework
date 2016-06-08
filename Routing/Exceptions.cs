using System;
using OwinFramework.Builder;

namespace OwinFramework.Routing
{
    public class RoutingException: Exception
    {
        public RoutingException() { }
        public RoutingException(string message) : base(message) { }
    }

    public class CircularDependencyException : BuilderException
    {
        public CircularDependencyException() { }
        public CircularDependencyException(string message) : base(message) { }
    }
}
