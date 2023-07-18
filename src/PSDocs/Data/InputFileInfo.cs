// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.IO;
using PSDocs.Configuration;

namespace PSDocs.Data
{
    public sealed class InputFileInfo : ITargetInfo
    {
        private readonly string _TargetType;

        internal readonly bool IsUrl;

        internal InputFileInfo(string basePath, string path)
        {
            if (path.IsUri())
            {
                FullName = path;
                IsUrl = true;
                return;
            }
            path = PSDocumentOption.GetRootedPath(path);
            FullName = path;
            BasePath = basePath;
            Name = System.IO.Path.GetFileName(path);
            Extension = System.IO.Path.GetExtension(path);
            DirectoryName = System.IO.Path.GetDirectoryName(path);
            Path = ExpressionHelpers.NormalizePath(basePath, FullName);
            _TargetType = string.IsNullOrEmpty(Extension) ? System.IO.Path.GetFileNameWithoutExtension(path) : Extension;
        }

        public string FullName { get; }

        public string BasePath { get; }

        public string Name { get; }

        public string Extension { get; }

        public string DirectoryName { get; }

        public string Path { get; }

        string ITargetInfo.TargetName => Path;

        string ITargetInfo.TargetType => _TargetType;

        /// <summary>
        /// Convert to string.
        /// </summary>
        public override string ToString()
        {
            return FullName;
        }

        /// <summary>
        /// Convert to FileInfo.
        /// </summary>
        public FileInfo AsFileInfo()
        {
            return new FileInfo(FullName);
        }
    }
}
