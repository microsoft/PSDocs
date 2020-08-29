
using PSDocs.Definitions;
using PSDocs.Pipeline;
using System;
using System.Diagnostics;
using System.Management.Automation;

namespace PSDocs.Runtime
{
    /// <summary>
    /// A Document block.
    /// </summary>
    [DebuggerDisplay("{Id} @{SourcePath}")]
    internal sealed class ScriptDocumentBlock : IDocumentDefinition, IDisposable
    {
        internal readonly PowerShell Body;
        internal readonly string[] Tag;

        private readonly SourceFile Source;

        // Track whether Dispose has been called.
        private bool _Disposed;

        internal ScriptDocumentBlock(SourceFile source, string name, PowerShell body, string[] tag, IResourceExtent extent)
        {
            Source = source;
            Name = name;
            Id = ResourceHelper.GetId(source.ModuleName, name);
            Body = body;
            Tag = tag;
            Extent = extent;
        }

        public string Id { get; }

        public string Name { get; }

        public string SourcePath => Source.Path;

        public string Module => Source.ModuleName;

        internal IResourceExtent Extent { get; }

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
