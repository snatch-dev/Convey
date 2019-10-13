using System;

namespace Convey.Types
{
    public class ConveyException : Exception
    {
        public string Code { get; }

        public ConveyException()
        {
        }

        public ConveyException(string code)
        {
            Code = code;
        }

        public ConveyException(string message, params object[] args) 
            : this(string.Empty, message, args)
        {
        }

        public ConveyException(string code, string message, params object[] args) 
            : this(null, code, message, args)
        {
        }

        public ConveyException(Exception innerException, string message, params object[] args)
            : this(innerException, string.Empty, message, args)
        {
        }

        public ConveyException(Exception innerException, string code, string message, params object[] args)
            : base(string.Format(message, args), innerException)
        {
            Code = code;
        }        
    }
}