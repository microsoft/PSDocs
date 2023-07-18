// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Management.Automation;

namespace PSDocs.Pipeline
{
    internal abstract class StreamPipeline : PipelineBase
    {
        private readonly PipelineStream _Stream;

        internal StreamPipeline(PipelineContext context, Source[] source)
            : base(context, source)
        {
            _Stream = context.Stream;
        }

        public override void Begin()
        {
            _Stream.Open();
        }

        public sealed override void Process(PSObject sourceObject)
        {
            _Stream.Enqueue(sourceObject);
            while (!_Stream.IsEmpty && _Stream.TryDequeue(out var nextObject))
                ProcessObject(nextObject);
        }

        protected abstract void ProcessObject(TargetObject targetObject);
    }
}
