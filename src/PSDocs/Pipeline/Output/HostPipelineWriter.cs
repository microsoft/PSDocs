using System.Management.Automation;

namespace PSDocs.Pipeline.Output
{
    internal sealed class HostPipelineWriter : PipelineWriterBase, IPipelineWriter
    {
        private readonly HostContext _HostContext;

        // Track whether Dispose has been called.
        private bool _Disposed;

        internal HostPipelineWriter(HostContext hostContext)
            : base(hostContext)
        {
            _HostContext = hostContext;
        }

        protected override void DoWriteError(ErrorRecord record)
        {
            if (GuardOutputReady())
                return;

            _HostContext.CommandRuntime.WriteError(record);
        }

        protected override void DoWriteWarning(string text)
        {
            if (GuardOutputReady())
                return;

            _HostContext.CommandRuntime.WriteWarning(text);
        }

        protected override void DoWriteInformation(InformationRecord record)
        {
            if (GuardOutputReady() || record == null)
                return;

            _HostContext.CommandRuntime.WriteInformation(record);
        }

        protected override void DoWriteVerbose(string text)
        {
            if (GuardOutputReady())
                return;

            _HostContext.CommandRuntime.WriteVerbose(text);
        }

        protected override void DoWriteDebug(DebugRecord record)
        {
            if (GuardOutputReady())
                return;

            _HostContext.CommandRuntime.WriteDebug(record.Message);
        }

        protected override void DoWriteObject(object sendToPipeline, bool enumerateCollection)
        {
            if (GuardOutputReady())
                return;

            _HostContext.CommandRuntime.WriteObject(sendToPipeline, enumerateCollection);
        }

        private bool GuardOutputReady()
        {
            return _HostContext == null || _HostContext.CommandRuntime == null;
        }

        #region IDisposable

        private void Dispose(bool disposing)
        {
            if (!_Disposed)
            {
                if (disposing)
                {
                    // Do nothing
                }
                _Disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            System.GC.SuppressFinalize(this);
        }

        #endregion IDisposable
    }
}
