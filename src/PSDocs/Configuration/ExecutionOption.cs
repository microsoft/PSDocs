// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.ComponentModel;

namespace PSDocs.Configuration
{
    public sealed class ExecutionOption
    {
        private const LanguageMode DEFAULT_LANGUAGEMODE = Configuration.LanguageMode.FullLanguage;

        internal static readonly ExecutionOption Default = new ExecutionOption
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
                int hash = 17;
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

        [DefaultValue(null)]
        public LanguageMode? LanguageMode { get; set; }
    }
}
