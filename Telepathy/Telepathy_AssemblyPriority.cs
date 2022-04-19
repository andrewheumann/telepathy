using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Grasshopper.Kernel;

namespace Telepathy
{
    public class Telepathy_AssemblyPriority : GH_AssemblyPriority
    {
        public override GH_LoadingInstruction PriorityLoad()
        {
            Grasshopper.Instances.ComponentServer.AddAlias("send", new Guid("{ADA99447-8A42-4C8E-BAA4-C8EF36A372B6}"));
            Grasshopper.Instances.ComponentServer.AddAlias("rec", new Guid("{08CDCD26-518A-4FE2-8313-A2DB5DCDF800}"));
            return GH_LoadingInstruction.Proceed;
        }
    }
}
