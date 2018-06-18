using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace PSDocs.Configuration
{
    /// <summary>
    /// A delgate to allow callback to PowerShell to get current working path.
    /// </summary>
    public delegate string GetWorkingPathDelegate();

    public sealed class PSDocumentOption
    {
        public PSDocumentOption()
        {
            // Set defaults
            Markdown = new MarkdownOption();
        }

        public PSDocumentOption(PSDocumentOption option)
        {
            // Set from existing option instance
            Markdown = new MarkdownOption
            {
                WrapSeparator = option.Markdown.WrapSeparator,
                Encoding = option.Markdown.Encoding,
                SkipEmptySections = option.Markdown.SkipEmptySections
            };
        }

        /// <summary>
        /// A callback that is overridden by PowerShell so that the current working path can be retrieved.
        /// </summary>
        public static GetWorkingPathDelegate GetWorkingPath = () => Directory.GetCurrentDirectory();

        /// <summary>
        /// Reserved for internal use.
        /// </summary>
        public string Generator { get; set; }

        /// <summary>
        /// Options that affect markdown formatting.
        /// </summary>
        public MarkdownOption Markdown { get; set; }

        public string ToYaml()
        {
            var s = new SerializerBuilder()
                .WithNamingConvention(new CamelCaseNamingConvention())
                .Build();

            return s.Serialize(this);
        }

        public PSDocumentOption Clone()
        {
            return new PSDocumentOption(this);
        }

        public static PSDocumentOption FromFile(string path, bool silentlyContinue = false)
        {
            // Ensure that a full path instead of a path relative to PowerShell is used for .NET methods
            var rootedPath = GetRootedPath(path);

            // Fallback to defaults even if file does not exist when silentlyContinue is true
            if (!File.Exists(rootedPath))
            {
                if (!silentlyContinue)
                {
                    throw new FileNotFoundException("", rootedPath);
                }
                else
                {
                    // Use the default options
                    return new PSDocumentOption();
                }
            }

            return FromYaml(File.ReadAllText(rootedPath));
        }

        public static PSDocumentOption FromYaml(string yaml)
        {
            var d = new DeserializerBuilder()
                .IgnoreUnmatchedProperties()
                .WithNamingConvention(new CamelCaseNamingConvention())
                .Build();

            return d.Deserialize<PSDocumentOption>(yaml) ?? new PSDocumentOption();
        }

        /// <summary>
        /// Convert from hashtable to options by processing key values. This enables -Option @{ } from PowerShell.
        /// </summary>
        /// <param name="hashtable"></param>
        public static implicit operator PSDocumentOption(Hashtable hashtable)
        {
            var option = new PSDocumentOption();

            // Build index to allow mapping
            var index = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);

            foreach (DictionaryEntry entry in hashtable)
            {
                index.Add(entry.Key.ToString(), entry.Value);
            }

            // Start loading matching values

            object value;

            if (index.TryGetValue("markdown.wrapseparator", out value))
            {
                option.Markdown.WrapSeparator = (string)value;
            }

            if (index.TryGetValue("markdown.encoding", out value))
            {
                option.Markdown.Encoding = (MarkdownEncoding)Enum.Parse(typeof(MarkdownEncoding), (string)value);
            }

            if (index.TryGetValue("markdown.skipemptysections", out value))
            {
                option.Markdown.SkipEmptySections = (bool)value;
            }

            return option;
        }

        /// <summary>
        /// Convert from string to options by loading the yaml file from disk. This enables -Option '.\.psdocs.yml' from PowerShell.
        /// </summary>
        /// <param name="path"></param>
        public static implicit operator PSDocumentOption(string path)
        {
            var option = FromFile(path);

            return option;
        }

        /// <summary>
        /// Get a full path instead of a relative path that may be passed from PowerShell.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        private static string GetRootedPath(string path)
        {
            return Path.IsPathRooted(path) ? path : Path.Combine(GetWorkingPath(), path);
        }
    }
}
