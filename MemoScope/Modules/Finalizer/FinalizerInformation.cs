using BrightIdeasSoftware;
using MemoScope.Core;
using MemoScope.Core.Data;
using Microsoft.Diagnostics.Runtime;
using System.Collections.Generic;
using WinFwk.UITools;

namespace MemoScope.Modules.Finalizer
{
    public class FinalizerInformation : ITypeNameData
    {
        public AddressList AddressList { get; }

        public FinalizerInformation(ClrDump clrDump, ClrType type, List<ulong> addresses) => AddressList = new AddressList(clrDump, type, addresses);

        [OLVColumn]
        public string TypeName => AddressList.ClrType.Name;

        [IntColumn]
        public int Count => AddressList.Addresses.Count;
    }
}