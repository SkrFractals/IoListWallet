using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Interactivity;
using Avalonia.Media;
using IoListWallet.ElementControls.Basic;
using System;
using System.Collections.Generic;
using static IoListWallet.App;
using static IoListWallet.ConstructionHelpers;

namespace IoListWallet.ElementControls;

internal abstract class IoUserControl : UserControl {

    protected readonly Dictionary<string, Grid?> dGrids = [];
    protected readonly Dictionary<string, ScrollViewer?> dScrolls = [];
    protected readonly Dictionary<string, WrapPanel?> dTools = [];
    protected readonly Dictionary<string, (TextBlock Text, Viewbox View)?> dTexts = [];
    protected readonly Dictionary<string, TextBlock?> sTexts = [];
    protected readonly Dictionary<string, (IoTextBox Text, Viewbox View, Border Border)?> dInputs = [];
    //protected readonly Dictionary<string, (IoPasswordBox Text, Viewbox View, Border Border)?> dPasswords = [];
    protected readonly Dictionary<string, (TextBlock Text, Viewbox View, IoToggleButton Button)?> dButtons = [];
    protected readonly Dictionary<string, (TextBlock Text, IoToggleButton Button)?> dSButtons = [];
    protected readonly Dictionary<string, IoNumericLeftRight?> dNumerics = [];
    protected readonly Dictionary<string, (IoCheckBox Check, Viewbox View)?> dChecks = [];
    protected readonly Dictionary<string, (IoComboBox Combo, Viewbox View)?> dCombos = [];
    protected readonly Dictionary<string, (IoDatePicker Date, Viewbox View)?> dDates = [];

    internal IoUserControl() : base() { }

    internal static string Dis(bool dis) => L(dis ? "Disabled" : "Enabled");

    #region MakeHelpers
    protected Grid? MakeGrid(string key, bool clear = false) {
        if (!dGrids.TryGetValue(key, out var e))
            e = null;
        JustGrid(ref e, clear);
        return dGrids[key] = e;
    }
    protected ScrollViewer? MakeScroll(string key) {
        if (!dScrolls.TryGetValue(key, out var e))
            e = null;
        JustScroll(ref e);
        return dScrolls[key] = e;
    }
    protected WrapPanel? MakeToolbar(string key) {
        if (!dTools.TryGetValue(key, out var e))
            e = null;
        JustWrapPanel(ref e);
        return dTools[key] = e;
    }
    protected (TextBlock Text, Viewbox View)? MakeTextBlock(string key, string tooltip, string text) {
        if (!dTexts.TryGetValue(key, out var e))
            e = null;
        BigTextBlock(ref e, tooltip, text);
        return dTexts[key] = e;
    }
    protected TextBlock? MakeSmallTextBlock(string key, string tooltip, string text) {
        if (!sTexts.TryGetValue(key, out var e))
            e = null;
        JustTextBlock(ref e, tooltip, text);
        return sTexts[key] = e;
    }
    protected (IoTextBox Text, Viewbox View, Border Border)? MakeTextBox(string key, string tooltip = "", string text = "", string watermarkLocaleId = "", EventHandler<EventArgs>? Response = null, EventHandler<EventArgs>? ResponseChange = null) {
        if (!dInputs.TryGetValue(key, out var e))
            e = null;
        BigTextBox(ref e, tooltip, text, L(watermarkLocaleId), Response, ResponseChange);
        return dInputs[key] = e;
    }
    /*protected (IoPasswordBox Text, Viewbox View, Border Border)? MakePasswordBox(string key, string tooltip = "", string text = "", string watermarkLocaleId = "", EventHandler<EventArgs>? Response = null, EventHandler<EventArgs>? ResponseChange = null) {
        if (!dPasswords.TryGetValue(key, out var e))
            e = null;
        BigPasswordBox(ref e, tooltip, text, L(watermarkLocaleId), Response, ResponseChange);
        return dPasswords[key] = e;
    }*/
    protected (TextBlock Text, Viewbox View, IoToggleButton Button)? MakeButton(string key, string tooltip = "", string text = "", EventHandler<RoutedEventArgs>? Response = null, bool isChecked = false, FontWeight weight = FontWeight.Normal, string? areYouSure = null) {
        //if (button.Count <= buttons)
        //    button.AddRange(new (TextBlock? Text, Viewbox?, Button? Button)[buttons - button.Count + 1]);
        if (!dButtons.TryGetValue(key, out var e))
            e = null;
        BigButton(ref e, tooltip, text, Response, isChecked, weight, areYouSure);
        return dButtons[key] = e;
    }
    /*protected (TextBlock Text, Viewbox View, IoToggleButton Button, Viewbox Round)? MakeRoundButton(string key, string tooltip = "", string text = "", EventHandler<RoutedEventArgs>? Response = null, bool isChecked = false, FontWeight weight = FontWeight.Normal, bool uncheckOnClick = true) {
        if (!rButtons.TryGetValue(key, out var e))
            e = null;
        BigRoundButton(ref e, tooltip, text, Response, isChecked, weight, uncheckOnClick);
        return rButtons[key] = e;
    }*/
    protected (TextBlock Text, IoToggleButton Button)? MakeSmallButton(string key, string tooltip = "", string text = "", EventHandler<RoutedEventArgs>? Response = null, bool isChecked = false, FontWeight weight = FontWeight.Normal, string? areYouSure = null) {
        if (!dSButtons.TryGetValue(key, out var e))
            e = null;
        JustButton(ref e, tooltip, text, Response, isChecked, weight, areYouSure);
        return dSButtons[key] = e;
    }
    protected IoNumericLeftRight? MakeNumeric(string key, string tooltip = "", ulong value = 0, ulong minimum = 0, ulong maximum = 0, ulong increment = 1, ulong speed = 1, string watermarkLocaleId = "", string textLeft = "", string textRight = "", EventHandler<EventArgs>? response = null) {
        if (!dNumerics.TryGetValue(key, out var e))
            e = null;
        JustNumeric(ref e, tooltip, e == null ? value : e.Value, minimum, maximum, increment, speed, watermarkLocaleId, true, textLeft, textRight, response);
        return dNumerics[key] = e;
    }
    protected (IoCheckBox Check, Viewbox View)? MakeCheckBox(string key, string tooltip = "", bool IsChecked = false, EventHandler<EventArgs>? Response = null) {
        if (!dChecks.TryGetValue(key, out var e))
            e = null;
        BigCheckBox(ref e, tooltip, IsChecked, Response);
        return dChecks[key] = e;
    }
    //doesn't like T? SelectedItem = null
    protected (IoComboBox Combo, Viewbox View)? MakeComboBox<T>(string key, string tooltip, IEnumerable<T>? itemSource, T? selectedItem, IDataTemplate? itemTemplate = null, EventHandler<EventArgs>? response = null) {
        if (!dCombos.TryGetValue(key, out var e))
            e = null;
        BigComboBox(ref e, tooltip, itemSource, selectedItem, itemTemplate, response); // doens't like 4th argument because null
        return dCombos[key] = e;
    }
    protected (IoDatePicker Date, Viewbox View)? MakeDatePick(string key, string tooltip, DateTime value, EventHandler<DatePickerSelectedValueChangedEventArgs>? response = null) {
        if (!dDates.TryGetValue(key, out var e))
            e = null;
        BigDatePick(ref e, tooltip, value, response);
        return dDates[key] = e;
    }
    #endregion
}

