using PSDocs.Data;
using PSDocs.Data.Internal;
using PSDocs.Definitions;
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
            return ToDocumentBuilder(ToDocumentBlock(GetLanguageBlock(runspace, source), runspace));
        }

        internal static IDocumentDefinition[] GetDocumentBlock(RunspaceContext runspace, Source[] source)
        {
            return ToDocumentBlock(GetLanguageBlock(runspace, source), runspace);
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
                                if (ir.BaseObject is ScriptDocumentBlock block)
                                    results.Add(block);
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
                ps.Runspace = null;
                ps.Dispose();
            }
            return results.ToArray();
        }

        /// <summary>
        /// Convert document blocks to document builders.
        /// </summary>
        private static IDocumentBuilder[] ToDocumentBuilder(ScriptDocumentBlock[] blocks)
        {
            var result = new ScriptDocumentBuilder[blocks.Length];
            for (var i = 0; i < blocks.Length; i++)
                result[i] = new ScriptDocumentBuilder(blocks[i]);

            return result;
        }

        /// <summary>
        /// Convert language blocks to document blocks.
        /// </summary>
        private static ScriptDocumentBlock[] ToDocumentBlock(ILanguageBlock[] blocks, RunspaceContext context)
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

        private static bool Match(RunspaceContext context, ScriptDocumentBlock block)
        {
            return context.Pipeline.Filter.Match(block.Name, block.Tag);
        }
    }
}
