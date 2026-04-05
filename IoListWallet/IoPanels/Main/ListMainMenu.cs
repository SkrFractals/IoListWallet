using IoListWallet.ElementControls;
using IoListWallet.MainControls;
using IoListWallet.MainControls.Panels;
using IoListWallet.IoPanels.Network;
using IoListWallet.Structs;
using System;
using System.Threading;
using static IoListWallet.App;
using static IoListWallet.AppSave;
using IoListWallet.IoPanels.Profile;
using IoListWallet.Structs.Crypto;

namespace IoListWallet.IoPanels.Main;

internal enum MainMenuButtons {
    ProfileSelect,
    NetworkSelect,
    AccountSelect,
    OpenAccount,
    Transactions
}
internal class ListMainMenu : IoListPanel<MainMenuButtons> {
    public ListMainMenu(IoDockPanel Dock) : base() {
        (ParentDock = Dock).Panel = GetK;
        Register(GetK, this);
        /*if (AppAllNetworks.Count == 1) {
            // autoselect the only network:
            var n = AppAllNetworks[0];
            if (AppNetworks.Contains(n))
                AppNetworks.Add(n);
            SetAppNetwork(n);
        }*/
        OnOpen();
        MyList.MyFilter.Filters = new IoFilter() {
            Filters = [
                new FilterEnum() { // Blockchain 
                    Name = "ShowFrom",
                    dRule = new(0, (byte)CompareRule.NotLess), 
                    Rule = [new(0, (byte)CompareRule.NotLess)], 
                    Max = 3, 
                    EnumType = typeof(MainMenuButtons).Name }, 
                new FilterText() { Name = "Text" }, // Text 
            ], SortIndex = 0, SortIndex2 = 0, Increasing = true, Increasing2 = true
        };
    }

    protected override bool CloseOnClick() => false;
    protected override bool CloseOnDoubleClick() => false;
    internal override void Snapshot() {
        base.Snapshot();
        if (GetProfile?.Pan is IoProfile.SaveFilePan p) {
            p.SaveMenu.PageSize = PageSize;
            p.SaveMenu.Page = Page;
            p.SaveMenu.MyFilter = MyFilter;
        }
    }
    internal override bool OnResponse(IoPanel From, object Data) {
        if (base.OnResponse(From, Data))
            return true;
       
        // Every list in the app will let me know here know when the selection changed, so it can globally apply updates:
        if (From is ListNetworkSelect n) {
            RefreshPanel(this);
            // Selected a Network
            return true;
        }
        /*if (From is WalletSelectPanel n)
        {
            // Selected a Wallet
            texts[1].Text = L("WalletSelect") + (AppWallet == "" ? L("Select") : AppWallet);
            return true;
        }*/
        RefreshPanel(this);
        return false;
    }
    /*internal override void ClickTool(int i) {
        switch (i) {
            case 0: // First Tool click
                break;
        }
    }*/
    internal override void RefreshData(CancellationToken? token) {
       
        ((IoListButtons<MainMenuButtons>)MyList).Columns = 2; // 0 = Select Network, 1 = Edit Network
        MyList.ItemSource = Enum.GetValues<MainMenuButtons>();//Enum.GetValues<MainMenuButtons>().ToList().AsEnumerable<object>();
        MyList.ItemTemplate = (button) => {
            var p = GetProfile;
            var n = GetNetwork;
            var a = GetWallet?.Accounts;
            var loggedIn = IsCorrectPassword();

            return (MainMenuButtons?)button switch {
                MainMenuButtons.ProfileSelect => [
                    "T_MenuSelectProfile", L("ProfileSelect") + ": " + L(p != null ? p.Name == "" ? "NoName" : p.Name : "Select"),
                    "T_MenuEditProfile", L(loggedIn ? "EditProfile" : "Login")],
                MainMenuButtons.NetworkSelect => [
                    (n == null ? "-" : "") + "T_MenuSelectNetwork",
                    loggedIn ? L("NetworkSelect") + ": " + (n == null ? L("Select") : L(n.Blockchain) + " - " + L(n.GetNetwork())) : L("PleaseLogin"),
                    "T_EditNetwork",
                    L("EditNetwork")],
                MainMenuButtons.AccountSelect => [
                    (GetWallet == null ? "-" : "") + "T_MenuSelectAccount" ,
                     !loggedIn ? L("PleaseLogin") : (n != null)
                        ? ( L("WalletSelect") + ": " + (GetWallet?.Name ?? L("Select"))) + "\n" + (
                            L("AccountSelect") + ": " + (GetAccount?.Name ?? L("Select")))
                        : L("PleaseSelectNetwork") , // if network is not selected
                    "T_EditWallet",
                    L("EditWallet")],
                MainMenuButtons.OpenAccount => [
                     (GetAccount == null ? "-" : "") + "T_MenuOpenAccount",
                    L(!loggedIn ? "PleaseLogin" : (a != null)
                        ? "OpenAccount" : n != null
                            ? "PleaseSelectAccount" : "PleaseSelectNetwork"),
                    "T_EditAccount", "EditAccount"],
                MainMenuButtons.Transactions => [
                     (n == null ? "-" : "") + "T_MenuTransactions",  L(!loggedIn ? "PleaseLogin" :(GetNetworks?.Count ?? 0) <= 0 ? "PleaseAddNetworks" : "Transactions"), // Transactions Button
                    "T_AllTransactions", L("AllTransactions")],
                _ => ["--"]
            };
        };
        MyList.FilterCommandTemplate = (o) => { 
            var A = (MainMenuButtons?)o;
            return A == null ? [] : [   // Search Data:
                ((byte)A).ToString(),   // Button Index/Enum
                GetEnumText(A)      // Enum name
                ];
        };

        base.RefreshData(token);
    }
    internal override bool DisableEntries(object? o) => ((MainMenuButtons?)o ?? MainMenuButtons.ProfileSelect) switch {
        MainMenuButtons.NetworkSelect => !IsCorrectPassword(),
        MainMenuButtons.AccountSelect => GetNetwork == null || GetNetwork == null,
        MainMenuButtons.OpenAccount => GetWallet == null || GetWallet?.Account == null,
        MainMenuButtons.Transactions => (GetNetworks?.Count ?? 0) <= 0,
        _ => false,
    };
    internal override bool SkipEntries(object? o) => ((MainMenuButtons?)o ?? MainMenuButtons.ProfileSelect) switch {
        MainMenuButtons.NetworkSelect => GetProfile == null || (Networks?.Count ?? 0) <= 0 || (GetNetwork != null && (Networks?.Count ?? 0) <= 1),
        _ => false,
    };
    protected override IoList<MainMenuButtons> MakeList()
        => new IoListButtons<MainMenuButtons>(this, [/*("T_Tooltip", L("Button"))*/]);
    protected override object? Selected() => GetProfile?.Networks;
    /*protected override void PerformSelect(object? o) {
        var N = (MainMenuButtons?)o;
        if (N != null) switch (N) {
                case MainMenuButtons.ProfileSelect: OpenRefreshPanel(this, typeof(ListProfileSelect), ""); break;
                case MainMenuButtons.NetworkSelect: OpenRefreshPanel(this, typeof(ListNetworkSelect), ""); break;
                case MainMenuButtons.AccountSelect: OpenRefreshPanel(this, typeof(ListNetworkSelect), ""); break;
                case MainMenuButtons.OpenAccount: OpenPanel(this, typeof(ListNetworkSelect)); break;
                case MainMenuButtons.Transactions: OpenPanel(this, typeof(ListNetworkSelect)); break;
            }
    }*/
    protected override void OnClickButton(object? s, IoList<MainMenuButtons>.ClickButtonEventArgs e) {
        var b = e.ClickedButton ?? throw new("Failed ListMainMenu.OnClickButton");
        var N = (MainMenuButtons?)b.Button.Tag;
        if (N != null) switch (e.Index) {
            case 0: // Main button - Edit Address
                    switch (N) {
                        case MainMenuButtons.ProfileSelect: OpenRefreshPanel(this, typeof(ListProfileSelect), ""); break; // Profile Select
                        case MainMenuButtons.NetworkSelect: OpenRefreshPanel(this, typeof(ListNetworkSelect), ""); break; // Network Select
                        //case MainMenuButtons.AccountSelect: EnqueueTask(((ListWalletSelectPanel)OpenPanel(this, typeof(ListWalletSelectPanel)), 1)); break; // Wallet/Account select
                        //case MainMenuButtons.OpenAccount: OpenPanel(this, typeof(OpenAccountPanel)); break;// TODO Open your account screen
                        //case MainMenuButtons.Transactions: OpenPanel(this, typeof(ListTransactionsPanel)); break;
                    }
                    break;
            case 1: // Main Button - Open
                    switch (N) {
                        case MainMenuButtons.ProfileSelect: OpenPanel(this, typeof(BookProfile), "", true); break; // Profile Settings
                        case MainMenuButtons.NetworkSelect:
                            var network = GetNetwork ?? throw new("No network selected!");
                            var editPanel = (BookNetwork?)OpenPanel(this, typeof(BookNetwork), network.Blockchain + " - " + network.GetNetwork(), false, false);
                            editPanel?.MyNetwork = network.GetType();
                            EnqueueTask((editPanel, 1)); 
                            break; // Edit Network
                        //case MainMenuButtons.AccountSelect: EnqueueTask(((SettingsAccountPanel)OpenPanel(this, typeof(SettingsAccountPanel)), 1)); break; // TODO open wallet settings (should also be available inside it's open-overview)
                        //case MainMenuButtons.OpenAccount: OpenPanel(this, typeof(BookAccount)); break; // TODO Open Account Settings
                        //case MainMenuButtons.Transactions: OpenPanel(this, typeof(ListHistoryPanel)); break; // TODO Open unfiltered Transaction History of every address
                    }
                    break;
        }
        // TODO figure out why it's resetting all texts to login without this refresh:
        //RefreshPanel(this);
    }
}