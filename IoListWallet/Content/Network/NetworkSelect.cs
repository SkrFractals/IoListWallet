using IoListWallet.Core;
using IoListWallet.Core.Lists;
using IoListWallet.Core.Panels;
using IoListWallet.IoPanels.Main;
using System;
using System.Threading;
using static IoListWallet.App;
using static IoListWallet.Core.Static.AppSave;

namespace IoListWallet.IoPanels.Network;
internal class NetworkSelect : IoListPanel {
    private bool initNetwork = true;
    public NetworkSelect() : base() {
        MakeList<Type>(new IoFilter() {
            Filters = [ // Search Formats:
                new FilterText() { Name = "Blockchain" }, // Blockchain 
                new FilterText() { Name = "Network" } // Network 
                // TODO add filters: Staking, Smart...
            ], SortIndex = 0, SortIndex2 = 1, Increasing = true, Increasing2 = true
        }, [("T_AddNetwork", L("AddNetwork"))], // Toolbar: 0 = Add Network
        2, // Columns: 0 = Select Network, 1 = Edit Network
        -1, 0, null, null, IsSelected);
    }
    internal override string TrueParentType => MakeK(typeof(MainMenu), "");
    internal override IoPanel OnOpen(bool initial = false) {
        var r = base.OnOpen(initial);
        if (GetNetworks?.Count <= 0 && initNetwork) {
            initNetwork = false;
            OpenRefreshPanel(this, typeof(NetworkAdd));
        }
        return r;
    }
    internal override void RefreshData(int job, CancellationToken? token) {
        MyList[0].RefreshTemplate(
            GetNetworks?.Keys,
            (o) => [ // Columns:
                "T_SelectNetwork", IsNetwork((Type?)o, out var n) && n != null ? L(n.Blockchain) + " - " + L(n.GetNetwork()) : "", // Select Network
                "T_EditNetwork", L("EditNetwork") // Edit Network
            ], 
            (o) => IsNetwork((Type?)o, out var n) && n != null ? [ // Search Data:
                L(n.Blockchain), // Blockchain
                L(n.GetNetwork()) // Network
                //((int)N.Smart).ToString(), // Smart
                //((int)N.Staking).ToString() // Staking
            ] : []);
        base.RefreshData(job, token);
    }
    protected override Type? Selected(int fromList) => GetProfile?.Network;
    protected override bool CloseOnDoubleClick(int fromList) => true;
    protected override void PerformSelect(int fromList, object? o) {
        LoadNetwork((Type?)o);
        RefreshPanel(this, 1 << MyList[fromList].MyIndex, initNetwork = true);
    }
    protected override void OnClickButton(object? sender, ClickButtonEventArgs e) { 
        switch (e.Index) {
            case 0: // Select Network
                base.OnClickButton(sender, e);
                break;
            case 1: // Edit Network
                var b = e.ClickedButton 
                    ?? throw new("Failed ListNetworkSelect.OnClickButton");
                var netId = (Type?)b.Button.Tag
                    ?? throw new("EditNetwork.Button.Tag is not a proper IoNetwork!");
                var n = TryNetwork(netId) 
                    ?? throw new("EditNetwork.Button.Tag is not a proper IoNetwork!");
                
                var editPanel = OpenRefreshPanel(this, typeof(NetworkContent), netId.Name/*n.Blockchain + " - " + n.GetNetwork()*/) as NetworkContent 
                    ?? throw new("Failed ListNetworkSelect.editPanel");
                break;
        }
    }
    protected override void OnClickTool(object? sender, ClickToolEventArgs e) {
        switch (e.Index) {
            case 0: // Add Network
                OpenRefreshPanel(this, typeof(NetworkAdd));
                break;
        }
    }
    protected bool IsSelected(object? o) => GetProfile?.Network is Type t && o is Type t2 && t == t2;
}