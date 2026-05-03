using IoListWallet.Content.Abstract;
using IoListWallet.Core;
using IoListWallet.Core.Basic;
using IoListWallet.Core.Lists;
using IoListWallet.IoPanels.Network;
using IoListWallet.IoPanels.Profile;
using System;
using System.Threading;
using static IoListWallet.App;
using static IoListWallet.Core.Static.AppSave;

namespace IoListWallet.IoPanels.Main;

internal enum MainMenuButtons {
    ProfileSelect,
    NetworkSelect,
    WalletSelect,
    AccountSelect,
    Transactions
}
internal class MainMenu : IoListPanel {

    internal bool initProfile = true, initLogin = true, initNetwork = true, initWallet = true, initAccount = true;
    public MainMenu() : base() {

        MakeList<MainMenuButtons>(new IoFilter() {
            Filters = [
                new FilterEnum() { // Blockchain 
                    Name = "ShowRows",
                    //dRule = new(0, (byte)CompareRule.NotLess), 
                    //Rule = [new(0, (byte)CompareRule.NotLess)], 
                    Mask = ~(ulong)0,
                    dMatch = ~(ulong)0,
                    Match = [~(ulong)0],
                    EnumType = typeof(MainMenuButtons).Name }//,new FilterText() { Name = "Text" }, // Text 
            ], SortIndex = 0, SortIndex2 = 0, Increasing = true, Increasing2 = true},
            new Gen_Buttons(2), // 0 = Select Network, 1 = Edit Network 
            [("T_Help",L("Help"))], // No Tool Buttons
            1, // Only a single merged filter page 
            0, (object? o) => ((MainMenuButtons?)o ?? MainMenuButtons.ProfileSelect) switch {
                MainMenuButtons.NetworkSelect => !IsCorrectPassword(),
                MainMenuButtons.WalletSelect => GetNetwork == null,
                MainMenuButtons.AccountSelect => GetWallet == null,
                //MainMenuButtons.OpenAccount => GetWallet == null || GetWallet?.Account == null,
                MainMenuButtons.Transactions => (GetNetworks?.Count ?? 0) <= 0,
                _ => false,
            }, (object? o) => ((MainMenuButtons?)o ?? MainMenuButtons.ProfileSelect) switch {
                MainMenuButtons.NetworkSelect => GetProfile == null || (Networks?.Count ?? 0) <= 0 || (GetNetwork != null && (Networks?.Count ?? 0) <= 1),
                _ => false,
            });
        Book.Swipe.OnLeftEvent += (_, _) => ToggleFilter();
    }

    internal override void RefreshData(int job, CancellationToken? token) {
        MyList[0].RefreshTemplate(
            Enum.GetValues<MainMenuButtons>(), // Button Value source
            (button) => { // Button Row Templates:
                var p = GetProfile;
                var n = GetNetwork;
                //var a = GetWallet?.Accounts;
                var w = GetWallet;
                var loggedIn = IsCorrectPassword();
                string? login() => loggedIn ? null : L("PleaseLogin");
                string? guard() => login() ?? (n != null ? null : L("PleaseSelectNetwork"));
                return new Gen_Buttons.GenData((MainMenuButtons?)button switch {
                    MainMenuButtons.ProfileSelect => ["T_MenuSelectProfile",
                        L("MainMenu.ProfileSelect") + ": " + (p != null ? p.Name == "" ? L("NoName") : p.Name : L("Select")),
                        "T_MenuEditProfile", 
                        L(loggedIn ? "EditProfile" : "Login")],
                    MainMenuButtons.NetworkSelect => [(n == null ? "-" : "") + "T_MenuSelectNetwork",
                        login() ?? (L("MainMenu.NetworkSelect") + ": " + (n == null ? L(p?.Networks.Count > 0 ? "Select" : "Add") : L(n.Blockchain) + " - " + L(n.GetNetwork()))),
                        "T_EditNetwork",
                        L("EditNetwork")],
                    MainMenuButtons.WalletSelect => [(w == null ? "-" : "") + "T_MenuSelectWallet",
                        guard() ?? (L("MainMenu.WalletSelect") + ": " + (w?.Name ?? L(n?.Wallets.Count > 0 ? "Select" : "Add"))),
                    "T_EditWallet",
                    L("EditWallet")],
                    MainMenuButtons.AccountSelect => [(GetAccount == null ? "-" : "") + "T_MenuSelectAccount" ,
                        guard() ?? (L("MainMenu.AccountSelect") + ": " + (GetAccount?.Name ?? L(w?.Accounts.Count > 0 ? "Select" : "Add"))),
                    "T_EditWallet",
                    L("EditWallet")],
                    /*MainMenuButtons.OpenAccount => [
                         (GetAccount == null ? "-" : "") + "T_MenuOpenAccount",
                    L(!loggedIn ? "PleaseLogin" : (a != null)
                        ? "OpenAccount" : n != null
                            ? "PleaseSelectAccount" : "PleaseSelectNetwork"),
                    "T_EditAccount", "EditAccount"],*/
                    MainMenuButtons.Transactions => [(n == null ? "-" : "") + "T_MenuTransactions",  
                        guard() ?? "Transactions", // Transactions Button
                    "T_AllTransactions", L("AllTransactions")],
                    _ => ["--"]
                });
            }, (o) => { // Filter Templates:
                var A = (MainMenuButtons?)o;
                return A == null ? [] : [   // Search Data:
                    ((byte)A).ToString()//,   // Button Index/Enum
                    //GetEnumText(A)
                ];
            });
        base.RefreshData(job, token);
    }
    protected override object? Selected(int from) => GetProfile?.Networks;
    protected override void OnClickTool(object? s, IoListBase.ClickToolEventArgs e) {
        OpenRefreshPanel(this, typeof(Tutorial), "");
    }
    protected override void OnClickRow(object? s, IoListBase.ClickRowEventArgs e) {
        var b = e.ClickedRow as WToggleButton ?? throw new("Failed ListMainMenu.OnClickButton");
        var N = (MainMenuButtons?)b.Button.Tag;
        if (N != null) switch (e.Index) {
            case 0: // Main button - Edit Address
                    switch (N) {
                        case MainMenuButtons.ProfileSelect: OpenRefreshPanel(this, typeof(ProfileSelect), ""); break; // Profile Select
                        case MainMenuButtons.NetworkSelect: OpenRefreshPanel(this, typeof(NetworkSelect), ""); break; // Network Select
                        //case MainMenuButtons.AccountSelect: EnqueueTask(((ListWalletSelectPanel)OpenPanel(this, typeof(ListWalletSelectPanel)), 1)); break; // Wallet/Account select
                        //case MainMenuButtons.OpenAccount: OpenPanel(this, typeof(OpenAccountPanel)); break;// TODO Open your account screen
                        //case MainMenuButtons.Transactions: OpenPanel(this, typeof(ListTransactionsPanel)); break;
                    }
                    break;
            case 1: // Main Button - Open
                    switch (N) {
                        case MainMenuButtons.ProfileSelect: OpenRefreshPanel(this, typeof(ProfileContent), ""); break; // Profile Settings
                        case MainMenuButtons.NetworkSelect:
                            var network = GetNetwork ?? throw new("No network selected!");
                            OpenRefreshPanel(this, typeof(NetworkContent), network.GetType().Name);
                            break; // Edit Network
                        //case MainMenuButtons.AccountSelect: EnqueueTask(((SettingsAccountPanel)OpenPanel(this, typeof(SettingsAccountPanel)), 1)); break; // TODO open wallet settings (should also be available inside it's open-overview)
                        //case MainMenuButtons.OpenAccount: OpenPanel(this, typeof(BookAccount)); break; // TODO Open Account Settings
                        //case MainMenuButtons.Transactions: OpenPanel(this, typeof(ListHistoryPanel)); break; // TODO Open unfiltered Transaction History of every address
                    }
                    break;
        }
    }
    internal override IoPanel OnOpen(bool initial = false) {
        var r = base.OnOpen(initial);
        if (!initial)
            Initialize();
        return r;
    }
    internal void Initialize() {
        // MainMenu will automatically open initialization windows you need first setup.
        // It opens it only once, if you back out, it won't do it again.
        // Selecting these will reset the init bools again, so it could do it once again.
        if (DisableHandHolding)
            return;
        // open tutorial on first launch
        if (SApp.First) { 
            OpenRefreshPanel(AppMainMenu, typeof(Tutorial)); 
            SApp.First = false;
            return;
        }
        // open profile selector when if profile is selected
        if (GetProfile == null) { 
            if (initProfile)    
                OpenRefreshPanel(AppMainMenu, typeof(ProfileSelect)); 
            initProfile = false;
            return;
        }
        // open profile settings to login page if not logged in
        if (!(GetProfile?.CorrectPassword ?? true)) { 
            if (initLogin)
                OpenRefreshPanel(AppMainMenu, typeof(ProfileContent)); 
            initLogin = false;
            return;
        }
        // open network selector if no network is selected
        if (GetNetwork == null) {
            if (initNetwork)
                OpenRefreshPanel(AppMainMenu, typeof(NetworkSelect)); 
            initNetwork = false;
            return;
        }
        // open wallet selector if no wallet is selected
        if (GetWallet == null) {
            if (initWallet)
                OpenRefreshPanel(AppMainMenu, typeof(ProfileContent));
            initWallet = false;
            return;
        }
        // open account selector if no account is selected
        if (GetAccount == null) {
            if(initAccount)
                OpenRefreshPanel(AppMainMenu, typeof(ProfileContent));
            initAccount = false;
        }
    }
}