// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using PSDocs.Resources;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Management.Automation;
using System.Management.Automation.Language;
using System.Threading;

namespace PSDocs.Pipeline
{
    public interface IPipelineWriter : IPipelineDisposable
    {
        void WriteError(ErrorRecord record);

        void WriteWarning(string text, params object[] args);

        void WriteInformation(InformationRecord record);

        void WriteVerbose(string text, params object[] args);

        void WriteDebug(DebugRecord record);

        void WriteDebug(string text, params object[] args);

        void WriteObject(object sendToPipeline, bool enumerateCollection);

        void WriteHost(HostInformationMessage info);
    }

    public static class PipelineWriterExtensions
    {
        [DebuggerStepThrough]
        public static void WriteError(this IPipelineWriter writer, Exception exception, string errorId, ErrorCategory errorCategory, object targetObject)
        {
            if (writer == null)
                return;

            writer.WriteError(new ErrorRecord(exception, errorId, errorCategory, targetObject));
        }

        public static void Debug(this IPipelineWriter writer, string message)
        {
            if (writer == null)
                return;

            writer.WriteDebug(new DebugRecord(message));
        }

        public static void WarnSourcePathNotFound(this IPipelineWriter writer)
        {
            if (writer == null)
                return;

            writer.WriteWarning(PSDocsResources.SourceNotFound);
        }

        public static void WarnTitleEmpty(this IPipelineWriter writer)
        {
            if (writer == null)
                return;

            writer.WriteWarning(PSDocsResources.TitleEmpty);
        }

        public static void ErrorInvariantCulture(this IPipelineWriter writer)
        {
            if (writer == null)
                return;

            writer.WriteError(new ErrorRecord(
                exception: new PipelineBuilderException(PSDocsResources.InvariantCulture),
                errorId: "PSDocs.PipelineBuilder.InvariantCulture",
                errorCategory: ErrorCategory.InvalidOperation,
                null
            ));
        }

        public static void WriteError(this IPipelineWriter writer, ParseError error)
        {
            if (writer == null)
                return;

            var record = new ErrorRecord
            (
                exception: new ParseException(message: error.Message, errorId: error.ErrorId),
                errorId: error.ErrorId,
                errorCategory: ErrorCategory.InvalidOperation,
                targetObject: null
            );
            writer.WriteError(record);
        }
    }

    /// <summary>
    /// A base class for implementing IPipelineWriter.
    /// </summary>
    public abstract class PipelineWriterBase
    {
        protected PipelineWriterBase(IHostContext hostContext)
        {
            HostContext = hostContext;
        }

        protected IHostContext HostContext { get; }

        public virtual void Begin()
        {
            // Do nothing
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Naming", "CA1716:Identifiers should not match keywords", Justification = "Matches PowerShell pipeline.")]
        public virtual void End()
        {
            // Do nothing
        }

        public void WriteError(ErrorRecord record)
        {
            if (record == null || !ShouldWriteError())
                return;

            DoWriteError(record);
        }

        public void WriteWarning(string text, params object[] args)
        {
            if (string.IsNullOrEmpty(text) || !ShouldWriteWarning())
                return;

            text = args == null || args.Length == 0 ? text : string.Format(Thread.CurrentThread.CurrentCulture, text, args);
            DoWriteWarning(text);
        }

        public void WriteInformation(InformationRecord record)
        {
            if (record == null || !ShouldWriteInformation())
                return;

            DoWriteInformation(record);
        }

        public void WriteVerbose(string text, params object[] args)
        {
            if (string.IsNullOrEmpty(text) || !ShouldWriteVerbose())
                return;

            text = args == null || args.Length == 0 ? text : string.Format(Thread.CurrentThread.CurrentCulture, text, args);
            DoWriteVerbose(text);
        }

        public void WriteDebug(DebugRecord record)
        {
            if (record == null || !ShouldWriteDebug())
                return;

            DoWriteDebug(record);
        }

        public void WriteDebug(string text, params object[] args)
        {
            if (string.IsNullOrEmpty(text) || !ShouldWriteDebug())
                return;

            text = args == null || args.Length == 0 ? text : string.Format(Thread.CurrentThread.CurrentCulture, text, args);
            DoWriteDebug(new DebugRecord(text));
        }

        public void WriteObject(object sendToPipeline, bool enumerateCollection)
        {
            if (sendToPipeline == null || !ShouldWriteObject())
                return;

            DoWriteObject(sendToPipeline, enumerateCollection);
        }

        public void WriteHost(HostInformationMessage info)
        {
            if (info == null)
                return;

            DoWriteHost(info);
        }

        protected virtual bool ShouldWriteError()
        {
            return HostContext.GetErrorPreference() != ActionPreference.Ignore;
        }

        protected virtual bool ShouldWriteWarning()
        {
            return HostContext.GetWarningPreference() != ActionPreference.Ignore;
        }

        protected virtual bool ShouldWriteInformation()
        {
            return HostContext.GetInformationPreference() != ActionPreference.Ignore;
        }

        protected virtual bool ShouldWriteVerbose()
        {
            return HostContext.GetVerbosePreference() != ActionPreference.Ignore;
        }

        protected virtual bool ShouldWriteDebug()
        {
            return HostContext.GetDebugPreference() != ActionPreference.Ignore;
        }

        protected virtual bool ShouldWriteObject()
        {
            return true;
        }

        protected virtual void DoWriteError(ErrorRecord record)
        {

        }

        protected virtual void DoWriteWarning(string text)
        {

        }

        protected virtual void DoWriteInformation(InformationRecord record)
        {

        }

        protected virtual void DoWriteVerbose(string text)
        {

        }

        protected virtual void DoWriteDebug(DebugRecord record)
        {

        }

        protected virtual void DoWriteObject(object sendToPipeline, bool enumerateCollection)
        {

        }

        protected virtual void DoWriteHost(HostInformationMessage info)
        {

        }
    }

    public abstract class PipelineWriter : PipelineWriterBase
    {
        private readonly IPipelineWriter _Inner;

        protected PipelineWriter(IHostContext hostContext, IPipelineWriter inner)
            : base(hostContext)
        {
            _Inner = inner;
        }

        public override void Begin()
        {
            if (_Inner == null)
                return;

            _Inner.Begin();
        }

        public override void End()
        {
            if (_Inner == null)
                return;

            _Inner.End();
        }

        //public void WriteVerbose(string format, params object[] args)
        //{
        //    if (!ShouldWriteVerbose())
        //        return;

        //    WriteVerbose(string.Format(Thread.CurrentThread.CurrentCulture, format, args));
        //}

        protected override void DoWriteError(ErrorRecord errorRecord)
        {
            if (_Inner == null)
                return;

            _Inner.WriteError(errorRecord);
        }

        protected override void DoWriteWarning(string message)
        {
            if (_Inner == null)
                return;

            _Inner.WriteWarning(message);
        }

        protected override void DoWriteInformation(InformationRecord informationRecord)
        {
            if (_Inner == null)
                return;

            _Inner.WriteInformation(informationRecord);
        }

        protected override void DoWriteVerbose(string message)
        {
            if (_Inner == null)
                return;

            _Inner.WriteVerbose(message);
        }

        protected override void DoWriteDebug(DebugRecord debugRecord)
        {
            if (_Inner == null || debugRecord == null)
                return;

            _Inner.WriteDebug(debugRecord);
        }

        protected override void DoWriteObject(object sendToPipeline, bool enumerateCollection)
        {
            if (_Inner == null)
                return;

            _Inner.WriteObject(sendToPipeline, enumerateCollection);
        }

        protected override void DoWriteHost(HostInformationMessage info)
        {
            if (_Inner == null)
                return;

            _Inner.WriteHost(info);
        }
    }

    internal abstract class BufferedPipelineWriter<T> : PipelineWriter
    {
        private readonly List<T> _Result;

        protected BufferedPipelineWriter(IHostContext hostContext, IPipelineWriter inner)
            : base(hostContext, inner)
        {
            _Result = new List<T>();
        }

        protected override void DoWriteObject(object sendToPipeline, bool enumerateCollection)
        {
            Add(sendToPipeline);
            if (ShouldFlush())
                Flush();
        }

        protected void Add(object o)
        {
            if (o is T[] collection)
                _Result.AddRange(collection);
            else if (o is T item)
                _Result.Add(item);
        }

        public void Flush()
        {
            var results = _Result.ToArray();
            _Result.Clear();
            Flush(results);
        }

        public sealed override void End()
        {
            Flush();
        }

        protected virtual bool ShouldFlush()
        {
            return false;
        }

        protected virtual void Flush(T[] o)
        {
            base.WriteObject(o, true);
        }
    }
}
