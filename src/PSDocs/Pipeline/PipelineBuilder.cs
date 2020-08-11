
using PSDocs.Configuration;
using PSDocs.Processor;
using System;
using System.IO;
using System.Management.Automation;
using System.Text;

namespace PSDocs.Pipeline
{
    public static class PipelineBuilder
    {
        public static IInvokePipelineBuilder Invoke(Source[] source, PSDocumentOption option)
        {
            var builder = new InvokePipelineBuilder(source);
            builder.Configure(option);
            return builder;
        }

        public static SourceBuilder Source()
        {
            return new SourceBuilder();
        }
    }

    public interface IPipelineBuilder
    {
        void UseCommandRuntime(ICommandRuntime2 commandRuntime);

        void UseExecutionContext(EngineIntrinsics executionContext);

        IPipelineBuilder Configure(PSDocumentOption option);

        IPipeline Build();
    }

    public interface IPipeline
    {
        void Process(PSObject sourceObject);
    }

    internal abstract class PipelineBuilderBase : IPipelineBuilder
    {
        protected readonly Source[] Source;
        protected readonly PSDocumentOption Option;
        protected readonly PipelineLogger Logger;

        protected Action<IDocumentResult, bool> OutputVisitor;

        internal PipelineBuilderBase(Source[] source)
        {
            Option = new PSDocumentOption();
            Source = source;
            Logger = new PipelineLogger();
            OutputVisitor = (o, enumerate) => WriteToString(o, enumerate, Logger);
        }

        public virtual void UseCommandRuntime(ICommandRuntime2 commandRuntime)
        {
            Logger.UseCommandRuntime(commandRuntime);
        }

        public void UseExecutionContext(EngineIntrinsics executionContext)
        {
            Logger.UseExecutionContext(executionContext);
        }

        public virtual IPipelineBuilder Configure(PSDocumentOption option)
        {
            Option.Document = new DocumentOption(option.Document);
            Option.Execution = new ExecutionOption(option.Execution);
            Option.Markdown = new MarkdownOption(option.Markdown);
            Option.Output = new OutputOption(option.Output);

            if (!string.IsNullOrEmpty(Option.Output.Path))
            {
                OutputVisitor = (o, enumerate) => WriteToFile(o, enumerate, Option, Logger);
            }

            ConfigureCulture();
            return this;
        }

        public abstract IPipeline Build();

        protected virtual PipelineContext PrepareContext()
        {
            return new PipelineContext(Option, Logger, OutputVisitor, null);
        }

        private static void WriteToFile(IDocumentResult result, bool enumerate, PSDocumentOption option, PipelineLogger logger)
        {
            var outputPath = PSDocumentOption.GetRootedPath(option.Output.Path);
            var filePath = Path.Combine(outputPath, result.Name);
            var encoding = GetEncoding(option.Markdown.Encoding);
            File.WriteAllText(filePath, result.ToString(), encoding);

            // Write file info instead
            var fileInfo = new FileInfo(filePath);
            logger.WriteObject(fileInfo, false);
        }

        private static void WriteToString(IDocumentResult result, bool enumerate, PipelineLogger logger)
        {
            logger.WriteObject(result.ToString(), enumerate);
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
        private bool _Disposed = false;

        protected PipelineBase(PipelineContext context, Source[] source)
        {
            Context = context;
            Source = source;
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
