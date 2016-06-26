using System;

namespace OwinFramework.Utility
{
    public class DuplicateKeyException : Exception
    {
        public DuplicateKeyException(string message)
            : base(message) { }
    }
}
