using System;

namespace Khepri.Types.Exceptions
{
    /// <summary> An exception thrown by a data object. </summary>
    public class DataException : Exception
    {
        /// <summary> An exception thrown by a data object. </summary>
        public DataException() { }


        /// <summary> An exception thrown by a data object. </summary>
        /// <param name="message"> The exception's error message. </param>
        public DataException(String message) : base(message) { }


        /// <summary> An exception thrown by a data object. </summary>
        /// <param name="message"> The exception's error message. </param>
        /// <param name="innerException"> The exception wrapped by this one. </param>
        public DataException(String message, Exception innerException) : base(message, innerException) { }
    }
}
