using IoListWallet.Core;
using IoListWallet.Core.Lists;
using IoListWallet.Core.Panels;
using IoListWallet.Core.Static;
using IoListWallet.IoPanels.Network;
using IoListWallet.IoPanels.Profile;
using IoListWallet.Structs.Crypto;
using System;
using System.Collections.Generic;
using System.Threading;
using static IoListWallet.App;
using static IoListWallet.Core.Static.AppSave;

namespace IoListWallet.IoPanels.Main;

internal enum MainMenuButtons {
    ProfileSelect,
    NetworkSelect,
    AccountSelect,
    OpenAccount,
    Transactions
}
internal class MainMenu : IoListPanel {

    //protected override string[] GetPageNames() => ["", "Test"];

    internal bool initProfile = true, initLogin = true, initNetwork = true, initWallet = true, initAccount = true;
    public MainMenu() : base() => Construct();


    public MainMenu(IoDockPanels Dock) : base() {
        (ParentDock = Dock).Panel = [GetK];
        Construct();
    }
    private void Construct() {
        Register(GetK, this);
        AppDockRoot = ParentDock?.Parent as IoDockRoot;
        AppDockRoot?.Original = GetK;
        MyList[0] = new IoListButtons<MainMenuButtons>(this, []) { MyIndex = 0 };
        /*if (AppAllNetworks.Count == 1) {
            // autoselect the only network:
            var n = AppAllNetworks[0];
            if (AppNetworks.Contains(n))
                AppNetworks.Add(n);
            SetAppNetwork(n);
        }*/
        OnOpen(true);
        MyList[0].MyFilter.Filters = new IoFilter() {
            Filters = [
                new FilterEnum() { // Blockchain 
                    Name = "ShowRows",
                    //dRule = new(0, (byte)CompareRule.NotLess), 
                    //Rule = [new(0, (byte)CompareRule.NotLess)], 
                    Mask = ~(ulong)0,
                    dMatch = ~(ulong)0,
                    Match = [~(ulong)0],
                    EnumType = typeof(MainMenuButtons).Name }//,new FilterText() { Name = "Text" }, // Text 
            ], SortIndex = 0, SortIndex2 = 0, Increasing = true, Increasing2 = true
        };
        MyList[0].MyFilter.MaxSearch = 1;
        MyList[0].MyFilter.PageNames = ["Sort"];
        MyList[0].Disable += (object? o) => ((MainMenuButtons?)o ?? MainMenuButtons.ProfileSelect) switch {
            MainMenuButtons.NetworkSelect => !IsCorrectPassword(),
            MainMenuButtons.AccountSelect => GetNetwork == null || GetNetwork == null,
            MainMenuButtons.OpenAccount => GetWallet == null || GetWallet?.Account == null,
            MainMenuButtons.Transactions => (GetNetworks?.Count ?? 0) <= 0,
            _ => false,
        };
        MyList[0].Skip += (object? o) => ((MainMenuButtons?)o ?? MainMenuButtons.ProfileSelect) switch {
            MainMenuButtons.NetworkSelect => GetProfile == null || (Networks?.Count ?? 0) <= 0 || (GetNetwork != null && (Networks?.Count ?? 0) <= 1),
            _ => false,
        };
        //MyContent.Content = Book;//MyList[0].Content;
        Init(Available);
        Book.Swipe.OnLeftEvent += (_, _) => ToggleFilter();
    }
    protected override bool CloseOnClick(int fromList) => false;
    protected override bool CloseOnDoubleClick(int fromList) => false;
    internal override void Snapshot() {
        base.Snapshot();
        if (GetProfile?.Pan is IoProfile.SaveFilePan p) {
            p.SaveMenu.PageSize = PageSize;
            p.SaveMenu.Page = Page;
            p.SaveMenu.MyFilter = MyFilter;
            notConst = false;
            p.SaveMenu.MenuPanel = new IoPanelSave(){ /*ChildPanels = ChildPanels,*/ TitleColor = TitleColor, UserTitle = UserTitle };
            notConst = true;
        }
    }
    /*internal override bool OnResponse(IoPanel From, object Data) {
        if (base.OnResponse(From, Data))
            return true;
       
        // Every list in the app will let me know here know when the selection changed, so it can globally apply updates:
        if (From is ListNetworkSelect n) {
            RefreshPanel(this);
            // Selected a Network
            return true;
        }
        //if (From is WalletSelectPanel n) {// Selected a Wallet
        //    texts[1].Text = L("WalletSelect") + (AppWallet == "" ? L("Select") : AppWallet);return true; }
        RefreshPanel(this);
        return false;
    }*/
    /*internal override void ClickTool(int i) {
        switch (i) {
            case 0: // First Tool click
                break;
        }
    }*/
    internal override void RefreshData(int job, CancellationToken? token) {
        if (MyList[0] is not IoListButtons<MainMenuButtons> list)
            return;
        list.Columns = 2; // 0 = Select Network, 1 = Edit Network
        list.ItemSource = Enum.GetValues<MainMenuButtons>();//Enum.GetValues<MainMenuButtons>().ToList().AsEnumerable<object>();
        list.ItemTemplate = (button) => {
            var p = GetProfile;
            var n = GetNetwork;
            var a = GetWallet?.Accounts;
            var loggedIn = IsCorrectPassword();

            return (MainMenuButtons?)button switch {
                MainMenuButtons.ProfileSelect => [
                    "T_MenuSelectProfile", L("MainMenu.ProfileSelect") + ": " + (p != null ? p.Name == "" ? L("NoName") : p.Name : L("Select")),
                    "T_MenuEditProfile", L(loggedIn ? "EditProfile" : "Login")],
                MainMenuButtons.NetworkSelect => [
                    (n == null ? "-" : "") + "T_MenuSelectNetwork",
                    loggedIn ? L("MainMenu.NetworkSelect") + ": " + (n == null ? L("Select") : L(n.Blockchain) + " - " + L(n.GetNetwork())) : L("PleaseLogin"),
                    "T_EditNetwork",
                    L("EditNetwork")],
                MainMenuButtons.AccountSelect => [
                    (GetWallet == null ? "-" : "") + "T_MenuSelectAccount" ,
                     !loggedIn ? L("PleaseLogin") : (n != null)
                        ? ( L("MainMenu.WalletSelect") + ": " + (GetWallet?.Name ?? L("Select"))) + "\n" + (
                            L("MainMenu.AccountSelect") + ": " + (GetAccount?.Name ?? L("Select")))
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
        list.FilterCommandTemplate = (o) => { 
            var A = (MainMenuButtons?)o;
            return A == null ? [] : [   // Search Data:
                ((byte)A).ToString()//,   // Button Index/Enum
                //GetEnumText(A)
                ];
        };

        base.RefreshData(job, token);
    }

    protected override object? Selected(int from) => GetProfile?.Networks;
    protected override void OnClickButton(object? s, ClickButtonEventArgs e) {
        var b = e.ClickedButton ?? throw new("Failed ListMainMenu.OnClickButton");
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
        // TODO figure out why it's resetting all texts to login without this refresh:
        //RefreshPanel(this);
    }
    internal override IoPanel OnOpen(bool initial = false) {
        if(!initial)
            Initialize();
        return base.OnOpen(initial);
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