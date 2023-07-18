// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Management.Automation;
using System.Threading;
using PSDocs.Resources;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace PSDocs.Configuration
{
    /// <summary>
    /// A delgate to allow callback to PowerShell to get current working path.
    /// </summary>
    internal delegate string PathDelegate();

    public interface IPSDocumentOption
    {
        /// <summary>
        /// A set of key/ value configuration options for document definitions.
        /// </summary>
        ConfigurationOption Configuration { get; }

        DocumentOption Document { get; }

        /// <summary>
        /// Options that affect document execution.
        /// </summary>
        ExecutionOption Execution { get; }

        /// <summary>
        /// Options that affect how input types are processed.
        /// </summary>
        InputOption Input { get; }

        /// <summary>
        /// Options that affect markdown formatting.
        /// </summary>
        MarkdownOption Markdown { get; }

        /// <summary>
        /// Options that affect how output is generated.
        /// </summary>
        OutputOption Output { get; }
    }

    public sealed class PSDocumentOption : IPSDocumentOption
    {
        private const string DEFAULT_FILENAME = "ps-docs.yaml";

        private static readonly PSDocumentOption Default = new()
        {
            Document = DocumentOption.Default,
            Execution = ExecutionOption.Default,
            Input = InputOption.Default,
            Markdown = MarkdownOption.Default,
            Output = OutputOption.Default,
        };

        private string SourcePath;

        public PSDocumentOption()
        {
            // Set defaults
            Configuration = new ConfigurationOption();
            Document = new DocumentOption();
            Execution = new ExecutionOption();
            Input = new InputOption();
            Markdown = new MarkdownOption();
            Output = new OutputOption();
        }

        private PSDocumentOption(string sourcePath, PSDocumentOption option)
        {
            SourcePath = sourcePath;

            // Set from existing option instance
            Configuration = new ConfigurationOption(option?.Configuration);
            Document = new DocumentOption(option?.Document);
            Execution = new ExecutionOption(option?.Execution);
            Input = new InputOption(option?.Input);
            Markdown = new MarkdownOption(option?.Markdown);
            Output = new OutputOption(option?.Output);
        }

        /// <summary>
        /// A callback that is overridden by PowerShell so that the current working path can be retrieved.
        /// </summary>
        private static PathDelegate _GetWorkingPath = () => Directory.GetCurrentDirectory();

        /// <summary>
        /// Sets the current culture to use when processing documents unless otherwise specified.
        /// </summary>
        private static CultureInfo _CurrentCulture = Thread.CurrentThread.CurrentCulture;

        /// <summary>
        /// Reserved for internal use.
        /// </summary>
        public string Generator { get; set; }

        /// <summary>
        /// A set of key/ value configuration options for document definitions.
        /// </summary>
        public ConfigurationOption Configuration { get; set; }

        public DocumentOption Document { get; set; }

        /// <summary>
        /// Options that affect document execution.
        /// </summary>
        public ExecutionOption Execution { get; set; }

        /// <summary>
        /// Options that affect how input types are processed.
        /// </summary>
        public InputOption Input { get; set; }

        /// <summary>
        /// Options that affect markdown formatting.
        /// </summary>
        public MarkdownOption Markdown { get; set; }

        /// <summary>
        /// Options that affect how output is generated.
        /// </summary>
        public OutputOption Output { get; set; }

        public string ToYaml()
        {
            var s = new SerializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .Build();

            return s.Serialize(this);
        }

        public PSDocumentOption Clone()
        {
            return new PSDocumentOption(SourcePath, option: this);
        }

        private static PSDocumentOption Combine(PSDocumentOption o1, PSDocumentOption o2)
        {
            var result = new PSDocumentOption(o1?.SourcePath ?? o2?.SourcePath, o1);
            result.Configuration = ConfigurationOption.Combine(result.Configuration, o2?.Configuration);
            result.Document = DocumentOption.Combine(result.Document, o2?.Document);
            result.Execution = ExecutionOption.Combine(result.Execution, o2?.Execution);
            result.Input = InputOption.Combine(result.Input, o2?.Input);
            result.Markdown = MarkdownOption.Combine(result.Markdown, o2?.Markdown);
            result.Output = OutputOption.Combine(result.Output, o2?.Output);
            return result;
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
            return FromEnvironment(FromYaml(path: filePath, yaml: File.ReadAllText(filePath)));
        }

        public static PSDocumentOption FromDefault()
        {
            return Default.Clone();
        }

        // <summary>
        /// Load a YAML formatted PSDocumentOption object from disk.
        /// </summary>
        /// <param name="path">A file or directory to read options from.</param>
        /// <returns>An options object.</returns>
        /// <remarks>
        /// This method is called from PowerShell.
        /// </remarks>
        public static PSDocumentOption FromFileOrEmpty(string path)
        {
            // Get a rooted file path instead of directory or relative path
            var filePath = GetFilePath(path);

            // Return empty options if file does not exist
            if (!File.Exists(filePath))
                return new PSDocumentOption();

            return FromEnvironment(FromYaml(path: filePath, yaml: File.ReadAllText(filePath)));
        }

        /// <summary>
        /// Load a YAML formatted PSDocumentOption object from disk.
        /// </summary>
        /// <param name="option"></param>
        /// <param name="path">A file or directory to read options from.</param>
        /// <returns>An options object.</returns>
        /// <remarks>
        /// This method is called from PowerShell.
        /// </remarks>
        public static PSDocumentOption FromFileOrEmpty(PSDocumentOption option, string path)
        {
            if (option == null)
                return FromFileOrEmpty(path);

            return !string.IsNullOrEmpty(path) ? Combine(option, FromFileOrEmpty(path)) : option;
        }

        public static PSDocumentOption FromYaml(string yaml)
        {
            var d = new DeserializerBuilder()
                .IgnoreUnmatchedProperties()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .WithTypeConverter(new StringArrayTypeConverter())
                .Build();

            return d.Deserialize<PSDocumentOption>(yaml) ?? new PSDocumentOption();
        }

        public static PSDocumentOption FromYaml(string path, string yaml)
        {
            var d = new DeserializerBuilder()
                .IgnoreUnmatchedProperties()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .WithTypeConverter(new StringArrayTypeConverter())
                .Build();
            var option = d.Deserialize<PSDocumentOption>(yaml) ?? new PSDocumentOption();
            option.SourcePath = path;
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

        public static void UseCurrentCulture()
        {
            UseCurrentCulture(Thread.CurrentThread.CurrentCulture);
        }

        public static void UseCurrentCulture(string culture)
        {
            UseCurrentCulture(CultureInfo.CreateSpecificCulture(culture));
        }

        public static void UseCurrentCulture(CultureInfo culture)
        {
            _CurrentCulture = culture;
        }

        public static string GetWorkingPath()
        {
            return _GetWorkingPath();
        }

        public static CultureInfo GetCurrentCulture()
        {
            return _CurrentCulture;
        }

        public override bool Equals(object obj)
        {
            return obj is PSDocumentOption option && Equals(option);
        }

        public bool Equals(PSDocumentOption other)
        {
            return other != null &&
                Configuration == other.Configuration &&
                Document == other.Document &&
                Execution == other.Execution &&
                Input == other.Input &&
                Markdown == other.Markdown &&
                Output == other.Output;
        }

        public override int GetHashCode()
        {
            unchecked // Overflow is fine
            {
                var hash = 17;
                hash = hash * 23 + (Configuration != null ? Configuration.GetHashCode() : 0);
                hash = hash * 23 + (Document != null ? Document.GetHashCode() : 0);
                hash = hash * 23 + (Execution != null ? Execution.GetHashCode() : 0);
                hash = hash * 23 + (Input != null ? Input.GetHashCode() : 0);
                hash = hash * 23 + (Markdown != null ? Markdown.GetHashCode() : 0);
                hash = hash * 23 + (Output != null ? Output.GetHashCode() : 0);
                return hash;
            }
        }

        /// <summary>
        /// Convert from hashtable to options by processing key values. This enables -Option @{ } from PowerShell.
        /// </summary>
        /// <param name="hashtable"></param>
        public static implicit operator PSDocumentOption(Hashtable hashtable)
        {
            return FromHashtable(hashtable);
        }

        /// <summary>
        /// Convert from string to options by loading the yaml file from disk. This enables -Option '.\ps-docs.yaml' from PowerShell.
        /// </summary>
        /// <param name="path"></param>
        public static implicit operator PSDocumentOption(string path)
        {
            return FromFile(path);
        }

        private static PSDocumentOption FromEnvironment(PSDocumentOption option)
        {
            if (option == null)
                option = new PSDocumentOption();

            // Start loading matching values
            var env = EnvironmentHelper.Default;
            //option.Document.Load(env); // Currently set from cmdlet
            option.Execution.Load(env);
            option.Input.Load(env);
            option.Markdown.Load(env);
            option.Output.Load(env);
            option.Configuration.Load(env);
            return option;
        }

        public static PSDocumentOption FromHashtable(Hashtable hashtable)
        {
            var option = new PSDocumentOption();
            if (hashtable == null)
                return option;

            // Start loading matching values
            var index = BuildIndex(hashtable);
            //option.Document.Load(index); // Currently set from cmdlet
            option.Execution.Load(index);
            option.Input.Load(index);
            option.Markdown.Load(index);
            option.Output.Load(index);
            option.Configuration.Load(index);
            return option;
        }

        /// <summary>
        /// Build index to allow mapping values.
        /// </summary>
        [DebuggerStepThrough]
        internal static Dictionary<string, object> BuildIndex(Hashtable hashtable)
        {
            var index = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
            foreach (DictionaryEntry entry in hashtable)
                index.Add(entry.Key.ToString(), entry.Value);

            return index;
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
                    return rootedPath;
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
            if (string.IsNullOrEmpty(path))
                return Path.GetFullPath(GetWorkingPath());

            return Path.IsPathRooted(path) ? path : Path.GetFullPath(Path.Combine(GetWorkingPath(), path));
        }

        /// <summary>
        /// Get a full path instead of a relative path that may be passed from PowerShell.
        /// </summary>
        internal static string GetRootedBasePath(string path)
        {
            var rootedPath = GetRootedPath(path);
            if (rootedPath.Length > 0 && IsSeparator(rootedPath[rootedPath.Length - 1]))
                return rootedPath;

            return string.Concat(rootedPath, Path.DirectorySeparatorChar);
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

        [DebuggerStepThrough]
        private static bool IsSeparator(char c)
        {
            return c == Path.DirectorySeparatorChar || c == Path.AltDirectorySeparatorChar;
        }
    }
}
