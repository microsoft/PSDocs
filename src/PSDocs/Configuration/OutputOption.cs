// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace PSDocs.Configuration
{
    /// <summary>
    /// Options that affect how output is generated.
    /// </summary>
    public sealed class OutputOption : IEquatable<OutputOption>
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

        public override bool Equals(object obj)
        {
            return obj is OutputOption option && Equals(option);
        }

        public bool Equals(OutputOption other)
        {
            return other != null &&
                Culture == other.Culture &&
                Path == other.Path;
        }

        public override int GetHashCode()
        {
            unchecked // Overflow is fine
            {
                int hash = 17;
                hash = hash * 23 + (Culture != null ? Culture.GetHashCode() : 0);
                hash = hash * 23 + (Path != null ? Path.GetHashCode() : 0);
                return hash;
            }
        }

        internal static OutputOption Combine(OutputOption o1, OutputOption o2)
        {
            return new OutputOption(o1)
            {
                Culture = o1.Culture ?? o2.Culture,
                Path = o1.Path ?? o2.Path
            };
        }

        [DefaultValue(null)]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1819:Properties should not return arrays", Justification = "Exposed for serialization.")]
        public string[] Culture { get; set; }

        /// <summary>
        /// The file path location to save results.
        /// </summary>
        [DefaultValue(null)]
        public string Path { get; set; }

        internal void Load(EnvironmentHelper env)
        {
            if (env.TryStringArray("PSDOCS_OUTPUT_CULTURE", out string[] culture))
                Culture = culture;

            if (env.TryString("PSDOCS_OUTPUT_PATH", out string path))
                Path = path;
        }

        internal void Load(Dictionary<string, object> index)
        {
            if (index.TryPopStringArray("Output.Culture", out string[] culture))
                Culture = culture;

            if (index.TryPopString("Output.Path", out string path))
                Path = path;
        }
    }
}
