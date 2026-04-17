using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Interactivity;
using Avalonia.Media;
using IoListWallet.Core.Basic;
using IoListWallet.Core.Books;
using IoListWallet.Core.Panels;
using IoListWallet.Core.Static;
using IoListWallet.IoPanels.Main;
using IoListWallet.Structs.Crypto;
using System;
using System.IO;
using System.Runtime.CompilerServices;
using static IoListWallet.App;
using static IoListWallet.Core.Static.AppSave;

namespace IoListWallet.IoPanels.Profile;
internal class ProfileContent : IoBookPanel {
    internal enum TabProfilePage : byte {
        TabProfileProfile,
        TabProfileInterface,
        TabProfilePerformance,
        TabProfileDebug
    }
    protected override string[] GetPageNames() 
        => ["TabProfileProfile", "TabProfileInterface", "TabProfilePerformance", "TabProfileDebug"];
    private IoTextBox? passwordText = null, newPasswordText = null;
    private TextBlock? changeButton = null;
    private int newPassCase = 0;
    private string newPass = "", changeWatermark = "", changeButtonText = "ChangePassword";
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

    private const string F = "Failed ProfileContent.", T = "T_ProfileContent.";
    protected override void FillPage(object? sender, IoBook.FillPageEventArgs e) {
        if (GetProfile is not IoProfile profile)
            return;
        var s = e.Simulated ? "s." : "";
        var loggedIn = IsCorrectPassword();
        switch ((TabProfilePage)e.Page.X) {

            #region Profile
            case TabProfilePage.TabProfileProfile:

                // Delete Profile Button
                void clickProfileDelete(object? s, EventArgs e) {
                    if(Directory.Exists(GetProfileDir()))
                        Directory.Delete(GetProfileDir());
                    var p = GetProfile;
                    if (p == null) throw new("No Profile");
                    bool left;
                    do {
                        left = true;
                        foreach (var i in p.Pan.OpenedWindows)
                            if (i.Value != AppMainWindow) {
                                i.Value.Close();
                                left = false;
                            }
                    } while (left);
                    SApp.Profiles.Remove(SApp.Profile);
                    SApp.Profile = -1;
                    var enumerator = SApp.Profiles.Keys.GetEnumerator();
                   

                    LoadProfile(enumerator.MoveNext() ? enumerator.Current : 0);
                }

                var profileDeleteButton = MakeButton(s + "profileDeleteButton", T + "ProfileDeleteButton",
                    L("ProfileDeleteButton"), clickProfileDelete, false, FontWeight.Bold, L("AreYouSure"))
                    ?? throw new(F + "profileDeleteButton");

                // Profile Name
                void changeNameBox(object? s, EventArgs e) {
                    profile.Name = ((TextBox?)s)?.Text ?? DPro.Name;
                    RefreshPanel((typeof(MainMenu), ""), true);
                }
                var nameBox = MakeTextBox(s + "nameBox", T + "Name", profile.Name, "Name", changeNameBox)
                    ?? throw new(F + "nameBox");

                // LOGIN -----------------------------------------------------

                // Enter Pass Block
                
                var submitMessage = SetPassBlock(loggedIn);
                var passBlock = MakeTextBlock(s + "passBlock", "", submitMessage.Text)
                    ?? throw new(F + "validNameBlock");
                passBlock.Text.Foreground = submitMessage.Brush;

                // Enter Pass Box
                //void changePasswordBox(object? s, EventArgs e)
                //    => profile.Password = ((TextBox?)s)?.Text ?? "";
                var passwordBox = MakeTextBox(s + "passwordBox", T + "Password", profile.Password, "Password")
                    ?? throw new(F + "passwordBox");
                (passwordText = passwordBox.Text).PasswordChar = '*';
                passwordText?.Enabled = !loggedIn; // lock it if already logged in

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
                        passwordText?.Enabled = false;    // lock password box
                        ((IoToggleButton?)s)?.IsEnabled = false;   // lock submit button
                        RefreshPanel(this, ~0, true);
                        RefreshPanel(AppMainMenu, ~0, true);
                        return;
                    }
                    profile.Password = pass;
                }

                var submitPasswordButton = MakeButton(s + "submitPasswordButton", T + "SubmitPasswordButton",
                    L("SubmitPasswordButton"), clickSetPass)
                    ?? throw new(F + "submitPasswordButton");
                submitPasswordButton.Button.IsEnabled = !loggedIn;

                // CHANGE PASSWORD -----------------------------------------------------

                // Enter New Pass Block
                var message = SetNewPassBlock();

                // Enter New Pass Box
                var newPasswordBox = MakeTextBox(s + "newPasswordBox", T + changeWatermark, "", changeWatermark)
                    ?? throw new(F + "newPasswordBox");
                (newPasswordText = newPasswordBox.Text).PasswordChar = '*';

                // Change Pass Button
                void clickChangePass(object? s, EventArgs e) {

                    switch (newPassCase) {
                        case 6: case 0:
                            newPassCase = 1;
                            newPasswordText?.Enabled = true;
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
                        newPasswordText?.Enabled = false;
                        SavePassword(newPass);
                        SaveAll(true); // encrypts the network/wallet/transaction/panel files
                    }
                    //(newPassBlock.Text.Text, newPassBlock.Text.Foreground) = SetNewPassBlock();
                    newPasswordBox.Text.Enabled = newPassCase > 0 && newPassCase < 6;
                    RefreshPanel(this, ~0, true);
                }
                var changePasswordButton = MakeButton(s + "changePasswordButton", T + "ChangePasswordButton",
                    L(changeButtonText), clickChangePass)
                    ?? throw new(F + "changePasswordButton");
                changeButton = changePasswordButton.Text;
                // LAYOUT
                if(loggedIn)
                    Book.MakeRow(e, "R", "ProfileDelete", [(profileDeleteButton.Button, 1)]);
                Book.MakeRow(e, "0", "ProfileName", [(nameBox.Border, 1)]);
                Book.MakeRow(e, "1", "LoginStatus", [(passBlock.View, 1)]);
                Book.MakeRow(e, "2", "ProfilePassword", [(passwordBox.Border, 1)]);
                Book.MakeRow(e, "3", "SubmitPassword", [(submitPasswordButton.Button, 1)]);


                if (newPassCase > 0 || e.Simulated) {
                    //if (newPassCase < 6) 
                    Book.MakeRow(e, "4", message.Text, [(newPasswordBox.Border, 1)], message.Brush);
                    //Book.MakeRow(e, "", [(newPassBlock.View, 1)]);
                }
                if (loggedIn || e.Simulated)
                    Book.MakeRow(e, "5", "ChangePassword", [(changePasswordButton.Button, 1)]);

                break;
            #endregion

            #region Interface
            case TabProfilePage.TabProfileInterface:
                if (!loggedIn)
                    break;
                // Locale
                void clickLocaleBox(object? s, EventArgs e)
                    => OpenRefreshPanel(this, typeof(LocaleSelect));
                var localeBox = MakeButton(s + "localeBox", T + "Locale", LocaleNames[profile.Locale], clickLocaleBox)
                    ?? throw new(F + "languageBox");

                var verbLabel = MakeTextBlock(s + "verbLabel", T + "Verbosity", L("Verbosity")) ?? throw new(F + "verbLabel");

                // SubLocale
                void clickVerbBox(object? s, EventArgs e)
                    => OpenRefreshPanel(this, typeof(SubLocaleSelect));
                var verbBox = MakeButton(s + "verbBox", T + "Verbosity", SubLocaleNames[profile.SubLocale], clickVerbBox)
                    ?? throw new(F + "verbBox");

                // DateFormat
                void clickDateFormat(object? s, EventArgs e)
                     => SetDateFormat(s, e);
                var dateFormat = MakeComboBox(s + "dateFormat", T + "DateFormat",
                    DateFormats, profile.DateFormat,
                    new FuncDataTemplate<string>((mode, _) => new TextBlock { Text = mode }, false), clickDateFormat)
                    ?? throw new(F + "dateFormat");
                dateFormat.Combo.IsEditable = true;

                // Display Mode
                void clickDisplayMode(object? s, EventArgs e)
                    => SetDisplayMode(((IoComboBox?)s)?.SelectedItem);
                var displayBox = MakeComboBox(s + "displayBox", T + "DisplayMode",
                    Enum.GetValues<DisplayMode>(), profile.DisplayMode,
                    new FuncDataTemplate<DisplayMode>((mode, _) 
                        => new TextBlock { Text = GetEnumText(mode) }, false), clickDisplayMode)
                    ?? throw new(F + "displayBox");

                // Display Rotation
                void clickDisplayRotation(object? s, EventArgs e)
                    => SetDisplayRotation(((IoComboBox?)s)?.SelectedItem);
                var displayRotBox = MakeComboBox(s + "displayRotBox", T + "DisplayRotation",
                    new byte[4] { 0, 1, 2, 3 }, profile.DisplayRotation,
                    new FuncDataTemplate<byte>((mode, _)
                        => new TextBlock { Text = (mode*90).ToString()+"°" }, false), clickDisplayRotation)
                    ?? throw new(F + "displayRotBox");

                // TODO resolution if fullscreen (make sure to e.Simulated it!)

                // Show Tooltips
                void changeShowTooltips(object? s, EventArgs e) {
                    profile.ShowTooltips = (ushort?)((IoNumericLeftRight?)s)?.Value ?? DPro.ShowTooltips;
                    //var C = Current ?? throw new("Failed Set ShowTooltips");
                    RefreshAll();
                    SApp.Dirty = true;;
                }
                var showTooltips 
                    = MakeNumeric(s + "showTooltips", T + "ShowTooltips", profile.ShowTooltips, 
                        0, 60000, 10, 10, "Milliseconds", "", "", changeShowTooltips)
                    ?? throw new(F + "showTooltips");

                // Swipe
                void changeSwipeTime(object? s, EventArgs e)
                    => profile.SwipeTime = ((IoNumericLeftRight?)s)?.Value ?? DPro.SwipeTime;
                var swipeTime = MakeNumeric(s + "swipeTime", T + "SwipeTime",
                   (ulong)profile.SwipeTime, 100, 2000, 100, 1, "Milliseconds", "", "", changeSwipeTime) // TODO Setting the time all the way to 0 is glitchy, so for now I only allow 100ms min
                   ?? throw new(F + "swipeTime");
                void changeSwipeSensitivity(object? s, EventArgs e)
                    => profile.SwipeSensitivity = ((IoNumericLeftRight?)s)?.Value ?? DPro.SwipeSensitivity;
                var swipeSensitivity = MakeNumeric(s + "swipeSensitivity", T + "SwipeSensitivity",
                    (ulong)profile.SwipeSensitivity, 0, 300, 1, 10, "Sensitivity", L("SwipeSensitivity"), "", changeSwipeSensitivity)
                   ?? throw new(F + "swipeSensitivity");

                // Masking
                void changeMaskOpacity(object? s, EventArgs e)
                   => profile.MaskOpacity = (byte)(((IoNumericLeftRight?)s)?.Value ?? DPro.MaskOpacity);
                var maskOpacity = MakeNumeric(s + "maskOpacity", T + "MaskOpacity",
                    profile.MaskOpacity, 0, 100, 1, 1, "Opacity", "", "%", changeMaskOpacity)
                    ?? throw new(F + "maskOpacity");

                void changeUnMaskTime(object? s, EventArgs e)
                   => profile.UnMaskTime = (byte)(((IoNumericLeftRight?)s)?.Value ?? DPro.UnMaskTime);
                var unMaskTime = MakeNumeric(s + "unMaskTime", T + "UnMaskTime",
                    profile.UnMaskTime, 1, 1000000, 1, 1, "Milliseconds", "", "ms", changeUnMaskTime)
                    ?? throw new(F + "unMaskTime");

                void changeUnMaskSpeed(object? s, EventArgs e)
                   => profile.UnMaskSpeed = (byte)(((IoNumericLeftRight?)s)?.Value ?? DPro.UnMaskSpeed);
                var unMaskSpeed = MakeNumeric(s + "unMaskSpeed", T + "UnMaskSpeed",
                    profile.UnMaskSpeed, 1, 1000000, 1, 1, "Speed", "UnMaskSpeed", "x", changeUnMaskSpeed)
                    ?? throw new(F + "unMaskSpeed");

                // Title Color
                var titleColor = MakeColorPicker(s + "titleColor", "T_TitleColor", "🎨", TitleColor, ColorPick_ColorChanged) ?? throw new(F + "titleColor");

                // Control Font
                void clickControlFontBox(object? s, EventArgs e)
                    => OpenRefreshPanel(this, typeof(FontControlSelect));
                var controlFontBox = MakeButton(s + "controlFontBox", T + "ControlFont",
                    profile.ControlFont.ToString(), clickControlFontBox)
                    ?? throw new(F + "controlFontBox");
                void clickControlFontSize(object? s, EventArgs e)
                    => SetControlFont(profile.ControlFont, ((IoNumericLeftRight?)s)?.Value ?? DPro.ControlFontSize);
                var controlFontSize = MakeNumeric(s + "controlFontSize", T + "ControlFontSize",
                    (ulong)profile.ControlFontSize, 6, 120, 1, 1, "Size", "", "", clickControlFontSize)
                    ?? throw new(F + "controlFontSize");

                // Info Font
                void clickInfoFontBox(object? s, EventArgs e)
                    => OpenRefreshPanel(this, typeof(FontInfoSelect));
                var infoFontBox = MakeButton(s + "infoFontBox", T + "InfoFont",
                    profile.InfoFont.ToString(), clickInfoFontBox)
                    ?? throw new(F + "infoFontBox");
                void clickInfoFontSize(object? s, EventArgs e)
                    => SetInfoFont(profile.InfoFont, ((IoNumericLeftRight?)s)?.Value ?? DPro.InfoFontSize);
                var infoFontSize = MakeNumeric(s + "infoFontSize", T + "InfoFontSize",
                    (ulong)profile.InfoFontSize, 6, 120, 1, 1, "Size", "", "", clickInfoFontSize)
                    ?? throw new(F + "infoFontSize");



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
                    maskOpacity.Value = DPro.MaskOpacity;
                    unMaskTime.Value = DPro.UnMaskTime;
                    unMaskSpeed.Value = DPro.UnMaskSpeed;
                    swipeSensitivity.Value = (ulong)DPro.SwipeSensitivity;
                    titleColor.Color.Color = DPro.TitleColor;
                    // Reset Language
                    profile.Locale = DPro.Locale;
                    RefreshPanel((typeof(LocaleSelect), ""), true);
                    // Reset Display
                    displayRotBox.Combo.SelectedIndex = DPro.DisplayRotation;
                    displayBox.Combo.SelectedIndex = (int)DPro.DisplayMode;                }
                var resetIntButton = MakeButton(s + "resetIntButton", T + "Reset", "Reset", clickResetInt)
                    ?? throw new(F + "resetButton");

                Book.MakeRow(e, "0", "ResetToDefault", [(resetIntButton.Button, 1)]);
                Book.MakeRow(e, "1", "Locale", [(localeBox.Button, 1), (verbLabel.View, 1), (verbBox.Button, 1)]);
                Book.MakeRow(e, "2", "DateFormat", [(dateFormat.View, 1)]);
                Book.MakeRow(e, "3", "DisplayMode", [(displayBox.View, 1)]);
                Book.MakeRow(e, "4", "DisplayRotation", [(displayRotBox.View, 1)]);
                Book.MakeRow(e, "5", "ShowTooltips", [(showTooltips, 1)]);
                Book.MakeRow(e, "6", "MaskOpacity", [(maskOpacity, 1)]);
                Book.MakeRow(e, "7", "UnMaskTime", [(unMaskTime, 2), (unMaskSpeed, 3)]);
                Book.MakeRow(e, "8", "SwipeTime", [(swipeTime, 2), (swipeSensitivity, 3)]);
                Book.MakeRow(e, "9", "TitleColor", [(titleColor.Button, 1)]);
                Book.MakeRow(e, "A", "ControlFont", [(controlFontBox.Button, 1), (controlFontSize, 2)]);
                Book.MakeRow(e, "B", "InfoFont", [(infoFontBox.Button, 1), (infoFontSize, 2)]);
                

                break;
            #endregion

            #region Performance:
            case TabProfilePage.TabProfilePerformance:
                if (!loggedIn)
                    break;
                // Max Tasks
                void clickMaxTasks(object? s, EventArgs e) {
                    profile.MaxTasks = (ushort?)((IoNumericLeftRight?)s)?.Value ?? DPro.MaxTasks;
                }
                var maxTasks = MakeNumeric(s + "maxTasks", T + "MaxTasks",
                    profile.MaxTasks, 1, MaxTasks, 1, 1, "MaxTasks", "", " / " + MaxTasks.ToString(), clickMaxTasks)
                    ?? throw new(F + "maxTasks");

                // Max Search Tabs
                void clickMaxSearchTabs(object? s, EventArgs e) {
                    profile.MaxSearchTabs = (ushort?)((IoNumericLeftRight?)s)?.Value ?? DPro.MaxSearchTabs;
                }
                var maxSearchTabs = MakeNumeric(s + "maxSearchTabs", T + "MaxSearchTabs",
                    profile.MaxSearchTabs, 1, 64, 1, 1, "MaxSearchTabs", "", " / " + 64.ToString(), clickMaxSearchTabs)
                    ?? throw new(F + "maxSearchTabs");

                // Sort Algo
                var sortAlgoBox = MakeComboBox(s + "sortAlgo", T + "SortAlgo", 
                    Enum.GetValues<SortAlgo>(), profile.SortAlgo,
                    new FuncDataTemplate<SortAlgo>((mode, _) 
                        => new TextBlock { Text = GetEnumText(mode) }, false), SetSortAlgo)
                    ?? throw new(F + "sortAlgoBox");
               
                void clickDefaultToSingle(object? s, EventArgs e) {
                    profile.DefaultToSingle = (ushort)(((IoNumericLeftRight?)s)?.Value ?? DPro.DefaultToSingle);
                }
                var defaultToSingle 
                    = MakeNumeric(s + "defaultToSingle", T + "DefaultToSingle", profile.DefaultToSingle, 
                        9, 20, 1, 1, "Items", "", "", clickDefaultToSingle)
                    ?? throw new(F + "PowerOf2");

                // Sort properties:
                void changePbiSplitChildren(object? s, EventArgs e) {
                    profile.SplitChildren = (ushort)(((IoNumericLeftRight?)s)?.Value ?? DPro.SplitChildren);
                }
                var pbiSplitChilren 
                    = MakeNumeric(s + "pbiSplitChilren", T + "SplitChildren", profile.SplitChildren, 
                        1, 8, 1, 1, "PowerOf2", "", "", changePbiSplitChildren)
                    ?? throw new(F + "pbiSplitChilren");
                
                void changePbiMinMaxBucketSize(object? s, EventArgs e) {
                    profile.MinMaxBucketSize = (ushort)(((IoNumericLeftRight?)s)?.Value ?? DPro.MinMaxBucketSize);
                }
                var pbiMinMaxBucketSize 
                    = MakeNumeric(s + "pbiMinMaxBucketSize", T + "MinMaxBucketSize", profile.MinMaxBucketSize, 
                        7, 12, 1, 1, "PowerOf2", "", "", changePbiMinMaxBucketSize)
                    ?? throw new(F + "pbiMinMaxBucketSize");
                
                void changePbiDistributionTiers(object? s, EventArgs e) {
                    profile.DistributionTiers = (ushort)(((IoNumericLeftRight?)s)?.Value ?? DPro.DistributionTiers);
                }
                var pbiDistributionTiers 
                    = MakeNumeric(s + "pbiDistributionTiers", T + "DistributionTiers", profile.DistributionTiers, 
                        1, 5, 1, 1, "PowerOf2", "", "", changePbiDistributionTiers)
                    ?? throw new(F + "pbiDistributionTiers");
                
                void clickPimSpin(object? s, EventArgs e) {
                    profile.MergeSpin = (ushort)(((IoNumericLeftRight?)s)?.Value 
                        ?? DPro.MergeSpin);
                }
                var pimSpin = MakeNumeric(s + "pimSpin", T + "SpinInstructions", profile.MergeSpin, 
                        5, 9, 1, 4, "Instructions", "", "", clickPimSpin)
                    ?? throw new(F + "pimSpin");
                
                void clickPimVolatile(object? s, EventArgs e) {
                    profile.MergeVolatileBuffer = (ushort)(((IoNumericLeftRight?)s)?.Value 
                        ?? DPro.MergeVolatileBuffer);
                }
                var pimVolatile = MakeNumeric(s + "pimVolatile", T + "VolatileBuffer", profile.MergeVolatileBuffer, 
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
                var resetPerfButton = MakeButton(s + "ResetPerfButton", T + "Reset", "Reset", clickResetPerf)
                    ?? throw new(F + "resetButton");

                // Show Perf Page:
                Book.MakeRow(e, "0", "ResetToDefault", [(resetPerfButton.Button, 1)]);
                Book.MakeRow(e, "1", "MaxTasks", [(maxTasks, 1)]);
                Book.MakeRow(e, "2", "MaxSearchTabs", [(maxSearchTabs, 1)]);
                Book.MakeRow(e, "3", "SortAlgo", [(sortAlgoBox.View, 1)]);
                Book.MakeRow(e, "4", "DefaultToSingle", [(defaultToSingle, 1)]);

                // Show sort properties:
                var sac = e.Simulated ? SortAlgo.P_BucketIntro : (SortAlgo?)sortAlgoBox.Combo.SelectedItem;
                if (sac != null)
                    switch (sac) {
                        case SortAlgo.S_Intro:
                            break;
                        case SortAlgo.P_IntroMerge:
                            Book.MakeRow(e, "5", "SpinInstructions", [(pimSpin, 1)]);
                            Book.MakeRow(e, "6", "VolatileBuffer", [(pimVolatile, 1)]);
                            break;
                        case SortAlgo.P_BucketIntro:
                            Book.MakeRow(e, "7", "SplitChilren", [(pbiSplitChilren, 1)]);
                            Book.MakeRow(e, "8", "MinMaxBucketSize", [(pbiMinMaxBucketSize, 1)]);
                            Book.MakeRow(e, "9", "DistributionTiers", [(pbiDistributionTiers, 1)]);
                            break;
                    }

                break;
            #endregion

                #region Debug
               
            case TabProfilePage.TabProfileDebug:
                if (!loggedIn)
                    break;
                var debugLocale 
                    = MakeButton(s + "debugLocale", T + "DebugLocale", Dis(!profile.DebugLocale),
                        SetDebugLocale, profile.DebugLocale, FontWeight.Normal, "")
                    ?? throw new(F + "debugLocale");
                Book.MakeRow(e, "0", "DebugLocale", [(debugLocale.Button, 1)]);

                break;
            #endregion
        }
        base.FillPage(sender, e);
    }
    #region Setters
    private static void SetDisplayMode(object? modeObject) {
        if (GetProfile is not IoProfile profile 
            || modeObject == null 
            || modeObject is not DisplayMode mode
            || mode == profile.DisplayMode)
            return;
        ResetToHome(profile, mode);
        OpenRefreshPanel(AppMainMenu, typeof(ProfileContent), "", 1);
    }

    void ColorPick_ColorChanged(object? s, EventArgs e) {
        if (GetProfile is not IoProfile profile || s is not ColorPicker cp)
            return;

        var oldColor = profile.TitleColor;
        profile.TitleColor = cp.Color;

        foreach (var p in profile.Pan.LoadedPanels) {
            if (p.Value.TitleColor == oldColor)
                p.Value.TitleColor = profile.TitleColor;
        }
    }

    private static void SetDisplayRotation(object? modeObject) {
        if (GetProfile is not IoProfile profile
            || modeObject == null
            || modeObject is not byte mode
            || mode == profile.DisplayRotation)
            return;
        profile.DisplayRotation = mode;
        AppSave.UpdateRotation();
        Application.Current?.Resources["RotateTransform"] = 90 * mode;
        RefreshSizes();
    }
    internal void SetSortAlgo(object? s, EventArgs e) {
        if (((ComboBox?)s)?.SelectedItem is not object algoObject || algoObject is not SortAlgo Algo)
            return;
        GetProfile?.SortAlgo = Algo;
        RefreshPanel(this, 0, true); // Refresh this 
    }
    internal void SetLocale(string? lang) {
        if(GetProfile is not IoProfile profile || lang == null || lang == profile.Locale || !LocaleNames.ContainsKey(lang))
            return;
        Locale = LocaleData[profile.Locale = lang];
        if (dButtons.TryGetValue("languageBox", out var t))
            t?.Text.Text = LocaleNames[profile.Locale];
        RefreshAll(); // refresh all panels (translate contents)
        foreach (var w in profile.Pan.OpenedWindows)
            w.Value.MyMainPanel.Dock.Panel?.Update(); // Update mainpanels (translates bottom toolbars)
    }
    internal void SetSubLocale(string? lang) {
        if (GetProfile is not IoProfile profile || lang == null || lang == profile.SubLocale || !SubLocaleNames.ContainsKey(lang))
            return;
        SubLocale = SubLocaleData[profile.SubLocale = lang];
        if (dButtons.TryGetValue("languageBox", out var t))
            t?.Text.Text = SubLocaleNames[profile.Locale];
        RefreshAll(); // refresh all panels (translate contents)
        foreach (var w in profile.Pan.OpenedWindows)
            w.Value.MyMainPanel.Dock.Panel?.Update(); // Update mainpanels (translates bottom toolbars)
    }
    internal void SetControlFont(object? familyObject, double size) {
        if (GetProfile is not IoProfile profile || familyObject is not string family || (family == profile.ControlFont && size == profile.ControlFontSize))
            return;
        Application.Current?.Resources["ControlFontFamily"] = new FontFamily(profile.ControlFont = family);
        Application.Current?.Resources["ControlFontSize"] = profile.ControlFontSize = size;
        if(dInputs.TryGetValue("controlFontBox", out var t))
            t?.Text.Text = family.ToString();
        RefreshSizes();
    }
    private static void RefreshSizes() {
        if(GetProfile is IoProfile profile)
            foreach (var o in profile.Pan.LoadedPanels) {
            if (o.Value is IoBookPanel b) {
                b.Book.PrepareLayout(Available);
            }
            o.Value.PrepareLayout(Available);
            o.Value.UpdateMinSize();
        }
    }
    internal void SetInfoFont(object? familyObject, double size) {
        if (GetProfile is not IoProfile profile || familyObject is not string family || (family == profile.InfoFont && size == profile.InfoFontSize))
            return;
        Application.Current?.Resources["InfoFontFamily"] = new FontFamily(profile.InfoFont = family);
        Application.Current?.Resources["InfoFontSize"] = profile.InfoFontSize = size;
        if (dInputs.TryGetValue("infoFontBox", out var t))
            t?.Text.Text = family.ToString();
        RefreshSizes();
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