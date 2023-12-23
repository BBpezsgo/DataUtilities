using System;

#nullable enable

namespace DataUtilities
{
    public class EndlessLoopException : Exception
    {
        public EndlessLoopException() { }
        public EndlessLoopException(string message) : base(message) { }
        public EndlessLoopException(string message, Exception inner) : base(message, inner) { }
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
