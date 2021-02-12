
using PSDocs.Commands;
using System;
using System.Management.Automation;
using System.Management.Automation.Runspaces;

namespace PSDocs.Runtime
{
    internal static class HostState
    {
        public sealed class PSDocsVariable : PSVariable
        {
            internal const string VariableName = "PSDocs";

            private readonly PSDocs _Value;

            public PSDocsVariable()
                : base(VariableName, null, ScopedItemOptions.ReadOnly)
            {
                _Value = new PSDocs();
            }

            public override object Value => _Value;
        }

        public sealed class LocalizedDataVariable : PSVariable
        {
            internal const string VariableName = "LocalizedData";

            private readonly LocalizedData _Value;

            public LocalizedDataVariable()
                : base(VariableName, null, ScopedItemOptions.ReadOnly)
            {
                _Value = new LocalizedData();
            }

            internal LocalizedDataVariable(RunspaceContext context)
                : this()
            {
                _Value = new LocalizedData(context);
            }

            public override object Value => _Value;
        }

        public sealed class InstanceNameVariable : PSVariable
        {
            internal const string VariableName = "InstanceName";

            public InstanceNameVariable()
                : base(VariableName, null, ScopedItemOptions.ReadOnly) { }

            public override object Value
            {
                get
                {
                    return RunspaceContext.CurrentThread?.DocumentContext.InstanceName ?? RunspaceContext.CurrentThread.Builder.Name;
                }
            }
        }

        public sealed class TargetObjectVariable : PSVariable
        {
            internal const string VariableName = "TargetObject";

            public TargetObjectVariable()
                : base(VariableName, null, ScopedItemOptions.ReadOnly) { }

            public override object Value => RunspaceContext.CurrentThread.TargetObject;
        }

        public sealed class InputObjectVariable : PSVariable
        {
            internal const string VariableName = "InputObject";

            public InputObjectVariable()
                : base(VariableName, null, ScopedItemOptions.ReadOnly) { }

            public override object Value => RunspaceContext.CurrentThread.TargetObject;
        }

        public sealed class DocumentVariable : PSVariable
        {
            internal const string VariableName = "Document";

            public DocumentVariable()
                : base(VariableName, null, ScopedItemOptions.ReadOnly) { }

            public override object Value => RunspaceContext.CurrentThread.Builder.Document;
        }

        /// <summary>
        /// Define language commands.
        /// </summary>
        private readonly static SessionStateCmdletEntry[] BuiltInCmdlets = new SessionStateCmdletEntry[]
        {
            new SessionStateCmdletEntry(LanguageCmdlets.NewDefinition, typeof(DefinitionCommand), null),
            new SessionStateCmdletEntry(LanguageCmdlets.ExportConvention, typeof(ExportConventionCommand), null),
            new SessionStateCmdletEntry(LanguageCmdlets.NewSection, typeof(SectionCommand), null),
            new SessionStateCmdletEntry(LanguageCmdlets.InvokeBlock, typeof(InvokeDocumentCommand), null),
            new SessionStateCmdletEntry(LanguageCmdlets.InvokeConvention, typeof(InvokeConventionCommand), null),
            new SessionStateCmdletEntry(LanguageCmdlets.FormatBlockQuote, typeof(BlockQuoteCommand), null),
            new SessionStateCmdletEntry(LanguageCmdlets.FormatCode, typeof(CodeCommand), null),
            new SessionStateCmdletEntry(LanguageCmdlets.FormatList, typeof(ListCommand), null),
            new SessionStateCmdletEntry(LanguageCmdlets.FormatNote, typeof(NoteCommand), null),
            new SessionStateCmdletEntry(LanguageCmdlets.FormatTable, typeof(TableCommand), null),
            new SessionStateCmdletEntry(LanguageCmdlets.FormatWarning, typeof(WarningCommand), null),
            new SessionStateCmdletEntry(LanguageCmdlets.SetMetadata, typeof(MetadataCommand), null),
            new SessionStateCmdletEntry(LanguageCmdlets.SetTitle, typeof(TitleCommand), null),
            new SessionStateCmdletEntry(LanguageCmdlets.AddInclude, typeof(IncludeCommand), null),
        };

        /// <summary>
        /// Define language aliases.
        /// </summary>
        private readonly static SessionStateAliasEntry[] BuiltInAliases = new SessionStateAliasEntry[]
        {
            new SessionStateAliasEntry(LanguageKeywords.Document, LanguageCmdlets.NewDefinition, string.Empty, ScopedItemOptions.ReadOnly),
            new SessionStateAliasEntry(LanguageKeywords.Section, LanguageCmdlets.NewSection, string.Empty, ScopedItemOptions.ReadOnly),
            new SessionStateAliasEntry(LanguageKeywords.BlockQuote, LanguageCmdlets.FormatBlockQuote, string.Empty, ScopedItemOptions.ReadOnly),
            new SessionStateAliasEntry(LanguageKeywords.Code, LanguageCmdlets.FormatCode, string.Empty, ScopedItemOptions.ReadOnly),
            new SessionStateAliasEntry(LanguageKeywords.List, LanguageCmdlets.FormatList, string.Empty, ScopedItemOptions.ReadOnly),
            new SessionStateAliasEntry(LanguageKeywords.Note, LanguageCmdlets.FormatNote, string.Empty, ScopedItemOptions.ReadOnly),
            new SessionStateAliasEntry(LanguageKeywords.Table, LanguageCmdlets.FormatTable, string.Empty, ScopedItemOptions.ReadOnly),
            new SessionStateAliasEntry(LanguageKeywords.Warning, LanguageCmdlets.FormatWarning, string.Empty, ScopedItemOptions.ReadOnly),
            new SessionStateAliasEntry(LanguageKeywords.Metadata, LanguageCmdlets.SetMetadata, string.Empty, ScopedItemOptions.ReadOnly),
            new SessionStateAliasEntry(LanguageKeywords.Title, LanguageCmdlets.SetTitle, string.Empty, ScopedItemOptions.ReadOnly),
            new SessionStateAliasEntry(LanguageKeywords.Include, LanguageCmdlets.AddInclude, string.Empty, ScopedItemOptions.ReadOnly),
        };

        public static InitialSessionState CreateSessionState()
        {
            var state = InitialSessionState.CreateDefault();
            state.ThreadOptions = PSThreadOptions.UseCurrentThread;
            state.ThrowOnRunspaceOpenError = true;
            RemoveDefault(state);

            // Add in language elements
            state.Commands.Add(BuiltInCmdlets);
            state.Commands.Add(BuiltInAliases);

            // Set execution policy
            SetExecutionPolicy(state, executionPolicy: Microsoft.PowerShell.ExecutionPolicy.RemoteSigned);
            return state;
        }

        private static void RemoveDefault(InitialSessionState state)
        {
            if (state.Commands[LanguageCmdlets.FormatTable].Count > 0)
                state.Commands.Remove(LanguageCmdlets.FormatTable, null);

            if (state.Commands[LanguageCmdlets.FormatList].Count > 0)
                state.Commands.Remove(LanguageCmdlets.FormatList, null);
        }

        private static bool IsReplacedCommand(string name)
        {
            return name == LanguageCmdlets.FormatTable || name == LanguageCmdlets.FormatList;
        }

        private static void SetExecutionPolicy(InitialSessionState state, Microsoft.PowerShell.ExecutionPolicy executionPolicy)
        {
            // Only set execution policy on Windows
            if (Environment.OSVersion.Platform == PlatformID.Win32NT)
                state.ExecutionPolicy = executionPolicy;
        }
    }
}
