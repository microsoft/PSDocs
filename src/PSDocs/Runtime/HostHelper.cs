using PSDocs.Data;
using PSDocs.Data.Internal;
using PSDocs.Pipeline;
using PSDocs.Resources;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;

namespace PSDocs.Runtime
{
    internal static class HostHelper
    {
        public static IDocumentBuilder[] GetDocumentBuilder(RunspaceContext runspace, Source[] source)
        {
            return ToDocumentBuilder(GetLanguageBlock(runspace, source), runspace);
        }

        /// <summary>
        /// Execute one or more PowerShell script files to get language blocks.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="sources"></param>
        /// <returns></returns>
        private static ILanguageBlock[] GetLanguageBlock(RunspaceContext context, Source[] sources)
        {
            var results = new Collection<ILanguageBlock>();
            var ps = context.NewPowerShell();

            try
            {
                //context.EnterScope("[Discovery.Rule]");

                // Process scripts

                foreach (var source in sources)
                {
                    foreach (var file in source.File)
                    {
                        ps.Commands.Clear();

                        if (!context.TrySourceFile(file))
                            throw new FileNotFoundException(PSDocsResources.ScriptNotFound, file.Path);

                        //context.
                        //PipelineContext.CurrentThread.Source = source;
                        //PipelineContext.CurrentThread.VerboseRuleDiscovery(path: source.Path);
                        //PipelineContext.CurrentThread.UseSource(source: source);

                        try
                        {
                            // Invoke script
                            ps.AddScript(string.Concat("& '", file.Path, "'"), true);
                            var invokeResults = ps.Invoke();

                            if (ps.HadErrors)
                            {
                                // Discovery has errors so skip this file
                                continue;
                            }

                            foreach (var ir in invokeResults)
                            {
                                if (ir.BaseObject is ScriptDocumentBlock block)
                                {
                                    results.Add(block);
                                }
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
                //context.ExitScope();
                context.ExitSourceFile();
                //PipelineContext.CurrentThread.Source = null;
                ps.Runspace = null;
                ps.Dispose();
            }
            return results.ToArray();
        }

        /// <summary>
        /// Convert language blocks to a document builder.
        /// </summary>
        private static IDocumentBuilder[] ToDocumentBuilder(ILanguageBlock[] blocks, RunspaceContext context)
        {
            // Index rules by RuleId
            var results = new Dictionary<string, IDocumentBuilder>(StringComparer.OrdinalIgnoreCase);
            try
            {
                foreach (var block in blocks.OfType<ScriptDocumentBlock>())
                {
                    // Ignore rule blocks that don't match
                    if (!Match(context, block))
                    {
                        continue;
                    }
                    if (!results.ContainsKey(block.Id))
                    {
                        //results[block.RuleId] = block.Info;
                        results[block.Id] = new ScriptDocumentBuilder(block);
                    }
                }
            }
            finally
            {
                context.ExitSourceFile();
            }
            return results.Values.ToArray();
        }

        private static bool Match(RunspaceContext context, ScriptDocumentBlock block)
        {
            return context.Pipeline.Filter.Match(block.Name, block.Tag);
        }
    }
}
