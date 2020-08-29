
using PSDocs.Models;
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

        private SectionNode _Current;

        [ThreadStatic]
        private static Stack<SectionNode> _Parent;

        // Track whether Dispose has been called.
        private bool _Disposed;

        internal ScriptDocumentBuilder(ScriptDocumentBlock block)
        {
            _Block = block;
        }

        public string Name => _Block.Name;

        internal Document Document { get; private set; }

        Document IDocumentBuilder.Process(RunspaceContext context, PSObject sourceObject)
        {
            context.EnterBuilder(this);
            try
            {
                _Current = Document = new Document(context.InstanceName, context.Culture);
                _Parent = new Stack<SectionNode>();
                Document.AddNodes(_Block.Body.Invoke());
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
