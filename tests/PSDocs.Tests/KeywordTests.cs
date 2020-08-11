
using PSDocs.Configuration;
using PSDocs.Models;
using PSDocs.Pipeline;
using System;
using System.IO;
using System.Management.Automation;
using Xunit;

namespace PSDocs
{
    public sealed class KeywordTests
    {
        [Fact]
        public void TableTests()
        {
            var actual = BuildDocument("TableWithExpression");
            Assert.Single(actual);
            Assert.IsType<Table>(actual[0].Node[0]);
            var table = actual[0].Node[0] as Table;
            Assert.Equal("Dummy", table.Rows[0][0]);
            Assert.Equal("3", table.Rows[0][3]);

            actual = BuildDocument("TableSingleEntryMarkdown");
            Assert.Single(actual);
            Assert.IsType<Table>(actual[0].Node[0]);

            actual = BuildDocument("TableWithMultilineColumn");
            Assert.Single(actual);
            Assert.IsType<Table>(actual[0].Node[0]);
            Assert.True((actual[0].Node[0] as Table).Rows[0].Length > 1);

            actual = BuildDocument("TableWithEmptyColumn");
            Assert.Single(actual);
            Assert.IsType<Table>(actual[0].Node[1]);
        }

        private static Document[] BuildDocument(string documentName)
        {
            var builder = PipelineBuilder.Invoke(GetSource(), GetOption(new string[] { documentName }));
            var pipeline = builder.Build() as InvokePipeline;
            var targetObject = PSObject.AsPSObject(new TestModel());
            return pipeline.BuildDocument(targetObject);
        }

        private static PSDocumentOption GetOption(string[] name = null)
        {
            var option = new PSDocumentOption();
            if (name != null && name.Length > 0)
            {
                option.Document.Include = name;
            }
            return option;
        }

        private static Source[] GetSource()
        {
            var builder = new SourceBuilder();
            builder.Directory(GetSourcePath("FromFile.Keyword.Doc.ps1"));
            return builder.Build();
        }

        private static string GetSourcePath(string fileName)
        {
            return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, fileName);
        }
    }
}
