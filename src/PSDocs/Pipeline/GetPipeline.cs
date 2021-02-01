using PSDocs.Runtime;

namespace PSDocs.Pipeline
{
    public interface IGetPipelineBuilder : IPipelineBuilder
    {
        
    }

    /// <summary>
    /// A helper to construct a get pipeline.
    /// </summary>
    internal sealed class GetPipelineBuilder : PipelineBuilderBase, IGetPipelineBuilder
    {
        internal GetPipelineBuilder(Source[] source, HostContext hostContext)
            : base(source, hostContext) { }

        public override IPipeline Build()
        {
            if (RequireSources())
                return null;

            return new GetPipeline(PrepareContext(), Source);
        }
    }

    internal sealed class GetPipeline : PipelineBase, IPipeline
    {
        private readonly RunspaceContext _Runspace;

        internal GetPipeline(PipelineContext context, Source[] source)
            : base(context, source)
        {
            _Runspace = new RunspaceContext(Context);
        }

        public override void End()
        {
            Context.Writer.WriteObject(HostHelper.GetDocumentBlock(_Runspace, Source), true);
        }
    }
}
