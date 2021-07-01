// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using PSDocs.Configuration;
using PSDocs.Models;
using PSDocs.Processor;
using System;

namespace PSDocs.Pipeline
{
    /// <summary>
    /// A context for an end-to-end pipeline execution.
    /// </summary>
    internal sealed class PipelineContext : IDisposable
    {
        internal readonly PSDocumentOption Option;
        internal readonly LanguageMode LanguageMode;
        internal readonly DocumentFilter Filter;
        internal readonly IPipelineWriter Writer;
        internal readonly InstanceNameBinder InstanceNameBinder;
        internal readonly string[] Convention;

        private readonly Action<IDocumentResult, bool> _OutputVisitor;

        // Track whether Dispose has been called.
        private bool _Disposed;

        public PipelineContext(PSDocumentOption option, IPipelineWriter writer, Action<IDocumentResult, bool> _Output, InstanceNameBinder instanceNameBinder, string[] convention)
        {
            Option = option;
            LanguageMode = option.Execution.LanguageMode.GetValueOrDefault(ExecutionOption.Default.LanguageMode.Value);
            Filter = DocumentFilter.Create(Option.Document.Include, Option.Document.Tag);
            Writer = writer;
            InstanceNameBinder = instanceNameBinder;
            _OutputVisitor = _Output;
            Convention = convention;
        }

        internal void WriteOutput(IDocumentResult result)
        {
            _OutputVisitor(result, false);
        }

        #region IDisposable

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (!_Disposed)
            {
                //if (disposing)
                //{

                //}
                _Disposed = true;
            }
        }

        #endregion IDisposable
    }
}
