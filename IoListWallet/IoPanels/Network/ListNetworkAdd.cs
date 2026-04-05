using IoListWallet.ElementControls;
using IoListWallet.ElementControls.Basic;
using IoListWallet.MainControls.Panels;
using IoListWallet.Structs;
using IoListWallet.Structs.Crypto;
using System;
using System.Threading;
using static IoListWallet.App;
using static IoListWallet.AppSave;
namespace IoListWallet.IoPanels.Network;
internal class ListNetworkAdd : IoListPanel<Type> { // <NetId>
    public ListNetworkAdd() : base() {
        MyList.MyFilter.Filters = new IoFilter() {
            Filters = [ // Search Formats:
                new FilterText() { Name = "Blockchain" }, // Blockchain 
                new FilterText() { Name = "Network" } // Network 
                // TODO add filters: Staking, Smart...
        ], SortIndex = 0, SortIndex2 = 1, Increasing = true, Increasing2 = true
        };
    }
    internal override bool IsModal() => true;
    internal override string TrueParentType => MakeK(typeof(ListNetworkSelect), "");
    internal override void Select(IoToggleButton? T, object? o) => PerformSelect(o);
    internal override bool SkipEntries(object? netId) => IsNetwork((Type?) netId, out _); // already added
    internal override void RefreshData(CancellationToken? token) {
        //((IoListButtons<IoNetwork>)MyList).Columns = 1; // 0 = Select Network, 1 = Edit Network
        MyList.ItemSource = Networks.Values;
        MyList.ItemTemplate = (type) => [
            // Select Network Collumn
            "T_SelectNetwork", type is Type t && Activator.CreateInstance(t) is IoNetwork n ? L(n.Blockchain) + " - " + L(n.GetNetwork()) : ""
            ];
        MyList.FilterCommandTemplate = (type) => type is Type t && Activator.CreateInstance(t) is IoNetwork n ? [ // Search Data:
            L(n.Blockchain),    // Blockchain
            L(n.GetNetwork())   // Network
            ] : [];
        /*MyList.FilterCommandTemplate = (o) => {
            var t = o as Type ?? throw new();
            var a = Activator.CreateInstance(t);
            
            return a is IoNetwork n ? [ // Search Data:
            L(n.Blockchain),    // Blockchain
            L(n.GetNetwork())   // Network
            ] : [];
        };*/
        base.RefreshData(token);
    }
    protected override IoList<Type> MakeList() 
        => new IoListButtons<Type>(this, []);
    protected override void PerformSelect(object? o) { LoadNetwork((Type?)o); }
}