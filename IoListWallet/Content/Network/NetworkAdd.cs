using IoListWallet.Content.Abstract;
using IoListWallet.Core;
using IoListWallet.Core.Basic;
using IoListWallet.Core.Lists;
using IoListWallet.Structs.Crypto;
using System;
using System.Threading;
using static IoListWallet.App;
using static IoListWallet.Core.Static.AppSave;

namespace IoListWallet.Content.Network;
internal class NetworkAdd : IoListPanel { // <NetId>
    public NetworkAdd() : base() {
        _ = MakeList<Type>(new IoFilter() {
            Filters = [ // Search Formats:
                new FilterText() { Name = "Blockchain" }, // Blockchain 
                new FilterText() { Name = "Network" } // Network 
                // TODO add filters: Staking, Smart...
        ], SortIndex = 0, SortIndex2 = 1, Increasing = true, Increasing2 = true
        }, new Gen_Buttons(1), [], -1, 0, null, (object? netId) => IsNetwork((Type?)netId, out _));
    }
    internal override bool IsModal() => true;
    internal override string TrueParentType => MakeK(typeof(NetworkSelect), "");
    internal override void Select(int fromList, IoToggleButton? T, object? o) => PerformSelect(fromList, o);
    internal override void RefreshData(int job, CancellationToken? token) {
        MyList[0].RefreshTemplate(
            Networks.Values,
            (type) => new Gen_Buttons.GenData([
                "T_SelectNetwork",
                type is Type t && Activator.CreateInstance(t) is IoNetwork n ? L(n.Blockchain) + " - " + L(n.GetNetwork()) : ""
            ]),
            (type) => type is Type t && Activator.CreateInstance(t) is IoNetwork n ? [ // Search Data:
                L(n.Blockchain),    // Blockchain
                L(n.GetNetwork())   // Network
            ] : []);
        base.RefreshData(job, token);
    }
    protected override void PerformSelect(int fromList, object? o) { LoadNetwork((Type?)o); RefreshPanel(TrueParentType, true); RefreshPanel(AppMainMenu, ~0, true); }
    protected override bool CloseOnClick(int fromList) => true;
}