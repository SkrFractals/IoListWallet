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
    public NetworkSelect() : base() {
        (MyList[0] = new IoListButtons<Type>(this, [("T_AddNetwork", L("AddNetwork"))]) { MyIndex = 0 })
            .MyFilter.Filters = new IoFilter() {
            Filters = [ // Search Formats:
                new FilterText() { Name = "Blockchain" }, // Blockchain 
                new FilterText() { Name = "Network" } // Network 
                // TODO add filters: Staking, Smart...
            ], SortIndex = 0, SortIndex2 = 1, Increasing = true, Increasing2 = true
        };
        MyList[0].Selected = IsSelected;
        //MyContent.Content = MyList[0].Content;
    }
    internal override string TrueParentType => MakeK(typeof(MainMenu), "");
    protected override bool CloseOnClick(int fromList) => false;

    internal override IoPanel OnOpen(bool initial = false) {
        if (GetNetworks?.Count <= 0)
            OpenRefreshPanel(this, typeof(NetworkAdd));
        return base.OnOpen(initial);
    }

    /*internal override bool OnResponse(IoPanel From, object Data) {
        if (base.OnResponse(From, Data))
            return true;
        if (From is ListNetworkAdd) { // Deeper NetworkAdd selected something and is letting me know:
            AppMainMenu?.OnResponse(this, 0);
            Close();
            return true;
        }
        return false;
    }*/
    internal override void RefreshData(int job, CancellationToken? token) {
       
        if (MyList[0] is not IoListButtons<Type> list)
            return;
        list.Columns = 2; // 0 = Select Network, 1 = Edit Network
        list.ItemSource = GetNetworks?.Keys;
        list.ItemTemplate = (o) => [ // Columns:
                "T_SelectNetwork", IsNetwork((Type?)o, out var n) && n != null ? L(n.Blockchain) + " - " + L(n.GetNetwork()) : "", // Select Network
                "T_EditNetwork", L("EditNetwork") // Edit Network
                ];
        list.FilterCommandTemplate = (o) => IsNetwork((Type?)o, out var n) && n != null ? [ // Search Data:
                L(n.Blockchain), // Blockchain
                L(n.GetNetwork()) // Network
                //((int)N.Smart).ToString(), // Smart
                //((int)N.Staking).ToString() // Staking
                ] : [];
        
        base.RefreshData(job, token);
    }
    protected bool IsSelected(object? o) => GetProfile?.Network is Type t && o is Type t2 && t == t2;
    protected override Type? Selected(int fromList) => GetProfile?.Network;
    protected override void PerformSelect(int fromList, object? o) {
        LoadNetwork((Type?)o);
        RefreshPanel(this, 1 << MyList[fromList].MyIndex, true);
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
                //editPanel.MyNetwork = netId;
                //RefreshPanel(editPanel, -1, true);
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
}