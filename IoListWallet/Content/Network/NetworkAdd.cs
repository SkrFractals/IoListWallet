using IoListWallet.Core;
using IoListWallet.Core.Basic;
using IoListWallet.Core.Lists;
using IoListWallet.Core.Panels;
using IoListWallet.Structs.Crypto;
using System;
using System.Threading;
using static IoListWallet.App;
using static IoListWallet.Core.Static.AppSave;

namespace IoListWallet.IoPanels.Network;
internal class NetworkAdd : IoListPanel { // <NetId>
    public NetworkAdd() : base() {
        (MyList[0] = new IoListButtons<Type>(this, [/*("T_AddAddress", L("AddAddress"))*/]) { MyIndex = 0 })
            .MyFilter.Filters = new IoFilter() {
            Filters = [ // Search Formats:
                new FilterText() { Name = "Blockchain" }, // Blockchain 
                new FilterText() { Name = "Network" } // Network 
                // TODO add filters: Staking, Smart...
        ], SortIndex = 0, SortIndex2 = 1, Increasing = true, Increasing2 = true
        };
        MyList[0].Skip += (object? netId) => IsNetwork((Type?)netId, out _);
        //MyContent.Content = MyList[0].Content;
    }
    internal override bool IsModal() => true;
    internal override string TrueParentType => MakeK(typeof(NetworkSelect), "");
    internal override void Select(int fromList, IoToggleButton? T, object? o) => PerformSelect(fromList, o);
    internal override void RefreshData(int job, CancellationToken? token) {
        if (MyList[0] is not IoListButtons<Type> list)
            return;
            //((IoListButtons<IoNetwork>)MyList).Columns = 1; // 0 = Select Network, 1 = Edit Network
        list.ItemSource = Networks.Values;
        list.ItemTemplate = (type) => [
            // Select Network Collumn
            "T_SelectNetwork", type is Type t && Activator.CreateInstance(t) is IoNetwork n ? L(n.Blockchain) + " - " + L(n.GetNetwork()) : ""
            ];
        list.FilterCommandTemplate = (type) => type is Type t && Activator.CreateInstance(t) is IoNetwork n ? [ // Search Data:
            L(n.Blockchain),    // Blockchain
            L(n.GetNetwork())   // Network
            ] : [];
        base.RefreshData(job, token);
    }
    protected override void PerformSelect(int fromList, object? o) { LoadNetwork((Type?)o); RefreshPanel(TrueParentType, true); RefreshPanel(AppMainMenu, ~0, true); }
}