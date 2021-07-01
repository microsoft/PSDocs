// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections;
using System.Collections.Generic;
using System.Management.Automation;
using System.Management.Automation.Host;

namespace PSDocs
{
    internal sealed class TestCommandRuntime : ICommandRuntime2
    {
        public readonly List<ErrorRecord> Error;
        public readonly List<string> Warning;
        public readonly List<string> Verbose;
        public readonly List<InformationRecord> Information;
        public readonly List<object> Output;

        public TestCommandRuntime()
        {
            Error = new List<ErrorRecord>();
            Warning = new List<string>();
            Verbose = new List<string>();
            Information = new List<InformationRecord>();
            Output = new List<object>();
        }

        PSTransactionContext ICommandRuntime.CurrentPSTransaction => throw new System.NotImplementedException();

        PSHost ICommandRuntime.Host => throw new System.NotImplementedException();

        bool ICommandRuntime2.ShouldContinue(string query, string caption, bool hasSecurityImpact, ref bool yesToAll, ref bool noToAll)
        {
            return true;
        }

        bool ICommandRuntime.ShouldContinue(string query, string caption)
        {
            return true;
        }

        bool ICommandRuntime.ShouldContinue(string query, string caption, ref bool yesToAll, ref bool noToAll)
        {
            return true;
        }

        bool ICommandRuntime.ShouldProcess(string target)
        {
            return true;
        }

        bool ICommandRuntime.ShouldProcess(string target, string action)
        {
            return true;
        }

        bool ICommandRuntime.ShouldProcess(string verboseDescription, string verboseWarning, string caption)
        {
            return true;
        }

        bool ICommandRuntime.ShouldProcess(string verboseDescription, string verboseWarning, string caption, out ShouldProcessReason shouldProcessReason)
        {
            shouldProcessReason = ShouldProcessReason.None;
            return true;
        }

        void ICommandRuntime.ThrowTerminatingError(ErrorRecord errorRecord)
        {

        }

        bool ICommandRuntime.TransactionAvailable()
        {
            return false;
        }

        void ICommandRuntime.WriteCommandDetail(string text)
        {

        }

        void ICommandRuntime.WriteDebug(string text)
        {

        }

        void ICommandRuntime.WriteError(ErrorRecord errorRecord)
        {
            Error.Add(errorRecord);
        }

        void ICommandRuntime2.WriteInformation(InformationRecord informationRecord)
        {
            Information.Add(informationRecord);
        }

        void ICommandRuntime.WriteObject(object sendToPipeline)
        {
            Output.Add(sendToPipeline);
        }

        void ICommandRuntime.WriteObject(object sendToPipeline, bool enumerateCollection)
        {
            if (enumerateCollection && sendToPipeline is IEnumerable collection)
            {
                foreach (var o in collection)
                {
                    Output.Add(o);
                }
            }
            else
                Output.Add(sendToPipeline);
        }

        void ICommandRuntime.WriteProgress(long sourceId, ProgressRecord progressRecord)
        {

        }

        void ICommandRuntime.WriteProgress(ProgressRecord progressRecord)
        {

        }

        void ICommandRuntime.WriteVerbose(string text)
        {
            Verbose.Add(text);
        }

        void ICommandRuntime.WriteWarning(string text)
        {
            Warning.Add(text);
        }
    }
}
