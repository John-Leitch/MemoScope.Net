using BrightIdeasSoftware;
using MemoScope.Core;
using MemoScope.Core.Data;
using Microsoft.Diagnostics.Runtime;

namespace MemoScope.Modules.Stack
{

    public class StackInstanceInformation : IAddressData, ITypeNameData
    {
        private ClrDump ClrDump { get; }
        private ClrRoot ClrRoot { get; }
        public StackInstanceInformation(ClrDump clrDump, ClrRoot clrRoot)
        {
            ClrDump = clrDump;
            ClrRoot = clrRoot;
        }

        [OLVColumn]
        public ulong Address => ClrRoot.Object;

        [OLVColumn(Title = "Name")]
        public string TypeName => ClrRoot.Type?.Name;

        public ClrType Type => ClrRoot.Type;
    }
}