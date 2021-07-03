// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using PSDocs.Configuration;
using PSDocs.Definitions;
using PSDocs.Definitions.Selectors;
using PSDocs.Models;
using PSDocs.Processor;
using System;
using System.Collections.Generic;

namespace PSDocs.Pipeline
{
    /// <summary>
    /// A context for an end-to-end pipeline execution.
    /// </summary>
    internal sealed class PipelineContext : IDisposable
    {
        internal readonly OptionContext Option;
        internal readonly LanguageMode LanguageMode;
        internal readonly DocumentFilter Filter;
        internal readonly PipelineStream Stream;
        internal readonly IPipelineWriter Writer;
        internal readonly InstanceNameBinder InstanceNameBinder;
        internal readonly string[] Convention;
        internal readonly Dictionary<string, SelectorVisitor> Selector;

        private readonly Action<IDocumentResult, bool> _OutputVisitor;

        // Track whether Dispose has been called.
        private bool _Disposed;

        public PipelineContext(OptionContext option, PipelineStream stream, IPipelineWriter writer, Action<IDocumentResult, bool> _Output, InstanceNameBinder instanceNameBinder, string[] convention)
        {
            Option = option;
            LanguageMode = option.Execution.LanguageMode.GetValueOrDefault(ExecutionOption.Default.LanguageMode.Value);
            Filter = DocumentFilter.Create(option.Document.Include, option.Document.Tag);
            Stream = stream ?? new PipelineStream(null, null);
            Writer = writer;
            InstanceNameBinder = instanceNameBinder;
            _OutputVisitor = _Output;
            Convention = convention;
            Selector = new Dictionary<string, SelectorVisitor>();
        }

        internal void WriteOutput(IDocumentResult result)
        {
            _OutputVisitor(result, false);
        }

        internal void Import(IResource resource)
        {
            if (resource.Kind == ResourceKind.Selector && resource is SelectorV1 selector)
                Selector[selector.Id] = new SelectorVisitor(selector.Id, selector.Spec.If);
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
