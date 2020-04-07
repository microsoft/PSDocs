using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Management.Automation;

namespace PSDocs.Models
{
    public abstract class SectionNode : DocumentNode
    {
        public SectionNode()
        {
            Title = string.Empty;
            Node = new List<DocumentNode>();
            Level = 1;
        }

        public string Title { get; set; }

        public int Level { get; set; }

        public List<DocumentNode> Node { get; set; }

        internal bool AddNodes(Collection<PSObject> collection)
        {
            if (collection == null || collection.Count == 0)
                return false;

            var items = new PSObject[collection.Count];
            collection.CopyTo(items, 0);

            int count = 0;
            for (var i = 0; i < items.Length; i++)
            {
                if (items[i] == null || items[i].BaseObject == null)
                    continue;

                if (!(items[i].BaseObject is DocumentNode node))
                {
                    node = new Text { Content = items[i].BaseObject.ToString() };
                }
                count++;
                Node.Add(node);
            }
            return count > 0;
        }
    }
}
