using PSDocs.Configuration;
using PSDocs.Models;
using PSDocs.Processor.Markdown;
using PSDocs.Runtime;
using Xunit;

namespace PSDocs
{
    public sealed class MarkdownProcessorTests
    {
        [Fact]
        public void LineEndings()
        {
            var document = GetDocument();
            var actual = GetProcessor().Process(GetOption(), document).ToString();
            var expected = @"# Test document

## Section 1
";
            Assert.Equal(expected, actual);
        }

        private static Document GetDocument()
        {
            var result = new Document(new DocumentContext(null))
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
    }
}
