using System;

namespace OwinFramework.Builder
{
    public class BuilderException: Exception
    {
        public BuilderException() { }
        public BuilderException(string message): base(message) { }
    }
}
