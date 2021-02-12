
using PSDocs.Definitions;
using PSDocs.Models;
using PSDocs.Processor;
using PSDocs.Runtime;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Management.Automation;

namespace PSDocs.Data.Internal
{
    /// <summary>
    /// Executes a script block to generate a document model.
    /// </summary>
    internal sealed class ScriptDocumentBuilder : IDocumentBuilder
    {
        private readonly ScriptDocumentBlock _Block;
        private readonly IDocumentConvention[] _Conventions;

        private SectionNode _Current;

        [ThreadStatic]
        private static Stack<SectionNode> _Parent;

        // Track whether Dispose has been called.
        private bool _Disposed;

        internal ScriptDocumentBuilder(ScriptDocumentBlock block, IDocumentConvention[] conventions)
        {
            _Block = block;
            _Conventions = conventions;
        }

        public string Name => _Block.Name;

        internal Document Document { get; private set; }

        Document IDocumentBuilder.Process(RunspaceContext context, PSObject sourceObject)
        {
            context.EnterBuilder(this);
            context.EnterSourceFile(_Block.Source);
            try
            {
                BeginConventions(context, sourceObject);
                _Parent = new Stack<SectionNode>();
                _Current = Document = new Document(context.DocumentContext);
                Document.AddNodes(_Block.Body.Invoke());
                ProcessConventions(context, sourceObject);
                return Document;
            }
            catch (Exception e)
            {
                context.WriteRuntimeException(_Block.SourcePath, e);
                return null;
            }
            finally
            {
                Document = null;
                _Current = null;
                _Parent.Clear();
                _Parent = null;
                context.ExitBuilder();
            }
        }

        void IDocumentBuilder.End(RunspaceContext context, IDocumentResult[] completed)
        {
            EndConventions(context, completed);
        }

        internal void Title(string text)
        {
            Document.Title = text;
        }

        internal void Metadata(IDictionary metadata)
        {
            foreach (DictionaryEntry kv in metadata)
                Document.Metadata[kv.Key] = kv.Value;
        }

        internal SectionNode EnterSection(string name)
        {
            _Parent.Push(_Current);
            _Current = new Section
            {
                Title = name,
                Level = _Current.Level+1,
            };
            return _Current;
        }

        internal void ExitSection()
        {
            if (_Parent.Count == 0)
                return;

            _Current = _Parent.Pop();
        }

        private void BeginConventions(RunspaceContext context, PSObject sourceObject)
        {
            if (_Conventions == null || _Conventions.Length == 0)
                return;

            try
            {
                context.PushScope(RunspaceScope.ConventionBegin);
                for (var i = 0; i < _Conventions.Length; i++)
                {
                    _Conventions[i].Begin(context, new PSObject[] { sourceObject });
                }
            }
            finally
            {
                context.PopScope();
            }
        }

        private void ProcessConventions(RunspaceContext context, PSObject sourceObject)
        {
            if (_Conventions == null || _Conventions.Length == 0)
                return;

            try
            {
                context.PushScope(RunspaceScope.ConventionProcess);
                for (var i = 0; i < _Conventions.Length; i++)
                {
                    _Conventions[i].Process(context, new PSObject[] { sourceObject });
                }
            }
            finally
            {
                context.PopScope();
            }
        }

        private void EndConventions(RunspaceContext context, IEnumerable results)
        {
            if (_Conventions == null || _Conventions.Length == 0)
                return;

            try
            {
                context.PushScope(RunspaceScope.ConventionEnd);
                for (var i = 0; i < _Conventions.Length; i++)
                {
                    _Conventions[i].End(context, results);
                }
            }
            finally
            {
                context.PopScope();
            }
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
                if (disposing)
                {
                    _Block.Dispose();
                    Document = null;
                    _Current = null;
                    _Parent = null;
                }
                _Disposed = true;
            }
        }

        #endregion IDisposable
    }
}
