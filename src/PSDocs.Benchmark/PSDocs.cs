// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using BenchmarkDotNet.Attributes;
using PSDocs.Configuration;
using PSDocs.Models;
using PSDocs.Pipeline;
using PSDocs.Processor.Markdown;
using PSDocs.Runtime;
using System;
using System.IO;
using System.Management.Automation;
using System.Reflection;

namespace PSDocs.Benchmark
{
    /// <summary>
    /// Define a set of benchmarks for performance testing PSDocs internals.
    /// </summary>
    [MemoryDiagnoser]
    [MarkdownExporterAttribute.GitHub]
    public class PSDocs
    {
        private Document[] _Document;
        private PSObject _SourceObject;
        private Action<Document> _InvokeMarkdownProcessor;
        private Action<PSObject> _InvokePipeline;

        [GlobalSetup]
        public void Prepare()
        {
            PrepareMarkdownProcessor();
            PrepareInvokePipeline();
            PrepareDocument();
            PrepareSourceObject();
        }

        private void PrepareMarkdownProcessor()
        {
            var option = GetOption();
            var processor = GetProcessor();
            _InvokeMarkdownProcessor = (document) => processor.Process(option, document);
        }

        private void PrepareInvokePipeline()
        {
            var option = GetOption();
            var builder = PipelineBuilder.Invoke(GetSource(), option, null, null);
            var pipeline = builder.Build();
            _InvokePipeline = pipeline.Process;
        }

        private void PrepareDocument()
        {
            _Document = new Document[]
            {
                GetDocument()
            };
        }

        private static Document GetDocument()
        {
            var context = new DocumentContext(null)
            {
                InstanceName = "test-benchmark"
            };
            var result = new Document(context)
            {
                Title = "Test document"
            };
            var section = new Section
            {
                Title = "Section 1",
                Level = 2
            };
            result.Node.Add(section);
            return result;
        }

        private void PrepareSourceObject()
        {
            _SourceObject = PSObject.AsPSObject("Test");
        }

        private static MarkdownProcessor GetProcessor()
        {
            return new MarkdownProcessor();
        }

        private static PSDocumentOption GetOption()
        {
            return new PSDocumentOption();
        }

        private static Source[] GetSource()
        {
            return new Source[] {
                new Source(GetSourcePath(), new SourceFile[] { new SourceFile(GetSourcePath("Benchmark.Doc.ps1"), null, SourceType.Script, null) })
            };
        }

        private static string GetSourcePath()
        {
            return Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
        }

        private static string GetSourcePath(string fileName)
        {
            return Path.Combine(GetSourcePath(), fileName);
        }

        [Benchmark]
        public void InvokeMarkdownProcessor()
        {
            _InvokeMarkdownProcessor(_Document[0]);
        }

        [Benchmark]
        public void InvokePipeline()
        {
            _InvokePipeline(_SourceObject);
        }
    }
}
