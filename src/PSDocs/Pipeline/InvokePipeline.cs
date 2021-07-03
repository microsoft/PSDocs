// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using PSDocs.Configuration;
using PSDocs.Data;
using PSDocs.Models;
using PSDocs.Processor;
using PSDocs.Processor.Markdown;
using PSDocs.Runtime;
using System.Collections.Generic;

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
        private InputFileInfo[] _InputPath;


        internal InvokePipelineBuilder(Source[] source, HostContext hostContext)
            : base(source, hostContext)
        {
            _InputPath = null;
        }

        public void InputPath(string[] path)
        {
            if (path == null || path.Length == 0)
                return;

            var basePath = PSDocumentOption.GetWorkingPath();
            var filter = PathFilterBuilder.Create(basePath, Option.Input.PathIgnore);
            filter.UseGitIgnore();

            var builder = new InputPathBuilder(Writer, basePath, "*", filter.Build());
            builder.Add(path);
            _InputPath = builder.Build();
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
            var context = new PipelineContext(GetOptionContext(), PrepareStream(), Writer, OutputVisitor, instanceNameBinder, _Convention);
            return context;
        }

        protected override PipelineStream PrepareStream()
        {
            if (!string.IsNullOrEmpty(Option.Input.ObjectPath))
            {
                AddVisitTargetObjectAction((targetObject, next) =>
                {
                    return PipelineReceiverActions.ReadObjectPath(targetObject, next, Option.Input.ObjectPath, true);
                });
            }

            if (Option.Input.Format == InputFormat.Yaml)
            {
                AddVisitTargetObjectAction((targetObject, next) =>
                {
                    return PipelineReceiverActions.ConvertFromYaml(targetObject, next);
                });
            }
            else if (Option.Input.Format == InputFormat.Json)
            {
                AddVisitTargetObjectAction((targetObject, next) =>
                {
                    return PipelineReceiverActions.ConvertFromJson(targetObject, next);
                });
            }
            else if (Option.Input.Format == InputFormat.PowerShellData)
            {
                AddVisitTargetObjectAction((targetObject, next) =>
                {
                    return PipelineReceiverActions.ConvertFromPowerShellData(targetObject, next);
                });
            }
            else if (Option.Input.Format == InputFormat.Detect && _InputPath != null)
            {
                AddVisitTargetObjectAction((targetObject, next) =>
                {
                    return PipelineReceiverActions.DetectInputFormat(targetObject, next);
                });
            }
            return new PipelineStream(VisitTargetObject, _InputPath);
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
            HostHelper.ImportResource(Source, _Runspace);
            _Builder = HostHelper.GetDocumentBuilder(_Runspace, Source);
            _Processor = new MarkdownProcessor();
            _Completed = new List<IDocumentResult>();
        }

        protected override void ProcessObject(TargetObject targetObject)
        {
            try
            {
                var doc = BuildDocument(targetObject);
                for (var i = 0; doc != null && i < doc.Length; i++)
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

        internal Document[] BuildDocument(TargetObject targetObject)
        {
            _Runspace.EnterTargetObject(targetObject);
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
                            var document = _Builder[i].Process(_Runspace, targetObject.Value);
                            if (document != null)
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
