using MemoScope.Core;
using MemoScope.Core.Data;
using MemoScope.Core.Helpers;
using MemoScope.Modules.Instances;
using System.Collections.Generic;
using System.Windows.Forms;
using WinFwk.UICommands;

namespace MemoScope.Modules.Disposables
{
    public partial class DisposableTypesModule : UIClrDumpModule, UIDataProvider<ClrDumpType>
    {
        private List<DisposableTypeInformation> disposableInformations;

        public DisposableTypesModule() => InitializeComponent();

        public void Setup(ClrDump clrDump)
        {
            ClrDump = clrDump;
            Icon = Properties.Resources.macro_show_all_actions_small;
            Name = $"#{clrDump.Id} - Disposable Types";

            dlvDisposableTypes.InitColumns<DisposableTypeInformation>();
            dlvDisposableTypes.SetUpTypeColumn<DisposableTypeInformation>(this);
            dlvDisposableTypes.SetTypeNameFilter<DisposableTypeInformation>(regexFilterControl);
        }

        public override void Init() => disposableInformations = DisposableAnalysis.GetDisposableTypeInformations(ClrDump);

        public override void PostInit()
        {
            Summary = $"{disposableInformations.Count:###,###,###,##0} Disposable Types";
            dlvDisposableTypes.Objects = disposableInformations;
            dlvDisposableTypes.Sort(nameof(DisposableTypeInformation.Count), SortOrder.Descending);
        }

        public ClrDumpType Data
        {
            get
            {
                var delegateInformation = dlvDisposableTypes.SelectedObject<DisposableTypeInformation>();
                return delegateInformation != null ? new ClrDumpType(ClrDump, delegateInformation.ClrType) : null;
            }
        }

        private void dlvDelegates_CellClick(object sender, BrightIdeasSoftware.CellClickEventArgs e)
        {
            if (e.ClickCount != 2)
            {
                return;
            }

            var selectedDelegateType = Data;
            if (selectedDelegateType == null)
            {
                return;
            }

            TypeInstancesModule.Create(selectedDelegateType, this, mod => RequestDockModule(mod));
        }
    }
}
