// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

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
        public string[] Culture { get; set; }

        /// <summary>
        /// The file path location to save results.
        /// </summary>
        [DefaultValue(null)]
        public string Path { get; set; }
    }
}
