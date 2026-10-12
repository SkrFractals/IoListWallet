using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using IoListWallet.Core.Basic;
using IoListWallet.Core.Static;
using System;
using System.Collections.Generic;
using static IoListWallet.App;
using static IoListWallet.Core.Static.ConstructionHelpers;

namespace IoListWallet.Core;

internal abstract class IoUserControl : UserControl {
    protected readonly Dictionary<string, Grid> dGrids = [];
    protected readonly Dictionary<string, ScrollViewer> dScrolls = [];
    protected readonly Dictionary<string, WrapPanel> dTools = [];
    internal readonly Dictionary<string, WTextBlock> dTexts = [];
    protected readonly Dictionary<string, IoTextBlockBase> sTexts = [];
    protected readonly Dictionary<string, WTextBox> dInputs = [];
    protected readonly Dictionary<string, IoTextBox> dsInputs = [];
    protected readonly Dictionary<string, WToggleButton> dButtons = [];
    protected readonly Dictionary<string, WColorButton> dcButtons = [];
    protected readonly Dictionary<string, WSToggleButton> dSButtons = [];
    protected readonly Dictionary<string, IoNumericLeftRight> dNumerics = [];
    protected readonly Dictionary<string, WCheckBox> dChecks = [];
    protected readonly Dictionary<string, WComboBox> dCombos = [];
    protected readonly Dictionary<string, WDatePicker> dDates = [];

    internal IoUserControl() : base() { }

    internal static string Dis(bool dis) => L(dis ? "Disabled" : "Enabled");
    public Avalonia.Size MinSize { get; protected set; } = new(120, 120);
    internal virtual Avalonia.Size PrepareLayout(Avalonia.Size available) {
        Measure(available);
        return MinSize = DesiredSize;
    }
    #region MakeHelpers

    protected static IoToggleButton MakeToolbarButton(string text, string sure = "") {
        var t = new IoTextBlockControl() { Text = text };
        var s = new IoTextBlockControl() { Text = sure };
        return new IoToggleButton(t, s) {
            Content = t, HorizontalAlignment = HorizontalAlignment.Stretch,
            HorizontalContentAlignment = HorizontalAlignment.Center,
            Text = t.Text
        };
    }
    protected Grid MakeGrid(string key, bool clear = false) {
        if (!dGrids.TryGetValue(key, out var e))
            e = null;
        JustGrid(ref e, clear);
        return dGrids[key] = e!;
    }
    protected ScrollViewer MakeScroll(string key) {
        if (!dScrolls.TryGetValue(key, out var e))
            e = null;
        JustScroll(ref e);
        return dScrolls[key] = e!;
    }
    protected WrapPanel MakeToolbar(string key) {
        if (!dTools.TryGetValue(key, out var e))
            e = null;
        JustWrapPanel(ref e);
        return dTools[key] = e!;
    }
    protected WTextBlock MakeTextBlock(string key, IoUserControl? owner = null, string tooltip = "", string text = "") {
        if (!dTexts.TryGetValue(key, out var e))
            e = null;
        BigTextBlock(ref e, owner, tooltip, text);
        return dTexts[key] = e!;
    }
    protected IoTextBlockBase MakeSmallTextBlock(string key, IoUserControl? owner = null, string tooltip = "", string text = "") {
        if (!sTexts.TryGetValue(key, out var e))
            e = null;
        JustTextBlock(ref e, owner, tooltip, text);
        return sTexts[key] = e!;
    }
    protected WTextBox MakeTextBox(string key, IoUserControl? owner, string tooltip = "", string text = "", string watermarkLocaleId = "", EventHandler<EventArgs>? Response = null, EventHandler<EventArgs>? ResponseChange = null) {
        if (!dInputs.TryGetValue(key, out var e))
            e = null;
        BigTextBox(ref e, owner, tooltip, text, L(watermarkLocaleId), Response, ResponseChange);
        return dInputs[key] = e!;
    }
    protected IoTextBox MakeSmallTextBox(string key, IoUserControl? owner = null, string tooltip = "", string text = "", string watermarkLocaleId = "", EventHandler<EventArgs>? Response = null, EventHandler<EventArgs>? ResponseChange = null) {
        if (!dsInputs.TryGetValue(key, out var e))
            e = null;
        JustTextBox(ref e, owner, tooltip, text, L(watermarkLocaleId), Response, ResponseChange);
        return dsInputs[key] = e!;
    }
    protected WToggleButton MakeButton(string key, IoUserControl? owner = null, string tooltip = "", string text = "", EventHandler<RoutedEventArgs>? Response = null, bool isChecked = false, FontWeight weight = FontWeight.Normal, string? areYouSure = null, object? tag = null) {
        if (!dButtons.TryGetValue(key, out var e))
            e = null;
        BigButton(ref e, owner, tooltip, text, Response, isChecked, weight, areYouSure, tag);
        return dButtons[key] = e!;
    }
    protected WColorButton MakeColorPicker(string key, IoUserControl? owner, string tooltip, string text, Color isColor, EventHandler<ColorChangedEventArgs>? Response = null) {
        if (!dcButtons.TryGetValue(key, out var e))
            e = null;
        BigColorPicker(ref e, owner, tooltip, text, isColor, Response);
        return dcButtons[key] = e!;
    }
    protected WSToggleButton MakeSmallButton(string key, IoUserControl? owner = null, string tooltip = "", string text = "", EventHandler<RoutedEventArgs>? Response = null, bool isChecked = false, FontWeight weight = FontWeight.Normal, string? areYouSure = null, object? tag = null) {
        if (!dSButtons.TryGetValue(key, out var e))
            e = null;
        JustButton(ref e, owner, tooltip, text, Response, isChecked, weight, areYouSure, tag);
        return dSButtons[key] = e!;
    }
    protected IoNumericLeftRight MakeNumeric(string key, IoUserControl? owner = null, string tooltip = "", ulong value = 0, ulong minimum = 0, ulong maximum = 0, ulong increment = 1, ulong speed = 1, string watermarkLocaleId = "", string textLeft = "", string textRight = "", EventHandler<EventArgs>? response = null) {
        if (!dNumerics.TryGetValue(key, out var e))
            e = null;
        JustNumeric(ref e, owner, tooltip, e == null ? value : e.Value, minimum, maximum, increment, speed, watermarkLocaleId, true, textLeft, textRight, response);
        return dNumerics[key] = e!;
    }
    protected WCheckBox MakeCheckBox(string key, IoUserControl? owner = null, string tooltip = "", bool IsChecked = false, EventHandler<EventArgs>? Response = null) {
        if (!dChecks.TryGetValue(key, out var e))
            e = null;
        BigCheckBox(ref e, owner, tooltip, IsChecked, Response);
        return dChecks[key] = e!;
    }
    protected WComboBox MakeComboBox<T>(string key, IoUserControl? owner, string tooltip, IEnumerable<T>? itemSource, T? selectedItem, IDataTemplate? itemTemplate = null, EventHandler<EventArgs>? response = null) {
        if (!dCombos.TryGetValue(key, out var e))
            e = null;
        BigComboBox(ref e, owner, tooltip, itemSource, selectedItem, itemTemplate, response);
        return dCombos[key] = e!;
    }
    protected WComboBox MakeBitmaskBox(string key, IoUserControl? owner, string tooltip, List<byte?> itemSource, IDataTemplate? itemTemplate = null, string classes = "") {
        if (!dCombos.TryGetValue(key, out var e))
            e = null;
        BigComboBox(ref e, owner, tooltip, itemSource, null, itemTemplate, null, true, classes);
        e?.Combo.DontSelect = true;
        return dCombos[key] = e!;
    }
    protected WDatePicker MakeDatePick(string key, IoUserControl? owner, string tooltip, DateTime value, EventHandler<DatePickerSelectedValueChangedEventArgs>? response = null) {
        if (!dDates.TryGetValue(key, out var e))
            e = null;
        BigDatePick(ref e, owner, tooltip, value, response);
        return dDates[key] = e!;
    }
    internal IoToggleButton? GetBitmaskButtons(IoUserControl? owner, string key, byte? bit, ulong mask, Type enumType, EventHandler<RoutedEventArgs> e) => (
        bit is not byte _bit ? MakeSmallButton(key + "Null", owner, "T_" + key + "Null", 
            L(dCombos.TryGetValue(key, out var d) && d?.Combo is ComboBox c && c.IsDropDownOpen ? "Back" : "Toggle"), e)
        //: _bit < 0 ? MakeSmallButton(key + "Back", "T_" + key + "Back", L("Back"), e)
        : Enum.ToObject(enumType, _bit) is Enum entry
        ? MakeSmallButton(key + entry.ToString(), owner, "T_" + key + entry.ToString(),
            GetEnumText(entry), e, AppSave.GetBit(mask, _bit) > 0, FontWeight.Normal, "", _bit)
        : null)?.Button ?? null;
    internal IoToggleButton? GetComboButtons(IoUserControl? owner, string key, string nullBut, byte? bit, IoDockPanels panels, EventHandler<RoutedEventArgs> e) => (
        bit is not byte _bit ? nullBut == "" ? null : MakeSmallButton(key + "Null", owner, "T_" + key + "Null",
            L(dCombos.TryGetValue(key, out var d) && d?.Combo is ComboBox c && c.IsDropDownOpen ? "Back" : nullBut) , e)
        //: _bit < 0 ? MakeSmallButton(key + "Back", "T_" + key + "Back", L("Back"), e)
        : MakeSmallButton(key + _bit.ToString(), owner, "T_" + key + _bit.ToString(),
            FromK(panels.Panel[_bit]) is IoPanel p ? p.GetTitle() : "", e, false, FontWeight.Normal, "", _bit)
        )?.Button ?? null;
    #endregion

    internal virtual void OnTooltip(object? s, ToolTipEventArgs e) {}
    internal virtual void OnTooltipStart(object? s, EventArgs e) { }
    internal virtual void OnTooltipStop(object? s, EventArgs e) { }
    public class ToolTipEventArgs : EventArgs {
        public string tooltip = "";
    }
}

