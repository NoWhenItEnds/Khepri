using System;

namespace Khepri.Types.Exceptions
{
    /// <summary> An exception thrown by an actor entity. </summary>
    public class ActorException : Exception
    {
        /// <summary> An exception thrown by an actor entity. </summary>
        public ActorException() { }


        /// <summary> An exception thrown by an actor entity. </summary>
        /// <param name="message"> The exception's error message. </param>
        public ActorException(String message) : base(message) { }


        /// <summary> An exception thrown by an actor entity. </summary>
        /// <param name="message"> The exception's error message. </param>
        /// <param name="innerException"> The exception wrapped by this one. </param>
        public ActorException(String message, Exception innerException) : base(message, innerException) { }
    }
}
