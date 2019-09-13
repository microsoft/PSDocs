using BenchmarkDotNet.Attributes;
using PSDocs.Configuration;
using PSDocs.Models;
using PSDocs.Processor.Markdown;
using System;
using System.IO;
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
        private Action<Document> _InvokeMarkdownProcessor;

        [GlobalSetup]
        public void Prepare()
        {
            PrepareMarkdownProcessor();
            PrepareDocument();
        }

        private void PrepareMarkdownProcessor()
        {
            var option = GetOption();
            var processor = GetProcessor();
            _InvokeMarkdownProcessor = (document) => processor.Process(option, document);
        }

        //private string GetSourcePath(string fileName)
        //{
        //    return Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), fileName);
        //}

        private void PrepareDocument()
        {
            _Document = new Document[]
            {
                GetDocument()
            };
        }

        private static Document GetDocument()
        {
            var result = new Document
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

        private static MarkdownProcessor GetProcessor()
        {
            return new MarkdownProcessor();
        }

        private static PSDocumentOption GetOption()
        {
            return new PSDocumentOption();
        }

        [Benchmark]
        public void InvokeMarkdownProcessor() => _InvokeMarkdownProcessor(_Document[0]);
    }
}
