using System.ComponentModel;

namespace PSDocs.Configuration
{
    public sealed class OutputOption
    {
        internal static readonly OutputOption Default = new OutputOption
        {
            Culture = null,
            Path = null,
        };

        public OutputOption()
        {
            Culture = null;
            Path = null;
        }

        internal OutputOption(OutputOption option)
        {
            Culture = option.Culture;
            Path = option.Path;
        }

        [DefaultValue(null)]
        public string[] Culture { get; set; }

        /// <summary>
        /// The file path location to save results.
        /// </summary>
        [DefaultValue(null)]
        public string Path { get; set; }
    }
}
