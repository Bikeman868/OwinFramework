using System;

namespace OwinFramework.Builder
{
    public class BuilderException: Exception
    {
        public BuilderException() { }
        public BuilderException(string message): base(message) { }
    }

    public class CircularDependencyException : BuilderException
    {
        public CircularDependencyException() { }
        public CircularDependencyException(string message) : base(message) { }
    }
}
