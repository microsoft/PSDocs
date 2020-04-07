
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
            var builder = PipelineBuilder.Invoke(GetSource(), GetOption(new string[] { "TableWithExpression" }));
            var pipeline = builder.Build() as InvokePipeline;
            var targetObject = PSObject.AsPSObject(new TestModel());

            var actual = pipeline.BuildDocument(targetObject);
            Assert.Single(actual);
            Assert.IsType<Table>(actual[0].Node[0]);

            var table = actual[0].Node[0] as Table;
            Assert.Equal("Dummy", table.Rows[0][0]);
            Assert.Equal("3", table.Rows[0][3]);
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
            builder.Directory(GetSourcePath("FromFile.Keyword.doc.ps1"));
            return builder.Build();
        }

        private static string GetSourcePath(string fileName)
        {
            return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, fileName);
        }
    }
}
