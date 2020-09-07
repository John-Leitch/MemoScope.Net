using MemoScope.Core;
using MemoScope.Core.Helpers;
using Microsoft.Diagnostics.Runtime;
using System.Collections.Generic;
using System.Linq;

namespace MemoScope.Modules.StackTrace
{
    public partial class StackTraceModule : UIClrDumpModule
    {
        private List<StackFrameInformation> StackFrames;

        public ClrThread Thread { get; private set; }

        public StackTraceModule()
        {
            InitializeComponent();
            dlvStackFrames.SetRegexFilter(regexFilterControl1, o => ((StackFrameInformation)o).DisplayString);
        }

        public void Setup(ClrDump clrDump, ClrThread thread)
        {
            ClrDump = clrDump;
            Thread = thread;
            Icon = Properties.Resources.red_line_small;
            Name = $"#{clrDump.Id} - StackTrace - Id: {Thread?.ManagedThreadId}";

            dlvStackFrames.InitColumns<StackFrameInformation>();
        }

        public override void PostInit()
        {
            base.PostInit();
            Summary = $"Id: {Thread?.ManagedThreadId}";
            if (Thread != null && ClrDump.ThreadProperties.TryGetValue(Thread.ManagedThreadId, out ThreadProperty props))
            {
                Summary = $"Name: {props.Name}, " + Summary;
            }

            dlvStackFrames.Objects = StackFrames;
        }

        public override void Init() => StackFrames = ClrDump.Eval(() => Thread.StackTrace.Select(frame => new StackFrameInformation(ClrDump, frame))).ToList();
    }
}
