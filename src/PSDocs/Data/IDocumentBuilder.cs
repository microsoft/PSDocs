using PSDocs.Models;
using PSDocs.Runtime;
using System;
using System.Management.Automation;

namespace PSDocs.Data
{
    internal interface IDocumentBuilder : IDisposable
    {
        string Name { get; }

        Document Process(RunspaceContext context, PSObject sourceObject);
    }
}
