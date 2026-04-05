using IoListWallet.ElementControls;
using IoListWallet.MainControls.Panels;
using IoListWallet.IoPanels.Main;
using IoListWallet.Structs;
using System;
using System.Threading;
using static IoListWallet.App;
using static IoListWallet.AppSave;

namespace IoListWallet.IoPanels.Network;
internal class ListNetworkSelect : IoListPanel<Type> {
    public ListNetworkSelect() : base() {
        MyList.MyFilter.Filters = new IoFilter() {
            Filters = [ // Search Formats:
                new FilterText() { Name = "Blockchain" }, // Blockchain 
                new FilterText() { Name = "Network" } // Network 
                // TODO add filters: Staking, Smart...
            ], SortIndex = 0, SortIndex2 = 1, Increasing = true, Increasing2 = true
        };
    }
    internal override string TrueParentType => MakeK(typeof(ListMainMenu), "");
    protected override bool CloseOnClick() => false;
    internal override bool OnResponse(IoPanel From, object Data) {
        if (base.OnResponse(From, Data))
            return true;
        if (From is ListNetworkAdd) { // Deeper NetworkAdd selected something and is letting me know:
            AppMainMenu?.OnResponse(this, 0);
            Close();
            return true;
        }
        return false;
    }
    internal override void RefreshData(CancellationToken? token) {
        ((IoListButtons<Type>)MyList).Columns = 2; // 0 = Select Network, 1 = Edit Network
        MyList.ItemSource = GetNetworks?.Keys;
        MyList.ItemTemplate = (o) => [ // Columns:
                "T_SelectNetwork", IsNetwork((Type?)o, out var n) && n != null ? L(n.Blockchain) + " - " + L(n.GetNetwork()) : "", // Select Network
                "T_EditNetwork", L("EditNetwork") // Edit Network
                ];
        MyList.FilterCommandTemplate = (o) => IsNetwork((Type?)o, out var n) && n != null ? [ // Search Data:
                L(n.Blockchain), // Blockchain
                L(n.GetNetwork()) // Network
                //((int)N.Smart).ToString(), // Smart
                //((int)N.Staking).ToString() // Staking
                ] : [];
        base.RefreshData(token);
    }
    protected override IoList<Type> MakeList()
        => new IoListButtons<Type>(this, [("T_AddNetwork", L("AddNetwork"))]);
    protected override Type? Selected() => GetProfile?.Network;
    protected override void PerformSelect(object? o) {
        LoadNetwork((Type?)o);
        RefreshPanel(this);
    }
    protected override void OnClickButton(object? sender, IoList<Type>.ClickButtonEventArgs e) { 
        switch (e.Index) {
            case 0: // Select Network
                base.OnClickButton(sender, e);
                break;
            case 1: // Edit Network
                var b = e.ClickedButton 
                    ?? throw new("Failed ListNetworkSelect.OnClickButton");
                var netId = (Type?)b.Button.Tag;
                var n = TryNetwork(netId) 
                    ?? throw new("EditNetwork.Button.Tag is not an IoNetwork!");
                var editPanel = OpenPanel(this, typeof(BookNetwork), n.Blockchain + " - " + n.GetNetwork(), false, false) as BookNetwork 
                    ?? throw new("Failed ListNetworkSelect.editPanel");
                editPanel.MyNetwork = netId;
                RefreshPanel(editPanel);
                break;
        }
    }
    protected override void OnClickTool(object? sender, IoList<Type>.ClickToolEventArgs e) {
        switch (e.Index) {
            case 0: // Add Network
                OpenRefreshPanel(this, typeof(ListNetworkAdd));
                break;
        }
    }
}