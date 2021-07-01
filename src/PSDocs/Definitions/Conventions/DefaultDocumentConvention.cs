// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using PSDocs.Configuration;
using PSDocs.Runtime;
using System.Collections;
using System.IO;

namespace PSDocs.Definitions.Conventions
{
    internal sealed class DefaultDocumentConvention : BaseDocumentConvention
    {
        internal DefaultDocumentConvention(string name)
            : base(name) { }

        public override void Process(RunspaceContext context, IEnumerable input)
        {
            var culture = context.Builder.Document.Culture;
            var rootedPath = PSDocumentOption.GetRootedPath(context.Pipeline.Option.Output.Path);
            var outputPath = !string.IsNullOrEmpty(culture) && context.Pipeline.Option.Output?.Culture?.Length > 1 ?
                Path.Combine(rootedPath, culture) : rootedPath;

            context.DocumentContext.InstanceName = context.Builder.Document.Name;
            context.DocumentContext.OutputPath = outputPath;
        }
    }
}
