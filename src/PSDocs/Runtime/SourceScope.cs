// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text;
using PSDocs.Pipeline;

namespace PSDocs.Runtime
{
    internal sealed class SourceScope
    {
        public readonly SourceFile File;
        public readonly string[] Content;

        public SourceScope(SourceFile source)
        {
            File = source;
            Content = System.IO.File.ReadAllLines(source.Path, Encoding.UTF8);
        }
    }
}
