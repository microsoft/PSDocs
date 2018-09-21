using PSDocs.Configuration;
using System.Collections.Generic;

namespace PSDocs.Models
{
    public delegate object WriteDocumentDelegate(PSDocumentOption option, Document document);

    public sealed class PSDocsContext
    {
        public PSDocumentOption Option { get; private set; }

        public DocumentFilter Filter { get; private set; }

        public string OutputPath { get; set; }

        public string[] InstanceName { get; set; }

        public WriteDocumentDelegate WriteDocumentHook { get; set; }

        public static PSDocsContext Create(PSDocumentOption option, string[] name, string[] tag)
        {
            return new PSDocsContext
            {
                Option = option,
                Filter = DocumentFilter.Create(name, tag)
            };
        }

        public object WriteDocument(Document document)
        {
            return WriteDocumentHook?.Invoke(Option, document);
        }
    }
}
