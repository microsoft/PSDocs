using System;
using System.Collections.Generic;
using System.Text;

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
    }
}
