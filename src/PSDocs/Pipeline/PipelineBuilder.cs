
using PSDocs.Configuration;
using PSDocs.Pipeline.Output;
using PSDocs.Processor;
using System;
using System.IO;
using System.Management.Automation;
using System.Text;

namespace PSDocs.Pipeline
{
    public static class PipelineBuilder
    {
        public static IInvokePipelineBuilder Invoke(Source[] source, PSDocumentOption option, PSCmdlet commandRuntime, EngineIntrinsics executionContext)
        {
            var hostContext = new HostContext(commandRuntime, executionContext);
            var builder = new InvokePipelineBuilder(source, hostContext);
            builder.Configure(option);
            return builder;
        }

        public static SourceBuilder Source(PSDocumentOption option, PSCmdlet commandRuntime, EngineIntrinsics executionContext)
        {
            var hostContext = new HostContext(commandRuntime, executionContext);
            var builder = new SourceBuilder(hostContext);
            builder.Configure(option);
            return builder;
        }
    }

    public interface IPipelineBuilder
    {
        IPipelineBuilder Configure(PSDocumentOption option);

        IPipeline Build();
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

        protected Action<IDocumentResult, bool> OutputVisitor;

        internal PipelineBuilderBase(Source[] source, HostContext hostContext)
        {
            Option = new PSDocumentOption();
            
            Source = source;
            Writer = new HostPipelineWriter(hostContext);
            OutputVisitor = (o, enumerate) => WriteToString(o, enumerate, Writer);
        }

        public virtual IPipelineBuilder Configure(PSDocumentOption option)
        {
            Option.Document = new DocumentOption(option.Document);
            Option.Execution = new ExecutionOption(option.Execution);
            Option.Markdown = new MarkdownOption(option.Markdown);
            Option.Output = new OutputOption(option.Output);

            if (!string.IsNullOrEmpty(Option.Output.Path))
            {
                OutputVisitor = (o, enumerate) => WriteToFile(o, enumerate, Option, Writer);
            }

            ConfigureCulture();
            return this;
        }

        public abstract IPipeline Build();

        protected virtual PipelineContext PrepareContext()
        {
            return new PipelineContext(Option, Writer, OutputVisitor, null);
        }

        private static void WriteToFile(IDocumentResult result, bool enumerate, PSDocumentOption option, IPipelineWriter writer)
        {
            var outputPath = PSDocumentOption.GetRootedPath(option.Output.Path);
            var filePath = Path.Combine(outputPath, result.Name);
            var encoding = GetEncoding(option.Markdown.Encoding);
            File.WriteAllText(filePath, result.ToString(), encoding);

            // Write file info instead
            var fileInfo = new FileInfo(filePath);
            writer.WriteObject(fileInfo, false);
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
                Option.Output.Culture = new string[] { current.Name };
            }
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
