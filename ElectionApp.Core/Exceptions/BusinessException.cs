using System;

namespace ElectionApp.Core.Exceptions
{
    public abstract class BusinessException : Exception
    {
        public abstract ushort Code { get;}
        protected BusinessException(string message) : base(message) { }
    }
}