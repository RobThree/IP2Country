using System;

namespace DemoWebService.Helpers
{
    public class ReloadException : Exception
    {
        public ReloadException() { }

        public ReloadException(string message) : this(message, null) { }

        public ReloadException(string message, Exception innerException = null)
            : base(message, innerException) { }
    }
}
