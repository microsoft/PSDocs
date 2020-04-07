using PSDocs.Configuration;
using System;
using System.Management.Automation;

namespace PSDocs.Pipeline
{
    internal sealed class PipelineLogger
    {
        private const string ErrorPreference = "ErrorActionPreference";
        private const string WarningPreference = "WarningPreference";
        private const string VerbosePreference = "VerbosePreference";
        private const string InformationPreference = "InformationPreference";
        private const string DebugPreference = "DebugPreference";

        private Action<string> OnWriteWarning;
        private Action<string> OnWriteVerbose;
        private Action<ErrorRecord> OnWriteError;
        private Action<InformationRecord> OnWriteInformation;
        private Action<string> OnWriteDebug;
        internal Action<object, bool> OnWriteObject;

        private string _ScopeName;

        private bool _LogError;
        private bool _LogWarning;
        private bool _LogVerbose;
        private bool _LogInformation;
        private bool _LogDebug;

        #region Logging

        public void WriteError(ErrorRecord errorRecord)
        {
            if (!ShouldWriteError() || errorRecord == null)
                return;

            DoWriteError(errorRecord);
        }

        public void WriteVerbose(string message)
        {
            if (!ShouldWriteVerbose() || string.IsNullOrEmpty(message))
                return;

            DoWriteVerbose(message);
        }

        public void WriteDebug(DebugRecord debugRecord)
        {
            if (!ShouldWriteDebug())
                return;

            DoWriteDebug(debugRecord);
        }

        public void WriteInformation(InformationRecord informationRecord)
        {
            if (!ShouldWriteInformation())
                return;

            DoWriteInformation(informationRecord);
        }

        public void WriteWarning(string message)
        {
            if (!ShouldWriteWarning())
                return;

            DoWriteWarning(message);
        }

        public void EnterScope(string scopeName)
        {
            _ScopeName = scopeName;
        }

        public void ExitScope()
        {
            _ScopeName = null;
        }

        #endregion Logging

        public void WriteObject(object sendToPipeline, bool enumerateCollection)
        {
            if (OnWriteObject == null)
                return;

            OnWriteObject(sendToPipeline, enumerateCollection);
        }

        internal void UseCommandRuntime(ICommandRuntime2 commandRuntime)
        {
            OnWriteVerbose = commandRuntime.WriteVerbose;
            OnWriteWarning = commandRuntime.WriteWarning;
            OnWriteError = commandRuntime.WriteError;
            OnWriteInformation = commandRuntime.WriteInformation;
            OnWriteDebug = commandRuntime.WriteDebug;
            OnWriteObject = commandRuntime.WriteObject;
        }

        internal void UseExecutionContext(EngineIntrinsics executionContext)
        {
            _LogError = GetPreferenceVariable(executionContext, ErrorPreference);
            _LogWarning = GetPreferenceVariable(executionContext, WarningPreference);
            _LogVerbose = GetPreferenceVariable(executionContext, VerbosePreference);
            _LogInformation = GetPreferenceVariable(executionContext, InformationPreference);
            _LogDebug = GetPreferenceVariable(executionContext, DebugPreference);
        }

        internal void Configure(PSDocumentOption option)
        {

        }

        private bool GetPreferenceVariable(EngineIntrinsics executionContext, string variableName)
        {
            var preference = (ActionPreference)executionContext.SessionState.PSVariable.GetValue(variableName);
            if (preference == ActionPreference.Ignore)
                return false;

            return !(preference == ActionPreference.SilentlyContinue && (variableName == VerbosePreference || variableName == DebugPreference));
        }

        #region Internal logging methods

        /// <summary>
        /// Core methods to hand off to logger.
        /// </summary>
        /// <param name="errorRecord">A valid PowerShell error record.</param>
        private void DoWriteError(ErrorRecord errorRecord)
        {
            if (OnWriteError == null)
                return;

            OnWriteError(errorRecord);
        }

        /// <summary>
        /// Core method to hand off verbose messages to logger.
        /// </summary>
        /// <param name="message">A message to log.</param>
        private void DoWriteVerbose(string message)
        {
            if (OnWriteVerbose == null)
                return;

            OnWriteVerbose(message);
        }

        /// <summary>
        /// Core method to hand off warning messages to logger.
        /// </summary>
        /// <param name="message">A message to log</param>
        private void DoWriteWarning(string message)
        {
            if (OnWriteWarning == null)
                return;

            OnWriteWarning(message);
        }

        /// <summary>
        /// Core method to hand off information messages to logger.
        /// </summary>
        private void DoWriteInformation(InformationRecord informationRecord)
        {
            if (OnWriteInformation == null)
                return;

            OnWriteInformation(informationRecord);
        }

        /// <summary>
        /// Core method to hand off debug messages to logger.
        /// </summary>
        private void DoWriteDebug(DebugRecord debugRecord)
        {
            if (OnWriteDebug == null)
                return;

            OnWriteDebug(debugRecord.Message);
        }

        #endregion Internal logging methods

        public bool ShouldWriteError()
        {
            return true;
        }

        public bool ShouldWriteWarning()
        {
            return true;
        }

        public bool ShouldWriteVerbose()
        {
            return _LogVerbose;
        }

        public bool ShouldWriteInformation()
        {
            return true;
        }

        public bool ShouldWriteDebug()
        {
            return _LogDebug;
        }
    }
}
