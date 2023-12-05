using System;

#nullable enable

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

#if !NET7_0_OR_GREATER
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
    sealed class RequiresUnreferencedCodeAttribute : Attribute
    {
        public RequiresUnreferencedCodeAttribute(string _)
        {
        }
    }
#endif
}
