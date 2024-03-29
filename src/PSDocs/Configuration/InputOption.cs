﻿// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace PSDocs.Configuration
{
    /// <summary>
    /// Options that affect how input types are processed.
    /// </summary>
    public sealed class InputOption : IEquatable<InputOption>
    {
        private const InputFormat DEFAULT_FORMAT = PSDocs.Configuration.InputFormat.Detect;
        private const string DEFAULT_OBJECTPATH = null;
        private const string[] DEFAULT_PATHIGNORE = null;

        internal static readonly InputOption Default = new()
        {
            Format = DEFAULT_FORMAT,
            ObjectPath = DEFAULT_OBJECTPATH,
            PathIgnore = DEFAULT_PATHIGNORE,
        };

        public InputOption()
        {
            Format = null;
            ObjectPath = null;
            PathIgnore = null;
        }

        public InputOption(InputOption option)
        {
            if (option == null)
                return;

            Format = option.Format;
            ObjectPath = option.ObjectPath;
            PathIgnore = option.PathIgnore;
        }

        public override bool Equals(object obj)
        {
            return obj is InputOption option && Equals(option);
        }

        public bool Equals(InputOption other)
        {
            return other != null &&
                Format == other.Format &&
                ObjectPath == other.ObjectPath &&
                PathIgnore == other.PathIgnore;
        }

        public override int GetHashCode()
        {
            unchecked // Overflow is fine
            {
                var hash = 17;
                hash = hash * 23 + (Format.HasValue ? Format.Value.GetHashCode() : 0);
                hash = hash * 23 + (ObjectPath != null ? ObjectPath.GetHashCode() : 0);
                hash = hash * 23 + (PathIgnore != null ? PathIgnore.GetHashCode() : 0);
                return hash;
            }
        }

        internal static InputOption Combine(InputOption o1, InputOption o2)
        {
            var result = new InputOption(o1)
            {
                Format = o1.Format ?? o2.Format,
                ObjectPath = o1.ObjectPath ?? o2.ObjectPath,
                PathIgnore = o1.PathIgnore ?? o2.PathIgnore
            };
            return result;
        }

        /// <summary>
        /// The input string format.
        /// </summary>
        [DefaultValue(null)]
        public InputFormat? Format { get; set; }

        /// <summary>
        /// The object path to a property to use instead of the pipeline object.
        /// </summary>
        [DefaultValue(null)]
        public string ObjectPath { get; set; }

        /// <summary>
        /// Ignores input files that match the path spec.
        /// </summary>
        [DefaultValue(null)]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1819:Properties should not return arrays", Justification = "Exposed for serialization.")]
        public string[] PathIgnore { get; set; }

        internal void Load(EnvironmentHelper env)
        {
            if (env.TryEnum("PSDOCS_INPUT_FORMAT", out InputFormat format))
                Format = format;

            if (env.TryString("PSDOCS_INPUT_OBJECTPATH", out var objectPath))
                ObjectPath = objectPath;

            if (env.TryStringArray("PSDOCS_INPUT_PATHIGNORE", out var pathIgnore))
                PathIgnore = pathIgnore;
        }

        internal void Load(Dictionary<string, object> index)
        {
            if (index.TryPopEnum("Input.Format", out InputFormat format))
                Format = format;

            if (index.TryPopString("Input.ObjectPath", out var objectPath))
                ObjectPath = objectPath;

            if (index.TryPopStringArray("Input.PathIgnore", out var pathIgnore))
                PathIgnore = pathIgnore;
        }
    }
}
