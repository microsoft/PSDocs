using PSDocs.Models;
using PSDocs.Runtime;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Management.Automation;

namespace PSDocs.Data.Internal
{
    internal sealed class ScriptDocumentBuilder : IDocumentBuilder, IDisposable
    {
        private readonly ScriptDocumentBlock _Block;

        [ThreadStatic]
        private static Document _Document;

        [ThreadStatic]
        private static SectionNode _Current;

        [ThreadStatic]
        private static Stack<SectionNode> _Parent;

        // Track whether Dispose has been called.
        private bool _Disposed = false;

        internal ScriptDocumentBuilder(ScriptDocumentBlock block)
        {
            _Block = block;
        }

        internal string Name => _Block.Name;

        internal Document Document => _Document;

        Document IDocumentBuilder.Process(RunspaceContext context, PSObject sourceObject)
        {
            context.EnterBuilder(this);
            try
            {
                _Current = _Document = new Document(_Block.Name);
                _Parent = new Stack<SectionNode>();
                _Document.AddNodes(_Block.Body.Invoke());
                return _Document;
            }
            catch (Exception e)
            {
                context.WriteRuntimeException(_Block.SourcePath, e);
                return null;
            }
            finally
            {
                _Document = null;
                _Current = null;
                _Parent.Clear();
                _Parent = null;
                context.ExitBuilder();
            }
        }

        internal void Title(string text)
        {
            _Document.Title = text;
        }

        internal void Metadata(IDictionary metadata)
        {
            foreach (DictionaryEntry kv in metadata)
                _Document.Metadata[kv.Key] = kv.Value;
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

        internal Code Code()
        {
            return ModelHelper.NewCode();
        }

        internal BlockQuote BlockQuote(string info, string title)
        {
            return ModelHelper.BlockQuote(info, title);
        }

        internal TableBuilder Table()
        {
            return ModelHelper.Table();
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
                    _Document = null;
                    _Current = null;
                    _Parent = null;
                }
                _Disposed = true;
            }
        }

        #endregion IDisposable
    }
}
