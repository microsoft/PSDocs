
using System.Collections;
using System.Collections.Generic;

namespace PSDocs.Configuration
{
    /// <summary>
    /// A set of custom configuration values that are exposed at runtime.
    /// </summary>
    public sealed class ConfigurationOption : KeyMapDictionary<object>
    {
        private const string KEYMAP_PREFIX = "Configuration.";

        public ConfigurationOption()
            : base() { }

        public ConfigurationOption(ConfigurationOption option)
            : base(option) { }

        private ConfigurationOption(Hashtable hashtable)
            : base(hashtable) { }

        public static implicit operator ConfigurationOption(Hashtable hashtable)
        {
            return new ConfigurationOption(hashtable);
        }

        internal static ConfigurationOption Combine(ConfigurationOption o1, ConfigurationOption o2)
        {
            var result = new ConfigurationOption(o1);
            result.AddUnique(o2);
            return result;
        }

        internal void Load(IDictionary<string, object> dictionary)
        {
            Load(KEYMAP_PREFIX, dictionary);
        }
    }
}
