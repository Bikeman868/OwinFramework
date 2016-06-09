using System;

namespace OwinFramework.Utility
{
    public class CircularDependencyException : Exception
    {
        public CircularDependencyException(string message)
            : base(message) { }
    }
}
