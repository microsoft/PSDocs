using System;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace PSDocs.Pipeline
{
    /// <summary>
    /// A base class for all pipeline exceptions.
    /// </summary>
    public abstract class PipelineExeception : Exception
    {
        /// <summary>
        /// Creates a pipeline exception.
        /// </summary>
        protected PipelineExeception()
        {
        }

        /// <summary>
        /// Creates a pipeline exception.
        /// </summary>
        /// <param name="message">The detail of the exception.</param>
        protected PipelineExeception(string message) : base(message)
        {
        }

        /// <summary>
        /// Creates a pipeline exception.
        /// </summary>
        /// <param name="message">The detail of the exception.</param>
        /// <param name="innerException">A nested exception that caused the issue.</param>
        protected PipelineExeception(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected PipelineExeception(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }

    [Serializable]
    public sealed class RuntimeException : PipelineExeception
    {
        /// <summary>
        /// Creates a serialization exception.
        /// </summary>
        public RuntimeException()
        {
        }

        /// <summary>
        /// Creates a serialization exception.
        /// </summary>
        /// <param name="message">The detail of the exception.</param>
        internal RuntimeException(string message) : base(message)
        {
        }

        internal RuntimeException(string sourceFile, Exception innerException) : base(innerException.Message, innerException)
        {
            SourceFile = sourceFile;
        }

        private RuntimeException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public string SourceFile { get; }

        [SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (info == null) throw new ArgumentNullException("info");
            base.GetObjectData(info, context);
        }
    }
}
