// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using PSDocs.Configuration;
using PSDocs.Pipeline;
using PSDocs.Runtime;
using System.Management.Automation;
using Xunit;

namespace PSDocs
{
    public sealed class LanguageVisitorTests
    {
        [Fact]
        public void NestedDefintion()
        {
            var content = @"
# Header comment
Document 'Doc1' {

}
";
            var scriptAst = ScriptBlock.Create(content).Ast;
            var visitor = new LanguageAst(GetContext());
            scriptAst.Visit(visitor);

            Assert.Null(visitor.Errors);

            content = @"
# Header comment
Document 'Doc1' {
    Document 'Doc2' {

    }
}
";
            scriptAst = ScriptBlock.Create(content).Ast;
            visitor = new LanguageAst(GetContext());
            scriptAst.Visit(visitor);

            Assert.Single(visitor.Errors);
        }

        [Fact]
        public void UnvalidDefinition()
        {
            var content = @"
Document '' {

}

Document {

}

Document 'Doc1';

Document '' {

}

Document 'Doc2' {

}

Document -Name 'Doc3' {

}

Document -Name 'Doc3' -Body {

}

";

            var scriptAst = ScriptBlock.Create(content).Ast;
            var visitor = new LanguageAst(GetContext());
            scriptAst.Visit(visitor);

            Assert.NotNull(visitor.Errors);
            Assert.Equal(4, visitor.Errors.Count);
        }

        private static PipelineContext GetContext()
        {
            return new PipelineContext(GetOption(), null, null, null, null, null);
        }

        private static OptionContext GetOption(string[] name = null)
        {
            var option = new PSDocumentOption();
            return new OptionContext(option);
        }
    }
}
