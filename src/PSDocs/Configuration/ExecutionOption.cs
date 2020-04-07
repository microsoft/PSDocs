using System.ComponentModel;

namespace PSDocs.Configuration
{
    public sealed class ExecutionOption
    {
        private const LanguageMode DEFAULT_LANGUAGEMODE = Configuration.LanguageMode.FullLanguage;

        internal static readonly ExecutionOption Default = new ExecutionOption
        {
            LanguageMode = DEFAULT_LANGUAGEMODE
        };

        public ExecutionOption()
        {
            LanguageMode = null;
        }

        internal ExecutionOption(ExecutionOption option)
        {
            LanguageMode = option.LanguageMode;
        }

        [DefaultValue(DEFAULT_LANGUAGEMODE)]
        public LanguageMode? LanguageMode { get; set; }
    }
}
