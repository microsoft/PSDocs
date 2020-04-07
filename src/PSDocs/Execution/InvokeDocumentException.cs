using System;

namespace PSDocs.Execution
{
    public sealed class InvokeDocumentException : Exception
    {
        public InvokeDocumentException()
        {

        }

        public InvokeDocumentException(string message)
            : base(message)
        {

        }

        public InvokeDocumentException(string message, Exception innerException)
            : base(message, innerException)
        {

        }

        public InvokeDocumentException(string message, Exception innerException, string path, string positionMessage)
            : base(message, innerException)
        {
            Path = path;
            PositionMessage = positionMessage;
        }

        public string Path { get; }

        public string PositionMessage { get; }
    }
}
