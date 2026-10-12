using IoListWallet.Content.Abstract;
using IoListWallet.Core;
using IoListWallet.Core.Basic;
using IoListWallet.Core.Lists;
using IoListWallet.Content.Main;
using System;
using System.Threading;
using static IoListWallet.App;
using static IoListWallet.Core.Static.AppSave;

namespace IoListWallet.Content.Network;
internal class NetworkSelect : IoListPanel {
    private const string F = "Failed IoListPanel.";
    private bool initNetwork = true;
    public NetworkSelect() : base() {
        _ = MakeList<Type>(new IoFilter() {
            Filters = [ // Search Formats:
                new FilterText() { Name = "Blockchain" }, // Blockchain 
                new FilterText() { Name = "Network" } // Network 
                // TODO add filters: Staking, Smart...
            ], SortIndex = 0, SortIndex2 = 1, Increasing = true, Increasing2 = true
        }, new Gen_Buttons(2), // Columns: 0 = Select Network, 1 = Edit Network
        [("T_AddNetwork", L("Add") + " " + L("Network"))], // Toolbar: 0 = Add Network
        -1, 0, null, null, IsSelected);
    }
    internal override string TrueParentType => MakeK(typeof(MainMenu), "");
    internal override IoPanel OnOpen() {
        var r = base.OnOpen();
        if (GetNetworks?.Count <= 0 && initNetwork) {
            initNetwork = false;
            _ = OpenRefreshPanel(this, typeof(NetworkAdd));
        }
        return r;
    }
    internal override void RefreshData(int job, CancellationToken? token) {
        MyList[0].RefreshTemplate(
            GetNetworks?.Keys,
            (o) => new Gen_Buttons.GenData([ // Columns:
                "T_SelectNetwork", IsNetwork((Type?)o, out var n) && n != null ? L(n.Blockchain) + " - " + L(n.GetNetwork()) : "", // Select Network
                "T_EditNetwork", L("Edit") + " " + L("Network") // Edit Network
            ]), 
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
        _ = LoadNetwork((Type?)o);
        _ = RefreshPanel(this, 1 << MyList[fromList].MyIndex, initNetwork = true);
    }
    protected override void OnClickRow(object? sender, IoListBase.ClickRowEventArgs e) { 
        switch (e.Index) {
            case 0: // Select Network
                base.OnClickRow(sender, e);
                _ = RefreshPanel(AppMainMenu, ~0, true);
                break;
            case 1: // Edit Network
                var b = e.ClickedRow as WToggleButton ?? throw new(F + "OnClickButton");
                var netId = (Type?)b.Button.Tag ?? throw new(F + "Button.Tag");
                _ = TryNetwork(netId) ?? throw new(F + "TryNetwork");
                _ = OpenRefreshPanel(this, typeof(NetworkContent), netId.Name);
                break;
        }
    }
    protected override void OnClickTool(object? sender, IoListBase.ClickToolEventArgs e) {
        switch (e.Index) {
            case 0: // Add Network
                _ = OpenRefreshPanel(this, typeof(NetworkAdd));
                break;
        }
    }
    protected bool IsSelected(object? o) => GetProfile?.Network is Type t && o is Type t2 && t == t2;
}