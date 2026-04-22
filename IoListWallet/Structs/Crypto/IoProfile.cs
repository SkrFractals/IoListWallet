using Avalonia;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using IoListWallet.Core;
using IoListWallet.Core.Panels;
using IoListWallet.IoPanels.Profile;
using System;
using System.Collections.Generic;
using System.IO;
using static IoListWallet.App;
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
    internal string SelectedLocale { get; set {
            if (GetProfile is not IoProfile profile || value == null || value == field || !LocaleNames.ContainsKey(value))
                return;
            Locale = LocaleData[field = value];
            //if (dButtons.TryGetValue("languageBox", out var t))
            //    t?.Text.Text = LocaleNames[profile.Locale];
            RefreshAll(); // refresh all panels (translate contents)
            foreach (var w in profile.Win.OpenedWindows)
                w.Value.MyMainPanel.Dock.Panel?.Update(); // Update mainpanels (translates bottom toolbars)
            Dirty = true; } } = "en-us";
    [Save] // Selected App Locale Language
    internal string SelectedSubLocale { get; set {
            if (GetProfile is not IoProfile profile || value == null || value == field || !SubLocaleNames.ContainsKey(value))
                return;
            SubLocale = SubLocaleData[field = value];
            RefreshAll(); // refresh all panels (translate contents)
            foreach (var w in profile.Win.OpenedWindows)
                w.Value.MyMainPanel.Dock.Panel?.Update(); // Update mainpanels (translates bottom toolbars)
            Dirty = true; } } = "x";
    [Save] // Regional or custom date format
    internal string DateFormat { get; set { 
            field = value; 
            if(GetProfile != null)
                RefreshAll();
            Dirty = true; } } = "yyyy-MM-dd HH:mm";
    [Save] // Selected Display Mode (Fullscreen/Windowed...)
    internal IMode? InterfaceMode { get; set { 
            if (value is not IMode mode || mode == field)
                return;
            if (field == IMode.MultiWindow)
                SaveWin(); // SavePan only saves windows when it multi-window mode (has to be a separate file)
            CloseAll(true);
            if (field == null || GetProfile == null) {
                field = mode;
                return;
            }
            field = mode;
            StartProfile(IsCorrectPassword(), false);
            OpenRefreshPanel(AppMainMenu, typeof(ProfileContent), "", 1);
            Dirty = true; } } = null; 
    [Save] // Selected Display Rotation
    internal byte DisplayRotation { get; set {
            if (value == DisplayRotation)
                return;
            field = value;
            UpdateRotation();
            Application.Current?.Resources["RotateTransform"] = 90 * value;
            RefreshSizes();
            Dirty = true; } } = 0; // This one too
    [Save] // Selected Display Rotation
    internal byte DockSplitterWidth { get; set { field = value;
            if (GetProfile is IoProfile profile) {
                foreach (var w in profile.Win.OpenedWindows)
                    w.Value.MyMainPanel.Dock.SanitizeRatios(w.Value.GetSize());
                AppMainPanel?.Dock.SanitizeRatios(AppMainPanel.Dock.MySize);
            }
            Dirty = true; 
        } } = 12; // TODO implement settings and propagation into all docks (2-32)
    [Save] // Selected Display Rotation
    internal byte MaskOpacity { get; set { field = value; Dirty = true; } } = 80; 
    [Save] // Selected Display Rotation
    internal byte UnMaskTime { get; set { field = value; Dirty = true; } } = 10;
    [Save] // Selected Display Rotation
    internal byte UnMaskSpeed { get; set { field = value; Dirty = true; } } = 8;

    [Save] // how slow is the swipe/snaback animation?
    internal double SwipeTime { get; set { field = value; Dirty = true; } } = 500;
    [Save] // How far or fast you need to swipe to commit it?
    internal double SwipeSensitivity { get; set { field = value; Dirty = true; } } = 100;
    [Save] // Tooltip delay (0 = disabled)
    internal ushort ShowTooltips { get; set { field = value; Dirty = true; } } = 2000;
    void Size() {
        if (GetProfile == null)
            return;
        RefreshPanel((typeof(ProfileContent), ""), true); // Refresh Profile Settings
        RefreshSizes();
    }
    [Save] // Font for control items (interactible)
    internal string ControlFont { get; set { Application.Current?.Resources["ControlFontFamily"] = new FontFamily(field = value); Dirty = true; Size(); } } = "Segoe UI";
    [Save]  // Font size for control items
    internal double ControlFontSize { get; set { Application.Current?.Resources["ControlFontSize"] = field = value; Dirty = true; Size(); } } = 16;
    [Save]  // Font for info items (like long texts for reading)
    internal string InfoFont { get; set { Application.Current?.Resources["InfoFontFamily"] = new FontFamily(field = value); Dirty = true; Size(); } } = "Segoe UI";
    [Save] // Font size for info items
    internal double InfoFontSize { get; set { Application.Current?.Resources["InfoFontSize"] = field = value; Dirty = true; Size(); } } = 8;
    //[Save] // Font size for info items
    //internal bool AutomaticWindowSize { get; set { field = value; Dirty = true; } } = true;
    private void RefreshTitle() {
        if (GetProfile is IoProfile profile)
            foreach (var p in profile.Pan.LoadedPanels)
                p.Value.RefreshTitleBar();
    }
    [Save] // Titlebar color (only for multi-window mode)
    internal bool TitleUnlock { get; set { field = value; RefreshTitle(); Dirty = true; } } = true;
    [Save] // Titlebar color (only for multi-window mode)
    internal Color TitleColor { get; set {  field = value; RefreshTitle(); Dirty = true; } } = Colors.DarkRed; // TODO implement settings
    #endregion

    #region PRO - Performance Settings:
    [Save] // Shadows Texts
    internal byte ShadowTexts { get; set { 
            App.SetTextShadows(field = value); Dirty = true; } } = 3;
    [Save] // Shadows Buttons
    internal byte ShadowButtons { get; set { 
            App.SetButtonShadows(field = value); Dirty = true; } } = 1;
    [Save] // Maximum allowed total tasks
    internal ushort MaxTasks { get; set { field = value; Dirty = true; } } = 2;
    [Save] // Maximum allowed search tabs
    internal ushort MaxSearchTabs { get; set { field = value; Dirty = true; } } = 4;
    [Save]  // Selected Sorting algorithm for large sets
    internal SortAlgo SortAlgo { get; set { 
            if (value == field)
                return;
            field = value;
            if(GetProfile != null)
                RefreshPanel((typeof(ProfileContent), ""), true); // Refresh Profile Settings
            Dirty = true; } } = SortAlgo.P_IntroMerge;
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
    [Save] // How many merge outputs in bulk will get pushed across tasks
    internal ulong BackgroundOpacity {
        get; set {
            BackgroundBrush ??= new ImageBrush(Background) {
                Stretch = Stretch.Fill,
                Opacity = value * .01
            };
            BackgroundBrush.Opacity = (field = value) * .01;
            foreach (var p in Pan.LoadedPanels)
                p.Value.Background = BackgroundBrush;
            Dirty = true;
        }
    } = 25;
    internal Bitmap? Background { get; set {

            if (!App.DisableSaving) {
                // Save the image to IoWallet/<Profile>/background.png
                var path = GetProfileDir();
                Directory.CreateDirectory(path);
                using var fs = File.OpenWrite(Path.Combine(path, "background.png"));
                value?.Save(fs); // saves as PNG by default
            }
            BackgroundBrush ??= new ImageBrush(value) {
                Stretch = Stretch.Fill,
                Opacity = BackgroundOpacity * .01
            };
            BackgroundBrush.Source = field = value;
            foreach (var p in Pan.LoadedPanels)
                p.Value.Background = BackgroundBrush; 
            Dirty = true; 
        } } = null;
    
    #endregion

    #region PRO - Debug Settings:
    // Will put any texts with missing locale entry into brackets: "missing" -> "[missing]"
    [Save] // some texts do not expect a locale version, so keep that in mind
    internal bool DebugLocale { get; set { 
            field = value;
            if(GetProfile != null)
                RefreshAll(); 
            Dirty = true; } } = false;
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

        // TODO: Manually dirtied:

        [Save] // All Loaded panels (even closed ones, as long as they were not deleted)
        internal Dictionary<string, IoPanel> LoadedPanels { get; set { field = value; Dirty = true; } } = [];
        [Save] // All Pinned pannels, that will appear in the Pinned list, holding the panel's KeyIds
        internal List<string> PinnedPanels { get; set { field = value; Dirty = true; } } = [];
    }
    internal class SaveFileWin {
        internal bool Dirty = true;
        // TODO uncomment this when Saving and reloading OpenedWindows is fixed
        [Save]// All opened multi-windows the one of ListMainMenu type should be linked to MainMenuPanel, it laos stores dock data
        internal Dictionary<int, IoWindow> OpenedWindows { get; set {
                if (GetProfile is not IoProfile profile)
                    throw new("No profile");
                if (App.DisableSavingOpenedWindows)
                    return;
                field = value;
                Dirty = true;
                foreach (var P in value) {
                    P.Value.IsVisible = false;
                    P.Value.MyMainPanel.MyWindowK = P.Value.Index = P.Key;
                    App.FromK(P.Value.MyMainPanel.Dock.Original)?.OriginalMainWindow = P.Key;
                    P.Value.ReLink();
                }
                bool holdup;
                do {
                    holdup = false;
                    foreach (var P in value) {
                        if (P.Value.IsVisible)
                            continue;
                        if (value.TryGetValue(P.Value.OpenedFrom, out var o)) {
                            if (o.IsVisible)
                                P.Value.Show(o);
                            else
                                holdup = true;
                            continue;
                        }
                        P.Value.Show();
                    }
                } while (holdup);
            } } = [];
    }
    internal SaveFilePan Pan = new();
    internal SaveFileWin Win = new();
    #endregion

    #region Getters
    internal IoNetwork? GetNetwork => Network != null && Networks.TryGetValue(Network, out var n) ? n : null;
    internal IoWallet? GetWallet => GetNetwork?.GetWallet;
    internal IoAccount? GetAccount => GetNetwork?.GetAccount;
    #endregion

    #region VAR
    internal ImageBrush? BackgroundBrush { get; private set; } = null;
    #endregion

}