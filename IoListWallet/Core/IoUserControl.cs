using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using IoListWallet.Core.Basic;
using IoListWallet.Core.Dock;
using IoListWallet.Core.Panels;
using IoListWallet.Core.Static;
using System;
using System.Collections.Generic;
using static IoListWallet.App;
using static IoListWallet.Core.Static.ConstructionHelpers;

namespace IoListWallet.Core;

internal abstract class IoUserControl : UserControl {
    protected readonly Dictionary<string, Grid?> dGrids = [];
    protected readonly Dictionary<string, ScrollViewer?> dScrolls = [];
    protected readonly Dictionary<string, WrapPanel?> dTools = [];
    protected readonly Dictionary<string, (IoTextBlock Text, Viewbox View)?> dTexts = [];
    protected readonly Dictionary<string, IoTextBlock?> sTexts = [];
    protected readonly Dictionary<string, (IoTextBox Text, Viewbox View, Border Border)?> dInputs = [];
    protected readonly Dictionary<string, IoTextBox?> dsInputs = [];
    protected readonly Dictionary<string, (IoTextBlock Text, Viewbox View, IoToggleButton Button)?> dButtons = [];
    protected readonly Dictionary<string, (IoTextBlock Text, Viewbox View, IoColorButton Button)?> dcButtons = [];
    protected readonly Dictionary<string, (IoTextBlock Text, IoToggleButton Button)?> dSButtons = [];
    protected readonly Dictionary<string, IoNumericLeftRight?> dNumerics = [];
    protected readonly Dictionary<string, (IoCheckBox Check, Viewbox View)?> dChecks = [];
    protected readonly Dictionary<string, (IoComboBox Combo, Viewbox View)?> dCombos = [];
    protected readonly Dictionary<string, (IoDatePicker Date, Viewbox View)?> dDates = [];

    internal IoUserControl() : base() { }

    internal static string Dis(bool dis) => L(dis ? "Disabled" : "Enabled");
    public Avalonia.Size MinSize { get; protected set; } = new(120, 120);
    internal virtual Avalonia.Size PrepareLayout(Avalonia.Size available) {
        Measure(available);
        return MinSize = DesiredSize;
    }
    #region MakeHelpers

    protected static IoToggleButton MakeToolbarButton(string text) {
        var t = new IoTextBlock() { Text = text };
        return new IoToggleButton(t) {
            Content = t, HorizontalAlignment = HorizontalAlignment.Stretch,
            HorizontalContentAlignment = HorizontalAlignment.Center,
            Text = t.Text
        };
    }
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
    protected (IoTextBlock Text, Viewbox View)? MakeTextBlock(string key, string tooltip, string text) {
        if (!dTexts.TryGetValue(key, out var e))
            e = null;
        BigTextBlock(ref e, tooltip, text);
        return dTexts[key] = e;
    }
    protected IoTextBlock? MakeSmallTextBlock(string key, string tooltip, string text) {
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
    protected IoTextBox? MakeSmallTextBox(string key, string tooltip = "", string text = "", string watermarkLocaleId = "", EventHandler<EventArgs>? Response = null, EventHandler<EventArgs>? ResponseChange = null) {
        if (!dsInputs.TryGetValue(key, out var e))
            e = null;
        JustTextBox(ref e, tooltip, text, L(watermarkLocaleId), Response, ResponseChange);
        return dsInputs[key] = e;
    }
    protected (IoTextBlock Text, Viewbox View, IoToggleButton Button)? MakeButton(string key, string tooltip = "", string text = "", EventHandler<RoutedEventArgs>? Response = null, bool isChecked = false, FontWeight weight = FontWeight.Normal, string? areYouSure = null, object? tag = null) {
        if (!dButtons.TryGetValue(key, out var e))
            e = null;
        BigButton(ref e, tooltip, text, Response, isChecked, weight, areYouSure, tag);
        return dButtons[key] = e;
    }
    protected (IoTextBlock Text, Viewbox View, IoColorButton Button)? MakeColorPicker(string key, string tooltip, string text, Color isColor, EventHandler<ColorChangedEventArgs>? Response = null) {
        if (!dcButtons.TryGetValue(key, out var e))
            e = null;
        BigColorPicker(ref e, tooltip, text, isColor, Response);
        return dcButtons[key] = e;
    }
    protected (IoTextBlock Text, IoToggleButton Button)? MakeSmallButton(string key, string tooltip = "", string text = "", EventHandler<RoutedEventArgs>? Response = null, bool isChecked = false, FontWeight weight = FontWeight.Normal, string? areYouSure = null, object? tag = null) {
        if (!dSButtons.TryGetValue(key, out var e))
            e = null;
        JustButton(ref e, tooltip, text, Response, isChecked, weight, areYouSure, tag);
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
    protected (IoComboBox Combo, Viewbox View)? MakeComboBox<T>(string key, string tooltip, IEnumerable<T>? itemSource, T? selectedItem, IDataTemplate? itemTemplate = null, EventHandler<EventArgs>? response = null) {
        if (!dCombos.TryGetValue(key, out var e))
            e = null;
        BigComboBox(ref e, tooltip, itemSource, selectedItem, itemTemplate, response);
        return dCombos[key] = e;
    }
    protected (IoComboBox Combo, Viewbox View)? MakeBitmaskBox(string key, string tooltip, List<byte?> itemSource, IDataTemplate? itemTemplate = null, string classes = "") {
        if (!dCombos.TryGetValue(key, out var e))
            e = null;
        BigComboBox(ref e, tooltip, itemSource, null, itemTemplate, null, true, classes);
        return dCombos[key] = e;
    }
    protected (IoDatePicker Date, Viewbox View)? MakeDatePick(string key, string tooltip, DateTime value, EventHandler<DatePickerSelectedValueChangedEventArgs>? response = null) {
        if (!dDates.TryGetValue(key, out var e))
            e = null;
        BigDatePick(ref e, tooltip, value, response);
        return dDates[key] = e;
    }
    internal IoToggleButton? GetBitmaskButtons(string key, byte? bit, ulong mask, Type enumType, EventHandler<RoutedEventArgs> e) => (
        bit is not byte _bit ? MakeSmallButton(key + "Null", "T_" + key + "Null", 
            L(dCombos.TryGetValue(key, out var d) && d?.Combo is ComboBox c && c.IsDropDownOpen ? "Back" : "Toggle"), e)
        //: _bit < 0 ? MakeSmallButton(key + "Back", "T_" + key + "Back", L("Back"), e)
        : Enum.ToObject(enumType, _bit) is Enum entry
        ? MakeSmallButton(key + entry.ToString(), "T_" + key + entry.ToString(),
            GetEnumText(entry), e, AppSave.GetBit(mask, _bit) > 0, FontWeight.Normal, "", _bit)
        : null)?.Button ?? null;
    internal IoToggleButton? GetComboButtons(string key, string nullBut, byte? bit, IoDockPanels panels, EventHandler<RoutedEventArgs> e) => (
        bit is not byte _bit ? nullBut == "" ? null : MakeSmallButton(key + "Null", "T_" + key + "Null",
            L(dCombos.TryGetValue(key, out var d) && d?.Combo is ComboBox c && c.IsDropDownOpen ? "Back" : nullBut) , e)
        //: _bit < 0 ? MakeSmallButton(key + "Back", "T_" + key + "Back", L("Back"), e)
        : MakeSmallButton(key + _bit.ToString(), "T_" + key + _bit.ToString(),
            FromK(panels.Panel[_bit]) is IoPanel p ? p.GetTitle() : "", e, false, FontWeight.Normal, "", _bit)
        )?.Button ?? null;
    #endregion
}

