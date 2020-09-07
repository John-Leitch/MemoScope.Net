using BrightIdeasSoftware;
using MemoScope.Core;
using MemoScope.Core.Data;
using Microsoft.Diagnostics.Runtime;
using System.Collections.Generic;
using System.Linq;
using WinFwk.UITools;

namespace MemoScope.Modules.BlockingObjects
{
    public class BlockingObjectInformation : IAddressData, ITypeNameData
    {
        public ClrDump ClrDump { get; }
        public BlockingObject BlockingObject { get; }
        public HashSet<int> OwnersId { get; }
        public HashSet<int> WaitersId { get; }

        public BlockingObjectInformation(ClrDump clrDump, BlockingObject blockingObject)
        {
            ClrDump = clrDump;
            BlockingObject = blockingObject;

            OwnersId = blockingObject.HasSingleOwner && blockingObject.Owner != null
                ? new HashSet<int>
                {
                    blockingObject.Owner.ManagedThreadId
                }
                : new HashSet<int>(blockingObject.Owners.Where(thread => thread != null).Select(thread => thread.ManagedThreadId));
            WaitersId = blockingObject.Waiters != null
                ? new HashSet<int>(blockingObject.Waiters.Where(thread => thread != null).Select(thread => thread.ManagedThreadId))
                : new HashSet<int>();
        }

        [OLVColumn]
        public ulong Address => BlockingObject.Object;

        [OLVColumn]
        public string TypeName => ClrDump.GetObjectTypeName(BlockingObject.Object);

        [IntColumn(Width = 50)]
        public int Owners => BlockingObject.HasSingleOwner ? 1 : BlockingObject.Owners.Count;

        [BoolColumn(Width = 50)]
        public bool Taken => BlockingObject.Taken;

        [OLVColumn]
        public BlockingReason Reason => BlockingObject.Reason;

        [OLVColumn(IsVisible = false)]
        public int RecursionCount => BlockingObject.RecursionCount;
    }
}