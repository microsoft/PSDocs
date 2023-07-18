// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Management.Automation;
using PSDocs.Models;
using PSDocs.Processor;
using PSDocs.Runtime;

namespace PSDocs.Data
{
    internal interface IDocumentBuilder : IDisposable
    {
        string Name { get; }

        Document Process(RunspaceContext context, PSObject sourceObject);

        void End(RunspaceContext context, IDocumentResult[] completed);
    }
}
