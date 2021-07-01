// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace PSDocs.Runtime
{
    public interface ILanguageBlock
    {
        string Id { get; }

        string SourcePath { get; }

        string Module { get; }
    }
}
