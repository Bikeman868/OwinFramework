using System;

namespace OwinFramework.Utility
{
    public class MissingDependencyException : Exception
    {
        public MissingDependencyException(string message)
            : base(message) { }
    }
}
