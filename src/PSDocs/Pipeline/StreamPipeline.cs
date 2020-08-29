using System.Management.Automation;

namespace PSDocs.Pipeline
{
    internal abstract class StreamPipeline : PipelineBase
    {
        private readonly PipelineStream _Stream;

        internal StreamPipeline(PipelineContext context, Source[] source)
            : base(context, source)
        {
            _Stream = new PipelineStream(context);
        }

        public override void Process(PSObject sourceObject)
        {
            //if (sourceObject != null)
            //{
            _Stream.Enqueue(sourceObject);
            //}

            while (!_Stream.IsEmpty && _Stream.TryDequeue(out PSObject nextObject))
                ProcessObject(nextObject);
        }

        protected abstract void ProcessObject(PSObject sourceObject);
    }
}
