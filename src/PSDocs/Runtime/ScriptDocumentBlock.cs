
using PSDocs.Pipeline;
using System;
using System.Diagnostics;
using System.Management.Automation;

namespace PSDocs.Runtime
{
    [DebuggerDisplay("{Id} @{SourcePath}")]
    internal sealed class ScriptDocumentBlock : ILanguageBlock, IDisposable
    {
        public readonly string Id;

        public readonly string Name;
        public readonly PowerShell Body;
        public readonly string[] Tag;

        private readonly SourceFile Source;

        // Track whether Dispose has been called.
        private bool _Disposed = false;

        public ScriptDocumentBlock(SourceFile source, string name, PowerShell body, string[] tag)
        {
            Source = source;
            Id = name;
            Name = name;
            Body = body;
            Tag = tag;
        }

        public string SourcePath => Source.Path;

        public string Module => Source.ModuleName;

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
                    Body.Dispose();
                }
                _Disposed = true;
            }
        }

        #endregion IDisposable
    }
}
