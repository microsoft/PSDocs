
using PSDocs.Data;
using PSDocs.Models;
using PSDocs.Processor;
using PSDocs.Processor.Markdown;
using PSDocs.Runtime;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Management.Automation;

namespace PSDocs.Pipeline
{
    public interface IInvokePipelineBuilder : IPipelineBuilder
    {
        void InstanceName(string[] instanceName);

        void Convention(string[] convention);
    }

    /// <summary>
    /// The pipeline builder for Invoke-PSDocument.
    /// </summary>
    internal sealed class InvokePipelineBuilder : PipelineBuilderBase, IInvokePipelineBuilder
    {
        private string[] _InstanceName;
        private string[] _Convention;

        internal InvokePipelineBuilder(Source[] source, HostContext hostContext)
            : base(source, hostContext)
        {
            // Do nothing
        }

        public void InstanceName(string[] instanceName)
        {
            if (instanceName == null || instanceName.Length == 0)
                return;

            _InstanceName = instanceName;
        }

        public void Convention(string[] convention)
        {
            if (convention == null || convention.Length == 0)
                return;

            _Convention = convention;
        }

        public override IPipeline Build()
        {
            if (RequireSources() || RequireCulture())
                return null;

            return new InvokePipeline(PrepareContext(), Source);
        }

        protected override PipelineContext PrepareContext()
        {
            var instanceNameBinder = new InstanceNameBinder(_InstanceName);
            var context = new PipelineContext(Option, Writer, OutputVisitor, instanceNameBinder, _Convention);
            return context;
        }
    }

    /// <summary>
    /// The pipeline for Invoke-PSDocument.
    /// </summary>
    internal sealed class InvokePipeline : StreamPipeline, IPipeline
    {
        private readonly List<IDocumentResult> _Completed;

        private IDocumentBuilder[] _Builder;
        private MarkdownProcessor _Processor;
        private RunspaceContext _Runspace;

        internal InvokePipeline(PipelineContext context, Source[] source)
            : base(context, source)
        {
            _Runspace = new RunspaceContext(Context);
            _Builder = HostHelper.GetDocumentBuilder(_Runspace, Source);
            _Processor = new MarkdownProcessor();
            _Completed = new List<IDocumentResult>();
        }

        protected override void ProcessObject(PSObject sourceObject)
        {
            try
            {
                var doc = BuildDocument(sourceObject);
                for (var i = 0; i < doc.Length; i++)
                {
                    var result = WriteDocument(doc[i]);
                    if (result != null)
                    {
                        Context.WriteOutput(result);
                        _Completed.Add(result);
                    }
                }
            }
            finally
            {
                _Runspace.ExitTargetObject();
            }
        }

        public override void End()
        {
            if (_Completed.Count == 0)
                return;

            var completed = _Completed.ToArray();
            _Runspace.SetOutput(completed);
            try
            {
                for (var i = 0; i < _Builder.Length; i++)
                {
                    _Builder[i].End(_Runspace, completed);
                }
            }
            finally
            {
                _Runspace.ClearOutput();
            }
        }

        private IDocumentResult WriteDocument(Document document)
        {
            return _Processor.Process(Context.Option, document);
        }

        internal Document[] BuildDocument(PSObject sourceObject)
        {
            _Runspace.EnterTargetObject(sourceObject);
            var result = new List<Document>();
            for (var c = 0; c < Context.Option.Output.Culture.Length; c++)
            {
                _Runspace.EnterCulture(Context.Option.Output.Culture[c]);
                for (var i = 0; i < _Builder.Length; i++)
                {
                    foreach (var instanceName in Context.InstanceNameBinder.GetInstanceName(_Builder[i].Name))
                    {
                        _Runspace.EnterDocument(instanceName);
                        try
                        {
                            // TODO: Add target name binding
                            var document = _Builder[i].Process(_Runspace, sourceObject);
                            result.Add(document);
                        }
                        finally
                        {
                            _Runspace.ExitDocument();
                        }
                    }
                }
            }
            return result.ToArray();
        }

        #region IDisposable

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _Processor = null;
                if (_Builder != null)
                {
                    for (var i = 0; i < _Builder.Length; i++)
                        _Builder[i].Dispose();

                    _Builder = null;
                }
                _Runspace.Dispose();
                _Runspace = null;
            }
            base.Dispose(disposing);
        }

        #endregion IDisposable
    }
}
