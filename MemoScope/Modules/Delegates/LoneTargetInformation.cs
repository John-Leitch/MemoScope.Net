using BrightIdeasSoftware;
using MemoScope.Core;
using MemoScope.Core.Data;
using Microsoft.Diagnostics.Runtime;
using ClrObject = MemoScope.Core.Data.ClrObject;

namespace MemoScope.Modules.Delegates
{
    public class LoneTargetInformation : IAddressData, ITypeNameData
    {
        private readonly ClrDump clrDump;
        private readonly ClrMethod methInfo;
        private readonly ClrObject target;
        private readonly ClrObject owner;
        public LoneTargetInformation(ClrDump clrDump, ClrObject target, ClrMethod methInfo, ClrObject owner)
        {
            this.clrDump = clrDump;
            this.target = target;
            this.methInfo = methInfo;
            this.owner = owner;
        }

        [OLVColumn]
        public ulong Address => target.Address;

        [OLVColumn]
        public string TypeName => target.Type.Name;

        [OLVColumn]
        public string Method => methInfo?.GetFullSignature();

        [OLVColumn]
        public ulong OwnerAddress => owner.Address;

        [OLVColumn]
        public string OwnerType => owner.Type?.Name;
    }
}