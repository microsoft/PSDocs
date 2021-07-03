// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using PSDocs.Configuration;
using System.Collections.Generic;

namespace PSDocs.Pipeline
{
    internal sealed class OptionContext : IPSDocumentOption
    {
        private readonly IPSDocumentOption _Workspace;
        private readonly Dictionary<string, IPSDocumentOption> _ModuleScope;

        private IPSDocumentOption _Option;

        public OptionContext(IPSDocumentOption option)
        {
            _Workspace = option;
            _ModuleScope = new Dictionary<string, IPSDocumentOption>();
            _Option = _Workspace;
        }

        #region IPSDocumentOption

        public ConfigurationOption Configuration => _Option.Configuration;

        public DocumentOption Document => _Option.Document;

        public ExecutionOption Execution => _Option.Execution;

        public InputOption Input => _Option.Input;

        public MarkdownOption Markdown => _Option.Markdown;

        public OutputOption Output => _Option.Output;

        #endregion IPSDocumentOption

        internal enum ScopeType
        {
            Parameter = 0,

            Explicit = 1,

            Workspace = 2,

            Module = 3
        }

        internal void WithScope(IPSDocumentOption option, ScopeType type, string moduleName = null)
        {
            if (type == ScopeType.Module && !_ModuleScope.ContainsKey(moduleName))
            {
                _ModuleScope[moduleName] = option;
            }
        }

        internal void SwitchScope(string module)
        {
            _Option = !string.IsNullOrEmpty(module) && _ModuleScope.TryGetValue(module, out IPSDocumentOption moduleOption) ? moduleOption : _Workspace;
        }
    }
}
