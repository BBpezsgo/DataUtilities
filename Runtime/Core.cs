using System;

namespace DataUtilities
{
    [Serializable]
    public class EndlessLoopException : Exception
    {
        public EndlessLoopException() { }
        public EndlessLoopException(string message) : base(message) { }
        public EndlessLoopException(string message, Exception inner) : base(message, inner) { }
        protected EndlessLoopException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
