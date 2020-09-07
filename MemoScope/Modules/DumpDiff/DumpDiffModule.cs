using BrightIdeasSoftware;
using MemoScope.Core;
using MemoScope.Core.Data;
using MemoScope.Core.Helpers;
using MemoScope.Modules.Instances;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using WinFwk.UIModules;

namespace MemoScope.Modules.DumpDiff
{

    public partial class DumpDiffModule : UIModule
    {
        public enum SortMode { Value, AbsValue }
        private List<ClrDump> ClrDumps { get; set; }

        private readonly HashSet<string> typeNames = new HashSet<string>();

        public DumpDiffModule()
        {
            InitializeComponent();
            cbSortMode.Items.Add(SortMode.Value);
            cbSortMode.Items.Add(SortMode.AbsValue);
            cbSortMode.SelectedItem = SortMode.AbsValue;
        }

        public void Setup(List<ClrDump> clrDumps)
        {
            ClrDumps = clrDumps;
            Icon = Properties.Resources.subtotal_small;
            Name = "Dump diff";
            dlvDumpDiff.SetUpTypeColumn(colType);
            colType.Text = "Type";
            colType.AspectGetter = o => (string)o;
            ClrDump prevClrDump = null;
            foreach (var clrDump in ClrDumps.OrderBy(dump => dump.Id))
            {
                var stats = clrDump.GetTypeStats();
                DiffColumn diffCol = new DiffColumn(clrDump, stats, prevClrDump?.GetTypeStats());
                dlvDumpDiff.AllColumns.Add(diffCol);
                prevClrDump = clrDump;
                dlvDumpDiff.RegisterDataProvider(() => SelectedTypeInstancesAddressList(clrDump), this, $"#{clrDump.Id}");
            }
            dlvDumpDiff.RebuildColumns();
            dlvDumpDiff.UseCellFormatEvents = true;
            dlvDumpDiff.FormatCell += OnFormatCell;
            dlvDumpDiff.CellClick += OnCellClick;
            dlvDumpDiff.CustomSorter = DumpDiffSort;

            dlvDumpDiff.SetRegexFilter(regexFilterControl, o => (string)o);
        }

        private void DumpDiffSort(OLVColumn column, SortOrder sortOrder) => dlvDumpDiff.ListViewItemSorter = new DumpDiffComparer(column, sortOrder, (SortMode)cbSortMode.SelectedItem);

        private void OnCellClick(object sender, CellClickEventArgs e)
        {
            if (e.ClickCount != 2)
            {
                return;
            }
            if (!(e.Column is DiffColumn col))
            {
                return;
            }
            var clrDumpType = SelectedTypeInstancesAddressList(col.ClrDump);
            TypeInstancesModule.Create(clrDumpType, this, mod => RequestDockModule(mod));
        }

        private ClrDumpType SelectedTypeInstancesAddressList(ClrDump clrDump)
        {
            var selectedType = dlvDumpDiff.SelectedObject<string>();
            return new ClrDumpType(clrDump, selectedType);
        }

        private void OnFormatCell(object sender, FormatCellEventArgs e)
        {
            if (e.Column == colType || e.ColumnIndex <= 1 || e.CellValue == null || !(e.CellValue is long))
            {
                return;
            }

            var value = (long)e.CellValue;
            e.SubItem.BackColor = value > 0 ? Color.LightGreen : value < 0 ? Color.LightPink : Color.LightGray;
        }

        public override void Init()
        {
            foreach (var clrDump in ClrDumps)
            {
                foreach (var stat in clrDump.GetTypeStats())
                {
                    typeNames.Add(stat.Type.Name);
                }
            }
        }

        public override void PostInit()
        {
            base.PostInit();
            Summary = $"{ClrDumps.Count} dumps, {typeNames.Count} types";

            dlvDumpDiff.Objects = typeNames;
            dlvDumpDiff.Sort(dlvDumpDiff.AllColumns[2], SortOrder.Descending);
        }

        private void cbSortMode_SelectedIndexChanged(object sender, EventArgs e) => dlvDumpDiff.Sort();
    }

    public class DumpDiffComparer : IComparer
    {
        private readonly OLVColumn column;
        private readonly SortOrder sortOrder;
        private readonly DumpDiffModule.SortMode sortMode;
        public DumpDiffComparer(OLVColumn column, SortOrder sortOrder, DumpDiffModule.SortMode sortMode)
        {
            this.column = column;
            this.sortOrder = sortOrder;
            this.sortMode = sortMode;
        }

        public int Compare(object x, object y)
        {
            if (sortOrder == SortOrder.None)
            {
                return 0;
            }

            OLVListItem itemX = x as OLVListItem;
            OLVListItem itemY = y as OLVListItem;
            var objValueX = itemX.GetSubItem(column.Index).ModelValue;
            var objValueY = itemY.GetSubItem(column.Index).ModelValue;

            long res = 0;
            if (objValueX is string || objValueY is string)
            {
                res = string.Compare((string)objValueX, (string)objValueY);
            }
            if (objValueX is long || objValueY is long)
            {
                long valueX = objValueX != null ? (long)objValueX : 0;
                long valueY = objValueY != null ? (long)objValueY : 0;

                res = sortMode == DumpDiffModule.SortMode.AbsValue ? Math.Abs(valueX) - Math.Abs(valueY) : valueX - valueY;
            }

            return sortOrder == SortOrder.Ascending ? (int)res : -(int)res;
        }
    }
}
