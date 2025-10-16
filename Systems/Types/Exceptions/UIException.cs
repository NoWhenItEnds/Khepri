using System;

namespace Khepri.Types.Exceptions
{
    /// <summary> An exception thrown by a UI element. </summary>
    public class UIException : Exception
    {
        /// <summary> An exception thrown by a UI element. </summary>
        public UIException() { }


        /// <summary> An exception thrown by a UI element. </summary>
        /// <param name="message"> The exception's error message. </param>
        public UIException(String message) : base(message) { }


        /// <summary> An exception thrown by a UI element. </summary>
        /// <param name="message"> The exception's error message. </param>
        /// <param name="innerException"> The exception wrapped by this one. </param>
        public UIException(String message, Exception innerException) : base(message, innerException) { }
    }
}
