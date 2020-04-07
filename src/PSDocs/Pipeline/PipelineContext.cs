
using PSDocs.Configuration;
using PSDocs.Models;
using PSDocs.Processor;
using System;
using System.Management.Automation.Runspaces;

namespace PSDocs.Pipeline
{
    internal sealed class PipelineContext : IDisposable
    {
        private RunspacePool _Pool;

        internal readonly PSDocumentOption Option;
        internal readonly LanguageMode LanguageMode;
        internal readonly DocumentFilter Filter;
        internal readonly PipelineLogger Logger;
        internal readonly InstanceNameBinder InstanceNameBinder;

        private readonly Action<IDocumentResult, bool> _OutputVisitor;

        // Track whether Dispose has been called.
        private bool _Disposed = false;

        public PipelineContext(PSDocumentOption option, PipelineLogger logger, Action<IDocumentResult, bool> _Output, InstanceNameBinder instanceNameBinder)
        {
            Option = option;
            LanguageMode = option.Execution.LanguageMode.GetValueOrDefault(ExecutionOption.Default.LanguageMode.Value);
            Filter = DocumentFilter.Create(Option.Document.Include, Option.Document.Tag);
            Logger = logger;
            InstanceNameBinder = instanceNameBinder;

            _OutputVisitor = _Output;
        }

        //internal RunspacePool GetRunspace()
        //{
        //    if (_Pool == null)
        //    {
        //        var state = HostState.CreateSessionState();
        //        state.LanguageMode = LanguageMode == LanguageMode.FullLanguage ? PSLanguageMode.FullLanguage : PSLanguageMode.ConstrainedLanguage;
        //        state.ThreadOptions = PSThreadOptions.ReuseThread;

        //        _Pool = RunspaceFactory.CreateRunspacePool(
        //            //minRunspaces: 1,
        //            //maxRunspaces: 1,
        //            initialSessionState: state
        //        //host: new Host()
        //        );
        //        _Pool.ThreadOptions = PSThreadOptions.ReuseThread;
        //        _Pool.Open();
        //    }

        //    return _Pool;
        //}

        internal void WaitAll()
        {

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
                if (disposing)
                {

                }
                _Disposed = true;
            }
        }

        #endregion IDisposable
    }
}
