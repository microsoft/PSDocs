// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections;
using PSDocs.Runtime;

namespace PSDocs.Definitions.Conventions
{
    internal abstract class BaseDocumentConvention : IDocumentConvention
    {
        protected BaseDocumentConvention(string name)
        {
            Name = name;
        }

        public string Name { get; }

        public virtual void Begin(RunspaceContext context, IEnumerable input)
        {

        }

        public virtual void Process(RunspaceContext context, IEnumerable input)
        {

        }

        public virtual void End(RunspaceContext context, IEnumerable input)
        {

        }
    }
}
