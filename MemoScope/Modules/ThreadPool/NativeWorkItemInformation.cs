using BrightIdeasSoftware;
using Microsoft.Diagnostics.Runtime;

namespace MemoScope.Modules.ThreadPool
{
    public class NativeWorkItemInformation
    {
        private readonly NativeWorkItem workItem;

        public NativeWorkItemInformation(NativeWorkItem workItem) => this.workItem = workItem;

        [OLVColumn]
        public WorkItemKind Kind => workItem.Kind;
        [OLVColumn]
        public ulong Data => workItem.Data;
    }
}