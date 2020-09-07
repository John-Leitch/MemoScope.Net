using BrightIdeasSoftware;
using MemoScope.Core.Data;
using System.Collections.Generic;
using System.Linq;

namespace MemoScope.Modules.Instances
{
    // Pretty ugly but it works.
    // With the default datasource, I could not display more than 4 millions rows 
    // because of its index cache as this source is readonly and static (nothing is updated)
    // we don't need to update anything so we don't need a cache
    public class InstanceVirtualSource : AbstractVirtualListDataSource
    {
        private readonly AddressList addressList;
        private readonly HashSet<ulong> filteredAddresses;
        private ulong[] filtered;

        public InstanceVirtualSource(VirtualObjectListView listView, AddressList addressList, HashSet<ulong> filteredAddresses) : base(listView)
        {
            this.addressList = addressList;
            this.filteredAddresses = filteredAddresses;
        }

        public override void ApplyFilters(IModelFilter modelFilter, IListFilter listFilter) => filtered = filteredAddresses.ToArray();

        public override object GetNthObject(int n) => listView.UseFiltering ? filtered[n] : (object)addressList.ClrDump.Eval(() => addressList.Addresses[n]);

        public override int GetObjectCount() => listView.UseFiltering ? filteredAddresses.Count : addressList.Addresses.Count;
    }
}
