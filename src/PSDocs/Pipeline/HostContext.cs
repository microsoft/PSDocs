using System.Management.Automation;

namespace PSDocs.Pipeline
{
    public interface IHostContext
    {
        ActionPreference GetPreferenceVariable(string variableName);
    }

    internal static class HostContextExtensions
    {
        private const string ErrorPreference = "ErrorActionPreference";
        private const string WarningPreference = "WarningPreference";
        private const string InformationPreference = "InformationPreference";
        private const string VerbosePreference = "VerbosePreference";
        private const string DebugPreference = "DebugPreference";

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
    }
}
