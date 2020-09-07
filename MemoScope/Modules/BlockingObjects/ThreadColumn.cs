using BrightIdeasSoftware;
using MemoScope.Core;
using System.Windows.Forms;

namespace MemoScope.Modules.BlockingObjects
{
    internal class ThreadColumn : OLVColumn
    {
        private readonly ThreadProperty thread;

        public ThreadColumn(ThreadProperty thread)
        {
            this.thread = thread;
            ImageGetter = GetData;
            Name = $"THREAD_{thread.ManagedId} - {thread.Name}";
            Text = $"{thread.ManagedId} - {thread.Name}";
            ToolTipText = Text;
            TextAlign = HorizontalAlignment.Center;
        }

        private object GetData(object rowObject) => !(rowObject is BlockingObjectInformation blockingObjectInfo)
                ? null
                : (object)(blockingObjectInfo.OwnersId.Contains(thread.ManagedId)
                ? Properties.Resources._lock_small
                : blockingObjectInfo.WaitersId.Contains(thread.ManagedId) ? Properties.Resources.hourglass : null);
    }
}