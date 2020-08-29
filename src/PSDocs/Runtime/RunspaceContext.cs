
using PSDocs.Configuration;
using PSDocs.Data.Internal;
using PSDocs.Pipeline;
using System;
using System.IO;
using System.Management.Automation;
using System.Management.Automation.Runspaces;

namespace PSDocs.Runtime
{
    /// <summary>
    /// A context for a runspace.
    /// </summary>
    internal sealed class RunspaceContext : IDisposable
    {
        private const string ErrorPreference = "ErrorActionPreference";
        private const string WarningPreference = "WarningPreference";
        private const string VerbosePreference = "VerbosePreference";
        private const string DebugPreference = "DebugPreference";

        internal readonly PipelineContext Pipeline;

        [ThreadStatic]
        internal static RunspaceContext CurrentThread;
        private Runspace _Runspace;

        // Track whether Dispose has been called.
        private bool _Disposed;

        public RunspaceContext(PipelineContext pipeline)
        {
            Pipeline = pipeline;
            _Runspace = GetRunspace();
        }

        public SourceFile SourceFile { get; private set; }

        public ScriptDocumentBuilder Builder { get; private set; }

        public PSObject TargetObject { get; private set; }

        public string Culture { get; private set; }

        public string InstanceName { get; internal set; }

        internal PowerShell NewPowerShell()
        {
            CurrentThread = this;
            var runspace = GetRunspace();
            var ps = PowerShell.Create();
            ps.Runspace = runspace;
            EnableLogging(ps);
            return ps;
        }

        private Runspace GetRunspace()
        {
            if (_Runspace == null)
            {
                // Get session state
                var state = HostState.CreateSessionState();
                state.LanguageMode = Pipeline.LanguageMode == LanguageMode.FullLanguage ? PSLanguageMode.FullLanguage : PSLanguageMode.ConstrainedLanguage;

                _Runspace = RunspaceFactory.CreateRunspace(state);

                if (Runspace.DefaultRunspace == null)
                    Runspace.DefaultRunspace = _Runspace;

                _Runspace.Open();
                _Runspace.SessionStateProxy.PSVariable.Set(new HostState.PSDocsVariable());
                _Runspace.SessionStateProxy.PSVariable.Set(new HostState.InstanceNameVariable());
                _Runspace.SessionStateProxy.PSVariable.Set(new HostState.TargetObjectVariable());
                _Runspace.SessionStateProxy.PSVariable.Set(new HostState.InputObjectVariable());
                _Runspace.SessionStateProxy.PSVariable.Set(new HostState.DocumentVariable());
                _Runspace.SessionStateProxy.PSVariable.Set(ErrorPreference, ActionPreference.Continue);
                _Runspace.SessionStateProxy.PSVariable.Set(WarningPreference, ActionPreference.Continue);
                _Runspace.SessionStateProxy.PSVariable.Set(VerbosePreference, ActionPreference.Continue);
                _Runspace.SessionStateProxy.PSVariable.Set(DebugPreference, ActionPreference.Continue);
                _Runspace.SessionStateProxy.Path.SetLocation(PSDocumentOption.GetWorkingPath());
            }
            return _Runspace;
        }

        #region SourceFile

        public bool EnterSourceFile(SourceFile file)
        {
            if (file == null || !File.Exists(file.Path))
                return false;

            SourceFile = file;
            return true;
        }

        public void ExitSourceFile()
        {
            SourceFile = null;
        }

        #endregion SourceFile

        #region Builder

        public void EnterBuilder(ScriptDocumentBuilder builder)
        {
            CurrentThread = this;
            Builder = builder;
        }

        public void ExitBuilder()
        {
            Builder = null;
        }

        #endregion Builder

        #region TargetObject

        public void EnterTargetObject(PSObject targetObject)
        {
            TargetObject = targetObject;
        }

        public void ExitTargetObject()
        {
            TargetObject = null;
        }

        #endregion TargetObject

        #region Culture

        public void EnterCulture(string culture)
        {
            Culture = culture;
        }

        #endregion Culture

        #region Logging

        private static void EnableLogging(PowerShell ps)
        {
            ps.Streams.Error.DataAdded += Error_DataAdded;
            ps.Streams.Warning.DataAdded += Warning_DataAdded;
            ps.Streams.Verbose.DataAdded += Verbose_DataAdded;
            ps.Streams.Information.DataAdded += Information_DataAdded;
            ps.Streams.Debug.DataAdded += Debug_DataAdded;
        }

        internal void WriteRuntimeException(string sourceFile, Exception inner)
        {
            if (Pipeline == null || Pipeline.Writer == null)
                return;

            var record = new ErrorRecord(new Pipeline.RuntimeException(sourceFile: sourceFile, innerException: inner), "PSDocs.Pipeline.RuntimeException", ErrorCategory.InvalidOperation, null);
            Pipeline.Writer.WriteError(record);
        }

        internal static void ThrowRuntimeException(string sourceFile, Exception inner)
        {
            throw new Pipeline.RuntimeException(sourceFile: sourceFile, innerException: inner);
        }

        private static void Debug_DataAdded(object sender, DataAddedEventArgs e)
        {
            if (CurrentThread.Pipeline == null || CurrentThread.Pipeline.Writer == null)
                return;

            var collection = sender as PSDataCollection<DebugRecord>;
            var record = collection[e.Index];
            //CurrentThread._Logger.WriteDebug(debugRecord: record);
        }

        private static void Information_DataAdded(object sender, DataAddedEventArgs e)
        {
            if (CurrentThread.Pipeline == null || CurrentThread.Pipeline.Writer == null)
                return;

            var collection = sender as PSDataCollection<InformationRecord>;
            var record = collection[e.Index];
            //CurrentThread._Logger.WriteInformation(informationRecord: record);
        }

        private static void Verbose_DataAdded(object sender, DataAddedEventArgs e)
        {
            if (CurrentThread.Pipeline == null || CurrentThread.Pipeline.Writer == null)
                return;

            var collection = sender as PSDataCollection<VerboseRecord>;
            var record = collection[e.Index];
            CurrentThread.Pipeline.Writer.WriteVerbose(record.Message);
        }

        private static void Warning_DataAdded(object sender, DataAddedEventArgs e)
        {
            if (CurrentThread.Pipeline == null || CurrentThread.Pipeline.Writer == null)
                return;

            var collection = sender as PSDataCollection<WarningRecord>;
            var record = collection[e.Index];
            CurrentThread.Pipeline.Writer.WriteWarning(record.Message);
        }

        private static void Error_DataAdded(object sender, DataAddedEventArgs e)
        {
            if (CurrentThread.Pipeline == null || CurrentThread.Pipeline.Writer == null)
                return;

            var collection = sender as PSDataCollection<ErrorRecord>;
            var record = collection[e.Index];
            CurrentThread.Pipeline.Writer.WriteError(record);
        }

        #endregion Logging

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
                    if (Builder != null)
                    {
                        Builder.Dispose();
                        Builder = null;
                    }
                    if (_Runspace != null)
                    {
                        _Runspace.Dispose();
                        _Runspace = null;
                    }
                    CurrentThread = null;
                }
                _Disposed = true;
            }
        }

        #endregion IDisposable
    }
}
