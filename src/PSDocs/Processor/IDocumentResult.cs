// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections;
using System.Collections.Specialized;

namespace PSDocs.Processor
{
    internal interface IDocumentResult
    {
        string InstanceName { get; }

        string Extension { get; }

        string Culture { get; }

        string OutputPath { get; }

        string FullName { get; }

        OrderedDictionary Metadata { get; }

        Hashtable Data { get; }
    }
}
