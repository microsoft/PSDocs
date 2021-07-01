// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.IO;

namespace PSDocs.Models
{
    public sealed class Include : DocumentNode
    {
        private string _Path;

        public override DocumentNodeType Type => DocumentNodeType.Include;

        public string Path
        {
            get { return _Path; }
            set
            {
                _Path = value;
                Exists = File.Exists(_Path);
            }
        }

        public string Content { get; set; }

        internal bool Exists { get; private set; }

        public override string ToString()
        {
            return Content;
        }
    }
}
