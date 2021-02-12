
namespace PSDocs.Processor
{
    internal interface IDocumentResult
    {
        string InstanceName { get; }

        string Extension { get; }

        string Culture { get; }

        string OutputPath { get; }

        string FullName { get; }
    }
}
