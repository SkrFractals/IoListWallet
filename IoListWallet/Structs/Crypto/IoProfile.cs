using Avalonia.Media;
using IoListWallet.MainControls;
using IoListWallet.MainControls.Panels;
using System;
using System.Collections.Generic;
using static IoListWallet.AppSave;

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
    internal string Language { get; set { field = value; Dirty = true; } } = "en-us";
    [Save] // Regional or custom date format
    internal string DateFormat { get; set { field = value; Dirty = true; } } = "yyyy-MM-dd HH:mm";
    [Save] // Selected Display Mode (Fullscreen/Windowed...)
    internal DisplayMode DisplayMode { get; set { field = value; Dirty = true; } } = DisplayMode.MultiWindow;
    [Save] // how slow is the swipe/snaback animation?
    internal double SwipeTime { get; set { field = value; Dirty = true; } } = 500;
    [Save] // How far or fast you need to swipe to commit it?
    internal double SwipeSensitivity { get; set { field = value; Dirty = true; } } = 100;
    [Save] // Tooltip delay (0 = disabled)
    internal ushort ShowTooltips { get; set { field = value; Dirty = true; } } = 2000;
    [Save] // Font for control items (interactible)
    internal string ControlFont { get; set { field = value; Dirty = true; } } = "Segoe UI";
    [Save]  // Font size for control items
    internal double ControlFontSize { get; set { field = value; Dirty = true; } } = 40;
    [Save]  // Font for info items (like long texts for reading)
    internal string InfoFont { get; set { field = value; Dirty = true; } } = "Segoe UI";
    [Save] // Font size for info items
    internal double InfoFontSize { get; set { field = value; Dirty = true; } } = 20;
    [Save] // Font size for info items
    internal bool AutomaticWindowSize { get; set { field = value; Dirty = true; } } = true;
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
            internal uint PageSize { get; set { AppMainMenu?.PageSize = value; field = value; } } = 10;
            [Save]
            internal int Page { get; set { AppMainMenu?.Page = value; field = value; } } = 0;
            [Save]
            internal IoFilter? MyFilter { get; set { if (value == null) return; AppMainMenu?.MyFilter = value; field = value; } } = null;
        }

        // TODO: Manually dirtied:

        [Save] // All Loaded panels (even closed ones, as long as they were not deleted)
        internal Dictionary<string, IoPanel> LoadedPanels { get; set { field = value; Dirty = true; } } = [];
        [Save] // All Pinned pannels, that will appear in the Pinned list, holding the panel's KeyIds
        internal List<string> PinnedPanels { get; set { field = value; Dirty = true; } } = [];
        [Save]// All opened multi-windows the one of ListMainMenu type should be linked to MainMenuPanel, it laos stores dock data
        internal Dictionary<int, IoWindow> OpenedWindows { get; set { field = value; Dirty = true; } } = [];
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