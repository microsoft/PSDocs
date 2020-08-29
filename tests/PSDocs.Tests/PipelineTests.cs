
using PSDocs.Configuration;
using PSDocs.Models;
using PSDocs.Pipeline;
using PSDocs.Runtime;
using System;
using System.IO;
using System.Management.Automation;
using Xunit;

namespace PSDocs
{
    public sealed class PipelineTests
    {
        [Fact]
        public void GetDocumentBuilder()
        {
            var actual = HostHelper.GetDocumentBuilder(new RunspaceContext(new PipelineContext(GetOption(), null, null, null)), GetSource());
            Assert.Equal(7, actual.Length);
        }

        [Fact]
        public void InvokePipeline()
        {
            var builder = PipelineBuilder.Invoke(GetSource(), GetOption(new string[] { "FromFileTest1" }), null, null);
            var pipeline = builder.Build() as InvokePipeline;
            var targetObject = PSObject.AsPSObject(new TestModel());

            var actual = pipeline.BuildDocument(targetObject);
            Assert.Single(actual);
            Assert.Equal("Test title", actual[0].Title);
            Assert.Equal("Test1", actual[0].Metadata["test"]);
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
            var builder = new SourcePipelineBuilder(new HostContext(null, null));
            builder.Directory(GetSourcePath("FromFile.Doc.ps1"));
            return builder.Build();
        }

        private static string GetSourcePath(string fileName)
        {
            return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, fileName);
        }

        private static TestCommandRuntime GetCommandRuntime()
        {
            return new TestCommandRuntime();
        }
    }
}
