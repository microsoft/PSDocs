// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.IO;
using System.Management.Automation;
using System.Text;
using PSDocs.Configuration;
using PSDocs.Pipeline.Output;
using PSDocs.Processor;
using PSDocs.Resources;

namespace PSDocs.Pipeline
{
    internal delegate bool ShouldProcess(string target, string action);

    public static class PipelineBuilder
    {
        /// <summary>
        /// Invoke-PSDocument.
        /// </summary>
        public static IInvokePipelineBuilder Invoke(Source[] source, IPSDocumentOption option, PSCmdlet commandRuntime, EngineIntrinsics executionContext)
        {
            var hostContext = new HostContext(commandRuntime, executionContext);
            var builder = new InvokePipelineBuilder(source, hostContext);
            builder.Configure(option);
            return builder;
        }

        public static IGetPipelineBuilder Get(Source[] source, IPSDocumentOption option, PSCmdlet commandRuntime, EngineIntrinsics executionContext)
        {
            var hostContext = new HostContext(commandRuntime, executionContext);
            var builder = new GetPipelineBuilder(source, hostContext);
            builder.Configure(option);
            return builder;
        }

        public static SourcePipelineBuilder Source(IPSDocumentOption option, PSCmdlet commandRuntime, EngineIntrinsics executionContext)
        {
            var hostContext = new HostContext(commandRuntime, executionContext);
            var builder = new SourcePipelineBuilder(hostContext);
            //builder.Configure(option);
            return builder;
        }
    }

    public interface IPipelineBuilder
    {
        IPipelineBuilder Configure(IPSDocumentOption option);

        IPipeline Build();
    }

    /// <summary>
    /// Objects that follow the pipeline lifecycle.
    /// </summary>
    public interface IPipelineDisposable : IDisposable
    {
        void Begin();

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Naming", "CA1716:Identifiers should not match keywords", Justification = "Matches PowerShell pipeline.")]
        void End();
    }

    public interface IPipeline : IPipelineDisposable
    {
        void Process(PSObject sourceObject);
    }

    internal abstract class PipelineBuilderBase : IPipelineBuilder
    {
        protected readonly Source[] Source;
        protected readonly PSDocumentOption Option;
        protected readonly IPipelineWriter Writer;
        protected readonly ShouldProcess ShouldProcess;

        protected Action<IDocumentResult, bool> OutputVisitor;
        protected VisitTargetObject VisitTargetObject;

        private static readonly ShouldProcess EmptyShouldProcess = (target, action) => true;

        internal PipelineBuilderBase(Source[] source, HostContext hostContext)
        {
            Option = new PSDocumentOption();
            Source = source;
            Writer = new HostPipelineWriter(hostContext);
            ShouldProcess = hostContext == null ? EmptyShouldProcess : hostContext.ShouldProcess;
            OutputVisitor = (o, enumerate) => WriteToString(o, enumerate, Writer);
            VisitTargetObject = PipelineReceiverActions.PassThru;
        }

        public virtual IPipelineBuilder Configure(IPSDocumentOption option)
        {
            Option.Configuration = new ConfigurationOption(option.Configuration);
            Option.Document = DocumentOption.Combine(option.Document, DocumentOption.Default);
            Option.Execution = ExecutionOption.Combine(option.Execution, ExecutionOption.Default);
            Option.Input = InputOption.Combine(option.Input, InputOption.Default);
            Option.Markdown = MarkdownOption.Combine(option.Markdown, MarkdownOption.Default);
            Option.Output = OutputOption.Combine(option.Output, OutputOption.Default);

            if (!string.IsNullOrEmpty(Option.Output.Path))
                OutputVisitor = (o, enumerate) => WriteToFile(o, Option, Writer, ShouldProcess);

            ConfigureCulture();
            return this;
        }

        public abstract IPipeline Build();

        /// <summary>
        /// Require sources for pipeline execution.
        /// </summary>
        /// <returns>Returns true when the condition is not met.</returns>
        protected bool RequireSources()
        {
            if (Source == null || Source.Length == 0)
            {
                Writer.WarnSourcePathNotFound();
                return true;
            }
            return false;
        }

        /// <summary>
        /// Require culture for pipeline exeuction.
        /// </summary>
        /// <returns>Returns true when the condition is not met.</returns>
        protected bool RequireCulture()
        {
            if (Option.Output.Culture == null || Option.Output.Culture.Length == 0)
            {
                Writer.ErrorInvariantCulture();
                return true;
            }
            return false;
        }

        protected virtual PipelineContext PrepareContext()
        {
            return new PipelineContext(GetOptionContext(), PrepareStream(), Writer, OutputVisitor, null, null);
        }

        protected virtual OptionContext GetOptionContext()
        {
            return new OptionContext(Option);
        }

        protected virtual PipelineStream PrepareStream()
        {
            return new PipelineStream(null, null);
        }

        private static void WriteToFile(IDocumentResult result, PSDocumentOption option, IPipelineWriter writer, ShouldProcess shouldProcess)
        {
            // Calculate paths
            var fileName = string.Concat(result.InstanceName, result.Extension);
            var outputPath = PSDocumentOption.GetRootedPath(result.OutputPath);
            var filePath = Path.Combine(outputPath, fileName);
            var parentPath = Directory.GetParent(filePath);

            if (!parentPath.Exists && shouldProcess(target: parentPath.FullName, action: PSDocsResources.ShouldCreatePath))
                Directory.CreateDirectory(path: parentPath.FullName);

            if (shouldProcess(target: outputPath, action: PSDocsResources.ShouldWriteFile))
            {
                var encoding = GetEncoding(option.Markdown.Encoding.Value);
                File.WriteAllText(filePath, result.ToString(), encoding);

                // Write file info instead
                var fileInfo = new FileInfo(filePath);
                writer.WriteObject(fileInfo, false);
            }
        }

        private static void WriteToString(IDocumentResult result, bool enumerate, IPipelineWriter writer)
        {
            writer.WriteObject(result.ToString(), enumerate);
        }

        private static Encoding GetEncoding(MarkdownEncoding encoding)
        {
            switch (encoding)
            {
                case MarkdownEncoding.UTF7:
                    return Encoding.UTF7;
                case MarkdownEncoding.UTF8:
                    return Encoding.UTF8;
                case MarkdownEncoding.ASCII:
                    return Encoding.ASCII;
                case MarkdownEncoding.Unicode:
                    return Encoding.Unicode;
                case MarkdownEncoding.UTF32:
                    return Encoding.UTF32;
                default:
                    return new UTF8Encoding(false);
            }
        }

        private void ConfigureCulture()
        {
            if (Option.Output.Culture == null || Option.Output.Culture.Length == 0)
            {
                // Fallback to current culture
                var current = PSDocumentOption.GetCurrentCulture();
                if (current == null || string.IsNullOrEmpty(current.Name))
                    return;

                Option.Output.Culture = new string[] { current.Name };
            }
        }

        protected void AddVisitTargetObjectAction(VisitTargetObjectAction action)
        {
            // Nest the previous write action in the new supplied action
            // Execution chain will be: action -> previous -> previous..n
            var previous = VisitTargetObject;
            VisitTargetObject = (targetObject) => action(targetObject, previous);
        }
    }

    internal abstract class PipelineBase : IDisposable
    {
        protected readonly PipelineContext Context;
        protected readonly Source[] Source;

        // Track whether Dispose has been called.
        private bool _Disposed;

        protected PipelineBase(PipelineContext context, Source[] source)
        {
            Context = context;
            Source = source;
        }

        public virtual void Begin()
        {
            // Do nothing
        }

        public virtual void Process(PSObject sourceObject)
        {
            // Do nothing
        }

        public virtual void End()
        {
            // Do nothing
        }

        #region IDisposable

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_Disposed)
            {
                if (disposing)
                {
                    Context.Dispose();
                }
                _Disposed = true;
            }
        }

        #endregion IDisposable
    }
}
