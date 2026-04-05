using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Interactivity;
using Avalonia.Media;
using IoListWallet.ElementControls.Basic;
using IoListWallet.MainControls.Panels;
using IoListWallet.IoPanels.Main;
using IoListWallet.Structs.Crypto;
using System;
using static IoListWallet.App;
using static IoListWallet.AppSave;
using System.IO;
using System.Collections;

namespace IoListWallet.IoPanels.Profile;
internal class BookProfile : IoBookPanel {
    public BookProfile() : base() { }
    internal enum TabProfilePage : byte {
        TabProfileProfile,
        TabProfileInterface,
        TabProfilePerformance,
        TabProfileDebug
    }
    private IoTextBox? passwordText = null, newPasswordText = null;
    private TextBlock? changeButton = null;
    private int newPassCase = 0;
    private string newPass = "", changeWatermark = "", changeButtonText = "ChangePassword";
    internal override void Refresh() {
        Book.PageNames = ["TabProfileProfile", "TabProfileInterface", "TabProfilePerformance", "TabProfileDebug"];
        base.Refresh();
    }
    private (string Text, IImmutableSolidColorBrush Brush) SetPassBlock(bool correct) => correct 
        ? (L("CorrectPassword"), Brushes.Green) : ((passwordText?.Text ?? "") == "" 
        ? L("InputPassword") : L("WrongPassword"), Brushes.Red);
    private (string Text, IImmutableSolidColorBrush Brush) SetNewPassBlock() 
        => newPassCase switch {
        1 => ("TypeNewPass", Brushes.White),
        2 => ("TypeNewPassAgain", Brushes.White),
        3 => ("TypeNewPassRetry", Brushes.Red),
        4 => ("TypeOldPass", Brushes.White),
        5 => ("TypeOldPassRetry", Brushes.Red),
        6 => ("NewPassSuccess", Brushes.Green),
        _ => ("", Brushes.White)
    };

    protected override void FillPage(object? s, EventArgs e) {
        if (GetProfile is not IoProfile profile)
            return;
        
        var F = "Failed BookProfile.";
        var T = "T_BookProfile.";
        switch ((TabProfilePage)Book.Swipe.Page.X) {

            #region Profile
            case TabProfilePage.TabProfileProfile:

                // Delete Profile Button
                void clickProfileDelete(object? s, EventArgs e) {
                    Directory.Delete(GetProfileDir());
                    SApp.Profiles.Remove(SApp.Profile);
                    SApp.Profile = -1;
                    var enumerator = SApp.Profiles.Keys.GetEnumerator();
                    LoadProfile(enumerator.MoveNext() ? enumerator.Current : 0);
                }

                var profileDeleteButton = MakeButton("profileDeleteButton", T + "ProfileDeleteButton",
                    L("ProfileDeleteButton"), clickProfileDelete, false, FontWeight.Bold, L("AreYouSure"))
                    ?? throw new(F + "profileDeleteButton");

                // Profile Name
                void changeNameBox(object? s, EventArgs e) {
                    profile.Name = ((TextBox?)s)?.Text ?? DPro.Name;
                    RefreshPanel((typeof(ListMainMenu), ""));
                }
                var nameBox = MakeTextBox("nameBox", T + "Name", profile.Name, "Name", changeNameBox)
                    ?? throw new(F + "nameBox");

                // LOGIN -----------------------------------------------------

                // Enter Pass Block
                var loggedIn = IsCorrectPassword();
                var submitMessage = SetPassBlock(loggedIn);
                var passBlock = MakeTextBlock("passBlock", "", submitMessage.Text)
                    ?? throw new(F + "validNameBlock");
                passBlock.Text.Foreground = submitMessage.Brush;

                // Enter Pass Box
                //void changePasswordBox(object? s, EventArgs e)
                //    => profile.Password = ((TextBox?)s)?.Text ?? "";
                var passwordBox = MakeTextBox("passwordBox", T + "Password", profile.Password, "Password")
                    ?? throw new(F + "passwordBox");
                (passwordText = passwordBox.Text).PasswordChar = '*';
                passwordText?.IsEnabled = !loggedIn; // lock it if already logged in

                // Submit Pass Button
                void clickSetPass(object? s, EventArgs e) {
                   
                    var pass = profile.Password;
                    profile.Password = passwordText?.Text ?? "";
                    var c = IsCorrectPassword();
                    (passBlock.Text.Text, passBlock.Text.Foreground) = SetPassBlock(c);
                    if (c) {
                        profile.CorrectPassword = true;
                        LoadNetworks(); // Loads networks, (and wallets, and txs for the selected one)
                        LoadPanels();   // Loads panels, pinneds, and windows
                        passwordText?.IsEnabled = false;    // lock password box
                        ((Control?)s)?.IsEnabled = false;   // lock submit button
                        RefreshPanel(this);
                        RefreshPanel(AppMainMenu);
                        return;
                    }
                    profile.Password = pass;
                }
                
                var submitPasswordButton = MakeButton("submitPasswordButton", T + "SubmitPasswordButton",
                    L("SubmitPasswordButton"), clickSetPass)
                    ?? throw new(F + "submitPasswordButton");
                submitPasswordButton.Button.IsEnabled = !loggedIn;

                // CHANGE PASSWORD -----------------------------------------------------

                // Enter New Pass Block
                var message = SetNewPassBlock();

                // Enter New Pass Box
                var newPasswordBox = MakeTextBox("newPasswordBox", T + changeWatermark, "", changeWatermark)
                    ?? throw new(F + "newPasswordBox");
                (newPasswordText = newPasswordBox.Text).PasswordChar = '*';

                // Change Pass Button
                void clickChangePass(object? s, EventArgs e) {

                    switch (newPassCase) {
                        case 6: case 0: 
                            newPassCase = 1;
                            newPasswordText?.IsEnabled = true;
                            newPasswordText?.Watermark = changeWatermark = "NewPassword";
                            changeButton?.Text = L(changeButtonText = "ConfirmNew");
                            break;
                        case 1: case 3: 
                            newPass = newPasswordBox.Text.Text ?? ""; 
                            newPassCase = 2; 
                            break;
                        case 2: 
                            if (newPass == (newPasswordBox.Text.Text ?? "")) {
                                newPasswordText?.Watermark = changeWatermark = "OldPassword";
                                changeButton?.Text = L(changeButtonText = "ConfirmOld");
                                newPassCase = 4;
                            } else newPassCase = 3;
                            break;
                        case 4: case 5:
                            if (profile.Password == (newPasswordBox.Text.Text ?? "")) {
                                newPasswordText?.Watermark = changeWatermark = "Successful";
                                changeButton?.Text = L(changeButtonText = "ChangePasswordButton");
                                newPassCase = 6;
                            } else newPassCase = 5;
                            break;
                    }
                    newPasswordBox.Text.Text = "";
                    if (newPassCase == 6) {
                        // Re-save all the files in this profile encrypted with the new password:
                        passwordText?.Text = profile.Password = newPass;
                        newPasswordText?.IsEnabled = false;
                        SavePassword(newPass);
                        SaveAll(true); // encrypts the network/wallet/transaction/panel files
                    }
                    //(newPassBlock.Text.Text, newPassBlock.Text.Foreground) = SetNewPassBlock();
                    newPasswordBox.Text.IsEnabled = newPassCase > 0 && newPassCase < 6;
                    RefreshPanel(this);
                }
                var changePasswordButton = MakeButton("changePasswordButton", T + "ChangePasswordButton",
                    L(changeButtonText), clickChangePass)
                    ?? throw new(F + "changePasswordButton");
                changeButton = changePasswordButton.Text;
                // LAYOUT
                Book.MakeRow("R", "ProfileDelete", [(profileDeleteButton.Button, 1)]);
                Book.MakeRow("0", "ProfileName", [(nameBox.Border, 1)]);
                Book.MakeRow("1", "LoginStatus", [(passBlock.View, 1)]);
                Book.MakeRow("2", "ProfilePassword", [(passwordBox.Border, 1)]);
                Book.MakeRow("3", "SubmitPassword", [(submitPasswordButton.Button, 1)]);


                if (newPassCase > 0) {
                    //if (newPassCase < 6) 
                    Book.MakeRow("4", message.Text, [(newPasswordBox.Border, 1)], message.Brush);
                    //Book.MakeRow("", [(newPassBlock.View, 1)]);
                }
                if (loggedIn)
                    Book.MakeRow("5", "ChangePassword", [(changePasswordButton.Button, 1)]);

                break;
            #endregion

            #region Interface
            case TabProfilePage.TabProfileInterface:
                
                // Language
                void clickLanguageBox(object? s, EventArgs e)
                    => OpenRefreshPanel(this, typeof(ListLanguage));
                var languageBox = MakeButton("languageBox", T + "Display", Languages[profile.Language], clickLanguageBox)
                    ?? throw new(F + "languageBox");
               
                // DateFormat
                void clickDateFormat(object? s, EventArgs e)
                     => SetDateFormat(s, e);
                var dateFormat = MakeComboBox("dateFormat", T + "DateFormat",
                    DateFormats, profile.DateFormat,
                    new FuncDataTemplate<string>((mode, _) => new TextBlock { Text = mode }, false), clickDateFormat)
                    ?? throw new(F + "dateFormat");
                dateFormat.Combo.IsEditable = true;

                // Display Mode
                void clickDisplayMode(object? s, EventArgs e)
                    => SetDisplayMode(((IoComboBox?)s)?.SelectedItem);
                var displayBox = MakeComboBox("displayBox", T + "DisplayMode",
                    Enum.GetValues<DisplayMode>(), profile.DisplayMode,
                    new FuncDataTemplate<DisplayMode>((mode, _) 
                        => new TextBlock { Text = GetEnumText(mode) }, false), clickDisplayMode)
                    ?? throw new(F + "displayBox");
                
                // Show Tooltips
                void changeShowTooltips(object? s, EventArgs e) {
                    profile.ShowTooltips = (ushort?)((IoNumericLeftRight?)s)?.Value ?? DPro.ShowTooltips;
                    //var C = Current ?? throw new("Failed Set ShowTooltips");
                    RefreshAll();
                    SApp.Dirty = true;;
                }
                var showTooltips 
                    = MakeNumeric("showTooltips", T + "ShowTooltips", profile.ShowTooltips, 
                        0, 60000, 10, 10, "Milliseconds", "", "", changeShowTooltips)
                    ?? throw new(F + "showTooltips");
                
                // Control Font
                void clickControlFontBox(object? s, EventArgs e)
                    => OpenRefreshPanel(this, typeof(ListFontControl));
                var controlFontBox = MakeButton("controlFontBox", T + "ControlFont", 
                    profile.ControlFont.ToString(), clickControlFontBox)
                    ?? throw new(F + "controlFontBox");
                void clickControlFontSize(object? s, EventArgs e)
                    => SetControlFont(profile.ControlFont, ((IoNumericLeftRight?)s)?.Value ?? DPro.ControlFontSize);
                var controlFontSize = MakeNumeric("controlFontSize", T + "ControlFontSize", 
                    (ulong)profile.ControlFontSize, 6, 120, 1, 1, "Size", "", "", clickControlFontSize)
                    ?? throw new(F + "controlFontSize");
               
                // Info Font
                void clickInfoFontBox(object? s, EventArgs e)
                    => OpenRefreshPanel(this, typeof(ListFontInfo));
                var infoFontBox = MakeButton("infoFontBox", T + "InfoFont",
                    profile.InfoFont.ToString(), clickInfoFontBox)
                    ?? throw new(F + "infoFontBox");
                void clickInfoFontSize(object? s, EventArgs e)
                    => SetInfoFont(profile.InfoFont, ((IoNumericLeftRight?)s)?.Value ?? DPro.InfoFontSize);
                var infoFontSize = MakeNumeric("infoFontSize", T + "InfoFontSize", 
                    (ulong)profile.InfoFontSize, 6, 120, 1, 1, "Size", "", "", clickInfoFontSize)
                    ?? throw new(F + "infoFontSize");
                
                // Swipe
                void changeSwipeTime(object? s, EventArgs e)
                    => profile.SwipeTime = ((IoNumericLeftRight?)s)?.Value ?? DPro.SwipeTime;
                var swipeTime = MakeNumeric("swipeTime", T + "SwipeTime",
                   (ulong)profile.SwipeTime, 100, 2000, 100, 1, "Milliseconds", "", "", changeSwipeTime) // TODO Setting the time all the way to 0 is glitchy, so for now I only allow 100ms min
                   ?? throw new(F + "swipeTime");
                void changeSwipeSensitivity(object? s, EventArgs e)
                    => profile.SwipeSensitivity = ((IoNumericLeftRight?)s)?.Value ?? DPro.SwipeSensitivity;
                var swipeSensitivity = MakeNumeric("swipeSensitivity", T + "SwipeSensitivity",
                    (ulong)profile.SwipeSensitivity, 0, 300, 1, 10, "Sensitivity", L("SwipeSensitivity"), "", changeSwipeSensitivity)
                   ?? throw new(F + "swipeSensitivity");

                // Reset Settings
                void clickResetInt(object? s, EventArgs e) {
                    dateFormat.Combo.Text = DPro.DateFormat;
                    showTooltips.Value = DPro.ShowTooltips;
                    // Reset Fonts
                    controlFontBox.Text.Text = (profile.ControlFont = DPro.ControlFont).ToString();
                    infoFontBox.Text.Text = (profile.InfoFont = DPro.InfoFont).ToString();
                    controlFontSize.Value = (ulong)DPro.ControlFontSize;
                    infoFontSize.Value = (ulong)DPro.InfoFontSize;
                    swipeTime.Value = (ulong)DPro.SwipeTime;
                    swipeSensitivity.Value = (ulong)DPro.SwipeSensitivity;
                    // Reset Language
                    profile.Language = DPro.Language;
                    RefreshPanel((typeof(ListLanguage), ""));
                    // Reset Display
                    displayBox.Combo.SelectedIndex = 0;                }
                var resetIntButton = MakeButton("resetIntButton", T + "Reset", "Reset", clickResetInt)
                    ?? throw new(F + "resetButton");

                Book.MakeRow("0", "ResetToDefault", [(resetIntButton.Button, 1)]);
                Book.MakeRow("1", "Language", [(languageBox.Button, 1)]);
                Book.MakeRow("2", "DateFormat", [(dateFormat.View, 1)]);
                Book.MakeRow("3", "DisplayMode", [(displayBox.View, 1)]);
                Book.MakeRow("4", "ShowTooltips", [(showTooltips, 1)]);
                Book.MakeRow("5", "ControlFont", [(controlFontBox.Button, 1), (controlFontSize, 2)]);
                Book.MakeRow("6", "InfoFont", [(infoFontBox.Button, 1), (infoFontSize, 2)]);
                Book.MakeRow("7", "SwipeTime", [(swipeTime, 2), (swipeSensitivity, 3)]);

                break;
            #endregion

            #region Performance:
            case TabProfilePage.TabProfilePerformance:

                // Max Tasks
                void clickMaxTasks(object? s, EventArgs e) {
                    profile.MaxTasks = (ushort?)((IoNumericLeftRight?)s)?.Value ?? DPro.MaxTasks;
                }
                var maxTasks = MakeNumeric("maxTasks", T + "MaxTasks",
                    profile.MaxTasks, 1, MaxTasks, 1, 1, "MaxTasks", "", " / " + MaxTasks.ToString(), clickMaxTasks)
                    ?? throw new(F + "maxTasks");

                // Max Search Tabs
                void clickMaxSearchTabs(object? s, EventArgs e) {
                    profile.MaxSearchTabs = (ushort?)((IoNumericLeftRight?)s)?.Value ?? DPro.MaxSearchTabs;
                }
                var maxSearchTabs = MakeNumeric("maxSearchTabs", T + "MaxSearchTabs",
                    profile.MaxSearchTabs, 1, 64, 1, 1, "MaxSearchTabs", "", " / " + 64.ToString(), clickMaxSearchTabs)
                    ?? throw new(F + "maxSearchTabs");

                // Sort Algo
                var sortAlgoBox = MakeComboBox("sortAlgo", T + "SortAlgo", 
                    Enum.GetValues<SortAlgo>(), profile.SortAlgo,
                    new FuncDataTemplate<SortAlgo>((mode, _) 
                        => new TextBlock { Text = GetEnumText(mode) }, false), SetSortAlgo)
                    ?? throw new(F + "sortAlgoBox");
               
                void clickDefaultToSingle(object? s, EventArgs e) {
                    profile.DefaultToSingle = (ushort)(((IoNumericLeftRight?)s)?.Value ?? DPro.DefaultToSingle);
                }
                var defaultToSingle 
                    = MakeNumeric("defaultToSingle", T + "DefaultToSingle", profile.DefaultToSingle, 
                        9, 20, 1, 1, "Items", "", "", clickDefaultToSingle)
                    ?? throw new(F + "PowerOf2");

                // Sort properties:
                void changePbiSplitChildren(object? s, EventArgs e) {
                    profile.SplitChildren = (ushort)(((IoNumericLeftRight?)s)?.Value ?? DPro.SplitChildren);
                }
                var pbiSplitChilren 
                    = MakeNumeric("pbiSplitChilren", T + "SplitChildren", profile.SplitChildren, 
                        1, 8, 1, 1, "PowerOf2", "", "", changePbiSplitChildren)
                    ?? throw new(F + "pbiSplitChilren");
                
                void changePbiMinMaxBucketSize(object? s, EventArgs e) {
                    profile.MinMaxBucketSize = (ushort)(((IoNumericLeftRight?)s)?.Value ?? DPro.MinMaxBucketSize);
                }
                var pbiMinMaxBucketSize 
                    = MakeNumeric("pbiMinMaxBucketSize", T + "MinMaxBucketSize", profile.MinMaxBucketSize, 
                        7, 12, 1, 1, "PowerOf2", "", "", changePbiMinMaxBucketSize)
                    ?? throw new(F + "pbiMinMaxBucketSize");
                
                void changePbiDistributionTiers(object? s, EventArgs e) {
                    profile.DistributionTiers = (ushort)(((IoNumericLeftRight?)s)?.Value ?? DPro.DistributionTiers);
                }
                var pbiDistributionTiers 
                    = MakeNumeric("pbiDistributionTiers", T + "DistributionTiers", profile.DistributionTiers, 
                        1, 5, 1, 1, "PowerOf2", "", "", changePbiDistributionTiers)
                    ?? throw new(F + "pbiDistributionTiers");
                
                void clickPimSpin(object? s, EventArgs e) {
                    profile.MergeSpin = (ushort)(((IoNumericLeftRight?)s)?.Value 
                        ?? DPro.MergeSpin);
                }
                var pimSpin = MakeNumeric("pimSpin", T + "SpinInstructions", profile.MergeSpin, 
                        5, 9, 1, 4, "Instructions", "", "", clickPimSpin)
                    ?? throw new(F + "pimSpin");
                
                void clickPimVolatile(object? s, EventArgs e) {
                    profile.MergeVolatileBuffer = (ushort)(((IoNumericLeftRight?)s)?.Value 
                        ?? DPro.MergeVolatileBuffer);
                }
                var pimVolatile = MakeNumeric("pimVolatile", T + "VolatileBuffer", profile.MergeVolatileBuffer, 
                        4, 7, 1, 10, "PowerOf2", "", "", clickPimVolatile)
                    ?? throw new(F + "pimVolatile");
                
                // Reset Settings
                void clickResetPerf(object? s, EventArgs e) {
                    maxSearchTabs.Value = DPro.MaxSearchTabs;
                    maxTasks.Value = MaxTasks;
                    defaultToSingle.Value = DPro.DefaultToSingle;
                    pbiSplitChilren.Value = DPro.SplitChildren;
                    pbiMinMaxBucketSize.Value = DPro.MinMaxBucketSize;
                    pbiDistributionTiers.Value = DPro.DistributionTiers;
                    pimSpin.Value = DPro.MergeSpin;
                    pimVolatile.Value = DPro.MergeVolatileBuffer;
                    sortAlgoBox.Combo.SelectedItem = DPro.SortAlgo;
                }
                var resetPerfButton = MakeButton("ResetPerfButton", T + "Reset", "Reset", clickResetPerf)
                    ?? throw new(F + "resetButton");

                // Show Perf Page:
                Book.MakeRow("0", "ResetToDefault", [(resetPerfButton.Button, 1)]);
                Book.MakeRow("1", "MaxTasks", [(maxTasks, 1)]);
                Book.MakeRow("2", "MaxSearchTabs", [(maxSearchTabs, 1)]);
                Book.MakeRow("3", "SortAlgo", [(sortAlgoBox.View, 1)]);
                Book.MakeRow("4", "DefaultToSingle", [(defaultToSingle, 1)]);

                // Show sort properties:
                if (sortAlgoBox.Combo.SelectedItem is SortAlgo sac) {
                    switch (sac) {
                        case SortAlgo.S_Intro:
                            break;
                        case SortAlgo.P_IntroMerge:
                            Book.MakeRow("5", "SpinInstructions", [(pimSpin, 1)]);
                            Book.MakeRow("6", "VolatileBuffer", [(pimVolatile, 1)]);
                            break;
                        case SortAlgo.P_BucketIntro:
                            Book.MakeRow("7", "SplitChilren", [(pbiSplitChilren, 1)]);
                            Book.MakeRow("8", "MinMaxBucketSize", [(pbiMinMaxBucketSize, 1)]);
                            Book.MakeRow("9", "DistributionTiers", [(pbiDistributionTiers, 1)]);
                            break;
                    }
                }

                break;
            #endregion

            #region Debug
            case TabProfilePage.TabProfileDebug:

                var debugLocale 
                    = MakeButton("debugLocale", T + "DebugLocale", Dis(!profile.DebugLocale),
                        SetDebugLocale, profile.DebugLocale, FontWeight.Normal, "")
                    ?? throw new(F + "debugLocale");
                Book.MakeRow("0", "DebugLocale", [(debugLocale.Button, 1)]);

                break;
            #endregion
        }
        base.FillPage(s, e);
    }
    #region Setters
    private static void SetDisplayMode(object? modeObject) {
        if (GetProfile is not IoProfile profile 
            || modeObject == null 
            || modeObject is not DisplayMode mode
            || mode == profile.DisplayMode)
            return;
        ResetToHome(profile, mode);
    }
    internal void SetSortAlgo(object? s, EventArgs e) {
        if (((ComboBox?)s)?.SelectedItem is not object algoObject || algoObject is not SortAlgo Algo)
            return;
        GetProfile?.SortAlgo = Algo;
        RefreshPanel(this); // Refresh this 
    }
    internal void SetLanguage(string? lang) {
        if(GetProfile is not IoProfile profile || lang == null || lang == profile.Language || !Languages.ContainsKey(lang))
            return;
        LoadedLocale = LoadedLocales[profile.Language = lang];
        if (dButtons.TryGetValue("languageBox", out var t))
            t?.Text.Text = Languages[profile.Language];
        RefreshAll();
    }
    internal void SetControlFont(object? familyObject, double size) {
        if (GetProfile is not IoProfile profile || familyObject is not string family || (family == profile.ControlFont && size == profile.ControlFontSize))
            return;
        Application.Current?.Resources["ControlFontFamily"] = new FontFamily(profile.ControlFont = family);
        Application.Current?.Resources["ControlFontSize"] = profile.ControlFontSize = size;
        if(dInputs.TryGetValue("controlFontBox", out var t))
            t?.Text.Text = family.ToString();
    }
    internal void SetInfoFont(object? familyObject, double size) {
        if (GetProfile is not IoProfile profile || familyObject is not string family || (family == profile.InfoFont && size == profile.InfoFontSize))
            return;
        Application.Current?.Resources["InfoFontFamily"] = new FontFamily(profile.InfoFont = family);
        Application.Current?.Resources["InfoFontSize"] = profile.InfoFontSize = size;
        if (dInputs.TryGetValue("infoFontBox", out var t))
            t?.Text.Text = family.ToString();
    }
    private void SetDebugLocale(object? s, RoutedEventArgs e) {
        if (GetProfile is not IoProfile profile)
            return;
        profile.DebugLocale = ((IoToggleButton?)s)?.IsChecked ?? false;
        if (dButtons.TryGetValue("debugLocale", out var b) && b != null) 
            b.Value.Text.Text = Dis(profile.DebugLocale);
        RefreshAll();
    }
    private void SetDateFormat(object? s, EventArgs e) {
        if (GetProfile is not IoProfile profile)
            return;
        var dateFormat = ((IoComboBox?)s)?.Text ?? DPro.DateFormat;
        profile.DateFormat = dateFormat;
        RefreshAll();
    }
    #endregion
}