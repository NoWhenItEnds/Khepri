using System;

namespace Khepri.Types.Exceptions
{
    /// <summary> An exception thrown by a GOAP action strategy. </summary>
    public class ActionStrategyException : Exception
    {
        /// <summary> An exception thrown by a GOAP action strategy. </summary>
        public ActionStrategyException() { }


        /// <summary> An exception thrown by a GOAP action strategy. </summary>
        /// <param name="message"> The exception's error message. </param>
        public ActionStrategyException(String message) : base(message) { }


        /// <summary> An exception thrown by a GOAP action strategy. </summary>
        /// <param name="message"> The exception's error message. </param>
        /// <param name="innerException"> The exception wrapped by this one. </param>
        public ActionStrategyException(String message, Exception innerException) : base(message, innerException) { }
    }
}
