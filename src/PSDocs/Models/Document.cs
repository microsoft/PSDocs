﻿// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections;
using System.Collections.Specialized;
using PSDocs.Runtime;

namespace PSDocs.Models
{
    public sealed class Document : SectionNode
    {
        internal readonly DocumentContext Context;

        internal Document(DocumentContext context)
        {
            Context = context;
        }

        public string Name => Context?.InstanceName;

        public string Culture => Context?.Culture;

        public override DocumentNodeType Type => DocumentNodeType.Document;

        public OrderedDictionary Metadata => Context?.Metadata;

        public Hashtable Data => Context?.Data;

        public string Path => Context?.OutputPath;
    }
}
