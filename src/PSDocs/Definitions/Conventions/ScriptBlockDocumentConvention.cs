// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections;
using PSDocs.Pipeline;
using PSDocs.Runtime;

namespace PSDocs.Definitions.Conventions
{
    internal sealed class ScriptBlockDocumentConvention : BaseDocumentConvention, ILanguageBlock
    {
        private readonly LanguageScriptBlock _Begin;
        private readonly LanguageScriptBlock _Process;
        private readonly LanguageScriptBlock _End;

        public ScriptBlockDocumentConvention(SourceFile source, string name, LanguageScriptBlock begin, LanguageScriptBlock process, LanguageScriptBlock end)
            : base(name)
        {
            Source = source;
            Id = ResourceHelper.GetId(source.ModuleName, name);
            _Begin = begin;
            _Process = process;
            _End = end;
        }

        public string Id { get; }

        public SourceFile Source { get; }

        string ILanguageBlock.Module => Source.ModuleName;

        string ILanguageBlock.SourcePath => Source.Path;

        public override void Begin(RunspaceContext context, IEnumerable input)
        {
            InvokeConventionBlock(_Begin, input);
        }

        public override void Process(RunspaceContext context, IEnumerable input)
        {
            InvokeConventionBlock(_Process, input);
        }

        public override void End(RunspaceContext context, IEnumerable input)
        {
            InvokeConventionBlock(_End, input);
        }

        private void InvokeConventionBlock(LanguageScriptBlock block, IEnumerable input)
        {
            if (block == null)
                return;

            try
            {
                block.Invoke();
            }
            finally
            {

            }
        }
    }
}
