using BrightIdeasSoftware;
using MemoScope.Core;
using MemoScope.Core.Data;
using MemoScope.Core.Helpers;
using MemoScope.Modules.Arrays;
using MemoScope.Modules.InstanceDetails;
using System.Collections.Generic;
using System.Windows.Forms;
using WinFwk.UITools.Log;

namespace MemoScope.Modules.ArrayInstances
{
    public partial class ArrayInstancesModule : UIClrDumpModule
    {
        private ArraysAddressList ArrayAddressList { get; set; }
        private List<ArrayInstanceInformation> ArrayInstances { get; set; }

        public ArrayInstancesModule() => InitializeComponent();

        public void Setup(ArraysAddressList arrayAddressList)
        {
            ArrayAddressList = arrayAddressList;
            ClrDump = arrayAddressList.ClrDump;
            Icon = Properties.Resources.formatting_equal_to_small;
            Name = $"#{ArrayAddressList.ClrDump.Id} - Arrays: {ArrayAddressList.ClrType.Name}";

            dlvArrays.InitColumns<ArrayInstanceInformation>();
            dlvArrays.SetUpAddressColumn<ArrayInstanceInformation>(this);
            dlvArrays.CellClick += OnCellClick;
        }

        public override void Init() => ArrayInstances = ArrayInstanceAnalysis.Analyze(ArrayAddressList, MessageBus);

        private void OnCellClick(object sender, CellClickEventArgs e)
        {
            if (e.ClickCount == 2 && e.Model != null)
            {
                var arrayInstance = dlvArrays.SelectedObject<ArrayInstanceInformation>();
                if (arrayInstance != null)
                {
                    var address = arrayInstance.Address;
                    var type = ClrDump.GetObjectType(address);
                    if (type == null)
                    {
                        Log($"Can't find type for instance: {address:X}", LogLevelType.Error);
                        return;
                    }
                    var clrDumpObject = new ClrDumpObject(ClrDump, type, address);
                    InstanceDetailsCommand.Display(this, clrDumpObject);
                }
            }
        }

        public override void PostInit()
        {
            base.PostInit();
            Summary = $"{ArrayInstances.Count} Arrays";
            dlvArrays.Objects = ArrayInstances;
            dlvArrays.Sort(nameof(ArrayInstanceInformation.Length), SortOrder.Descending);
        }
    }
}
