using Avalonia.Media;
using Avalonia.Metadata;
using IoListWallet.Core;
using IoListWallet.Core.Panels;
using IoListWallet.Core.Static;
using IoListWallet.IoPanels.Main;
using System;
using System.Collections.Generic;
using static IoListWallet.Core.Static.AppSave;

namespace IoListWallet.Structs.Crypto;

internal class IoProfile {
    internal bool Dirty = true;

    #region PRO - Profile:
    [Save] // Name of this profile
    internal string Name { get; set { field = value; Dirty = true; } } = "";
    [Save] // Selected Network KeyId (like Bitcoin testnet)
    internal Type? Network { get; set { field = value; Dirty = true; } } = null;
    #endregion

    #region PRO - Interface Settings:
    [Save] // Selected App Locale Language
    internal string Locale { get; set { field = value; Dirty = true; } } = "en-us";
    [Save] // Selected App Locale Language
    internal string SubLocale { get; set { field = value; Dirty = true; } } = "x";
    [Save] // Regional or custom date format
    internal string DateFormat { get; set { field = value; Dirty = true; } } = "yyyy-MM-dd HH:mm";
    [Save] // Selected Display Mode (Fullscreen/Windowed...)
    internal DisplayMode DisplayMode { get; set { field = value; Dirty = true; } } = DisplayMode.MultiWindow;
    [Save] // Selected Display Rotation
    internal byte DisplayRotation { get; set { field = value; Dirty = true; } } = 0; // TODO implement settings (0-3)
    [Save] // Selected Display Rotation
    internal byte DockSplitterWidth { get; set { field = value; Dirty = true; } } = 12; // TODO implement settings (2-32)
    [Save] // Selected Display Rotation
    internal byte MaskOpacity { get; set { field = value; Dirty = true; } } = 80; // TODO implement settings (0-100)
    [Save] // Selected Display Rotation
    internal byte UnMaskTime { get; set { field = value; Dirty = true; } } = 10; // TODO implement settings (1-1000000)
    [Save] // Selected Display Rotation
    internal byte UnMaskSpeed { get; set { field = value; Dirty = true; } } = 8; // TODO implement settings (1-1000000)

    [Save] // how slow is the swipe/snaback animation?
    internal double SwipeTime { get; set { field = value; Dirty = true; } } = 500;
    [Save] // How far or fast you need to swipe to commit it?
    internal double SwipeSensitivity { get; set { field = value; Dirty = true; } } = 100;
    [Save] // Tooltip delay (0 = disabled)
    internal ushort ShowTooltips { get; set { field = value; Dirty = true; } } = 2000;
    [Save] // Font for control items (interactible)
    internal string ControlFont { get; set { field = value; Dirty = true; } } = "Segoe UI";
    [Save]  // Font size for control items
    internal double ControlFontSize { get; set { field = value; Dirty = true; } } = 16;
    [Save]  // Font for info items (like long texts for reading)
    internal string InfoFont { get; set { field = value; Dirty = true; } } = "Segoe UI";
    [Save] // Font size for info items
    internal double InfoFontSize { get; set { field = value; Dirty = true; } } = 8;
    //[Save] // Font size for info items
    //internal bool AutomaticWindowSize { get; set { field = value; Dirty = true; } } = true;
    [Save] // Titlebar color (only for multi-window mode)
    internal Color TitleColor { get; set { field = value; Dirty = true; } } = Colors.DarkRed; // TODO implement settings
    #endregion

    #region PRO - Performance Settings:
    [Save] // Maximum allowed total tasks
    internal ushort MaxTasks { get; set { field = value; Dirty = true; } } = 2;
    [Save] // Maximum allowed search tabs
    internal ushort MaxSearchTabs { get; set { field = value; Dirty = true; } } = 4;
    [Save]  // Selected Sorting algorithm for large sets
    internal SortAlgo SortAlgo { get; set { field = value; Dirty = true; } } = SortAlgo.P_IntroMerge;
    [Save] // If we want to sort less items than this, we'll just use the simple-task Intro sort, to avoid Parallel overhead.
    internal uint DefaultToSingle { get; set { field = value; Dirty = true; } } = 12;
    [Save] // How many buckets does a large bucket get split into?
    internal ushort SplitChildren { get; set { field = value; Dirty = true; } } = 64;
    [Save] // What size is considered a large bucket?
    internal ushort MinMaxBucketSize { get; set { field = value; Dirty = true; } } = 256;
    [Save] // A small number of bucket useful for balancing the final intro sort task loads
    internal ushort DistributionTiers { get; set { field = value; Dirty = true; } } = 16;
    [Save] // How long will a thread spin if it needs to wait for another thread's outputs
    internal ushort MergeSpin { get; set { field = value; Dirty = true; } } = 42;
    [Save] // How many merge outputs in bulk will get pushed across tasks
    internal ushort MergeVolatileBuffer { get; set { field = value; Dirty = true; } } = 6;
    #endregion

    #region PRO - Debug Settings:
    // Will put any texts with missing locale entry into brackets: "missing" -> "[missing]"
    [Save] // some texts do not expect a locale version, so keep that in mind
    internal bool DebugLocale { get; set { field = value; Dirty = true; } } = false;
    #endregion

    // This does not get saved, it must by typed by the user
    internal string Password = "";

    internal bool CorrectPassword = false;

    #region NET
    // All added networks
    internal Dictionary<Type, IoNetwork> Networks = [];
    #endregion

    #region PAN
    internal class SaveFilePan {
        internal bool Dirty = true;

        internal class SaveMainMenu {
            [Save]
            internal uint[] PageSize { get; set { 
                    AppMainMenu?.PageSize = value; field = value; } } = [];
            [Save]
            internal int[] Page { get; set { 
                    AppMainMenu?.Page = value; field = value; } } = [];
            [Save]
            internal IoFilter?[] MyFilter { get; set { 
                    if (value == null) return; 
                    AppMainMenu?.MyFilter = value; field = value;
                } } = [];
            [Save]
            internal IoPanel.IoPanelSave MenuPanel {
                get; set {
                    AppMainMenu?.Copy(field = value);
                }
            } = new IoPanel.IoPanelSave();

        }

        // TODO: Manually dirtied:

        [Save] // All Loaded panels (even closed ones, as long as they were not deleted)
        internal Dictionary<string, IoPanel> LoadedPanels { get; set { field = value;
                Dirty = true;
                if (GetProfile is not IoProfile profile)
                    throw new("No profile");
                field.Remove(IoPanel.MakeK(typeof(MainMenu), ""));
                // Sync Panels:
                List<string> L = [];
                var Old = profile.Pan.LoadedPanels;
                foreach (var P in Old)
                    if (!field.ContainsKey(P.Key))
                        L.Add(P.Key);
                foreach (var P in L) {
                    if (IoPanel.SplitK(P) is (Type, string) key) {
                        if (key.Item1 != typeof(MainMenu))
                            App.FromK(P)?.Delete(false);//Old.Remove(P);
                    } else App.FromK(P)?.Delete(false);//Old.Remove(P);
                }
                foreach (var P in field)
                    if (!Old.ContainsKey(P.Key))
                        Old[P.Key] = P.Value;
                } } = [];
        [Save] // All Pinned pannels, that will appear in the Pinned list, holding the panel's KeyIds
        internal List<string> PinnedPanels { get; set { field = value;
                Dirty = true;
                if (GetProfile is not IoProfile profile)
                    throw new("No profile");
                if (field.Count == 0)
                    return;
                // Sync Panels:
                List<string> L = [];
                var Old = profile.Pan.PinnedPanels;
                foreach (var P in Old)
                    if (!field.Contains(P))
                        L.Add(P);
                foreach (var P in L)
                    Old.Remove(P);
                foreach (var P in field)
                    if (!Old.Contains(P))
                        Old.Add(P);
                AppMainMenu?.GetMyMainPanel()?.Update();
                } } = [];
        [Save]// All opened multi-windows the one of ListMainMenu type should be linked to MainMenuPanel, it laos stores dock data
        internal Dictionary<int, IoWindow> OpenedWindows { get; set { field = value; Dirty = true;
                if (GetProfile is not IoProfile profile)
                    throw new("No profile");
                if (field.Count == 0)
                    return;

                // TODO implement OpenedWindows in general
                // TODO make sure a proper load / unload / closing / opening happens, maybe use the SetDisplayMode code

                // Sync Panels:
                List<int> L = [];
                var Old = profile.Pan.OpenedWindows;
                foreach (var P in Old)
                    if (!field.ContainsKey(P.Key))
                        L.Add(P.Key); // if opened isn't loaded, add to to the removal list
                foreach (var P in L)
                    Old.Remove(P);
                foreach (var P in field) // if loaded isn't opened, then put in in the list, excluding main menu (though main menu should alraedy exist, so this second cond is probably redundant)
                    if (!Old.ContainsKey(P.Key))// && ((IoDockPanel)P.Value.Dock.Dock.TopNonRoot).Panel != AppMainMenu?.GetK)
                        Old[P.Key] = P.Value;
                // open them in th correct order:
                bool opened;
                do {
                    opened = false;
                    foreach (var w in Old) {
                        if (!w.Value.IsVisible && w.Value.MyMainPanel.Dock != AppDockRoot && (TryWindow(w.Value.OpenedFrom) is not IoWindow par || par.IsVisible)) {
                            w.Value.Show();
                            w.Value.IsVisible = opened = true;
                        }
                    }
                } while (opened);
                // Remove unopened ones:
                L.Clear();
                foreach (var w in Old)
                    if (!w.Value.IsVisible)
                        L.Add(w.Key);
                foreach (var w in L)
                    Old.Remove(w);

                foreach (var P in Old) {
                    P.Value.MyMainPanel.Dock.UnBuild();
                    P.Value.MyMainPanel.DockHost.Content = null;
                }
                foreach (var P in Old) {
                    P.Value.ReLink();
                }
            } } = [];
        [Save] // Save the Main Menu's List settings separately, because it's excluded from normal panel reloading because of it's root speciality (different constructor)
        internal SaveMainMenu SaveMenu { get; set { field = value; Dirty = true; } } = new();
    }
    //[Save]
    internal SaveFilePan Pan = new();
    #endregion

    internal IoNetwork? GetNetwork => Network != null && Networks.TryGetValue(Network, out var n) ? n : null;
    internal IoWallet? GetWallet => GetNetwork?.GetWallet;
    internal IoAccount? GetAccount => GetNetwork?.GetAccount;
}