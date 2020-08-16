using PSDocs.Configuration;
using System.IO;

namespace PSDocs.Models
{
    public delegate object WriteDocumentDelegate(PSDocumentOption option, Document document);

    public sealed class PSDocsContext
    {
        public PSDocumentOption Option { get; private set; }

        public DocumentFilter Filter { get; private set; }

        public string OutputPath { get; set; }

        public string[] InstanceName { get; set; }

        public string[] Culture { get; set; }

        public WriteDocumentDelegate WriteDocumentHook { get; set; }

        public static PSDocsContext Create(PSDocumentOption option, string[] name, string[] tag, string outputPath)
        {
            var actualPath = outputPath;

            if (!Path.IsPathRooted(actualPath))
            {
                actualPath = Path.Combine(PSDocumentOption.GetWorkingPath(), outputPath);
            }

            return new PSDocsContext
            {
                Option = option,
                Filter = DocumentFilter.Create(name, tag),
                OutputPath = actualPath
            };
        }

        public object WriteDocument(Document document)
        {
            return WriteDocumentHook?.Invoke(Option, document);
        }
    }
}
