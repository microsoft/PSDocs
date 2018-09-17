using PSDocs.Configuration;
using PSDocs.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PSDocs.Processor
{
    public interface IDocumentProcessor
    {
        void Process(PSDocumentOption option, Document document);
    }
}
