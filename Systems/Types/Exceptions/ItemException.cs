using System;

namespace Khepri.Types.Exceptions
{
    /// <summary> An exception thrown by an item entity. </summary>
    public class ItemException : Exception
    {
        /// <summary> An exception thrown by an item entity. </summary>
        public ItemException() { }


        /// <summary> An exception thrown by an item entity. </summary>
        /// <param name="message"> The exception's error message. </param>
        public ItemException(String message) : base(message) { }


        /// <summary> An exception thrown by an item entity. </summary>
        /// <param name="message"> The exception's error message. </param>
        /// <param name="innerException"> The exception wrapped by this one. </param>
        public ItemException(String message, Exception innerException) : base(message, innerException) { }
    }
}
