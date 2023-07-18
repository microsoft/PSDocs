// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Management.Automation;
using PSDocs.Configuration;
using PSDocs.Pipeline.Output;
using PSDocs.Resources;

namespace PSDocs.Pipeline
{
    public enum SourceType
    {
        Script = 1,

        Yaml = 2
    }

    /// <summary>
    /// A source file for document definitions.
    /// </summary>
    [DebuggerDisplay("{Type}: {Path}")]
    public sealed class SourceFile
    {
        public readonly string Path;
        public readonly string ModuleName;
        public readonly SourceType Type;

        public SourceFile(string path, string moduleName, SourceType type, string helpPath)
        {
            Path = path;
            ModuleName = moduleName;
            Type = type;
            ResourcePath = helpPath;
        }

        public string ResourcePath { get; }
    }

    public sealed class Source
    {
        public readonly string Path;
        public readonly SourceFile[] File;
        internal readonly ModuleInfo Module;

        internal Source(string path, SourceFile[] file)
        {
            Path = path;
            Module = null;
            File = file;
        }

        internal Source(ModuleInfo module, SourceFile[] file)
        {
            Path = module.Path;
            Module = module;
            File = file;
        }

        internal sealed class ModuleInfo
        {
            public readonly string Path;
            public readonly string Name;

            public ModuleInfo(PSModuleInfo info)
            {
                Path = info.ModuleBase;
                Name = info.Name;
            }
        }
    }

    /// <summary>
    /// A helper to build a list of document sources for discovery.
    /// </summary>
    public sealed class SourcePipelineBuilder
    {
        private readonly List<Source> _Source;
        private readonly HostContext _HostContext;
        private readonly IPipelineWriter _Writer;

        internal SourcePipelineBuilder(HostContext hostContext)
        {
            _Source = new List<Source>();
            _HostContext = hostContext;
            _Writer = new HostPipelineWriter(hostContext);
        }

        public bool ShouldLoadModule => _HostContext.GetAutoLoadingPreference() == PSModuleAutoLoadingPreference.All;

        private void VerboseScanSource(string path)
        {
            _Writer.WriteVerbose(PSDocsResources.ScanSource, path);
        }

        private void VerboseFoundModules(int count)
        {
            _Writer.WriteVerbose(PSDocsResources.FoundModules, count);
        }

        private void VerboseScanModule(string moduleName)
        {
            _Writer.WriteVerbose(PSDocsResources.ScanModule, moduleName);
        }

        /// <summary>
        /// Add loose files as a source.
        /// </summary>
        /// <param name="path">A file or directory path containing one or more document files.</param>
        public void Directory(string[] path)
        {
            if (path == null || path.Length == 0)
                return;

            for (var i = 0; i < path.Length; i++)
                Directory(path[i]);
        }

        public void Directory(string path)
        {
            if (string.IsNullOrEmpty(path))
                return;

            VerboseScanSource(path);
            var files = GetFiles(path, null);

            if (files == null || files.Length == 0)
                return;

            Source(new Source(path, files));
        }

        /// <summary>
        /// Add a module source.
        /// </summary>
        /// <param name="module"></param>
        public void Module(PSModuleInfo[] module)
        {
            if (module == null)
                return;

            for (var i = 0; i < module.Length; i++)
            {
                VerboseScanModule(module[i].Name);
                var files = GetFiles(module[i].ModuleBase, module[i].ModuleBase, module[i].Name);

                if (files == null || files.Length == 0)
                    continue;

                Source(new Source(new Source.ModuleInfo(module[i]), file: files));
            }
        }

        public Source[] Build()
        {
            return _Source.ToArray();
        }

        private void Source(Source source)
        {
            _Source.Add(source);
        }

        private static SourceFile[] GetFiles(string path, string helpPath, string moduleName = null)
        {
            var result = new List<SourceFile>();
            var rootedPath = PSDocumentOption.GetRootedPath(path);
            var extension = Path.GetExtension(rootedPath);
            if (extension == ".ps1" || extension == ".yaml" || extension == ".yml")
            {
                if (!File.Exists(rootedPath))
                    throw new FileNotFoundException(PSDocsResources.SourceNotFound, rootedPath);

                if (helpPath == null)
                    helpPath = Path.GetDirectoryName(rootedPath);

                result.Add(new SourceFile(rootedPath, null, GetSourceType(rootedPath), helpPath));
            }
            else if (System.IO.Directory.Exists(rootedPath))
            {
                var files = System.IO.Directory.EnumerateFiles(rootedPath, "*.Doc.*", SearchOption.AllDirectories);
                foreach (var file in files)
                {
                    if (Include(file))
                    {
                        if (helpPath == null)
                            helpPath = Path.GetDirectoryName(file);

                        result.Add(new SourceFile(file, moduleName, GetSourceType(file), helpPath));
                    }
                }
            }
            else
            {
                return null;
            }
            return result.ToArray();
        }

        private static bool Include(string path)
        {
            return path.EndsWith(".Doc.ps1", System.StringComparison.OrdinalIgnoreCase) ||
                path.EndsWith(".Doc.yaml", System.StringComparison.OrdinalIgnoreCase) ||
                path.EndsWith(".Doc.yml", System.StringComparison.OrdinalIgnoreCase);
        }

        private static SourceType GetSourceType(string path)
        {
            var extension = Path.GetExtension(path);
            return (extension == ".yaml" || extension == ".yml") ? SourceType.Yaml : SourceType.Script;
        }
    }
}
