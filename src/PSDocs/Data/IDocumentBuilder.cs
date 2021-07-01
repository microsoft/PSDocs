// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using PSDocs.Models;
using PSDocs.Processor;
using PSDocs.Runtime;
using System;
using System.Management.Automation;

namespace PSDocs.Data
{
    internal interface IDocumentBuilder : IDisposable
    {
        string Name { get; }

        Document Process(RunspaceContext context, PSObject sourceObject);

        void End(RunspaceContext context, IDocumentResult[] completed);
    }
}
