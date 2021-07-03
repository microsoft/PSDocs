// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections;
using System.Collections.Generic;

namespace PSDocs.Configuration
{
    /// <summary>
    /// A set of key/ value configuration options for document definitions.
    /// </summary>
    public sealed class ConfigurationOption : KeyMapDictionary<object>
    {
        private const string ENVIRONMENT_PREFIX = "PSDOCS_CONFIGURATION_";
        private const string DICTIONARY_PREFIX = "Configuration.";

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

        internal void Load(EnvironmentHelper env)
        {
            base.Load(ENVIRONMENT_PREFIX, env);
        }

        internal void Load(IDictionary<string, object> dictionary)
        {
            base.Load(DICTIONARY_PREFIX, dictionary);
        }
    }
}
