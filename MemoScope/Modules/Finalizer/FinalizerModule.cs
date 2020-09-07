using MemoScope.Core;
using MemoScope.Core.Data;
using MemoScope.Core.Helpers;
using System.Collections.Generic;
using System.Linq;
using WinFwk.UICommands;

namespace MemoScope.Modules.Finalizer
{
    public partial class FinalizerModule : UIClrDumpModule, UIDataProvider<AddressList>, UIDataProvider<ClrDumpType>
    {
        private List<FinalizerInformation> FinalizerQueue;

        public FinalizerModule() => InitializeComponent();

        public void Setup(ClrDump clrDump)
        {
            ClrDump = clrDump;
            Icon = Properties.Resources.broom_small;
            Name = $"#{clrDump.Id} - Finalizer";

            dlvFinalizer.InitColumns<FinalizerInformation>();
            dlvFinalizer.SetUpTypeColumn<FinalizerInformation>(this);
            dlvFinalizer.RegisterDataProvider(() => ((UIDataProvider<AddressList>)this).Data, this, "Finalizer");
        }

        public override void Init()
        {
            base.Init();
            var groups = ClrDump.FinalizerQueueObjectAddressesByType;
            FinalizerQueue = groups.Select(g => new FinalizerInformation(ClrDump, g.Key, g.ToList())).ToList();
        }

        public override void PostInit()
        {
            base.PostInit();
            Summary = $"{FinalizerQueue.Count:###,###,###,##0} Finalizer";
            dlvFinalizer.Objects = FinalizerQueue;
        }

        AddressList UIDataProvider<AddressList>.Data
        {
            get
            {
                var finalizerInformation = dlvFinalizer.SelectedObject<FinalizerInformation>();
                return finalizerInformation?.AddressList;
            }
        }
        ClrDumpType UIDataProvider<ClrDumpType>.Data
        {
            get
            {
                var finalizerInformation = dlvFinalizer.SelectedObject<FinalizerInformation>();
                return finalizerInformation != null ? new ClrDumpType(ClrDump, finalizerInformation.AddressList.ClrType) : null;
            }
        }

    }
}
