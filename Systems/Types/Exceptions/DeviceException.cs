using System;

namespace Khepri.Types.Exceptions
{
    /// <summary> An exception thrown by a device entity. </summary>
    public class DeviceException : Exception
    {
        /// <summary> An exception thrown by a device entity. </summary>
        public DeviceException() { }


        /// <summary> An exception thrown by a device entity. </summary>
        /// <param name="message"> The exception's error message. </param>
        public DeviceException(String message) : base(message) { }


        /// <summary> An exception thrown by a device entity. </summary>
        /// <param name="message"> The exception's error message. </param>
        /// <param name="innerException"> The exception wrapped by this one. </param>
        public DeviceException(String message, Exception innerException) : base(message, innerException) { }
    }
}
