using Microsoft.Diagnostics.Runtime;
using System.Collections.Generic;

namespace MemoScope.Modules.TypeDetails
{
    public class DummyTypeInformation : AbstractTypeInformation
    {
        private readonly ClrInterface interf;

        public DummyTypeInformation(ClrInterface interf)
        {
            this.interf = interf;
            TypeName = interf.Name;
        }

        public override bool CanExpand => false;
        public override List<AbstractTypeInformation> Children => null;
    }
}