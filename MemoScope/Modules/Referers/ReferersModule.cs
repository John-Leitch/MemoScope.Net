using MemoScope.Core;
using MemoScope.Core.Data;
using MemoScope.Core.Helpers;
using System.Linq;
using System.Windows.Forms;
using WinFwk.UICommands;

namespace MemoScope.Modules.Referers
{
    public partial class ReferersModule : UIClrDumpModule, UIDataProvider<AddressList>
    {
        private ReferersInformation Referers { get; set; }

        public ReferersModule() => InitializeComponent();

        public override void Init()
        {
            dtlvDistribution.InitData<ReferersInformation>();
            dtlvDistribution.SetUpTypeColumn<ReferersInformation>(this);
            dtlvDistribution.RegisterDataProvider(() => Data, this, "Instances");
            dtlvDistribution.RegisterDataProvider(GetReferences, this, "References");
        }

        internal void Setup(AddressList addressList)
        {
            ClrDump = addressList.ClrDump;
            Icon = Properties.Resources.chart_organisation_small;
            Referers = new ReferersInformation(ClrDump, addressList.ClrType, MessageBus, addressList.Addresses);

            Name = $"#{ClrDump.Id} - Referers - {Referers.TypeName}";
        }

        public override void PostInit()
        {
            Summary = $"{Referers.Instances.Count:###,###,###,##0} instances";
            dtlvDistribution.Roots = new object[] { Referers };
            dtlvDistribution.Sort(dtlvDistribution[nameof(ReferersInformation.ReferencesCount)], SortOrder.Descending);
        }

        public AddressList Data
        {
            get
            {
                var refInfo = dtlvDistribution.SelectedObject<ReferersInformation>();
                return refInfo == null ? null : new AddressList(ClrDump, refInfo.ClrType, refInfo.Instances.ToList());
            }
        }
        public AddressList GetReferences()
        {
            var refInfo = dtlvDistribution.SelectedObject<ReferersInformation>();
            return refInfo == null ? null : new AddressList(ClrDump, Referers.ClrType, refInfo.References.ToList());
        }
    }
}
