using System;

namespace Khepri.Types.Exceptions
{
    /// <summary> An exception thrown by a resource. </summary>
    public class ResourceException : Exception
    {
        /// <summary> An exception thrown by a resource. </summary>
        public ResourceException() { }


        /// <summary> An exception thrown by a resource. </summary>
        /// <param name="message"> The exception's error message. </param>
        public ResourceException(String message) : base(message) { }


        /// <summary> An exception thrown by a resource. </summary>
        /// <param name="message"> The exception's error message. </param>
        /// <param name="innerException"> The exception wrapped by this one. </param>
        public ResourceException(String message, Exception innerException) : base(message, innerException) { }
    }
}
