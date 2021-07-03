// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using PSDocs.Runtime;
using System.Collections.ObjectModel;
using System.Management.Automation;

namespace PSDocs.Commands
{
    [Cmdlet(VerbsLifecycle.Invoke, LanguageKeywords.Block)]
    internal sealed class InvokeDocumentCommand : PSCmdlet
    {
        [Parameter()]
        public string[] With;

        [Parameter()]
        public ScriptBlock If;

        [Parameter()]
        public ScriptBlock Body;

        protected override void ProcessRecord()
        {
            try
            {
                if (Body == null)
                    return;

                // Evalute selector pre-condition
                if (!AcceptsWith())
                    return;

                // Evaluate script pre-condition
                if (!AcceptsIf())
                    return;

                WriteObject(Body.Invoke(), true);
            }
            finally
            {

            }
        }

        private bool AcceptsWith()
        {
            if (With == null || With.Length == 0)
                return true;

            for (var i = 0; i < With.Length; i++)
            {
                if (RunspaceContext.CurrentThread.TrySelector(With[i]))
                    return true;
            }
            return false;
        }

        private bool AcceptsIf()
        {
            if (If == null)
                return true;

            try
            {
                RunspaceContext.CurrentThread.PushScope(RunspaceScope.Condition);
                return GetResult(If.Invoke());
            }
            finally
            {
                RunspaceContext.CurrentThread.PopScope();
            }
        }

        private static bool GetResult(Collection<PSObject> result)
        {
            if (result == null || result.Count == 0)
                return false;

            for (var i = 0; i < result.Count; i++)
            {
                if (result[i] == null || result[i].BaseObject == null)
                    return false;

                if (result[i].BaseObject is bool bResult && !bResult)
                    return false;
            }
            return true;
        }
    }
}
