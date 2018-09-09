using PSDocs.Configuration;
using System.Collections.Generic;

namespace PSDocs.Models
{
    public delegate void WriteDocumentDelegate(PSDocumentOption option, Document document);

    public sealed class PSDocsContext
    {
        public PSDocumentOption Option { get; set; }

        public DocumentFilter Filter { get; set; }

        public string OutputPath { get; set; }

        public WriteDocumentDelegate WriteDocumentHook { get; set; }

        public static PSDocsContext Create(PSDocumentOption option, string[] name, string[] tag)
        {
            return new PSDocsContext
            {
                Option = option,
                Filter = DocumentFilter.Create(name, tag)
            };
        }

        public void WriteDocument(Document document)
        {
            WriteDocumentHook?.Invoke(Option, document);
        }
    }
}
