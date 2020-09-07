using BrightIdeasSoftware;
using MemoScope.Core;
using MemoScope.Core.Data;
using Microsoft.Diagnostics.Runtime;
using WinFwk.UITools;

namespace MemoScope.Modules.Delegates
{
    public class DelegateTypeInformation : ITypeNameData
    {
        private ClrDump ClrDump { get; }
        public ClrType ClrType { get; }

        public DelegateTypeInformation(ClrDump clrDump, ClrType clrType, int count, long targetCount)
        {
            ClrDump = clrDump;
            ClrType = clrType;
            Count = count;
            Targets = targetCount;
        }

        [OLVColumn(Title = "Type")]
        public string TypeName => ClrType.Name;

        [IntColumn(Title = "Count")]
        public int Count { get; }

        [IntColumn(Title = "Total Targets")]
        public long Targets { get; }
    }
}