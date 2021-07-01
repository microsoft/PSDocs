// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace PSDocs.Runtime
{
    internal interface IResourceExtent
    {
        string File { get; }

        int StartLineNumber { get; }
    }

    internal sealed class ResourceExtent : IResourceExtent
    {
        internal ResourceExtent(string file, int startLineNumber)
        {
            File = file;
            StartLineNumber = startLineNumber;
        }

        public string File { get; }

        public int StartLineNumber { get; }
    }
}
