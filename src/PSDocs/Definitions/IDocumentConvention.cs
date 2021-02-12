
using PSDocs.Runtime;
using System.Collections;

namespace PSDocs.Definitions
{
    internal interface IDocumentConvention
    {
        string Name { get; }

        void Begin(RunspaceContext context, IEnumerable input);

        void Process(RunspaceContext context, IEnumerable input);

        void End(RunspaceContext context, IEnumerable input);
    }
}
