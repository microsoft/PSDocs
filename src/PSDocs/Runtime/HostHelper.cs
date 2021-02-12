
using PSDocs.Data;
using PSDocs.Data.Internal;
using PSDocs.Definitions;
using PSDocs.Definitions.Conventions;
using PSDocs.Pipeline;
using PSDocs.Resources;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace PSDocs.Runtime
{
    internal static class HostHelper
    {
        /// <summary>
        /// Executes get document builders from Document script blocks.
        /// </summary>
        internal static IDocumentBuilder[] GetDocumentBuilder(RunspaceContext runspace, Source[] source)
        {
            runspace.PushScope(RunspaceScope.Source);
            var blocks = GetLanguageBlock(runspace, source);
            var documents = ToDocument(blocks, runspace);
            var conventions = GetConventions(blocks, runspace);
            return ToDocumentBuilder(documents, conventions);
        }

        internal static IDocumentDefinition[] GetDocumentBlock(RunspaceContext runspace, Source[] source)
        {
            return ToDocument(GetLanguageBlock(runspace, source), runspace);
        }

        /// <summary>
        /// Execute one or more PowerShell script files to get language blocks.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="sources"></param>
        /// <returns></returns>
        private static ILanguageBlock[] GetLanguageBlock(RunspaceContext context, Source[] sources)
        {
            var results = new List<ILanguageBlock>();
            var ps = context.NewPowerShell();
            context.PushScope(RunspaceScope.Source);
            try
            {
                // Process each source
                foreach (var source in sources)
                {
                    // Process search file per source
                    foreach (var file in source.File)
                    {
                        ps.Commands.Clear();
                        if (!context.EnterSourceFile(file))
                            throw new FileNotFoundException(PSDocsResources.ScriptNotFound, file.Path);

                        try
                        {
                            // Invoke script
                            ps.AddScript(string.Concat("& '", file.Path, "'"), true);
                            var invokeResults = ps.Invoke();

                            // Discovery has errors so skip this file
                            if (ps.HadErrors)
                                continue;

                            foreach (var ir in invokeResults)
                            {
                                if (ir.BaseObject is ScriptDocumentBlock document)
                                    results.Add(document);

                                if (ir.BaseObject is ScriptBlockDocumentConvention convention)
                                    results.Add(convention);
                            }
                        }
                        catch (Exception e)
                        {
                            context.WriteRuntimeException(sourceFile: file.Path, inner: e);
                        }
                    }
                }
            }
            finally
            {
                context.ExitSourceFile();
                context.PopScope();
                ps.Runspace = null;
                ps.Dispose();
            }
            return results.ToArray();
        }

        /// <summary>
        /// Convert document blocks to document builders.
        /// </summary>
        private static IDocumentBuilder[] ToDocumentBuilder(ScriptDocumentBlock[] documents, IDocumentConvention[] conventions)
        {
            var result = new ScriptDocumentBuilder[documents.Length];
            for (var i = 0; i < documents.Length; i++)
                result[i] = new ScriptDocumentBuilder(documents[i], conventions);

            return result;
        }

        /// <summary>
        /// Convert language blocks to documents.
        /// </summary>
        private static ScriptDocumentBlock[] ToDocument(ILanguageBlock[] blocks, RunspaceContext context)
        {
            // Index by Id
            var results = new Dictionary<string, ScriptDocumentBlock>(StringComparer.OrdinalIgnoreCase);
            try
            {
                foreach (var block in blocks.OfType<ScriptDocumentBlock>())
                {
                    // Ignore blocks that don't match
                    if (!Match(context, block))
                        continue;

                    if (!results.ContainsKey(block.Id))
                        results[block.Id] = block;
                }
            }
            finally
            {
                //context.ExitSourceFile();
            }
            return results.Values.ToArray();
        }

        /// <summary>
        /// Get conventions.
        /// </summary>
        private static IDocumentConvention[] GetConventions(ILanguageBlock[] blocks, RunspaceContext runspace)
        {
            // Index by Id
            var index = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var results = new List<IDocumentConvention>(blocks.Length);
            try
            {
                foreach (var block in blocks.OfType<ScriptBlockDocumentConvention>())
                {
                    // Ignore blocks that don't match
                    if (!Match(runspace, block, out int order))
                        continue;

                    if (!index.Contains(block.Id))
                        results.Insert(order, block);
                }
            }
            finally
            {
                //context.ExitSourceFile();
            }
            results.Insert(0, new DefaultDocumentConvention("default"));
            return results.ToArray();
        }

        private static bool Match(RunspaceContext context, ScriptDocumentBlock block)
        {
            return context.Pipeline.Filter.Match(block.Name, block.Tag);
        }

        private static bool Match(RunspaceContext runspace, ScriptBlockDocumentConvention block, out int order)
        {
            order = int.MaxValue;
            for (var i = 0; runspace.Pipeline.Convention != null && i < runspace.Pipeline.Convention.Length; i++)
            {
                if (StringComparer.OrdinalIgnoreCase.Equals(runspace.Pipeline.Convention[i], block.Name) || StringComparer.OrdinalIgnoreCase.Equals(runspace.Pipeline.Convention[i], block.Id))
                {
                    order = i;
                    return true;
                }
            }
            return false;
        }
    }
}
