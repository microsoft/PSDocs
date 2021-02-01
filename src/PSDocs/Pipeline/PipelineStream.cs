
using System.Collections.Concurrent;
using System.Management.Automation;

namespace PSDocs.Pipeline
{
    internal sealed class PipelineStream
    {
        private readonly ConcurrentQueue<PSObject> _Queue;

        public PipelineStream()
        {
            _Queue = new ConcurrentQueue<PSObject>();
        }

        public int Count
        {
            get { return _Queue.Count; }
        }

        public bool IsEmpty
        {
            get { return _Queue.IsEmpty; }
        }

        public void Enqueue(PSObject sourceObject)
        {
            _Queue.Enqueue(sourceObject);
        }

        public bool TryDequeue(out PSObject sourceObject)
        {
            return _Queue.TryDequeue(out sourceObject);
        }
    }
}
