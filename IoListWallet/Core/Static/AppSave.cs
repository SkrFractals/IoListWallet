using IoListWallet.Core.Panels;
using IoListWallet.IoPanels.Main;
using IoListWallet.Structs.Crypto;
using IoListWallet.Structs.Crypto.Bitcoin;
using IoListWallet.Structs.Crypto.Cardano;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq.Expressions;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Avalonia.Controls;
using Avalonia;
using Avalonia.Media;

namespace IoListWallet.Core.Static;


public class RotationHost : Decorator {
    public void Set(object? s, EventArgs e) {
        RenderTransform = new RotateTransform() { Angle = (AppSave.GetProfile?.DisplayRotation ?? 0) * 90 };
    }
    public RotationHost() {
        RenderTransformOrigin = new RelativePoint(0.5, 0.5, RelativeUnit.Relative);
        AppSave.RotationHosts += Set;
        Set(null, new EventArgs());
    }
    protected override Size MeasureOverride(Size availableSize) {
        Child?.Measure(Is90() ? (availableSize = Swap(availableSize)) : availableSize);
        return Child?.DesiredSize ?? new(0,0);
    }
    protected override Size ArrangeOverride(Size finalSize) {
        var angle = AppSave.GetProfile?.DisplayRotation ?? 0;
        if (angle == 1) {
            Child?.Arrange(new Rect(new(0, 0), new Size(finalSize.Height, finalSize.Width)));
            /*RenderTransform = new TransformGroup {
                Children =
                [
                new RotateTransform(90),
                new TranslateTransform(Math.Max(0, finalSize.Width - finalSize.Height) / 2, Math.Min(0, finalSize.Width - finalSize.Height) / 2 )
            ]
            };
            return Swap(finalSize);*/
            RenderTransform = new TransformGroup {
                Children =
                [
                new RotateTransform(90),
                new TranslateTransform((finalSize.Width - finalSize.Height) / 2, (finalSize.Width - finalSize.Height) / 2)
            ]
            };
            return finalSize;
        }
        if (angle == 3) {
            Child?.Arrange(new Rect(new(0, 0), new Size(finalSize.Height, finalSize.Width)));
            /*RenderTransform = new TransformGroup {
                Children =
                [
                new RotateTransform(270),
                new TranslateTransform(Math.Min(0, finalSize.Height - finalSize.Width) / 2, Math.Max(0, finalSize.Height - finalSize.Width) / 2 )
            ]
            };
            return Swap(finalSize);*/
            RenderTransform = new TransformGroup {
                Children =
                [
                new RotateTransform(270),
                new TranslateTransform((finalSize.Height - finalSize.Width) / 2, (finalSize.Height - finalSize.Width) / 2)
            ]
            };
            return finalSize;
        }
        Child?.Arrange(new Rect(new Point(0, 0), finalSize));
        return finalSize;
    }
    private bool Is90() => ((AppSave.GetProfile?.DisplayRotation ?? 0) & 1) == 1;
    private Size Swap(Size s) => new(s.Height, s.Width);
}

internal static class AppSave {
    internal static void UpdateRotation() => RotationHosts?.Invoke(null, new EventArgs()); 
    internal static event EventHandler<EventArgs>? RotationHosts;

    #region Enums
    internal enum DisplayMode {
        Fullscreen,
        MultiWindow,
        SingleWindow
    }
    internal enum SortAlgo {
        S_Intro,
        P_IntroMerge,
        P_BucketIntro
    }
    #endregion

    #region Defaults
    internal static IoProfile DPro = new();
    internal static ushort
        MaxTasks = (ushort)Math.Max(1, Environment.ProcessorCount - 2);
    internal static string[] DateFormats = [
        "yyyy-MM-dd",
        "dd.MM.yyyy",
        "MM/dd/yyyy",
        "yyyy-MM-dd HH:mm",
        "dd.MM.yyyy HH:mm",
        "MM/dd/yyyy HH:mm"
    ];
    internal static Dictionary<string, Type> Networks = new() {
        { typeof(NetworkBitcoin_MainNet).Name, typeof(NetworkBitcoin_MainNet) },
        { typeof(NetworkBitcoin_TestNet).Name, typeof(NetworkBitcoin_TestNet) },
        { typeof(NetworkBitcoin_RegTest).Name, typeof(NetworkBitcoin_RegTest) },
        { typeof(NetworkCardano_MainNet).Name, typeof(NetworkCardano_MainNet) },
        { typeof(NetworkCardano_TestNet).Name, typeof(NetworkCardano_TestNet) }
    };
    #endregion

    #region AppState (Loaded profile)
    internal static IoApp SApp = new();
    internal static Dictionary<string, string> LocaleNames = [];
    internal static Dictionary<string, Dictionary<string, string>> LocaleData = [];
    internal static Dictionary<string, string> Locale = [];
    internal static Dictionary<string, string> SubLocaleNames = [];
    internal static Dictionary<string, Dictionary<string, string>> SubLocaleData = [];
    internal static Dictionary<string, string> SubLocale = [];
    internal static MainMenu? AppMainMenu; // the main menu IoPanel
    internal static IoWindow? AppMainWindow; // the original window, where main menu is (if desktop)
    internal static IoMainPanel? AppMainPanel; // the original docking panel where the main menu is
    internal static IoDockRoot? AppDockRoot; // the dock of the AppMainPanel
    //Dictionary<string, (int, int)> LastWindowSize; // The last windows size the user did to that Panel type.
    #endregion

    #region Getters
    internal static Point Rotate(Point p) {
        var r = (GetProfile?.DisplayRotation ?? DPro.DisplayRotation) * Math.PI / 2;
        double c = Math.Cos(r), s = Math.Sin(r);
        return new Point(c * p.X + s * p.Y, c * p.Y - s * p.X);
    }
    internal static Dictionary<int, IoProfile> GetProfiles => SApp.Profiles;
    internal static IoProfile? GetProfile => SApp.GetProfile;
    internal static IoProfile? TryProfile(int profileId) => GetProfiles.TryGetValue(profileId, out var profile) ? profile : null;
    internal static bool IsProfile(int profileId, out IoProfile? profile) => (profile = TryProfile(profileId)) != null;
    internal static IoWindow? TryWindow(int index) => (GetProfile?.Pan.OpenedWindows.TryGetValue(index, out var window) ?? false) ? window : null;

    internal static IoWindow? GetWindow(int index) => (GetProfile?.Pan.OpenedWindows.TryGetValue(index, out var window) ?? false) ? window : new IoWindow();
    internal static bool IsWindow(int index, out IoWindow? window) => (window = TryWindow(index)) != null;
    internal static Dictionary<Type, IoNetwork>? GetNetworks => GetProfile?.Networks ?? null;
    internal static IoNetwork? GetNetwork => SApp.GetNetwork;
    internal static IoNetwork? TryNetwork(Type? netId) => (GetNetworks?.TryGetValue(netId ?? typeof(App), out var network) ?? false) ? network : null;
    internal static bool IsNetwork(Type? netId, out IoNetwork? network) => (network = TryNetwork(netId)) != null;
    internal static Dictionary<int, IoWallet>? GetWallets => GetNetwork?.Wallets ?? null;
    internal static IoWallet? GetWallet => SApp.GetWallet;
    internal static IoWallet? TryWallet(int walletId) => (GetWallets?.TryGetValue(walletId, out var wallet) ?? false) ? wallet : null;
    internal static bool IsWallet(int walletId, out IoWallet? wallet) => (wallet = TryWallet(walletId)) != null;
    internal static Dictionary<int, IoAccount>? GetAccounts => GetWallet?.Accounts ?? null;
    internal static IoAccount? GetAccount => SApp.GetAccount;
    internal static IoAccount? TryAccount(int accId) => (GetAccounts?.TryGetValue(accId, out var account) ?? false) ? account : null;
    internal static bool IsAccount(int accId, out IoAccount? account) => (account = TryAccount(accId)) != null;
    internal static IoTransaction? TryTransaction(string txId) => (GetNetwork?.Tx.Tx.TryGetValue(txId, out var tx) ?? false) ? tx : null;
    internal static bool IsTransaction(string txId, out IoTransaction? tx) => (tx = TryTransaction(txId)) != null;
    internal static IoTxo? TryTxo((string TxId, int Vout) txoId) => (GetNetwork?.Txo.TryGetValue(txoId, out var tx) ?? false) ? tx : null;
    internal static bool IsTxo((string TxId, int Vout) txoId, out IoTxo? txo) => (txo = TryTxo(txoId)) != null;
    internal static DisplayMode GetDisplayMode => (GetProfile?.DisplayMode ?? DPro.DisplayMode);
    #endregion

    #region Directories
    internal static string GetRootSaveDir() {
        string baseDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "IoListWallet");
        if(!App.DisableSaving) 
            Directory.CreateDirectory(baseDir);
        return baseDir;
    }
    internal static string GetProfileDir() => Path.Combine(GetRootSaveDir(), SApp.Profile.ToString());
    internal static string GetNetworkDir() => GetNetworkDir(SApp.GetProfile?.Network);
    internal static string GetNetworkDir(IoNetwork? network) => GetNetworkDir(network?.GetType());
    internal static string GetNetworkDir(Type? network) => Path.Combine(GetProfileDir(), network?.Name ?? "-1");
    internal static string GetWalletDir() => Path.Combine(GetNetworkDir(), SApp.GetNetwork?.Wallet.ToString() ?? "-1");
    internal static string GetWalletDir(int walId) => Path.Combine(GetNetworkDir(), walId.ToString() ?? "-1");
    #endregion

    #region Load Calls
    /// <summary>
    /// Looks up profiles and loads the selected one.
    /// </summary>
    /// <returns>If successfully loaded some profile</returns>
    internal static IoProfile? LoadProfiles() {
        string profilesDir = GetRootSaveDir();
        string profilePath = Path.Combine(profilesDir, "app.json");
        if (File.Exists(profilePath) && DeserializeObject(typeof(IoApp), File.ReadAllText(profilePath)) is IoApp sapp)
            SApp = sapp;
        string[] folders = Directory.GetDirectories(profilesDir);
        int last = -1;
        foreach (var folder in folders) // load profiles, try to fully load the selected one, remember a last successful profile
            if (int.TryParse(Path.GetFileName(folder), out var parse) && LoadProfile(parse, true, true) != null)
                last = parse; 
        // if we failed to load the selected, try to load the last successful
        return SApp.Profiles.ContainsKey(SApp.Profile) ? GetProfile : LoadProfile(Math.Max(0, last), false, true); 
    }
    internal static bool IsCorrectPassword() {
        if (GetProfile is not IoProfile profile)
            return false;
        string path = GetProfileDir();
        if (!App.DisableSaving) 
            Directory.CreateDirectory(path);
        var pathf = Path.Combine(path, "password.dat");

        if (!File.Exists(pathf)) {
            profile.Password = "";
            return true;
        }
        return ReadFile(pathf, profile.Password) is string s ? s == "password" : false;
    }
    internal static object? GetFirst(IEnumerable<object> e) {
        var enumerator = e.GetEnumerator();
        return enumerator.MoveNext() ? enumerator.Current : null;
    }
    /// <summary>
    /// Loads a profile and optionally everything with it
    /// </summary>
    /// <param name="profileName">name of the save folder</param>
    /// <param name="batch">only load it's existence and settings, also when true and SApp.Profile is already this id</param>
    /// <param name="init">onlytrue if called fro LoadProfiles!</param>
    /// <returns>selected profile</returns>
    internal static IoProfile? LoadProfile(int profileId, bool batch = false, bool init = false) {
        if (profileId < 0)
            return null; // negative is invalid, it's also the initial value of no profile
        if (profileId == SApp.Profile && !batch)
            return GetProfile; // already loaded
        string path = Path.Combine(GetRootSaveDir(), profileId.ToString());
        if (!App.DisableSaving)
            Directory.CreateDirectory(path);
        var pathf = Path.Combine(path, "profile.json");
        // if already have a profile...
        if ((IsProfile(profileId, out var p) ? SavePro(p, profileId) // ...save it and try reloading it's unloaded children (also saves if they already loaded too)
            : File.Exists(pathf) ? DeserializeObject(typeof(IoProfile), File.ReadAllText(pathf)) // actually load it
            : SavePro(new IoProfile(), profileId) // Doesn't exist -> create it
        ) is IoProfile profile) {
            (SApp.Profiles[profileId] = profile).Dirty = false; // Mark synced, and link directories
            if (batch && profileId != SApp.Profile)
                return profile;
            // Load Locale:
            if (LocaleData.Count == 0)
                throw new("No Locales found!");
            Locale = LocaleData.TryGetValue(profile.Locale, out var found)
                ? found : LocaleData[profile.Locale = (string?)GetFirst(LocaleData.Keys) ?? ""];
            SubLocale = SubLocaleData.TryGetValue(profile.SubLocale, out var foundSub)
                ? foundSub : SubLocaleData[profile.Locale = (string?)GetFirst(SubLocaleData.Keys) ?? ""];
            if (!init) // We are reloading, so instead of initializing menu, we just close everything else and go back to menu
                ResetToHome(profile, profile.DisplayMode); 
            SApp.Profile = profileId; // link pointers
            if (AppMainMenu is MainMenu m)
                m.initProfile = m.initLogin = m.initNetwork = m.initWallet = m.initAccount = true;
            SaveApp();
            void InitMenu() {
                if (init)
                    App.StartApp(); // Initializes and opens the special MainMenu panel
                else if (AppMainMenu != null)
                    App.Register(AppMainMenu.GetK, AppMainMenu); // registers the menu, because we dind't do so in StartApp
            }
            if (IsCorrectPassword()) {
                profile.CorrectPassword = true;
                LoadNetworks(); // Loads networks, (and wallets, and txs for the selected one)
                InitMenu();
                LoadPanels();   // Loads panels, pinneds, and windows
            } else {
                InitMenu(); // Just Init, because we are locked out, the user will have to input correct password to load networks and panels etc
            }
            App.RefreshAll();
            //AppMainPanel?.Update();
            return profile;
        }
        // Fail:
        if (profileId == SApp.Profile)
            SApp.Profile = -1;
        SApp.Profiles.Remove(profileId);
        return null;
    }
    internal static IoProfile? LoadPassword(bool init = false) {
        if (GetProfile is not IoProfile profile)
            return null;
        if (IsCorrectPassword()) {
            profile.CorrectPassword = true;
            LoadNetworks(); // loads networks, (and wallets, and txs for the selected one)
            if (init)
                App.StartApp();
            LoadPanels();   // loads panels, pinneds, and windows
        } else
            if (init)
                App.StartApp();
        App.RefreshAll();
        return profile;
    }
    /// <summary>
    /// Loads all networks, called in public after submitting decryption password
    /// </summary>
    /// <returns>selected network</returns>
    internal static IoNetwork? LoadNetworks() {
        if (GetProfile is not IoProfile profile)
            return null; // can't load network without a profile
        var path = GetProfileDir();
        var folders = Directory.Exists(path) ? Directory.GetDirectories(path) : [];
        Type? last = null;
        foreach (var folder in folders) // load networks, try to fully load the selected one, remember a last successful network
            if (Networks.TryGetValue(Path.GetFileName(folder), out var parsedType) && LoadNetwork(parsedType, true) != null)
                last = parsedType; 
        // if we failed to load the selected, try to load the last successful
        return (profile.Network is Type loadedType && profile.Networks.ContainsKey(loadedType)) ? GetNetwork : LoadNetwork(last);
    }
    /// <summary>
    /// Loads a Network and all wallets and transactions with it
    /// </summary>
    /// <param name="network">which network to load</param>
    /// <param name="batch">only load it's existence and settings, also when true and GetProfile.Network is already this id</param>
    /// <returns>selected network</returns>
    internal static IoNetwork? LoadNetwork(Type? _netId, bool batch = false) {
        if (GetProfile is not IoProfile profile || _netId  is not Type netId)
            return null;
        if (_netId == profile.Network && !batch)
            return GetNetwork; // already loaded
        string path = GetProfileDir();
        if (!App.DisableSaving)
            Directory.CreateDirectory(path);
        path = Path.Combine(path, netId.Name);
        if (!App.DisableSaving)
            Directory.CreateDirectory(path);
        var pathf = Path.Combine(path, "network.json");
        // If alredy loaded...
        if ((IsNetwork(_netId, out var n) ? SaveNet(n, _netId) // ...save it and try reloading it's unloaded children (also saves if they already loaded too) 
            : File.Exists(pathf) ? DeserializeObject(_netId, ReadFile(pathf, profile.Password)) // actually load it
            : SaveNet(Activator.CreateInstance(_netId) as IoNetwork, _netId) // Doesn't exist -> create it
        ) is IoNetwork network) {
            (profile.Networks[_netId] = network).Dirty = false; // Mark synced, and link directiories
            if (batch && _netId != profile.Network)
                return network;
            (network.GetProfile = profile).Network = _netId;  // link pointers
            if (AppMainMenu is MainMenu m)
                m.initNetwork = m.initWallet = m.initAccount = true;
            LoadWallets();      // loads wallets, with accounts
            LoadTransactions(); // loads transactions
            return network;
        }
        // Fail:
        if (_netId == profile.Network)
            profile.Network = null;
        profile.Networks.Remove(_netId);
        return null;
    }
    private static IoWallet? LoadWallets() {
        if (GetNetwork is not IoNetwork network)
            return null; // can't load wallet without a network
        var netDir = GetNetworkDir();
        if (!Directory.Exists(netDir))
            return null;
        var folders = Directory.GetDirectories(netDir);
        int last = -1;
        foreach (var folder in folders) // load wallets, try to fully load the selected one, remember a last successful wallet
            if (int.TryParse(Path.GetFileName(folder), out var parsedId) && LoadWallet(parsedId, true) != null)
                last = parsedId;
        var loadedId = network.Wallet; // if we failed to load the selected, try to load the last successful
        return network.Wallets.ContainsKey(loadedId) ? GetWallet : LoadWallet(last);
    }
    /// <summary>
    /// Loads a wallet, with accounts
    /// </summary>
    /// <param name="walletId">which wallet to load</param>
    /// <param name="batch">true - just load it for selection later, false - fully load it, wallet should be SApp.Wallet</param>
    /// <returns>selected wallet</returns>
    internal static IoWallet? LoadWallet(int walletId, bool batch = false) {
        if (GetNetwork is not IoNetwork network || walletId < 0)
            return null; // null is invalid
        if (walletId == network.Wallet && !batch)
            return GetWallet; // already loaded
        string path = GetNetworkDir();
        if (!App.DisableSaving)
            Directory.CreateDirectory(path);
        var pathf = Path.Combine(path, walletId.ToString() + ".json");
        // If alredy loaded...
        if ((IsWallet(walletId, out var w) ? SaveWal(w, walletId) // ...save it and try reloading it's unloaded children (also saves if they already loaded too)
            : File.Exists(pathf) ? DeserializeObject(network.WalletType(), File.ReadAllText(pathf)) // actually load it
            : SaveWal(Activator.CreateInstance(network.WalletType()) as IoWallet, walletId) // Doesn't exist -> create it
        ) is IoWallet wallet) {
            (network.Wallets[walletId] = wallet).Dirty = false; // Mark synced, and link directories
            if (batch && walletId != network.Wallet)
                return wallet;
            (wallet.GetNetwork = network).Wallet = walletId; // link pointers
            if (AppMainMenu is MainMenu m)
                m.initWallet = m.initAccount = true; // TODO also switch on initAccount when selecting an account, that's not a file load
            return wallet;
        }
        // Fail:
        if (walletId == network.Wallet)
            network.Wallet = -1;
        network.Wallets.Remove(walletId);
        return null;
    }
    /// <summary>
    /// Loads a wallet in AppNetwork
    /// </summary>
    /// <param name="first">Is this the first batch of transactions to be loaded? Or some update?</param>
    /// <param name="overrides">true - loads all tranasctions from Tx/Tx.json. false - loads block files incrementally from Chain/<Index>.json, as they get added</param>
    /// <returns>success</returns>
    internal static bool LoadTransactions(bool first = false, bool overrides = true) {
        if (GetProfile is not IoProfile profile || GetNetwork is not IoNetwork network)
            return false;
        string path = overrides ? GetNetworkDir() : Path.Combine(GetNetworkDir(), "Chain");
        bool _LoadTransactions() { // Try load Transactions
            if (!App.DisableSaving)
                Directory.CreateDirectory(path);
            var pathf = Path.Combine(path, overrides ? "tx.json" : network.Blocks.ToString() + ".json");
            if (network.Tx != null) {
                SaveTx(network);
                return true;
            }
            if (!File.Exists(pathf) // Doesn't exist or failed to raed -> exit
                || DeserializeObject(typeof(IoNetwork.History), ReadFile(pathf, profile.Password)) is not IoNetwork.History Txs) 
                return false;
            network.Tx ??= new();
            network.Blocks = overrides ? 0 : network.Blocks + 1;
            void LoadTx(string TxId, IoTransaction Tx) {
                // Unload Account Utxos from RAM into inputs:
                if (GetWallets is Dictionary<int, IoWallet> wallets) {
                    foreach (var txoId in Tx.Inputs) {
                        //var iTx = TryTransaction(TxId);
                        var addrId = TryTxo(txoId)?.Address.GetAddressString() ?? "";
                        foreach (var wallet in wallets)
                            foreach (var account in wallet.Value.Accounts)
                                if (account.Value.Addresses.ContainsKey(addrId))
                                    //(a.Value.Utxos[addrId] = iTxo).Spent = true;// we found our utxo used as input, mark it spent // else not mine
                                    account.Value.Utxos.Remove(txoId); // remove from available utxos
                    }
                    // Load Account Utxos from outputs into RAM:
                    foreach (IoTxo txo in Tx.Outputs) {
                        network.Txo[txo.Id] = txo; // Also, add txo to network dictionary
                        var addrId = txo.Address.GetAddressString();
                        if (wallets != null) foreach (var wallet in wallets)
                            foreach (var account in wallet.Value.Accounts)
                                if (account.Value.Addresses.ContainsKey(addrId))  // it's ours
                                    if (!account.Value.Utxos.ContainsKey(txo.Id)) // add utxo, since we don't have any recond about spending this yet
                                        account.Value.Utxos[txo.Id] = txo;
                    }
                }
            }
            // overrides will clear the transaction and reload them completely
            if (overrides) {
                network.Tx.Tx = [];
                network.Txo = [];
                if (GetWallets is Dictionary<int, IoWallet> wallets)
                    foreach (var wallet in wallets)
                        foreach (var account in wallet.Value.Accounts) {
                            account.Value.Addresses = [];
                            account.Value.Utxos = [];
                        }
            }
            // Process transactions:
            if (network.Tx.Tx.Count == 0) 
                foreach (var (K, V) in network.Tx.Tx = Txs.Tx) LoadTx(K, V);
            else foreach (var (K, V) in Txs.Tx) LoadTx(K, network.Tx.Tx[K] = V); 
            return true;
        }
        if (_LoadTransactions()) { // was at least one block?
            if(!overrides)
                while (_LoadTransactions()) ; // read all blocks
            network.Tx?.Dirty = false; // Mark synced
            return true;
        }
        // Fail:
        return false;
    }
    /// <summary>
    /// Loads Panels, Pinneds, and Windows
    /// </summary>
    internal static void LoadPanels() {
        if (GetProfile is not IoProfile profile)
            return; // null is invalid
        string path = GetProfileDir();
        void _LoadPan() { // Try load Panels
            if (!App.DisableSaving)
                Directory.CreateDirectory(path);
            var pathf = Path.Combine(path, "panel.json");
            if (!File.Exists(pathf)) { // Doesn't exist -> create it
                SavePan();
                return;
            }
            if (DeserializeObject(typeof(IoProfile.SaveFilePan), ReadFile(pathf, profile.Password)) is not IoProfile.SaveFilePan Panels)
                return;
            /*void _LoadPan(){
                // Menu is excluded from loading (though I am alrady excluding it in SAving, but just to be sure)
                Panels.LoadedPanels.Remove(IoPanel.MakeK(typeof(MainMenu), ""));
                // Sync Panels:
                List<string> L = [];
                var LoadedPanels = profile.Pan.LoadedPanels;
                foreach (var P in LoadedPanels)
                    if (!Panels.LoadedPanels.ContainsKey(P.Key))
                        L.Add(P.Key);
                foreach (var P in L) {
                    if (IoPanel.SplitK(P) is (Type, string) key) {
                        if (key.Item1 != typeof(MainMenu))
                            LoadedPanels.Remove(P);
                    } else LoadedPanels.Remove(P);
                }
                foreach (var P in Panels.LoadedPanels)
                    if (!LoadedPanels.ContainsKey(P.Key))
                        LoadedPanels[P.Key] = P.Value;
            }
            _LoadPan();
            void _LoadPin() {
                if (Panels.PinnedPanels.Count == 0)
                    return;
                // Sync Panels:
                List<string> L = [];
                var PinnedPanels = profile.Pan.PinnedPanels;
                foreach (var P in PinnedPanels)
                    if (!Panels.PinnedPanels.Contains(P))
                        L.Add(P);
                foreach (var P in L)
                    PinnedPanels.Remove(P);
                foreach (var P in Panels.PinnedPanels)
                    if (!PinnedPanels.Contains(P))
                        PinnedPanels.Add(P);
                AppMainMenu?.GetMyMainPanel()?.Update();
            }
            _LoadPin();
            void _LoadWin() {
                if (Panels.OpenedWindows.Count == 0)
                    return;

                // TODO implement OpenedWindows in general
                // TODO make sure a proper load / unload / closing / opening happens, maybe use the SetDisplayMode code

                // Sync Panels:
                List<int> L = [];
                var OldOpenedWindows = profile.Pan.OpenedWindows;
                foreach (var P in OldOpenedWindows)
                    if (!Panels.OpenedWindows.ContainsKey(P.Key))
                        L.Add(P.Key); // if opened isn't loaded, add to to the removal list
                foreach (var P in L)
                    OldOpenedWindows.Remove(P);
                foreach (var P in Panels.OpenedWindows) // if loaded isn't opened, then put in in the list, excluding main menu (though main menu should alraedy exist, so this second cond is probably redundant)
                    if (!OldOpenedWindows.ContainsKey(P.Key))// && ((IoDockPanel)P.Value.Dock.Dock.TopNonRoot).Panel != AppMainMenu?.GetK)
                        OldOpenedWindows[P.Key] = P.Value;
                // open them in th correct order:
                bool opened;
                do {
                    opened = false;
                    foreach (var w in OldOpenedWindows) {
                        if (!w.Value.IsVisible && w.Value.MyMainPanel.Dock != AppDockRoot && (TryWindow(w.Value.OpenedFrom) is not IoWindow par || par.IsVisible)) {
                            w.Value.Show();
                            w.Value.IsVisible = opened = true;
                        }
                    }
                } while (opened);
                // Remove unopened ones:
                L.Clear();
                foreach (var w in OldOpenedWindows)
                    if (!w.Value.IsVisible)
                        L.Add(w.Key);
                foreach (var w in L)
                    OldOpenedWindows.Remove(w);
                
            }
            _LoadWin();*/
        }
        _LoadPan();
    }
    #endregion

    #region Json
    [AttributeUsage(AttributeTargets.Property)]
    internal class SaveAttribute : Attribute { }
    private static Func<object, object> CreateGetter(PropertyInfo prop) {
        var objParam = Expression.Parameter(typeof(object));
        return Expression.Lambda<Func<object, object>>(Expression.Convert(
            Expression.Property(Expression.Convert(objParam, prop.DeclaringType!), prop), 
            typeof(object)), objParam).Compile();
    }
    private static Action<object, object?> CreateSetter(PropertyInfo prop) {
        var obj = Expression.Parameter(typeof(object));
        var value = Expression.Parameter(typeof(object));
        return Expression.Lambda<Action<object, object?>>(Expression.Call(
            Expression.Convert(obj, prop.DeclaringType!), 
            prop.SetMethod!, 
            Expression.Convert(value, prop.PropertyType)
            ), obj, value).Compile();
    }
    private static Func<object, object> CreateGetter(FieldInfo field) {
        var objParam = Expression.Parameter(typeof(object));
        return Expression.Lambda<Func<object, object>>(Expression.Convert(
            Expression.Field(Expression.Convert(objParam, field.DeclaringType!), field),
            typeof(object)), objParam).Compile();
    }
    private static Action<object, object?> CreateSetter(FieldInfo field) {
        var obj = Expression.Parameter(typeof(object));
        var value = Expression.Parameter(typeof(object));
        return Expression.Lambda<Action<object, object?>>(Expression.Assign(
            Expression.Field(Expression.Convert(obj, field.DeclaringType!), field),
            Expression.Convert(value, field.FieldType)
        ), obj, value).Compile();
    }
    private class SaveMember {
        public string Name = "";
        public Func<object, object?> Getter = null!;
        public Action<object, object?> Setter = null!;
        public Type Type = null!;
    }
    private static readonly Dictionary<Type, List<SaveMember>> SaveCache = [];
    private static List<SaveMember> GetSaveMembers(Type type) {
        if (SaveCache.TryGetValue(type, out var list))
            return list;
        list = [];
        foreach (var prop in type.GetProperties(
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
            if (prop.GetCustomAttribute<SaveAttribute>() != null)
                list.Add(new SaveMember {
                    Name = prop.Name,
                    Type = prop.PropertyType,
                    Getter = CreateGetter(prop),
                    Setter = CreateSetter(prop)
                });
        foreach (var field in type.GetFields(
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)) {
            if (field.GetCustomAttribute<SaveAttribute>() != null)
                list.Add(new SaveMember {
                    Name = field.Name,
                    Type = field.FieldType,
                    Getter = CreateGetter(field),
                    Setter = CreateSetter(field)
                });
        }
        return SaveCache[type] = list;
    }
    private static object? DeserializeObject(Type type, string? json) 
        => (json != null && JsonNode.Parse(json) is JsonObject node) ? DeserializeObject(type, node) : default;
    private static bool IsSaveType(Type t) => GetSaveMembers(t).Count > 0;
    private static object? DeserializeValue(Type type, JsonNode? node) {
        if (node == null)
            return null;
        if (type == typeof(Color)) {
            var t = node.GetType();
            if (node is JsonObject obj) {
                byte a = (byte)obj["A"]!.GetValue<int>();
                byte r = (byte)obj["R"]!.GetValue<int>();
                byte g = (byte)obj["G"]!.GetValue<int>();
                byte b = (byte)obj["B"]!.GetValue<int>();

                return new Color(a, r, g, b);
            }

            return null;
        }

        if (IsSaveType(type))
            return node is JsonObject obj ? DeserializeObject(type, obj) : null;
        if (type.IsArray) {
            var elemType = type.GetElementType()!;
            var arrNode = node.AsArray();
            var arr = Array.CreateInstance(elemType, arrNode.Count);
            for (int i = 0; i < arrNode.Count; i++) 
                arr.SetValue(DeserializeValue(elemType, arrNode[i]), i);
            return arr;
        }
        if (type.IsGenericType) {
            var gen = type.GetGenericTypeDefinition();
            // Dictionary<string,T>
            if (gen == typeof(Dictionary<,>)) {
                var args = type.GetGenericArguments();
                var valType = args[1];
                var dict = (IDictionary)Activator.CreateInstance(type)!;
                if (args[0] == typeof(string)) {
                    foreach (var kv in node.AsObject()) { // special handling for IoPanel
                        if ((typeof(IoPanel).IsAssignableFrom(valType)
                            ? IoPanel.SplitK(kv.Key) is (Type Type, string) key
                                ? key.Type == typeof(MainMenu)
                                    ? null // ListMainMenu is ignored, that is always spawned separately in App.StartApp()
                                    : DeserializeObject(key.Type, kv.Value?.AsObject())
                                : null // fail - invalid key
                            : DeserializeValue(valType, kv.Value)
                        ) is object panel)
                            dict[kv.Key] = panel;
                    }
                    return dict;
                }
                if (args[0] == typeof(int))
                    foreach (var kv in node.AsObject()) // special handling for non-IoPanel (like IoWindow)
                        if (int.TryParse(kv.Key, out var key) && DeserializeObject(valType, kv.Value?.AsObject()) is object obj)
                            dict[key] = obj;
                return dict;
            }
            // List<T>
            if (gen == typeof(List<>)) {
                var elemType = type.GetGenericArguments()[0];
                var list = (IList)Activator.CreateInstance(type)!;
                foreach (var item in node.AsArray())
                    // list.Add(DeserializeValue(elemType, item));
                    if ((typeof(FilterType).IsAssignableFrom(elemType) && item is JsonObject obj
                        ? obj.TryGetPropertyValue("__type", out var _tnode)
                            ? App.ATypes.TryGetValue(_tnode!.GetValue<string>(), out var _realType)
                                ? DeserializeObject(_realType, obj) : null
                            : DeserializeObject(elemType, obj)
                        : DeserializeValue(elemType, item)
                    ) is object _o) list.Add(_o);
                return list;
            }
        }
        // IoDock type
        return node is JsonObject o
            && typeof(IoDock).IsAssignableFrom(type)
            && o.TryGetPropertyValue("__type", out var tnode)
            && App.ATypes.TryGetValue(tnode!.GetValue<string>(), out var realType)
            ? DeserializeObject(realType, o) : node.Deserialize(type);
    }
    private static object? DeserializeObject(Type type, JsonObject? node) {
        if (node == null || Activator.CreateInstance(type) is not object obj)
            return null;
        foreach (var m in GetSaveMembers(type))
            if (node.TryGetPropertyValue(m.Name, out var j))
                m.Setter(obj, DeserializeValue(m.Type, j));
        return obj;
    }
    private static JsonNode? SerializeValue(object? value) {
        if (value == null)
            return null;
        var type = value.GetType();
        if (type == typeof(MainMenu))
            return null;
        /*if (IsTuple(type))            
            return JsonSerializer.SerializeToNode(value, tupleOptions);*/
        if (IsSaveType(type))
            return SerializeObject(value);
        // Dictionary
        if (value is IDictionary dict) {
            var obj = new JsonObject();
            foreach (var key in dict.Keys)
                obj[key!.ToString()!] = SerializeValue(dict[key]);
            return obj;
        }
        // List / array
        if (value is IEnumerable list && value is not string) {
            var arr = new JsonArray();
            foreach (var item in list)
                arr.Add(SerializeValue(item));
            return arr;
        }
        // primitive fallback
        return JsonSerializer.SerializeToNode(value);
    }
    private static JsonObject SerializeObject(object obj) {
        var json = new JsonObject();
        if (obj is FilterType || obj is IoDock)
            json["__type"] = obj.GetType().Name;
        foreach (var m in GetSaveMembers(obj.GetType()))
            json[m.Name] = SerializeValue(m.Getter(obj));
        return json;
    }
    private static byte[] EncryptV1(string json, string password) {
        using var ms = new MemoryStream();
        byte[] salt = RandomNumberGenerator.GetBytes(16),
            nonce = RandomNumberGenerator.GetBytes(12),
            key = new byte[32];
        Rfc2898DeriveBytes.Pbkdf2(password, salt, key, 200000, HashAlgorithmName.SHA256);
        const byte tagSize = 16;
        byte[] plaintext = Encoding.UTF8.GetBytes(json),
            ciphertext = new byte[plaintext.Length],
            tag = new byte[tagSize];
        using var aes = new AesGcm(key, tagSize);
        aes.Encrypt(nonce, plaintext, ciphertext, tag);
        ms.Write(Encoding.ASCII.GetBytes("1"));
        ms.Write(salt);
        ms.Write(nonce);
        ms.Write(tag);
        ms.Write(ciphertext);
        return ms.ToArray();
    }

    private static string? DecryptV1(byte[] data, string password) {
        if (Encoding.ASCII.GetString(data, 0, 1) != "1")
            return null;
        byte tagSize = 16;
        int i = 1;
        byte[] salt = data[i..(i += 16)], 
            nonce = data[i..(i += 12)], 
            tag = data[i..(i += tagSize)], 
            ciphertext = data[i..], 
            key = new byte[32];
        Rfc2898DeriveBytes.Pbkdf2(password, salt, key, 200000, HashAlgorithmName.SHA256);
        byte[] plaintext = new byte[ciphertext.Length];
        try {
            using var aes = new AesGcm(key, tagSize);
            aes.Decrypt(nonce, ciphertext, tag, plaintext);
            return Encoding.UTF8.GetString(plaintext);
        } catch { }
        return null; // wrong password or corrupted file
    }
    private static readonly JsonSerializerOptions JsonIndented = new() { WriteIndented = true };
    private static readonly JsonSerializerOptions JsonRaw = new() { WriteIndented = false };
    private static void WriteJson(object o, string path, string password = "") {
        if (password == "")
            WriteFile(SerializeObject(o).ToJsonString(JsonIndented), path, password);
        else WriteFile(SerializeObject(o).ToJsonString(JsonRaw), password);
    }

    private static void WriteFile(string o, string path, string password = "", string type = ".json") {
        var temp = (path = path + type) + ".tmp";
        if(File.Exists(path))
            File.Move(path, path + ".bak", true);
        if (password == "")
            File.WriteAllText(temp, o);
        else File.WriteAllBytes(temp, EncryptV1(o, password));
        if (File.Exists(temp))
            File.Move(temp, path, true);
    }
    private static string? ReadFile(string path, string password = "") {
        byte[] data = File.ReadAllBytes(path);
        return Encoding.ASCII.GetString(data, 0, 1) switch {
            "1" => DecryptV1(data, password), // version 1: AesGcm
            "{" => Encoding.UTF8.GetString(data),// old plaintext JSON (no password)
            _ => throw new Exception("Unknown file format"),
        };
    }
    #endregion

    #region Save
    internal static void SaveAll(bool forced = false) {
        if (GetProfile is not IoProfile profile)
            return;
       
        var originalNetwork = GetNetwork;
        var originalNetId = profile.Network;
        var origWallet = GetWallet;
        var origWalletId = originalNetwork?.Wallet;
        originalNetwork?.Wallet = -1;
        profile.Network = null;
        foreach (var n in profile.Networks) if (n.Key != originalNetId) {
            if (forced) {
                n.Value.Tx?.Dirty = n.Value.Dirty = true;
                foreach (var w in n.Value.Wallets)
                    w.Value.Dirty = true;
            }
            LoadNetwork(n.Key); // re-save unselected networks
            SaveTx(n.Key); // re-save transactions on unselected networks
        }
        // reload back and save the original selected network.
        if (originalNetwork != null && originalNetId != null) {
            if (forced) {
                originalNetwork.Tx?.Dirty = originalNetwork.Dirty = true;
                foreach (var w in originalNetwork.Wallets)
                    w.Value.Dirty = true;
            }
            LoadNetwork(originalNetId); // reload and save original network
            LoadWallet(origWalletId ?? -1); // reselect the original wallet
            SaveTx(originalNetId); // re-save transactions on the original network
        }
    }
    internal static IoApp SaveApp() => SaveApp(SApp);
    internal static IoApp SaveApp(IoApp app) {
        if (App.DisableSaving)
            return app;
        while (app.Dirty) {
            app.Dirty = false;
            WriteJson(SApp, Path.Combine(GetRootSaveDir(), "app"));
            /*var path = Path.Combine(GetRootSaveDir(), "app.txt");
            var temp = path + ".tmp"; // IoListWallet/app.txt
            File.WriteAllText(temp, SApp.Profile.ToString());
            File.Move(temp, path, true);*/
        }
        return app;
    }
    internal static void SavePassword(string password) {
        if (App.DisableSaving)
            return;
        string pathf = Path.Combine(GetProfileDir(), "password");
        if (password != "")
            WriteFile("password", pathf, password, ".dat");
        else if (File.Exists(pathf += ".dat"))
            File.Delete(pathf);
    }

    internal static IoProfile? SavePro() => SavePro(GetProfile, SApp.Profile);
    internal static IoProfile? SavePro(IoProfile? profile, int profileId) {
        if (App.DisableSaving)
            return profile;
        while (profile?.Dirty ?? false) {
            profile.Dirty = false; // IoListWallet/<profileId>/profile.json
            WriteJson(profile, Path.Combine(Path.Combine(GetRootSaveDir(), profileId.ToString()), "profile"));
        }
        return profile;
    }

    internal static IoNetwork? SaveNet() => SaveNet(GetNetwork, GetProfile?.Network);
    internal static IoNetwork? SaveNet(Type netId) 
        => (SApp.GetProfile?.Networks.TryGetValue(netId, out var network) ?? false) 
        ? SaveNet(network, netId) : null;
    internal static IoNetwork? SaveNet(IoNetwork? network, Type? netId) {
        if (netId == null || App.DisableSaving)
            return network;
        while (network?.Dirty ?? false) {
            network.Dirty = false; // IoListWallet/<profileId>/<netId>/network.json
            WriteJson(network, Path.Combine(Path.Combine(GetProfileDir(), netId.Name), "network"), GetProfile?.Password ?? "");
        }
        return network;
    }

    internal static IoWallet? SaveWal() => SaveWal(GetWallet, GetNetwork?.Wallet ?? -1);
    internal static IoWallet? SaveWal(int walletId)
       => (GetNetwork?.Wallets.TryGetValue(walletId, out var wallet) ?? false)
        ? SaveWal(wallet, walletId) : null;
    internal static IoWallet? SaveWal(Type netId, int walletId)
        => (GetProfile?.Networks.TryGetValue(netId, out var network) ?? false) 
        && (network?.Wallets.TryGetValue(walletId, out var wallet) ?? false) 
        ? SaveWal(wallet, walletId) : null;
    internal static IoWallet? SaveWal(IoWallet? wallet, int walletId) {
        if (App.DisableSaving)
            return wallet;
        while (wallet?.Dirty ?? false) {
            wallet.Dirty = false; // IoListWallet/<profileId>/<netId>/<walletId>.json
            WriteJson(wallet, Path.Combine(Path.Combine(GetProfileDir(), wallet.MyNetwork.Name), walletId.ToString()), GetProfile?.Password ?? "");
        }
        return wallet;
    }

    internal static IoNetwork? SaveTx() => SaveTx(SApp.GetNetwork);
    internal static IoNetwork? SaveTx(Type netId) 
        => (GetProfile?.Networks.TryGetValue(netId, out var network) ?? false)
        ? SaveTx(network) : null;
    internal static IoNetwork? SaveTx(IoNetwork? network) {
        if (App.DisableSaving)
            return network;
        while (network?.Tx?.Dirty ?? false) {
            network.Tx.Dirty = false; // TODO IoListWallet/<ProfileId>/<NetworkId>/tx.json
            WriteJson(network.Tx, Path.Combine(Path.Combine(GetProfileDir(), network.GetType().Name), "tx"), GetProfile?.Password ?? "");
        }
        return network;
    }
    internal static bool SavePan() {
        if (App.DisableSaving)
            return false;
        if (GetProfile?.Pan.Dirty ?? false) {
            GetProfile.Pan.Dirty = false; // <ProfileId>/panel.json
            WriteJson(GetProfile.Pan, Path.Combine(GetProfileDir(), "panel"), GetProfile?.Password ?? "");
        }
        return true;
    }
    #endregion

    internal static void FindLocales() {
        var subPath = Path.Combine(AppContext.BaseDirectory, "LocalesSub");
        Directory.CreateDirectory(subPath);
        foreach (var file in Directory.GetFiles(subPath, "*.json")) {
            var code = Path.GetFileNameWithoutExtension(file);
            if (JsonSerializer.Deserialize<Dictionary<string, string>>(File.ReadAllText(file)) is Dictionary<string, string> parsed) {
                SubLocaleData[code] = parsed;
                SubLocaleNames[code] = parsed["ThisLanguage"];
            }
        }
        var localesPath = Path.Combine(AppContext.BaseDirectory, "Locales");
        Directory.CreateDirectory(localesPath);
        foreach (var file in Directory.GetFiles(localesPath, "*.json")) {
            var code = Path.GetFileNameWithoutExtension(file);
            if (JsonSerializer.Deserialize<Dictionary<string, string>>(File.ReadAllText(file)) is Dictionary<string,string> parsed) {
                LocaleData[code] = parsed;
                LocaleNames[code] = parsed["ThisLanguage"];
            }
        }
    }
    internal static void ResetToHome() 
        => ResetToHome(GetProfile, GetDisplayMode);
    internal static void ResetToHome(IoProfile? updateProfile, DisplayMode mode) {
        if (GetProfile is not IoProfile profile)
            return;
        // Undock everything except MainMenu
        bool closed;
        do {
            closed = false;
            foreach (var p in profile.Pan.LoadedPanels.Values)
                if (p is not MainMenu)
                    if (p.IsVisible) {
                        p.Close();
                        closed = true;
                    }
        } while (closed);
        bool WasFullscreen = profile.DisplayMode == DisplayMode.Fullscreen,
            WillFullscreen = mode == DisplayMode.Fullscreen;
        updateProfile?.DisplayMode = mode;
        updateProfile?.Pan.Dirty = true;
        if (AppMainPanel != null) {
            AppMainPanel.Update();
            if (WasFullscreen && !WillFullscreen) {
                // Switch from fullscreen to window
                var oldWindow = AppMainPanel.Parent as Window;
                oldWindow?.Content = null;
                // spawn window and dock main panel into it:
                AppMainWindow = new IoWindow {
                    Content = AppMainPanel
                };
                AppMainPanel.MyWindowK = AppMainWindow.Index;
                AppMainWindow.Show();
                oldWindow?.Close();
                AppDockRoot = (AppMainPanel = AppMainWindow.MyMainPanel).Dock;
                
            }
            if (!WasFullscreen && WillFullscreen) {
                // Switch from window to fullscreen
                var oldWindowK = AppMainPanel.MyWindowK;
                var oldWindow = TryWindow(oldWindowK);
                oldWindow?.Content = null;
                AppMainPanel.MyWindowK = -1;
                AppMainWindow = null;
                var fullscreenWindow = new Window {
                    Content = AppMainPanel,
                    WindowState = WindowState.FullScreen,
                    CanResize = false,
                };
                fullscreenWindow.Show();
                oldWindow?.IsClosing = true;
                oldWindow?.Close();
            }
        }
        AppDockRoot?.Original = AppMainMenu?.GetK ?? "";
        AppMainWindow?.SystemDecorations = profile.DisplayMode == DisplayMode.MultiWindow ? SystemDecorations.BorderOnly : SystemDecorations.Full;
        App.RefreshPanel(AppMainMenu, 1, true);
    }

    internal static ulong GetBit(ulong mask, int S) => mask & ((ulong)1 << S);

    /*internal static IoProfile? SetProfile(int profileId) {
        if (!SApp.Profiles.TryGetValue(profileId, out var profile) || profile == null)
            return LoadProfile(profileId);
        SApp.Profile = profileId; // already exists, just load it
        return GetProfile;
    }
    internal static IoNetwork? SetNetwork(Type netId) {
        if (GetProfile is not IoProfile profile)
            return null;
        if (!profile.Networks.TryGetValue(netId, out var network) || network == null)
            return LoadNetwork(netId);
        profile.Network = netId;  // already exists, just load it
        return GetNetwork;
        if (profile.Network == netId)
            return GetNetwork; // already loaded, just return back
        if (profile.Networks.TryGetValue(netId, out var network) && network != null) {
            profile.Network = netId;  // already exists, just load it
            return GetNetwork;
        }
        if (LoadNetwork(netId) && AppMainMenu != null)
            RefreshPanel(AppMainMenu); // Update Menu (Unlocks buttons, labels the network button...)
        return GetNetwork;
    }
    internal static IoNetwork? SetNetwork(Type netId) {
        if (GetProfile is not IoProfile profile)
            return null;
        if (profile.Network == netId)
            return GetNetwork; // already loaded, just return back
        if (profile.Networks.TryGetValue(netId, out var network) && network != null) {
            profile.Network = netId; // already exists, just load it
            return GetNetwork;
        }
        if (LoadNetwork(netId) && AppMainMenu != null)
            RefreshPanel(AppMainMenu); // Update Menu (Unlocks buttons, labels the network button...)
        return GetNetwork;
    }*/
}
