// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Management.Automation;

namespace PSDocs.Pipeline
{
    public interface IHostContext
    {
        ActionPreference GetPreferenceVariable(string variableName);

        T GetVariable<T>(string variableName);
    }

    internal static class HostContextExtensions
    {
        private const string ErrorPreference = "ErrorActionPreference";
        private const string WarningPreference = "WarningPreference";
        private const string InformationPreference = "InformationPreference";
        private const string VerbosePreference = "VerbosePreference";
        private const string DebugPreference = "DebugPreference";
        private const string AutoLoadingPreference = "PSModuleAutoLoadingPreference";

        public static ActionPreference GetErrorPreference(this IHostContext hostContext)
        {
            return hostContext.GetPreferenceVariable(ErrorPreference);
        }

        public static ActionPreference GetWarningPreference(this IHostContext hostContext)
        {
            return hostContext.GetPreferenceVariable(WarningPreference);
        }

        public static ActionPreference GetInformationPreference(this IHostContext hostContext)
        {
            return hostContext.GetPreferenceVariable(InformationPreference);
        }

        public static ActionPreference GetVerbosePreference(this IHostContext hostContext)
        {
            return hostContext.GetPreferenceVariable(VerbosePreference);
        }

        public static ActionPreference GetDebugPreference(this IHostContext hostContext)
        {
            return hostContext.GetPreferenceVariable(DebugPreference);
        }

        public static PSModuleAutoLoadingPreference GetAutoLoadingPreference(this IHostContext hostContext)
        {
            return hostContext.GetVariable<PSModuleAutoLoadingPreference>(AutoLoadingPreference);
        }
    }

    internal sealed class HostContext : IHostContext
    {
        internal readonly PSCmdlet CommandRuntime;
        internal readonly EngineIntrinsics ExecutionContext;

        internal HostContext(PSCmdlet commandRuntime, EngineIntrinsics executionContext)
        {
            CommandRuntime = commandRuntime;
            ExecutionContext = executionContext;
        }

        public ActionPreference GetPreferenceVariable(string variableName)
        {
            if (ExecutionContext == null)
                return ActionPreference.SilentlyContinue;

            return (ActionPreference)ExecutionContext.SessionState.PSVariable.GetValue(variableName);
        }

        public T GetVariable<T>(string variableName)
        {
            if (ExecutionContext == null)
                return default;

            return (T)ExecutionContext.SessionState.PSVariable.GetValue(variableName);
        }

        public bool ShouldProcess(string target, string action)
        {
            if (CommandRuntime == null)
                return true;

            return CommandRuntime.ShouldProcess(target, action);
        }
    }
}
