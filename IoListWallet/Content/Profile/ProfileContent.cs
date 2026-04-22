using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform.Storage;
using IoListWallet.Core.Basic;
using IoListWallet.Core.Books;
using IoListWallet.Core.Panels;
using IoListWallet.IoPanels.Main;
using IoListWallet.Structs.Crypto;
using System;
using System.IO;
using static IoListWallet.App;
using static IoListWallet.Core.Static.AppSave;

namespace IoListWallet.IoPanels.Profile;
internal class ProfileContent : IoBookPanel {
    bool asyncing = false;
    protected enum Shadows : byte {
        None = 0,
        Weak = 1,
        Moderate = 2,
        Heavy = 3
    }
    protected enum TabProfilePage : byte {
        TabProfileProfile,
        TabProfileInterface,
        TabProfilePerformance,
        TabProfileDebug
    }
    protected override string[] GetPageNames() 
        => ["TabProfileProfile", "TabProfileInterface", "TabProfilePerformance", "TabProfileDebug"];
    private IoTextBox? passwordText = null, newPasswordText = null;
    private IoTextBlock? changeButton = null;
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
    private void PleaseLogin(IoBook.FillPageEventArgs e, string s) => Book.MakeRow(e, "X", "", [(MakeTextBlock(s + "pleaseLogin", "", L("PleaseLogin"))?.View!, 1)]);
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
                    if (Directory.Exists(GetProfileDir()))
                        Directory.Delete(GetProfileDir());
                    var p = GetProfile;
                    if (p == null) throw new("No Profile");
                    bool left;
                    do {
                        left = true;
                        foreach (var i in p.Win.OpenedWindows)
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
                        // Restore screens to the profile's save file:
                        CloseAll();
                        StartProfile(profile.CorrectPassword = true);
                        // Reopen this page:
                        //OpenRefreshPanel(TrueParent, GetType());
                        // old raections that didn't restart the screens
                        //passwordText?.Enabled = false;    // lock password box
                        //((IoToggleButton?)s)?.IsEnabled = false;   // lock submit button
                        //RefreshPanel(this, ~0, true);
                        //RefreshPanel(AppMainMenu, ~0, true);
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
                if (loggedIn)
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
                if (!loggedIn) {
                    PleaseLogin(e, s);
                    break;
                }

                // Locale
                void clickLocaleBox(object? s, EventArgs e)
                    => OpenRefreshPanel(this, typeof(LocaleSelect));
                var localeBox = MakeButton(s + "localeBox", T + "Locale", LocaleNames[profile.SelectedLocale], clickLocaleBox)
                    ?? throw new(F + "languageBox");
                var verbLabel = MakeTextBlock(s + "verbLabel", T + "Verbosity", L("Verbosity")) ?? throw new(F + "verbLabel");

                // SubLocale
                void clickVerbBox(object? s, EventArgs e)
                    => OpenRefreshPanel(this, typeof(SubLocaleSelect));
                var verbBox = MakeButton(s + "verbBox", T + "Verbosity", SubLocaleNames[profile.SelectedSubLocale], clickVerbBox)
                    ?? throw new(F + "verbBox");

                // DateFormat
                void setDateFormat(object? s, EventArgs e)
                    => profile.DateFormat = ((IoComboBox?)s)?.Text ?? DPro.DateFormat;
                var dateFormat = MakeComboBox(s + "dateFormat", T + "DateFormat",
                    DateFormats, profile.DateFormat,
                    new FuncDataTemplate<string>((mode, _) => new IoTextBlock { Text = mode }, false), setDateFormat)
                    ?? throw new(F + "dateFormat");
                dateFormat.Combo.IsEditable = true;

                // Display Mode
                void clickDisplayMode(object? s, EventArgs e)
                    => GetProfile?.InterfaceMode = (IMode?)((IoComboBox?)s)?.SelectedItem ?? DPro.InterfaceMode;
                var displayBox = MakeComboBox(s + "displayBox", T + "DisplayMode",
                    Enum.GetValues<IMode>(), profile.InterfaceMode ?? IMode.Fullscreen,
                    new FuncDataTemplate<IMode>((mode, _)
                        => new IoTextBlock { Text = GetEnumText(mode) }, false), clickDisplayMode)
                    ?? throw new(F + "displayBox");

                // Display Rotation
                void clickDisplayRotation(object? s, EventArgs e)
                    => GetProfile?.DisplayRotation = (byte?)((IoComboBox?)s)?.SelectedItem ?? 0;
                var displayRotBox = MakeComboBox(s + "displayRotBox", T + "DisplayRotation",
                    new byte[4] { 0, 1, 2, 3 }, profile.DisplayRotation,
                    new FuncDataTemplate<byte>((mode, _)
                        => new IoTextBlock { Text = (mode * 90).ToString() + "°" }, false), clickDisplayRotation)
                    ?? throw new(F + "displayRotBox");

                void clickBackButton(object? sender, RoutedEventArgs e) {
                    if (asyncing || AppMainPanel == null || TopLevel.GetTopLevel(AppMainPanel) is not TopLevel top)
                        return;
                    var options = new FilePickerOpenOptions {
                        AllowMultiple = false,
                        FileTypeFilter = [new("Images") { Patterns = ["*.png", "*.jpg", "*.jpeg", "*.bmp"] }]
                    };
                    asyncing = true;
                    async void back() {
                        var file = await top.StorageProvider.OpenFilePickerAsync(options);
                        if (file == null || file.Count <= 0)
                            return;
                        using var stream = await file[0].OpenReadAsync();
                        profile.Background = new Bitmap(stream);
                    }
                    back();
                    asyncing = false;
                }
                var backButton = MakeButton(s + "backButton", T + "BackButton", "Load", clickBackButton)
                   ?? throw new(F + "backButton");

                void backOpacityChange(object? s, EventArgs e)
                    => profile.BackgroundOpacity = ((IoNumericLeftRight?)s)?.Value ?? DPro.BackgroundOpacity;
                var backOpacity = MakeNumeric(s + "backOpacity", T + "BackOpacity",
                    profile.BackgroundOpacity, 0, 100, 1, 10, "BackOpacity", L("BackOpacity"), "", backOpacityChange)
                   ?? throw new(F + "backOpacity");

                // Title Color
                void titleUnlock_Toggle(object? s, EventArgs e)
                    => profile.TitleUnlock = ((IoToggleButton?)s)?.IsChecked ?? false;
                var titleUnlock = MakeButton(s + "titleUnlock", T + "TitleUnlock", L(profile.TitleUnlock ? "Individual" : "Unified"), titleUnlock_Toggle, profile.TitleUnlock, FontWeight.Normal, "")
                    ?? throw new(F + "titleUnlock");
                void titleColor_ColorChanged(object? s, EventArgs e)
                    => GetProfile?.TitleColor = ((ColorPicker?)s)?.Color ?? DPro.TitleColor;
                var titleColor = MakeColorPicker(s + "titleColor", "TitleColor", "🎨", TitleColor, titleColor_ColorChanged) ?? throw new(F + "titleColor");


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


                // Control Font
                void clickControlFontBox(object? s, EventArgs e)
                    => OpenRefreshPanel(this, typeof(FontControlSelect));
                var controlFontBox = MakeButton(s + "controlFontBox", T + "ControlFont",
                    profile.ControlFont.ToString(), clickControlFontBox)
                    ?? throw new(F + "controlFontBox");
                void clickControlFontSize(object? s, EventArgs e)
                    => profile.ControlFontSize = ((IoNumericLeftRight?)s)?.Value ?? DPro.ControlFontSize;
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
                    => profile.InfoFontSize = ((IoNumericLeftRight?)s)?.Value ?? DPro.InfoFontSize;
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
                    titleUnlock.Button.IsChecked = DPro.TitleUnlock;
                    titleColor.Button.isColor = profile.TitleColor = DPro.TitleColor;
                    // Reset Language
                    profile.SelectedLocale = DPro.SelectedLocale;
                    RefreshPanel((typeof(LocaleSelect), ""), true);
                    // Reset Display
                    displayRotBox.Combo.SelectedIndex = DPro.DisplayRotation;
                    displayBox.Combo.SelectedIndex = (int)(DPro.InterfaceMode ?? IMode.Fullscreen);
                }
                var resetIntButton = MakeButton(s + "resetIntButton", T + "Reset", "Reset", clickResetInt)
                    ?? throw new(F + "resetButton");

                Book.MakeRow(e, "0", "ResetToDefault", [(resetIntButton.Button, 1)]);
                Book.MakeRow(e, "1", "Locale", [(localeBox.Button, 1), (verbLabel.View, 1), (verbBox.Button, 1)]);
                Book.MakeRow(e, "2", "DateFormat", [(dateFormat.View, 1)]);
                if (Desktop != null) // Only Desktop lifetime can choose window modes
                    Book.MakeRow(e, "3", "DisplayMode", [(displayBox.View, 1)]);
                Book.MakeRow(e, "4", "DisplayRotation", [(displayRotBox.View, 1)]);
                Book.MakeRow(e, "5", "Background", [(backButton.Button, 1), (backOpacity, 1)]);
                Book.MakeRow(e, "A", "TitleColor", [(titleUnlock.Button, 2), (titleColor.Button, 1)]);
                Book.MakeRow(e, "6", "ShowTooltips", [(showTooltips, 1)]);
                if (profile.InterfaceMode != IMode.Fullscreen || e.Simulated) { // fullscreen doesn't mask
                    Book.MakeRow(e, "7", "MaskOpacity", [(maskOpacity, 1)]);
                    Book.MakeRow(e, "8", "UnMaskTime", [(unMaskTime, 2), (unMaskSpeed, 3)]);
                }
                Book.MakeRow(e, "9", "SwipeTime", [(swipeTime, 2), (swipeSensitivity, 3)]);
                Book.MakeRow(e, "B", "ControlFont", [(controlFontBox.Button, 1), (controlFontSize, 2)]);
                Book.MakeRow(e, "C", "InfoFont", [(infoFontBox.Button, 1), (infoFontSize, 2)]);
                

                break;
            #endregion

            #region Performance:
            case TabProfilePage.TabProfilePerformance:
                if (!loggedIn) {
                    PleaseLogin(e, s);
                    break;
                }

                var st = new FuncDataTemplate<Shadows>((mode, _) => new TextBlock { Text = GetEnumText(mode) }, false);
                // Shadow Texts (Toggling this dones't work right yet, so I'm just keeping it on low always, until fixed)
                /*void selectShadowTexts(object? s, EventArgs e)
                    => profile.ShadowTexts = (byte)(((ComboBox?)s)?.SelectedIndex ?? DPro.ShadowTexts);
                var shadowTexts
                   = MakeComboBox(s + "shadowTexts", T + "ShadowTexts", Enum.GetValues<Shadows>(), (Shadows)profile.ShadowTexts, st, selectShadowTexts)
                    ?? throw new(F + "shadowTexts");*/

                // Shadow Buttons
                void selectShadowButtons(object? s, EventArgs e)
                    => profile.ShadowButtons = (byte)(((ComboBox?)s)?.SelectedIndex ?? DPro.ShadowButtons);
                var shadowButtons
                   = MakeComboBox(s + "shadowButtons", T + "ShadowButtons", Enum.GetValues<Shadows>(), (Shadows)profile.ShadowButtons, st, selectShadowButtons)
                    ?? throw new(F + "shadowButtons");

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
                void setSortAlgo(object? s, EventArgs e) 
                    => profile.SortAlgo = (SortAlgo?)((ComboBox?)s)?.SelectedItem ?? DPro.SortAlgo;
                var sortAlgoBox = MakeComboBox(s + "sortAlgo", T + "SortAlgo", 
                    Enum.GetValues<SortAlgo>(), profile.SortAlgo,
                    new FuncDataTemplate<SortAlgo>((mode, _) 
                        => new TextBlock { Text = GetEnumText(mode) }, false), setSortAlgo)
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
                    //shadowTexts.Combo.SelectedIndex = DPro.ShadowTexts;
                    shadowButtons.Combo.SelectedIndex = DPro.ShadowButtons;
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
                //Book.MakeRow(e, "0", "ShadowTexts", [(shadowTexts.View, 1)]);
                Book.MakeRow(e, "1", "ShadowButtons", [(shadowButtons.View, 1)]);
                Book.MakeRow(e, "2", "ResetToDefault", [(resetPerfButton.Button, 1)]);
                Book.MakeRow(e, "3", "MaxTasks", [(maxTasks, 1)]);
                Book.MakeRow(e, "4", "MaxSearchTabs", [(maxSearchTabs, 1)]);
                Book.MakeRow(e, "5", "SortAlgo", [(sortAlgoBox.View, 1)]);
                Book.MakeRow(e, "6", "DefaultToSingle", [(defaultToSingle, 1)]);

                // Show sort properties:
                var sac = e.Simulated ? SortAlgo.P_BucketIntro : (SortAlgo?)sortAlgoBox.Combo.SelectedItem;
                if (sac != null)
                    switch (sac) {
                        case SortAlgo.S_Intro:
                            break;
                        case SortAlgo.P_IntroMerge:
                            Book.MakeRow(e, "7", "SpinInstructions", [(pimSpin, 1)]);
                            Book.MakeRow(e, "8", "VolatileBuffer", [(pimVolatile, 1)]);
                            break;
                        case SortAlgo.P_BucketIntro:
                            Book.MakeRow(e, "9", "SplitChilren", [(pbiSplitChilren, 1)]);
                            Book.MakeRow(e, "A", "MinMaxBucketSize", [(pbiMinMaxBucketSize, 1)]);
                            Book.MakeRow(e, "B", "DistributionTiers", [(pbiDistributionTiers, 1)]);
                            break;
                    }

                break;
            #endregion

                #region Debug
               
            case TabProfilePage.TabProfileDebug:
                if (!loggedIn) {
                    PleaseLogin(e, s);
                    break;
                }
                void setDebugLocale(object? s, RoutedEventArgs e) 
                    => profile.DebugLocale = ((IoToggleButton?)s)?.IsChecked ?? false;
                var debugLocale 
                    = MakeButton(s + "debugLocale", T + "DebugLocale", Dis(!profile.DebugLocale),
                        setDebugLocale, profile.DebugLocale, FontWeight.Normal, "")
                    ?? throw new(F + "debugLocale");
                Book.MakeRow(e, "0", "DebugLocale", [(debugLocale.Button, 1)]);

                break;
            #endregion
        }
        base.FillPage(sender, e);
    }
}