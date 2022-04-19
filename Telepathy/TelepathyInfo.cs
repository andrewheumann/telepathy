using System;
using System.Drawing;
using Grasshopper.Kernel;

namespace Telepathy
{
    public class TelepathyInfo : GH_AssemblyInfo
    {
        public override string Name => "Telepathy";

        public override Bitmap Icon => Properties.Resources.icon24;
        public override string Description => "Facilitates automatic wireless connection between special parameters.";

        public override Guid Id => new Guid("e03c3629-dd21-4a47-a710-b561b8803e8c");

        public override string AuthorName => "Andrew Heumann";
        public override string AuthorContact => "andheum@gmail.com";
    }
}
