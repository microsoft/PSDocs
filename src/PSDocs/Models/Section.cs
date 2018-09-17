using System.Collections.Generic;

namespace PSDocs.Models
{
    public sealed class Section : SectionNode
    {
        public override DocumentNodeType Type
        {
            get { return DocumentNodeType.Section; }
        }
    }
}
