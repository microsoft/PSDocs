// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace PSDocs.Configuration
{
    /// <summary>
    /// Options that affect document execution.
    /// </summary>
    public sealed class ExecutionOption : IEquatable<ExecutionOption>
    {
        private const LanguageMode DEFAULT_LANGUAGEMODE = Configuration.LanguageMode.FullLanguage;

        internal static readonly ExecutionOption Default = new()
        {
            LanguageMode = DEFAULT_LANGUAGEMODE,
        };

        public ExecutionOption()
        {
            LanguageMode = null;
        }

        internal ExecutionOption(ExecutionOption option)
        {
            LanguageMode = option.LanguageMode;
        }

        public override bool Equals(object obj)
        {
            return obj is ExecutionOption option && Equals(option);
        }

        public bool Equals(ExecutionOption other)
        {
            return other != null &&
                LanguageMode == other.LanguageMode;
        }

        public override int GetHashCode()
        {
            unchecked // Overflow is fine
            {
                var hash = 17;
                hash = hash * 23 + (LanguageMode.HasValue ? LanguageMode.Value.GetHashCode() : 0);
                return hash;
            }
        }

        internal static ExecutionOption Combine(ExecutionOption o1, ExecutionOption o2)
        {
            return new ExecutionOption(o1)
            {
                LanguageMode = o1.LanguageMode ?? o2.LanguageMode
            };
        }

        /// <summary>
        /// The PowerShell language mode to use for document execution.
        /// </summary>
        /// <remarks>
        /// The default is FullLanguage.
        /// </remarks>
        [DefaultValue(null)]
        public LanguageMode? LanguageMode { get; set; }

        internal void Load(EnvironmentHelper env)
        {
            if (env.TryEnum("PSDOCS_EXECUTION_LANGUAGEMODE", out LanguageMode languageMode))
                LanguageMode = languageMode;
        }

        internal void Load(Dictionary<string, object> index)
        {
            if (index.TryPopEnum("Execution.LanguageMode", out LanguageMode languageMode))
                LanguageMode = languageMode;
        }
    }
}
