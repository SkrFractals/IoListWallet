using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform.Storage;
using IoListWallet.Content.Abstract;
using IoListWallet.Core.Basic;
using IoListWallet.Core.Books;
using IoListWallet.Content.Main;
using IoListWallet.Structs.Crypto;
using System;
using System.IO;
using static IoListWallet.App;
using static IoListWallet.Core.Static.AppSave;

namespace IoListWallet.Content.Profile;
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
        TabProfileCustom,
        TabProfilePerformance,
        TabProfileDebug
    }
    protected override string[] GetPageNames() 
        => ["TabProfileProfile", "TabProfileInterface", "TabProfileCustom", "TabProfilePerformance", "TabProfileDebug"];
    private IoTextBox? passwordText = null, newPasswordText = null;
    private IoTextBlockBase? changeButton = null;
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
    private void PleaseLogin(IoBook.FillPageEventArgs e, string s) => Book.MakeRow(e, "X", "", [(MakeTextBlock(s + "pleaseLogin", this, "", L("PleaseLogin"))?.View!, 1)]);
    protected override void FillPage(object? sender, IoBook.FillPageEventArgs e) {
        if (GetProfile is not IoProfile profile)
            return;
        byte i = 0;
        string k() => (++i).ToString();
        var s = e.Simulated ? "s." : "";
        var loggedIn = IsCorrectPassword();
        switch ((TabProfilePage)e.Page.X) {

            #region Profile
            case TabProfilePage.TabProfileProfile:

                // Delete Profile Button
                void clickProfileDelete(object? s, EventArgs e) {
                    if (Directory.Exists(GetProfileDir()))
                        Directory.Delete(GetProfileDir());
                    var p = GetProfile ?? throw new("No Profile");
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

                var profileDeleteButton = MakeButton(s + "profileDeleteButton", this, T + "ProfileDeleteButton",
                    L("Delete"), clickProfileDelete, false, FontWeight.Bold, L("AreYouSure"));

                // Open Profile Save Location Button
                void clickProfileSave(object? s, EventArgs e) => OpenDirectory(GetProfileDir());
                var profileSaveButton = MakeButton(s + "profileSaveButton", this, T + "ProfileSaveButton",
                   L("Open"), clickProfileSave);

                // Profile Name
                void changeNameBox(object? s, EventArgs e) {
                    profile.Name = ((TextBox?)s)?.Text ?? DPro.Name;
                    RefreshPanel((typeof(MainMenu), ""), true);
                }
                var nameBox = MakeTextBox(s + "nameBox", this, T + "Name", profile.Name, "Name", changeNameBox);

                // LOGIN -----------------------------------------------------

                // Enter Pass Block

                var submitMessage = SetPassBlock(loggedIn);
                var passBlock = MakeTextBlock(s + "passBlock", this, "", submitMessage.Text);
                passBlock.Text.Foreground = submitMessage.Brush;

                // Enter Pass Box
                //void changePasswordBox(object? s, EventArgs e)
                //    => profile.Password = ((TextBox?)s)?.Text ?? "";
                var passwordBox = MakeTextBox(s + "passwordBox", this, T + "Password", profile.Password, "Password");
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

                var submitPasswordButton = MakeButton(s + "submitPasswordButton", this, T + "SubmitPasswordButton",
                    L("SubmitPasswordButton"), clickSetPass);
                submitPasswordButton.Button.IsEnabled = !loggedIn;

                // CHANGE PASSWORD -----------------------------------------------------

                // Enter New Pass Block
                var message = SetNewPassBlock();

                // Enter New Pass Box
                var newPasswordBox = MakeTextBox(s + "newPasswordBox", this, T + changeWatermark, "", changeWatermark);
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
                var changePasswordButton = MakeButton(s + "changePasswordButton", this, T + "ChangePasswordButton",
                    L(changeButtonText), clickChangePass);
                changeButton = changePasswordButton.Text;
                // LAYOUT
                Book.MakeRow(e, k(), "ProfileSave", [(profileDeleteButton.Button, 1), (profileSaveButton.Button, 1)], null, loggedIn);
                Book.MakeRow(e, k(), "ProfileName", [(nameBox.Border, 1)]);
                Book.MakeRow(e, k(), "LoginStatus", [(passBlock.View, 1)]);
                Book.MakeRow(e, k(), "ProfilePassword", [(passwordBox.Border, 1)]);
                Book.MakeRow(e, k(), "SubmitPassword", [(submitPasswordButton.Button, 1)]);
                Book.MakeRow(e, k(), message.Text, [(newPasswordBox.Border, 1)], message.Brush, newPassCase > 0 || e.Simulated);
                Book.MakeRow(e, k(), "ChangePassword", [(changePasswordButton.Button, 1)], null, loggedIn || e.Simulated);

                break;
            #endregion

            #region Interface
            case TabProfilePage.TabProfileInterface:
                if (!loggedIn) {
                    PleaseLogin(e, s);
                    break;
                }

                // Locale
                string getLocaleBoxText() => LocaleNames[profile.SelectedLocale];
                void clickLocaleBox(object? s, EventArgs e)
                    => OpenRefreshPanel(this, typeof(LocaleSelect));
                var localeBox = MakeButton(s + "localeBox", this, T + "Locale", getLocaleBoxText(), clickLocaleBox);
                var verbLabel = MakeTextBlock(s + "verbLabel", this, T + "Verbosity", L("Verbosity"));

                // SubLocale
                void clickVerbBox(object? s, EventArgs e)
                    => OpenRefreshPanel(this, typeof(SubLocaleSelect));
                var verbBox = MakeButton(s + "verbBox", this, T + "Verbosity", SubLocaleNames[profile.SelectedSubLocale], clickVerbBox);

                // DateFormat
                void setDateFormat(object? s, EventArgs e)
                    => profile.DateFormat = ((IoComboBox?)s)?.Text ?? DPro.DateFormat;
                var dateFormat = MakeComboBox(s + "dateFormat", this, T + "DateFormat",
                    DateFormats, profile.DateFormat,
                    new FuncDataTemplate<string>((mode, _) => new IoTextBlockControl { Text = mode }, false), setDateFormat);
                dateFormat.Combo.IsEditable = true;

                /*string[] sThousanths = ["<None>", "-", ",", "."];
                string[] sDecimals = [".", ",", "-", "|"];
                string[] sThousands = [",", "<Space>", "<None>", "-", "."];*/

                IoUnit u = new(5, "Unit");
                string[] thousanths = ["", "-", ",", "."];
                string[] decimals = [ ".", ",", "-", "|"];
                string[] thousands = [",", " ", "", "-", "."];
                var numberText = MakeTextBlock(s + "numberText", this, T + "NumberFormat", u.ToString(123456789, true));
                string Shake(string[] list) {
                    foreach (var t in list)
                        if (t != profile.Thousanth && t != profile.Decimal && t != profile.Thousand) 
                            return t;
                    return "";
                }
                void setThousanth(object? s, EventArgs e) {
                    var i = ((IoComboBox?)s)?.SelectedIndex;
                    profile.Thousanth = (i == null ? ((IoComboBox?)s)?.Text : thousanths[i.Value]) ?? DPro.Thousanth;
                    numberText.Text.Text = u.ToString(123456789, true);
                    if (profile.Decimal == profile.Thousanth)
                        profile.Decimal = Shake(decimals);
                    if (profile.Thousand == profile.Thousanth)
                        profile.Thousand = Shake(thousands);
                }
                void setDecimal(object? s, EventArgs e) {
                    var i = ((IoComboBox?)s)?.SelectedIndex;
                    profile.Decimal = (i == null ? ((IoComboBox?)s)?.Text : decimals[i.Value]) ?? DPro.Decimal;
                    numberText.Text.Text = u.ToString(123456789, true);
                    if (profile.Thousanth == profile.Decimal)
                        profile.Thousanth = Shake(thousanths);
                    if (profile.Thousand == profile.Decimal)
                        profile.Thousand = Shake(thousands);
                }
                void setThousand(object? s, EventArgs e) {
                    var i = ((IoComboBox?)s)?.SelectedIndex;
                    profile.Thousand = (i == null ? ((IoComboBox?)s)?.Text : thousands[i.Value]) ?? DPro.Thousand;
                    numberText.Text.Text = u.ToString(123456789, true);
                    if (profile.Thousanth == profile.Thousand)
                        profile.Thousanth = Shake(thousanths);
                    if (profile.Decimal == profile.Thousand)
                        profile.Decimal = Shake(decimals);
                }
                var format = new FuncDataTemplate<string>((mode, _) => new IoTextBlockControl { Text = mode switch { "" => "<" + L("None") + ">", " " => "<" + L("Space") + ">", _ => mode } }, false);
                var thousanthBox = MakeComboBox(s + "thousanthBox", this, T + "ThousanthBox", thousanths, profile.Thousanth, format, setThousanth);
                var decimalBox = MakeComboBox(s + "decimalBox", this, T + "DecimalBox", decimals, profile.Decimal, format, setDecimal);
                var thousandBox = MakeComboBox(s + "thousandBox", this, T + "ThousandBox", thousands, profile.Thousand, format, setThousand);
               
                // Display Mode
                void clickDisplayMode(object? s, EventArgs e)
                    => profile.InterfaceMode = (IMode?)((IoComboBox?)s)?.SelectedItem ?? DPro.InterfaceMode;
                var displayBox = MakeComboBox(s + "displayBox", this, T + "DisplayMode",
                    Enum.GetValues<IMode>(), profile.InterfaceMode ?? IMode.Fullscreen,
                    new FuncDataTemplate<IMode>((mode, _)
                        => new IoTextBlockControl { Text = GetEnumText(mode) }, false), clickDisplayMode);

                // Control Font
                void clickControlFontBox(object? s, EventArgs e)
                    => OpenRefreshPanel(this, typeof(FontControlSelect));
                var controlFontBox = MakeButton(s + "controlFontBox", this, T + "ControlFont",
                    profile.ControlFont.ToString(), clickControlFontBox);
                void clickControlFontSize(object? s, EventArgs e)
                    => profile.ControlFontSize = ((IoNumericLeftRight?)s)?.Value ?? DPro.ControlFontSize;
                var controlFontSize = MakeNumeric(s + "controlFontSize", this, T + "ControlFontSize",
                    (ulong)profile.ControlFontSize, 6, 120, 1, 1, "Size", "", "", clickControlFontSize);

                // Info Font
                void clickInfoFontBox(object? s, EventArgs e)
                    => OpenRefreshPanel(this, typeof(FontInfoSelect));
                var infoFontBox = MakeButton(s + "infoFontBox", this, T + "InfoFont",
                    profile.InfoFont.ToString(), clickInfoFontBox);
                void clickInfoFontSize(object? s, EventArgs e)
                    => profile.InfoFontSize = ((IoNumericLeftRight?)s)?.Value ?? DPro.InfoFontSize;
                var infoFontSize = MakeNumeric(s + "infoFontSize", this, T + "InfoFontSize",
                    (ulong)profile.InfoFontSize, 6, 120, 1, 1, "Size", "", "", clickInfoFontSize);

                // Reset Settings
                void clickResetInt(object? s, EventArgs e) {
                    dateFormat.Combo.Text = DPro.DateFormat;
                    // Reset Fonts
                    controlFontBox.Text.Text = (profile.ControlFont = DPro.ControlFont).ToString();
                    infoFontBox.Text.Text = (profile.InfoFont = DPro.InfoFont).ToString();
                    controlFontSize.Value = (ulong)DPro.ControlFontSize;
                    infoFontSize.Value = (ulong)DPro.InfoFontSize;
                    // Reset Language
                    profile.SelectedLocale = DPro.SelectedLocale;
                    localeBox.Text.Text = getLocaleBoxText();
                    RefreshPanel((typeof(LocaleSelect), ""), true);
                    // Reset Display
                    displayBox.Combo.SelectedIndex = (int)(DPro.InterfaceMode ?? IMode.Fullscreen);
                }
                var resetIntButton = MakeButton(s + "resetIntButton", this, T + "Reset", "Reset", clickResetInt);

                Book.MakeRow(e, k(), "ResetToDefault", [(resetIntButton.Button, 1)]);
                Book.MakeRow(e, k(), "Locale", [(localeBox.Button, 1), (verbLabel.View, 1), (verbBox.Button, 1)]);
                Book.MakeRow(e, k(), "DateFormat", [(dateFormat.View, 1)]);
                Book.MakeRow(e, k(), "NumberFormat", [(numberText.View, 1), (thousanthBox.View, 1), (decimalBox.View, 1), (thousandBox.View,1)]);

                // Only Desktop lifetime can choose window modes
                Book.MakeRow(e, k(), "DisplayMode", [(displayBox.View, 1)], null, Desktop != null);
                
                Book.MakeRow(e, k(), "ControlFont", [(controlFontBox.Button, 1), (controlFontSize, 2)]);
                Book.MakeRow(e, k(), "InfoFont", [(infoFontBox.Button, 1), (infoFontSize, 2)]);
                break;
            #endregion

            #region Custom
            case TabProfilePage.TabProfileCustom:
                if (!loggedIn) {
                    PleaseLogin(e, s);
                    break;
                }

                // Display Rotation
                void clickDisplayRotation(object? s, EventArgs e)
                    => profile.DisplayRotation = (byte?)((IoComboBox?)s)?.SelectedItem ?? 0;
                var displayRotBox = MakeComboBox(s + "displayRotBox", this, T + "DisplayRotation",
                    new byte[4] { 0, 1, 2, 3 }, profile.DisplayRotation,
                    new FuncDataTemplate<byte>((mode, _)
                        => new IoTextBlockControl { Text = (mode * 90).ToString() + "°" }, false), clickDisplayRotation);

                // Bakground Image
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
                var backButton = MakeButton(s + "backButton", this, T + "BackButton", "Load", clickBackButton);
                void backOpacityChange(object? s, EventArgs e)
                    => profile.BackgroundOpacity = ((IoNumericLeftRight?)s)?.Value ?? DPro.BackgroundOpacity;
                var backOpacity = MakeNumeric(s + "backOpacity", this, T + "BackOpacity",
                    profile.BackgroundOpacity, 0, 100, 1, 10, "BackOpacity", L("BackOpacity"), "", backOpacityChange);
                void changeBackStretch(object? s, EventArgs e)
                    => profile.BackgroundStretch = (Stretch?)((IoComboBox?)s)?.SelectedItem ?? DPro.BackgroundStretch;
                var backStretch = MakeComboBox(s + "backStretch", this, T + "BackStretch",
                    Enum.GetValues<Stretch>(), profile.BackgroundStretch,
                    new FuncDataTemplate<Stretch>((mode, _)
                        => new IoTextBlockControl { Text = GetEnumText(mode) }, false), changeBackStretch);

                // Title Color
                string TitleUnlockText() => L(profile.TitleUnlock ? "Individual" : "Unified");
                void titleUnlock_Toggle(object? s, EventArgs e) {
                    var b = (IoToggleButton?)s;
                    profile.TitleUnlock = b?.IsChecked ?? false;
                    b?.Text = TitleUnlockText();
                }
                var titleUnlock = MakeButton(s + "titleUnlock", this, T + "TitleUnlock", TitleUnlockText(), titleUnlock_Toggle, profile.TitleUnlock, FontWeight.Normal, "");
                void titleColor_ColorChanged(object? s, EventArgs e)
                    => profile.TitleColor = ((ColorPicker?)s)?.Color ?? DPro.TitleColor;
                var titleColor = MakeColorPicker(s + "titleColor", this, T + "TitleColor", "🎨", TitleColor, titleColor_ColorChanged);

                // Title Buttons
                void titleText_Toggle(object? s, EventArgs e)
                    => profile.TitleText = ((IoToggleButton?)s)?.IsChecked ?? false;
                var titleText = MakeButton(s + "titleText", this, T + "TitleText", L("CustomTitleText"), titleText_Toggle, profile.TitleText, FontWeight.Normal, "");
                void pins_Toggle(object? s, EventArgs e)
                    => profile.Pins = ((IoToggleButton?)s)?.IsChecked ?? false;
                var pins = MakeButton(s + "pins", this, T + "Pins", L("Pins"), pins_Toggle, profile.Pins, FontWeight.Normal, "");
                void homes_Toggle(object? s, EventArgs e)
                   => profile.Homes = ((IoToggleButton?)s)?.IsChecked ?? false;
                var homes = MakeButton(s + "homes", this, T + "Homes", L("Homes"), homes_Toggle, profile.Homes, FontWeight.Normal, "");

                // TODO resolution if fullscreen (make sure to e.Simulated it!)

                // Show Tooltips
                void changeShowTooltips(object? s, EventArgs e) {
                    profile.ShowTooltips = (ushort?)((IoNumericLeftRight?)s)?.Value ?? DPro.ShowTooltips;
                    //var C = Current ?? throw new("Failed Set ShowTooltips");
                    RefreshAll();
                    SApp.Dirty = true; ;
                }
                var showTooltips
                    = MakeNumeric(s + "showTooltips", this, T + "ShowTooltips", profile.ShowTooltips,
                        0, 60000, 10, 10, "Milliseconds", "", "", changeShowTooltips);

                // Swipe
                void changeSwipeTime(object? s, EventArgs e)
                    => profile.SwipeTime = ((IoNumericLeftRight?)s)?.Value ?? DPro.SwipeTime;
                var swipeTime = MakeNumeric(s + "swipeTime", this, T + "SwipeTime",
                   (ulong)profile.SwipeTime, 100, 2000, 100, 1, "Milliseconds", "", "", changeSwipeTime); // TODO Setting the time all the way to 0 is glitchy, so for now I only allow 100ms min
                void changeSwipeSensitivity(object? s, EventArgs e)
                    => profile.SwipeSensitivity = ((IoNumericLeftRight?)s)?.Value ?? DPro.SwipeSensitivity;
                var swipeSensitivity = MakeNumeric(s + "swipeSensitivity", this, T + "SwipeSensitivity",
                    (ulong)profile.SwipeSensitivity, 0, 300, 1, 10, "Sensitivity", L("SwipeSensitivity"), "", changeSwipeSensitivity);

                // Masking
                void changeMaskOpacity(object? s, EventArgs e)
                   => profile.MaskOpacity = (byte)(((IoNumericLeftRight?)s)?.Value ?? DPro.MaskOpacity);
                var maskOpacity = MakeNumeric(s + "maskOpacity", this, T + "MaskOpacity",
                    profile.MaskOpacity, 0, 100, 1, 1, "Opacity", "", "%", changeMaskOpacity);

                void changeUnMaskTime(object? s, EventArgs e)
                   => profile.UnMaskTime = (byte)(((IoNumericLeftRight?)s)?.Value ?? DPro.UnMaskTime);
                var unMaskTime = MakeNumeric(s + "unMaskTime", this, T + "UnMaskTime",
                    profile.UnMaskTime, 1, 1000000, 1, 1, "Milliseconds", "", "ms", changeUnMaskTime);

                void changeUnMaskSpeed(object? s, EventArgs e)
                   => profile.UnMaskSpeed = (byte)(((IoNumericLeftRight?)s)?.Value ?? DPro.UnMaskSpeed);
                var unMaskSpeed = MakeNumeric(s + "unMaskSpeed", this, T + "UnMaskSpeed",
                    profile.UnMaskSpeed, 1, 1000000, 1, 1, "Speed", "UnMaskSpeed", "x", changeUnMaskSpeed);

                // Reset Settings
                void clickResetCus(object? s, EventArgs e) {
                    showTooltips.Value = DPro.ShowTooltips;
                    swipeTime.Value = (ulong)DPro.SwipeTime;
                    maskOpacity.Value = DPro.MaskOpacity;
                    unMaskTime.Value = DPro.UnMaskTime;
                    unMaskSpeed.Value = DPro.UnMaskSpeed;
                    swipeSensitivity.Value = (ulong)DPro.SwipeSensitivity;
                    titleUnlock.Button.IsChecked = DPro.TitleUnlock;
                    titleText.Button.IsChecked = DPro.TitleText;
                    pins.Button.IsChecked = DPro.Pins;
                    homes.Button.IsChecked = DPro.Homes;
                    backStretch.Combo.SelectedItem = DPro.BackgroundStretch;
                    backOpacity.Value = DPro.BackgroundOpacity;
                    titleColor.Button.isColor = profile.TitleColor = DPro.TitleColor;
                    displayRotBox.Combo.SelectedIndex = DPro.DisplayRotation;
                }
                var resetCusButton = MakeButton(s + "resetCusButton", this, T + "Reset", "Reset", clickResetCus);

                Book.MakeRow(e, k(), "ResetToDefault", [(resetCusButton.Button, 1)]);
                Book.MakeRow(e, k(), "DisplayRotation", [(displayRotBox.View, 1)]);
                Book.MakeRow(e, k(), "Background", [(backButton.Button, 1), (backOpacity, 1), (backStretch.View, 2)]);
                Book.MakeRow(e, k(), "TitleColor", [(titleUnlock.Button, 2), (titleColor.Button, 1)]);
                Book.MakeRow(e, k(), "Toolbar", [(titleText.Button, 2), (pins.Button, 1), (homes.Button, 1)]);
                Book.MakeRow(e, k(), "ShowTooltips", [(showTooltips, 1)]);

                // fullscreen doesn't mask
                bool show = profile.InterfaceMode != IMode.Fullscreen || e.Simulated;
                Book.MakeRow(e, k(), "MaskOpacity", [(maskOpacity, 1)], null, show);
                Book.MakeRow(e, k(), "UnMaskTime", [(unMaskTime, 2), (unMaskSpeed, 3)], null, show);

                Book.MakeRow(e, k(), "SwipeTime", [(swipeTime, 2), (swipeSensitivity, 3)]);
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
                void selectShadowTexts(object? s, EventArgs e)
                    => profile.ShadowTexts = (byte)(((ComboBox?)s)?.SelectedIndex ?? DPro.ShadowTexts);
                var shadowTexts
                   = MakeComboBox(s + "shadowTexts", this, T + "ShadowTexts", Enum.GetValues<Shadows>(), (Shadows)profile.ShadowTexts, st, selectShadowTexts);

                // Shadow Buttons
                void selectShadowButtons(object? s, EventArgs e)
                    => profile.ShadowButtons = (byte)(((ComboBox?)s)?.SelectedIndex ?? DPro.ShadowButtons);
                var shadowButtons
                   = MakeComboBox(s + "shadowButtons", this, T + "ShadowButtons", Enum.GetValues<Shadows>(), (Shadows)profile.ShadowButtons, st, selectShadowButtons);

                // Max Tasks
                void clickMaxTasks(object? s, EventArgs e) => profile.MaxTasks = (ushort?)((IoNumericLeftRight?)s)?.Value ?? DPro.MaxTasks;
                var maxTasks = MakeNumeric(s + "maxTasks", this, T + "MaxTasks",
                    profile.MaxTasks, 1, MaxTasks, 1, 1, "MaxTasks", "", " / " + MaxTasks.ToString(), clickMaxTasks);

                // Max Search Tabs
                void clickMaxSearchTabs(object? s, EventArgs e) => profile.MaxSearchTabs = (ushort?)((IoNumericLeftRight?)s)?.Value ?? DPro.MaxSearchTabs;
                var maxSearchTabs = MakeNumeric(s + "maxSearchTabs", this, T + "MaxSearchTabs",
                    profile.MaxSearchTabs, 1, 64, 1, 1, "MaxSearchTabs", "", " / " + 64.ToString(), clickMaxSearchTabs);

                // Sort Algo
                void setSortAlgo(object? s, EventArgs e) 
                    => profile.SortAlgo = (SortAlgo?)((ComboBox?)s)?.SelectedItem ?? DPro.SortAlgo;
                var sortAlgoBox = MakeComboBox(s + "sortAlgo", this, T + "SortAlgo",
                    Enum.GetValues<SortAlgo>(), profile.SortAlgo,
                    new FuncDataTemplate<SortAlgo>((mode, _)
                        => new TextBlock { Text = GetEnumText(mode) }, false), setSortAlgo);
               
                void clickDefaultToSingle(object? s, EventArgs e) => profile.DefaultToSingle = (ushort)(((IoNumericLeftRight?)s)?.Value ?? DPro.DefaultToSingle);
                var defaultToSingle
                    = MakeNumeric(s + "defaultToSingle", this, T + "DefaultToSingle", profile.DefaultToSingle,
                        9, 20, 1, 1, "Items", "", "", clickDefaultToSingle);

                // Sort properties:
                void changePbiSplitChildren(object? s, EventArgs e) => profile.SplitChildren = (ushort)(((IoNumericLeftRight?)s)?.Value ?? DPro.SplitChildren);
                var pbiSplitChilren
                    = MakeNumeric(s + "pbiSplitChilren", this, T + "SplitChildren", profile.SplitChildren,
                        1, 8, 1, 1, "PowerOf2", "", "", changePbiSplitChildren);
                
                void changePbiMinMaxBucketSize(object? s, EventArgs e) => profile.MinMaxBucketSize = (ushort)(((IoNumericLeftRight?)s)?.Value ?? DPro.MinMaxBucketSize);
                var pbiMinMaxBucketSize
                    = MakeNumeric(s + "pbiMinMaxBucketSize", this, T + "MinMaxBucketSize", profile.MinMaxBucketSize,
                        7, 12, 1, 1, "PowerOf2", "", "", changePbiMinMaxBucketSize);
                
                void changePbiDistributionTiers(object? s, EventArgs e) => profile.DistributionTiers = (ushort)(((IoNumericLeftRight?)s)?.Value ?? DPro.DistributionTiers);
                var pbiDistributionTiers
                    = MakeNumeric(s + "pbiDistributionTiers", this, T + "DistributionTiers", profile.DistributionTiers,
                        1, 5, 1, 1, "PowerOf2", "", "", changePbiDistributionTiers);
                
                void clickPimSpin(object? s, EventArgs e) => profile.MergeSpin = (ushort)(((IoNumericLeftRight?)s)?.Value 
                        ?? DPro.MergeSpin);
                var pimSpin = MakeNumeric(s + "pimSpin", this, T + "SpinInstructions", profile.MergeSpin,
                        5, 9, 1, 4, "Instructions", "", "", clickPimSpin);
                
                void clickPimVolatile(object? s, EventArgs e) => profile.MergeVolatileBuffer = (ushort)(((IoNumericLeftRight?)s)?.Value 
                        ?? DPro.MergeVolatileBuffer);
                var pimVolatile = MakeNumeric(s + "pimVolatile", this, T + "VolatileBuffer", profile.MergeVolatileBuffer,
                        4, 7, 1, 10, "PowerOf2", "", "", clickPimVolatile);
                
                // Reset Settings
                void clickResetPerf(object? s, EventArgs e) {
                    shadowTexts.Combo.SelectedIndex = DPro.ShadowTexts;
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
                var resetPerfButton = MakeButton(s + "ResetPerfButton", this, T + "Reset", "Reset", clickResetPerf);

                // Show Perf Page:
                Book.MakeRow(e, k(), "ShadowTexts", [(shadowTexts.View, 1)]);
                Book.MakeRow(e, k(), "ShadowButtons", [(shadowButtons.View, 1)]);
                Book.MakeRow(e, k(), "ResetToDefault", [(resetPerfButton.Button, 1)]);
                Book.MakeRow(e, k(), "MaxTasks", [(maxTasks, 1)]);
                Book.MakeRow(e, k(), "MaxSearchTabs", [(maxSearchTabs, 1)]);
                Book.MakeRow(e, k(), "SortAlgo", [(sortAlgoBox.View, 1)]);
                Book.MakeRow(e, k(), "DefaultToSingle", [(defaultToSingle, 1)]);

                // Show sort properties:
                var sac = e.Simulated ? SortAlgo.P_BucketIntro : (SortAlgo?)sortAlgoBox.Combo.SelectedItem;
                if (sac != null)
                    switch (sac) {
                        case SortAlgo.S_Intro:
                            break;
                        case SortAlgo.P_IntroMerge:
                            Book.MakeRow(e, "M" + k(), "SpinInstructions", [(pimSpin, 1)]);
                            Book.MakeRow(e, "M" + k(), "VolatileBuffer", [(pimVolatile, 1)]);
                            break;
                        case SortAlgo.P_BucketIntro:
                            Book.MakeRow(e, "B" + k(), "SplitChilren", [(pbiSplitChilren, 1)]);
                            Book.MakeRow(e, "B" + k(), "MinMaxBucketSize", [(pbiMinMaxBucketSize, 1)]);
                            Book.MakeRow(e, "B" + k(), "DistributionTiers", [(pbiDistributionTiers, 1)]);
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
                string DebugLocaleText() => Dis(!profile.DebugLocale);
                void setDebugLocale(object? s, RoutedEventArgs e) {
                    var b = (IoToggleButton?)s;
                    profile.DebugLocale = b?.IsChecked ?? false;
                    b?.Text = DebugLocaleText();
                }
                var debugLocale
                    = MakeButton(s + "debugLocale", this, T + "DebugLocale", DebugLocaleText(),
                        setDebugLocale, profile.DebugLocale, FontWeight.Normal, "");
                Book.MakeRow(e, k(), "DebugLocale", [(debugLocale.Button, 1)]);

                break;
            #endregion
        }
        base.FillPage(sender, e);
    }
}