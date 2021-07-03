// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using PSDocs.Configuration;
using PSDocs.Data.Internal;
using PSDocs.Definitions;
using PSDocs.Definitions.Selectors;
using PSDocs.Pipeline;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Management.Automation;
using System.Management.Automation.Language;
using System.Management.Automation.Runspaces;

namespace PSDocs.Runtime
{
    internal enum RunspaceScope
    {
        None = 0,

        Source = 1,

        Document = 2,

        Condition = 4,

        ConventionBegin = 8,
        ConventionProcess = 16,
        ConventionEnd = 32,

        Convention = ConventionBegin | ConventionProcess | ConventionEnd,
        Runtime = Document | Condition | Convention,
        All = Source | Document | Condition | Convention,
    }

    /// <summary>
    /// A context for a runspace.
    /// </summary>
    internal sealed class RunspaceContext : IDisposable
    {
        private const string ErrorPreference = "ErrorActionPreference";
        private const string WarningPreference = "WarningPreference";
        private const string VerbosePreference = "VerbosePreference";
        private const string DebugPreference = "DebugPreference";

        [ThreadStatic]
        internal static RunspaceContext CurrentThread;

        internal readonly Dictionary<string, object> ExpressionCache;

        private readonly Dictionary<string, Hashtable> _LocalizedDataCache;

        private Runspace _Runspace;
        private string[] _Culture;
        private readonly Stack<RunspaceScope> _Scope;

        // Track whether Dispose has been called.
        private bool _Disposed;

        public RunspaceContext(PipelineContext pipeline)
        {
            CurrentThread = this;
            _Scope = new Stack<RunspaceScope>();
            Pipeline = pipeline;
            _Runspace = GetRunspace();
            _LocalizedDataCache = new Dictionary<string, Hashtable>();
            ExpressionCache = new Dictionary<string, object>();
        }

        public PipelineContext Pipeline { get; }

        public SourceScope Source { get; private set; }

        public ScriptDocumentBuilder Builder { get; private set; }

        public TargetObject TargetObject { get; private set; }

        public IEnumerable Output { get; private set; }

        public string Culture => _Culture[0];

        public DocumentContext DocumentContext { get; private set; }

        internal void EnterDocument(string instanceName)
        {
            DocumentContext = new DocumentContext(this)
            {
                InstanceName = instanceName
            };
            PushScope(RunspaceScope.Document);
        }

        internal void ExitDocument()
        {
            DocumentContext = null;
            PopScope();
        }

        internal void SetOutput(IEnumerable output)
        {
            Output = output;
        }

        internal void ClearOutput()
        {
            Output = null;
        }

        internal bool IsScope(RunspaceScope scope)
        {
            var current = _Scope.Peek();
            return (current & scope) == current;
        }

        internal void PushScope(RunspaceScope scope)
        {
            _Scope.Push(scope);
        }

        internal void PopScope()
        {
            _Scope.Pop();
        }

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
                _Runspace.SessionStateProxy.PSVariable.Set(new HostState.LocalizedDataVariable(this));
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

            Source = new SourceScope(file);
            return true;
        }

        public void ExitSourceFile()
        {
            Source = null;
        }

        #endregion SourceFile

        #region Builder

        public void EnterBuilder(ScriptDocumentBuilder builder)
        {
            CurrentThread = this;
            Builder = builder;
            Pipeline.Option.SwitchScope(builder.Module);
        }

        public void ExitBuilder()
        {
            Builder = null;
        }

        #endregion Builder

        #region TargetObject

        public void EnterTargetObject(TargetObject targetObject)
        {
            TargetObject = targetObject;
        }

        public void ExitTargetObject()
        {
            TargetObject = null;
        }

        public bool TrySelector(string name)
        {
            name = ResourceHelper.GetId(Source.File.ModuleName, name);
            if (TargetObject == null || Pipeline == null || !Pipeline.Selector.TryGetValue(name, out SelectorVisitor selector))
                return false;

            var annotation = TargetObject.GetAnnotation<SelectorTargetAnnotation>();
            if (annotation.TryGetSelectorResult(selector, out bool result))
                return result;

            result = selector.Match(TargetObject.Value);
            annotation.SetSelectorResult(selector, result);
            return result;
        }

        #endregion TargetObject

        #region Culture

        public void EnterCulture(string culture)
        {
            _Culture = GetCultures(culture);
        }

        /// <summary>
        /// Build a list of cultures.
        /// </summary>
        private static string[] GetCultures(string culture)
        {
            var cultures = new List<string>();
            if (!string.IsNullOrEmpty(culture))
            {
                var c = new CultureInfo(culture);
                while (c != null && !string.IsNullOrEmpty(c.Name))
                {
                    cultures.Add(c.Name);
                    c = c.Parent;
                }
            }
            return cultures.ToArray();
        }

        private const string DATA_FILENAME = "PSDocs-strings.psd1";

        private static readonly Hashtable Empty = new Hashtable();

        internal Hashtable GetLocalizedStrings()
        {
            var path = GetLocalizedPaths(DATA_FILENAME);
            if (path == null || path.Length == 0)
                return Empty;

            if (_LocalizedDataCache.TryGetValue(path[0], out Hashtable result))
                return result;

            result = ReadLocalizedStrings(path[0]) ?? new Hashtable();
            for (var i = 1; i < path.Length; i++)
                result.AddUnique(ReadLocalizedStrings(path[i]));

            _LocalizedDataCache[path[0]] = result;
            return result;
        }

        private static Hashtable ReadLocalizedStrings(string path)
        {
            var ast = Parser.ParseFile(path, out Token[] tokens, out ParseError[] errors);
            var data = ast.Find(a => a is HashtableAst, false);
            if (data != null)
            {
                var result = (Hashtable)data.SafeGetValue();
                return result;
            }
            return null;
        }

        public string GetLocalizedPath(string file)
        {
            if (string.IsNullOrEmpty(Source.File.ResourcePath))
                return null;

            for (var i = 0; i < _Culture.Length; i++)
            {
                if (TryLocalizedPath(_Culture[i], file, out string path))
                    return path;
            }
            return null;
        }

        public string[] GetLocalizedPaths(string file)
        {
            if (string.IsNullOrEmpty(Source.File.ResourcePath))
                return null;

            var result = new List<string>();
            for (var i = 0; i < _Culture.Length; i++)
            {
                if (TryLocalizedPath(_Culture[i], file, out string path))
                    result.Add(path);
            }
            return result.ToArray();
        }

        private bool TryLocalizedPath(string culture, string file, out string path)
        {
            path = null;
            if (Source == null || string.IsNullOrEmpty(Source.File.ResourcePath))
                return false;

            path = Path.Combine(Source.File.ResourcePath, culture, file);
            return File.Exists(path);
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
