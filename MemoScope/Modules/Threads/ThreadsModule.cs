using BrightIdeasSoftware;
using MemoScope.Core;
using MemoScope.Core.Data;
using MemoScope.Core.Helpers;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using WinFwk.UICommands;

namespace MemoScope.Modules.Threads
{
    public partial class ThreadsModule : UIClrDumpModule, UIDataProvider<ClrDumpThread>
    {
        private List<ThreadInformation> Threads;

        public ThreadsModule() => InitializeComponent();

        public void Setup(ClrDump clrDump)
        {
            ClrDump = clrDump;
            Icon = Properties.Resources.processor_small;
            Name = $"#{clrDump.Id} - Threads";

            dlvThreads.InitColumns<ThreadInformation>();
            dlvThreads.SetUpAddressColumn<ThreadInformation>(this);
            dlvThreads.RegisterDataProvider(() => Data, this);
            dlvThreads.SelectedIndexChanged += (s, e) =>
            {
                var threadInfo = dlvThreads.SelectedObject<ThreadInformation>();
                if (threadInfo == null)
                {
                    return;
                }
                stackTraceModule.Setup(ClrDump, threadInfo.Thread);
                stackTraceModule.Init();
                stackTraceModule.PostInit();

                stackModule.Setup(ClrDump, threadInfo.Thread);
                stackModule.Init();
                stackModule.PostInit();
            };

            dlvThreads.FormatCell += (o, e) =>
            {
                if (!(e.Model is ThreadInformation threadInfo))
                {
                    return;
                }
                if (threadInfo.CurrentException != null)
                {
                    e.SubItem.BackColor = Color.Orange;
                }
            };

            stackTraceModule.InitBus(MessageBus);
            stackModule.InitBus(MessageBus);

            stackModule.UIModuleParent = this;
            stackTraceModule.UIModuleParent = this;

            stackTraceModule.Setup(clrDump, null);
            stackModule.Setup(clrDump, null, this);
        }

        public override void Init()
        {
            base.Init();

            Threads = ClrDump.Threads.Select(thread =>
            {
                var threadInfo = new ThreadInformation(ClrDump, thread);
                if (ClrDump.ThreadProperties.TryGetValue(thread.ManagedThreadId, out ThreadProperty threadProp))
                {
                    threadInfo.Address = threadProp.Address;
                    threadInfo.Name = threadProp.Name;
                    threadInfo.Priority = threadProp.Priority;
                }
                return threadInfo;
            }).ToList();
        }
        public ClrDumpThread Data
        {
            get
            {
                var thread = dlvThreads.SelectedObject<ThreadInformation>();
                return thread == null ? null : new ClrDumpThread(thread.ClrDump, thread.Thread, thread.Name);
            }
        }

        public override void PostInit()
        {
            base.PostInit();
            Summary = $"{Threads.Count} Threads";
            dlvThreads.Objects = Threads;
        }

        public override IEnumerable<ObjectListView> ListViews
        {
            get
            {
                yield return dlvThreads;
                foreach (var lv in stackModule.ListViews)
                {
                    yield return lv;
                }
                foreach (var lv in stackTraceModule.ListViews)
                {
                    yield return lv;
                }
            }
        }
    }
}
