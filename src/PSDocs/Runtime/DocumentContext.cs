
using System.Collections;
using System.Collections.Specialized;

namespace PSDocs.Runtime
{
    internal sealed class DocumentContext
    {
        internal DocumentContext(RunspaceContext runspaceContext)
        {
            Culture = runspaceContext?.Culture;
            Metadata = new OrderedDictionary();
            Data = new Hashtable();
        }

        public string InstanceName { get; internal set; }

        public string Culture { get; }

        public string OutputPath { get; internal set; }

        public OrderedDictionary Metadata { get; }

        public Hashtable Data { get; }
    }
}
