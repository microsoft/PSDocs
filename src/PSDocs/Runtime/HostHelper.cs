// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using PSDocs.Annotations;
using PSDocs.Data;
using PSDocs.Data.Internal;
using PSDocs.Definitions;
using PSDocs.Definitions.Conventions;
using PSDocs.Definitions.Selectors;
using PSDocs.Pipeline;
using PSDocs.Resources;

namespace PSDocs.Runtime
{
    internal static class HostHelper
    {
        /// <summary>
        /// Executes get document builders from Document script blocks.
        /// </summary>
        internal static IDocumentBuilder[] GetDocumentBuilder(RunspaceContext context, Source[] source)
        {
            context.PushScope(RunspaceScope.Source);
            var blocks = GetLanguageBlock(context, source);
            var documents = ToDocument(blocks, context);
            var conventions = GetConventions(blocks, context);
            return ToDocumentBuilder(documents, conventions);
        }

        internal static IDocumentDefinition[] GetDocumentBlock(RunspaceContext context, Source[] source)
        {
            return ToDocument(GetLanguageBlock(context, source), context);
        }

        internal static void ImportResource(Source[] source, RunspaceContext context)
        {
            Import(ReadYamlObjects(source, context), context);
        }

        /// <summary>
        /// Read YAML objects and return selectors.
        /// </summary>
        internal static IEnumerable<SelectorV1> GetSelector(RunspaceContext context, Source[] source)
        {
            return ToSelectorV1(ReadYamlObjects(source, context), context);
        }

        /// <summary>
        /// Called from PowerShell to get additional metdata from a language block, such as comment help.
        /// </summary>
        /// <param name="path"></param>
        /// <param name="start"></param>
        /// <returns></returns>
        internal static CommentMetadata GetCommentMeta(string path, int lineNumber, int offset)
        {
            var context = RunspaceContext.CurrentThread;
            //if (lineNumber < 0 || context.Pipeline.ExecutionScope == ExecutionScope.None || context.Source.SourceContentCache == null)
            //    return new CommentMetadata();

            var lines = context.Source.Content;
            var i = lineNumber;
            var comments = new List<string>();

            // Back track lines with comments immediately before block
            for (; i >= 0 && lines[i].Contains("#"); i--)
                comments.Insert(0, lines[i]);

            // Check if any comments were found
            var metadata = new CommentMetadata();
            if (comments.Count > 0)
            {
                foreach (var comment in comments)
                {
                    if (comment.StartsWith("# Synopsis: ", StringComparison.OrdinalIgnoreCase))
                        metadata.Synopsis = comment.Substring(12);
                }
            }
            return metadata;
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
                        if (file.Type != SourceType.Script)
                            continue;

                        ps.Commands.Clear();
                        if (!context.EnterSourceFile(file))
                            throw new FileNotFoundException(PSDocsResources.ScriptNotFound, file.Path);

                        var scriptAst = System.Management.Automation.Language.Parser.ParseFile(file.Path, out var tokens, out var errors);
                        var visitor = new LanguageAst(context.Pipeline);
                        scriptAst.Visit(visitor);

                        if (visitor.Errors != null && visitor.Errors.Count > 0)
                        {
                            foreach (var record in visitor.Errors)
                                context.Pipeline.Writer?.WriteError(record);

                            continue;
                        }
                        if (errors != null && errors.Length > 0)
                        {
                            foreach (var error in errors)
                                context.Pipeline.Writer?.WriteError(error);

                            continue;
                        }

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

        private static IEnumerable<ILanguageBlock> ReadYamlObjects(Source[] sources, RunspaceContext context)
        {
            var builder = new ResourceBuilder();
            try
            {
                foreach (var source in sources)
                {
                    foreach (var file in source.File)
                    {
                        if (file.Type != SourceType.Yaml)
                            continue;

                        context.EnterSourceFile(file);
                        builder.FromFile(file);
                    }
                }
            }
            finally
            {
                context.ExitSourceFile();
            }
            return builder.Build();
        }

        private static void Import(IEnumerable<ILanguageBlock> blocks, RunspaceContext context)
        {
            foreach (var resource in blocks.OfType<IResource>().ToArray())
                context.Pipeline.Import(resource);
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

        private static SelectorV1[] ToSelectorV1(IEnumerable<ILanguageBlock> blocks, RunspaceContext context)
        {
            if (blocks == null)
                return Array.Empty<SelectorV1>();

            // Index selectors by Id
            var results = new Dictionary<string, SelectorV1>(StringComparer.OrdinalIgnoreCase);
            try
            {
                foreach (var block in blocks.OfType<SelectorV1>().ToArray())
                {
                    // Ignore selectors that don't match
                    if (!Match(context, block))
                        continue;

                    if (!results.ContainsKey(block.Id))
                        results[block.Id] = block;
                }
            }
            finally
            {
                context.ExitSourceFile();
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
                    if (!Match(runspace, block, out var order))
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

        private static bool Match(RunspaceContext context, SelectorV1 resource)
        {
            return true;
        }
    }
}
