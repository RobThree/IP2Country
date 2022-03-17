using System;

namespace DemoWebService.Helpers
{
    public class WarmingUpException : Exception
    {
        public WarmingUpException() { }

        public WarmingUpException(string message) : this(message, null) { }

        public WarmingUpException(string message, Exception? innerException = null)
            : base(message, innerException) { }
    }
}