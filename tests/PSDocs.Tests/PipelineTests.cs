// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.IO;
using System.Management.Automation;
using PSDocs.Configuration;
using PSDocs.Models;
using PSDocs.Pipeline;
using PSDocs.Runtime;

namespace PSDocs
{
    public sealed class PipelineTests
    {
        [Fact]
        public void GetDocumentBuilder()
        {
            var source = GetSource();
            var context = new RunspaceContext(new PipelineContext(GetOption(), null, null, null, null, null));
            HostHelper.ImportResource(source, context);
            var actual = HostHelper.GetDocumentBuilder(context, source);
            Assert.Equal(9, actual.Length);
        }

        [Fact]
        public void InvokePipeline()
        {
            var builder = PipelineBuilder.Invoke(GetSource(), GetOption(new string[] { "FromFileTest1" }), null, null);
            var pipeline = builder.Build() as InvokePipeline;
            var targetObject = PSObject.AsPSObject(new TestModel());

            var actual = pipeline.BuildDocument(new TargetObject(targetObject));
            Assert.Single(actual);
            Assert.Equal("Test title", actual[0].Title);
            Assert.Equal("Test1", actual[0].Metadata["test"]);
        }

        [Fact]
        public void InvokePipelineWithConvention()
        {
            var builder = PipelineBuilder.Invoke(GetSource(), GetOption(new string[] { "FromFileTest1" }), null, null);
            var pipeline = builder.Build() as InvokePipeline;
            var targetObject = PSObject.AsPSObject(new TestModel());

            var actual = pipeline.BuildDocument(new TargetObject(targetObject));
            Assert.Single(actual);
            Assert.Equal("Test title", actual[0].Title);
            Assert.Equal("Test1", actual[0].Metadata["test"]);
        }

        [Fact]
        public void InvokePipelineWithIf()
        {
            var builder = PipelineBuilder.Invoke(GetSource(), GetOption(new string[] { "WithIf" }), null, null);
            var pipeline = builder.Build() as InvokePipeline;
            var targetObject = PSObject.AsPSObject(new TestModel());

            var actual1 = pipeline.BuildDocument(new TargetObject(targetObject));
            Assert.Single(actual1);
            Assert.Equal("Test", actual1[0].Metadata["Name"]);

            targetObject.Properties["Generator"].Value = "NotPSDocs";
            var actual2 = pipeline.BuildDocument(new TargetObject(targetObject));
            Assert.Empty(actual2);
        }

        [Fact]
        public void InvokePipelineWithSelectors()
        {
            var builder = PipelineBuilder.Invoke(GetSourceWithSelectors(), GetOption(new string[] { "Selector.WithInputObject" }), null, null);
            var pipeline = builder.Build() as InvokePipeline;
            var targetObject = PSObject.AsPSObject(new TestModel());

            var actual1 = pipeline.BuildDocument(new TargetObject(targetObject));
            Assert.Single(actual1);
            Assert.Equal("Test", actual1[0].Metadata["Name"]);

            targetObject.Properties["Generator"].Value = "NotPSDocs";
            var actual2 = pipeline.BuildDocument(new TargetObject(targetObject));
            Assert.Empty(actual2);
        }

        private static OptionContext GetOption(string[] name = null)
        {
            var option = new PSDocumentOption();
            if (name != null && name.Length > 0)
                option.Document.Include = name;

            option.Output.Culture = new string[] { "en-US" };
            return new OptionContext(option);
        }

        private static Source[] GetSource()
        {
            var builder = new SourcePipelineBuilder(new HostContext(null, null));
            builder.Directory(GetSourcePath("FromFile.Doc.ps1"));
            builder.Directory(GetSourcePath("Selectors.Doc.yaml"));
            return builder.Build();
        }

        private static Source[] GetSourceWithSelectors()
        {
            var builder = new SourcePipelineBuilder(new HostContext(null, null));
            builder.Directory(GetSourcePath("FromFile.Selector.Doc.ps1"));
            builder.Directory(GetSourcePath("Selectors.Doc.yaml"));
            return builder.Build();
        }

        private static string GetSourcePath(string fileName)
        {
            return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, fileName);
        }
    }
}
