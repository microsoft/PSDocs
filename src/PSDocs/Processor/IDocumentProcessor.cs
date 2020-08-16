using PSDocs.Configuration;
using PSDocs.Models;

namespace PSDocs.Processor
{
    public interface IDocumentProcessor
    {
        void Process(PSDocumentOption option, Document document);
    }
}
