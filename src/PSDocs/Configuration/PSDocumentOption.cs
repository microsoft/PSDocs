using PSDocs.Resources;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Management.Automation;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace PSDocs.Configuration
{
    /// <summary>
    /// A delgate to allow callback to PowerShell to get current working path.
    /// </summary>
    internal delegate string PathDelegate();

    public sealed class PSDocumentOption
    {
        private const string DEFAULT_FILENAME = "psdocs.yml";

        private static readonly PSDocumentOption Default = new PSDocumentOption
        {
            Execution = ExecutionOption.Default,
            //Markdown = MarkdownOption.Default
        };

        public PSDocumentOption()
        {
            // Set defaults
            Document = new DocumentOption();
            Execution = new ExecutionOption();
            Markdown = new MarkdownOption();
            Output = new OutputOption();
        }

        public PSDocumentOption(PSDocumentOption option)
        {
            // Set from existing option instance
            Document = new DocumentOption(option.Document);
            Execution = new ExecutionOption(option.Execution);
            Markdown = new MarkdownOption(option.Markdown);
            Output = new OutputOption(option.Output);
        }

        /// <summary>
        /// A callback that is overridden by PowerShell so that the current working path can be retrieved.
        /// </summary>
        private static PathDelegate _GetWorkingPath = () => Directory.GetCurrentDirectory();

        /// <summary>
        /// Reserved for internal use.
        /// </summary>
        public string Generator { get; set; }

        public DocumentOption Document { get; set; }

        /// <summary>
        /// Options that affect script execution.
        /// </summary>
        public ExecutionOption Execution { get; set; }

        /// <summary>
        /// Options that affect markdown formatting.
        /// </summary>
        public MarkdownOption Markdown { get; set; }

        public OutputOption Output { get; set; }

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

        /// <summary>
        /// Load a YAML formatted PDocumentOption object from disk.
        /// </summary>
        /// <param name="path">The file or directory path to load options from.</param>
        /// <param name="silentlyContinue">When false, if the file does not exist, and exception will be raised.</param>
        /// <returns></returns>
        public static PSDocumentOption FromFile(string path, bool silentlyContinue = false)
        {
            // Get a rooted file path instead of directory or relative path
            var filePath = GetFilePath(path: path);

            // Fallback to defaults even if file does not exist when silentlyContinue is true
            if (!File.Exists(filePath))
            {
                if (!silentlyContinue)
                {
                    throw new FileNotFoundException(PSDocsResources.OptionsNotFound, filePath);
                }
                else
                {
                    // Use the default options
                    return Default.Clone();
                }
            }
            return FromYaml(path: filePath, yaml: File.ReadAllText(filePath));
        }

        public static PSDocumentOption FromYaml(string yaml)
        {
            var d = new DeserializerBuilder()
                .IgnoreUnmatchedProperties()
                .WithNamingConvention(new CamelCaseNamingConvention())
                .Build();

            return d.Deserialize<PSDocumentOption>(yaml) ?? new PSDocumentOption();
        }

        public static PSDocumentOption FromYaml(string path, string yaml)
        {
            var d = new DeserializerBuilder()
                .IgnoreUnmatchedProperties()
                .WithNamingConvention(new CamelCaseNamingConvention())
                .Build();
            var option = d.Deserialize<PSDocumentOption>(yaml) ?? new PSDocumentOption();
            //option.SourcePath = path;
            return option;
        }

        /// <summary>
        /// Set working path from PowerShell host environment.
        /// </summary>
        /// <param name="executionContext">An $ExecutionContext object.</param>
        /// <remarks>
        /// Called from PowerShell.
        /// </remarks>
        public static void UseExecutionContext(EngineIntrinsics executionContext)
        {
            if (executionContext == null)
            {
                _GetWorkingPath = () => Directory.GetCurrentDirectory();

                return;
            }

            _GetWorkingPath = () => executionContext.SessionState.Path.CurrentFileSystemLocation.Path;
        }

        public static string GetWorkingPath()
        {
            return _GetWorkingPath();
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


            if (index.TryGetValue("markdown.wrapseparator", out object value))
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

            if (index.TryGetValue("markdown.columnpadding", out value))
            {
                option.Markdown.ColumnPadding = (ColumnPadding)Enum.Parse(typeof(ColumnPadding), (string)value);
            }

            if (index.TryGetValue("markdown.useedgepipes", out value))
            {
                option.Markdown.UseEdgePipes = (EdgePipeOption)Enum.Parse(typeof(EdgePipeOption), (string)value);
            }

            if (index.TryGetValue("execution.languagemode", out value))
            {
                option.Execution.LanguageMode = (LanguageMode)Enum.Parse(typeof(LanguageMode), (string)value);
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
        /// Get a fully qualified file path.
        /// </summary>
        /// <param name="path">A file or directory path.</param>
        /// <returns></returns>
        public static string GetFilePath(string path)
        {
            var rootedPath = GetRootedPath(path);
            if (Path.HasExtension(rootedPath))
            {
                var ext = Path.GetExtension(rootedPath);
                if (string.Equals(ext, ".yaml", StringComparison.OrdinalIgnoreCase) || string.Equals(ext, ".yml", StringComparison.OrdinalIgnoreCase))
                {
                    return rootedPath;
                }
            }

            // Check if default files exist and 
            return UseFilePath(path: rootedPath, name: "ps-docs.yaml") ??
                UseFilePath(path: rootedPath, name: "ps-docs.yml") ??
                UseFilePath(path: rootedPath, name: "psdocs.yaml") ??
                UseFilePath(path: rootedPath, name: "psdocs.yml") ??
                Path.Combine(rootedPath, DEFAULT_FILENAME);
        }

        /// <summary>
        /// Get a full path instead of a relative path that may be passed from PowerShell.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        internal static string GetRootedPath(string path)
        {
            return Path.IsPathRooted(path) ? path : Path.GetFullPath(Path.Combine(GetWorkingPath(), path));
        }

        /// <summary>
        /// Determine if the combined file path is exists.
        /// </summary>
        /// <param name="path">A directory path where a options file may be stored.</param>
        /// <param name="name">A file name of an options file.</param>
        /// <returns>Returns a file path if the file exists or null if the file does not exist.</returns>
        private static string UseFilePath(string path, string name)
        {
            var filePath = Path.Combine(path, name);
            return File.Exists(filePath) ? filePath : null;
        }
    }
}
