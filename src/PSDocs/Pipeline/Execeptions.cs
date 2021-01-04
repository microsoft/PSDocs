using System;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace PSDocs.Pipeline
{
    /// <summary>
    /// A base class for all pipeline exceptions.
    /// </summary>
    public abstract class PipelineException : Exception
    {
        /// <summary>
        /// Creates a pipeline exception.
        /// </summary>
        protected PipelineException()
        {
        }

        /// <summary>
        /// Creates a pipeline exception.
        /// </summary>
        /// <param name="message">The detail of the exception.</param>
        protected PipelineException(string message) : base(message)
        {
        }

        /// <summary>
        /// Creates a pipeline exception.
        /// </summary>
        /// <param name="message">The detail of the exception.</param>
        /// <param name="innerException">A nested exception that caused the issue.</param>
        protected PipelineException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected PipelineException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }

    [Serializable]
    public sealed class RuntimeException : PipelineException
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
            if (info == null) throw new ArgumentNullException(nameof(info));
            base.GetObjectData(info, context);
        }
    }

    [Serializable]
    public sealed class InvokeDocumentException : PipelineException
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

        private InvokeDocumentException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public string Path { get; }

        public string PositionMessage { get; }

        [SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (info == null) throw new ArgumentNullException(nameof(info));
            base.GetObjectData(info, context);
        }
    }

    /// <summary>
    /// An exception when building the pipeline.
    /// </summary>
    [Serializable]
    public sealed class PipelineBuilderException : PipelineException
    {
        /// <summary>
        /// Creates a pipeline builder exception.
        /// </summary>
        public PipelineBuilderException()
            : base() { }

        /// <summary>
        /// Creates a pipeline builder exception.
        /// </summary>
        /// <param name="message">The detail of the exception.</param>
        public PipelineBuilderException(string message)
            : base(message) { }

        /// <summary>
        /// Creates a pipeline builder exception.
        /// </summary>
        /// <param name="message">The detail of the exception.</param>
        /// <param name="innerException">A nested exception that caused the issue.</param>
        public PipelineBuilderException(string message, Exception innerException)
            : base(message, innerException) { }

        private PipelineBuilderException(SerializationInfo info, StreamingContext context)
            : base(info, context) { }

        [SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
                throw new ArgumentNullException(nameof(info));

            base.GetObjectData(info, context);
        }
    }

    /// <summary>
    /// A serialization exception.
    /// </summary>
    [Serializable]
    public sealed class PipelineSerializationException : PipelineException
    {
        /// <summary>
        /// Creates a serialization exception.
        /// </summary>
        public PipelineSerializationException()
        {
        }

        /// <summary>
        /// Creates a serialization exception.
        /// </summary>
        /// <param name="message">The detail of the exception.</param>
        public PipelineSerializationException(string message) : base(message)
        {
        }

        /// <summary>
        /// Creates a serialization exception.
        /// </summary>
        /// <param name="message">The detail of the exception.</param>
        /// <param name="innerException">A nested exception that caused the issue.</param>
        public PipelineSerializationException(string message, Exception innerException) : base(message, innerException)
        {
        }

        private PipelineSerializationException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        [SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
                throw new ArgumentNullException(nameof(info));

            base.GetObjectData(info, context);
        }
    }
}
