using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using IoListWallet.Core.Basic;
using System;
using System.Collections.Generic;
using static IoListWallet.Core.Static.AppSave;
using static IoListWallet.App;

namespace IoListWallet.Core.Static;

internal static class ConstructionHelpers {
    #region Basics
    internal static void JustGrid(ref Grid? inst, bool clear = false) {
        inst ??= new Grid {
            ClipToBounds = false,
            ColumnDefinitions = [],
            RowDefinitions = [],
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch
        };
        inst.ColumnDefinitions.Clear();
        if (clear)
            inst.Children.Clear();
        inst.Tag = new List<Control>();
        inst.RowDefinitions.Clear();
    }
    internal static void JustWrapPanel(ref WrapPanel? inst) {
        inst ??= new WrapPanel {
            ClipToBounds = false,
            Orientation = Orientation.Horizontal,
            Margin = new Thickness(0),
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Top
        };
        inst.Tag = new List<Control>();
    }
    internal static void JustScroll(ref ScrollViewer? inst) {
        inst ??= new ScrollViewer {
            ClipToBounds = false,
            VerticalAlignment = VerticalAlignment.Stretch,
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
            HorizontalScrollBarVisibility = ScrollBarVisibility.Auto
        };
    }
    internal static void JustTextBlock(ref IoTextBlockBase? inst, IoUserControl? owner = null, string tooltip = "", string text = "", FontWeight weight = FontWeight.Normal) {
        if (inst != null) {
            inst.Text = text;
            inst.Tooltip.Tooltip = tooltip;
            inst.FontWeight = weight;
        } else {
            inst = new IoTextBlockControl {
                Text = text,
                ClipToBounds = false,
                FontWeight = weight,
                TextAlignment = TextAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center
            };
            SetupTooltip(ref inst.Tooltip, owner, tooltip);
        }
        //if (tooltip != "")
        //    SetTip(inst, L(tooltip));
    }
    internal static void JustTextBlockInfo(ref IoTextBlockBase? inst, IoUserControl? owner = null, string tooltip = "", string text = "", FontWeight weight = FontWeight.Normal) {
        if (inst != null) {
            inst.Text = text;
            inst.Tooltip.Tooltip = tooltip;
            inst.FontWeight = weight;
        } else {
            inst = new IoTextBlockInfo {
                Text = text,
                ClipToBounds = false,
                FontWeight = weight,
                TextAlignment = TextAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center
            };
            SetupTooltip(ref inst.Tooltip, owner, tooltip);
        }
        //if (tooltip != "")
        //    SetTip(inst, L(tooltip));
    }
    internal static void JustNumeric(ref IoNumericLeftRight? inst, IoUserControl? owner = null, string tooltip = "", ulong value = 0, ulong minimum = 0, ulong maximum = 0, ulong increment = 1, ulong speed = 1, string watermarkLocaleId = "", bool stretch = true, string textLeft = "", string textRight = "", EventHandler<EventArgs>? response = null) {
        if (inst != null) {
            inst.SetResponse(response);
            inst.Trigger = false;
            inst.Minimum = minimum;
            inst.Maximum = maximum;
            inst.Value = value;
            inst.Increment = increment;
            inst.Stretch = stretch;
            inst.TextLeft = textLeft;
            inst.TextRight = textRight;
            inst.Speed = speed;
            inst.Tooltip = tooltip;
            inst.NumberBox?.Text.Tooltip.Tooltip = tooltip;
            inst.Refresh();
            inst.Trigger = true;
        } else {
            inst = new IoNumericLeftRight(owner, tooltip, value, minimum, maximum, increment, speed, watermarkLocaleId, stretch, textLeft, textRight, response);
        }
        //if (tooltip != "")
        //    SetTip(inst, L(tooltip));
    }
    internal static void JustDatePick(ref IoDatePicker? inst, IoUserControl? owner, string tooltip, DateTime value, EventHandler<DatePickerSelectedValueChangedEventArgs>? response = null) {
        if (inst != null) {
            inst.Trigger = false;
            inst.SelectedDate = value;
            inst.Tooltip.Tooltip = tooltip;
            inst.Trigger = true;
        } else {
            inst = new IoDatePicker {
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center,
                SelectedDate = value
            };
            SetupTooltip(ref inst.Tooltip, owner, tooltip);
        }
        inst.SetResponse(response);
        //if (tooltip != "")
        //    SetTip(inst, L(tooltip));
    }
    internal static void JustCheckBox(ref IoCheckBox? inst, IoUserControl? owner, string tooltip, EventHandler<EventArgs>? response, bool isChecked) {
        if (inst != null) {
            inst.Trigger = false;
            inst.IsChecked = isChecked;
            inst.Tooltip.Tooltip = tooltip;
            inst.Trigger = true;
        } else {
            inst = new IoCheckBox {
                ClipToBounds = false,
                VerticalContentAlignment = VerticalAlignment.Center,
                HorizontalContentAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center,
                IsChecked = isChecked
            };
            SetupTooltip(ref inst.Tooltip, owner, tooltip);
        }
        inst.SetResponse(response);
        //if (tooltip != "")
        //    SetTip(inst, L(tooltip));
    }
    internal static void JustComboBox<T>(ref IoComboBox? inst, IoUserControl? owner, string tooltip, IEnumerable<T>? itemSource, T? selectedItem, IDataTemplate? itemTemplate = null, EventHandler<EventArgs>? response = null, bool bitmask = false, string classes = "") {
        if (inst != null) {
            inst.Trigger = false;
            inst.ItemsSource = itemSource; // i wrote here i can't update this, but i must for panelBook!
            inst.SelectedItem = selectedItem;
            inst.ItemTemplate = itemTemplate;
            inst.Tooltip.Tooltip = tooltip;
            inst.Trigger = true;
        } else {
            var i = inst = new IoComboBox {
                ItemsSource = itemSource,
                ClipToBounds = false,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
                ItemTemplate = itemTemplate,
                SelectedItem = selectedItem
            };
            if (classes != "")
                i.Classes.Add(classes);
            //HoldTip(inst);
            inst.DropDownOpened += SetPointerOverlay;
            inst.DropDownClosed += ResetPointerOverlay;
            if (bitmask)
                inst.AddHandler(InputElement.PointerPressedEvent, (sender, e) => {
                    if (e.Source is ComboBoxItem)
                        e.Handled = true;
                }, RoutingStrategies.Tunnel);
            SetupTooltip(ref inst.Tooltip, owner, tooltip);
        }
        inst.SetResponse(response);
        //if (tooltip != "")
        //    SetTip(inst, L(tooltip));
    }
    #endregion

    #region Wraps
    internal static void WrapViewbox(ref Viewbox? inst, Control control) {
        inst ??= new Viewbox {
            ClipToBounds = false,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch,
            Stretch = Stretch.Uniform,
            StretchDirection = StretchDirection.UpOnly,
            Child = control
        };
    }
    internal static void WrapBorder(ref Border? inst, Control control) {
        inst ??= new Border {
                ClipToBounds = false,
                BorderBrush = Brushes.Gray, // Match your arrow button colors
                BorderThickness = new Thickness(2),
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
                Child = control
            };
    }
    // areYouSure == "": the button will toggle, null: will reset to false and need to be set isChecked after, "XXX": will toggle to are you sure text, timeout back to normal, only clicking when checked will fire guraded clicked event
    internal static void WrapButton(ref IoToggleButton? inst, IoTextBlockBase text, IoTextBlockBase sure, Control control, IoUserControl? owner, string tooltip, bool isChecked = false, EventHandler<RoutedEventArgs>? response = null, string? areYouSure = null, object? tag = null) {
        if (inst != null) {
            inst.Trigger = false;
            inst.IsChecked = isChecked;
            inst.AreYouSure = areYouSure;
            inst.Text = text.Text ?? "";
            inst.Tag = tag;
            inst.Tooltip.Tooltip = tooltip;
            inst.Trigger = true;
        } else {
            inst = new IoToggleButton(text, sure) {
                Content = control,
                ClipToBounds = false,
                //CornerRadius = "{Binding $self.ActualHeight}",
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
                HorizontalContentAlignment = HorizontalAlignment.Stretch,
                VerticalContentAlignment = VerticalAlignment.Stretch,
                AreYouSure = areYouSure,
                IsChecked = isChecked,
                Tag = tag,
                Text = text.Text ?? ""
                /*Effect = new DropShadowEffect {
                    Color = Colors.Black,
                    BlurRadius = 4,
                    Opacity = 1.0,
                    OffsetX = 2,
                    OffsetY = 2
                }*/
            };
            SetupTooltip(ref inst.Tooltip, owner, tooltip);
        }
        inst.SetResponse(response);
    }
    internal static void WrapColorButton(ref IoColorButton? inst, Control control, IoUserControl? owner, string tooltip, Color isColor, EventHandler<ColorChangedEventArgs>? response = null) {
        if (inst != null) {
            inst.Tooltip.Tooltip = tooltip;
            inst.isColor = isColor;
        } else {
            inst = new IoColorButton() {
                Content = control,
                ClipToBounds = false,
                //CornerRadius = "{Binding $self.ActualHeight}",
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
                HorizontalContentAlignment = HorizontalAlignment.Stretch,
                VerticalContentAlignment = VerticalAlignment.Stretch,
                isColor = isColor
            };
            SetupTooltip(ref inst.Tooltip, owner, tooltip);
        }
        inst.SetResponse(response);
    }
    #endregion

    #region Composites
    internal static void JustTextBox(ref IoTextBox? inst, IoUserControl? owner = null, string tooltip = "", string text = "", string watermarkLocaleId = "", EventHandler<EventArgs>? response = null, EventHandler<EventArgs>? responseChange = null) {
        if (inst != null) {
            inst.Trigger = false;
            inst.Text = text;
            inst.Watermark = watermarkLocaleId;
            inst.Tooltip.Tooltip = tooltip;
            inst.Trigger = true;
        } else {
            inst = new IoTextBox {
                Text = text,
                ClipToBounds = false,
                Watermark = watermarkLocaleId,
                TextAlignment = TextAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalContentAlignment = VerticalAlignment.Center,
                HorizontalContentAlignment = HorizontalAlignment.Center,
                Padding = new Thickness(0),
                BorderThickness = new Thickness(0)
            };
            SetupTooltip(ref inst.Tooltip, owner, tooltip);
            if (response != null) {
                inst.KeyDown += static (sender, e) => {
                    if (e.Key == Key.Enter && sender is TextBox tb) {
                        // Clears focus of this textbox after hitting enter, which will furhter tigger submit of the edited text
                        // TODO DEPRECATED This deprecated call is the only thing that works like I want. Hopefully we'll figure out a different and long-term supported way!
                        var topLevel = TopLevel.GetTopLevel(tb);
                        topLevel?.FocusManager?.ClearFocus();
                        //topLevel?.Focus();
                        //if (tb.GetVisualRoot() is IInputElement inputRoot) inputRoot.Focus();
                    }
                };
            }
        }
        inst.SetResponse(response);
        inst.SetResponseChange(responseChange);
        //if (tooltip != "")
        //    SetTip(inst, L(tooltip));
    }
    internal static void JustButton(ref WSToggleButton? inst, IoUserControl? owner = null, string tooltip = "", string text = "", EventHandler<RoutedEventArgs>? response = null, bool isChecked = false, FontWeight weight = FontWeight.Normal, string? areYouSure = null, object? tag = null) {
        var t = inst?.Text;
        JustTextBlock(ref t, owner, "", text, weight);
        if (t == null)
            return;
        var s = inst?.Sure;
        JustTextBlock(ref s, owner, "", areYouSure ?? "", weight);
        if (s == null)
            return;
        var p = inst?.Panel;
        if (p == null) {
            p = new();
            p.Children.Add(t);
            p.Children.Add(s);
        }
        var b = inst?.Button;
        WrapButton(ref b, t, s, p, owner, tooltip, isChecked, response, areYouSure, tag);
        if (b == null)
            return;
        inst ??= new(t, s, p, b);
    }

    internal static void BigTextBlock(ref WTextBlock? inst, IoUserControl? owner = null, string tooltip = "", string text = "", FontWeight weight = FontWeight.Normal) {
        var t = inst?.Text;
        JustTextBlock(ref t, owner, tooltip, text, weight);
        if (t == null)
            return;
        var v = inst?.View;
        WrapViewbox(ref v, t);
        if (v == null)
            return;
        //if (tooltip != "")
        //    SetTip(v, L(tooltip));
        inst ??= new(t, v);
    }
    internal static void BigCheckBox(ref WCheckBox? inst, IoUserControl? owner = null, string tooltip = "", bool isChecked = false, EventHandler<EventArgs>? response = null) {
        var c = inst?.Check;
        JustCheckBox(ref c, owner, tooltip, response, isChecked);
        if (c == null)
            return;
        var v = inst?.View;
        WrapViewbox(ref v, c);
        if (v == null)
            return;
        //if (tooltip != "")
        //    SetTip(v, L(tooltip));
        inst ??= new(c, v);
    }
    internal static void BigDatePick(ref WDatePicker? inst, IoUserControl? owner, string tooltip, DateTime value, EventHandler<DatePickerSelectedValueChangedEventArgs>? response = null) {
        var d = inst?.Date;
        JustDatePick(ref d, owner, tooltip, value, response);
        if (d == null)
            return;
        var v = inst?.View;
        WrapViewbox(ref v, d);
        if (v == null)
            return;
        //if (tooltip != "")
        //    SetTip(v, L(tooltip));
        inst ??= new(d, v);
    }
    internal static void BigTextBox(ref WTextBox? inst, IoUserControl? owner = null, string tooltip = "", string text = "", string watermarkLocaleId = "", EventHandler<EventArgs>? response = null, EventHandler<EventArgs>? responseChange = null) {
        var t = inst?.Text;
        JustTextBox(ref t, owner, tooltip, text, watermarkLocaleId, response, responseChange);
        if (t == null)
            return;
        var v = inst?.View;
        WrapViewbox(ref v, t);
        if (v == null)
            return;
        var b = inst?.Border;
        WrapBorder(ref b, v);
        if (b == null)
            return;
        //if (tooltip != "")
        //    SetTip(b, L(tooltip));
        inst ??= new(t, v, b);
    }
    internal static void BigButton(ref WToggleButton? inst, IoUserControl? owner = null, string tooltip = "", string text = "", EventHandler<RoutedEventArgs>? response = null, bool isChecked = false, FontWeight weight = FontWeight.Normal, string? areYouSure = null, object? tag = null) {
        var t = inst?.Text;
        JustTextBlock(ref t, null, "", text, weight);
        if (t == null)
            return;
        var s = inst?.Sure;
        JustTextBlock(ref s, null, "", areYouSure ?? "", weight);
        if (s == null)
            return;
        var p = inst?.Panel;
        if (p == null) {
            p = new();
            p.Children.Add(t);
            p.Children.Add(s);
        }
        var v = inst?.View;
        WrapViewbox(ref v, p);
        if (v == null)
            return;
        var b = inst?.Button;
        WrapButton(ref b, t, s, v, owner, tooltip, isChecked, response, areYouSure, tag);
        if (b == null)
            return;
        //if (tooltip != "")
        //SetTip(b, L(tooltip));
        inst ??= new(t, s, v, p, b);
    }
    internal static void BigColorPicker(ref WColorButton? inst, IoUserControl? owner, string tooltip, string text, Color isColor, EventHandler<ColorChangedEventArgs>? response = null) {
        var t = inst?.Text;
        JustTextBlock(ref t, null, "", text, FontWeight.Normal);
        if (t == null)
            return;
        var v = inst?.View;
        WrapViewbox(ref v, t);
        if (v == null)
            return;
        var b = inst?.Button;
        WrapColorButton(ref b, v, owner, tooltip, isColor, response);
        if (b == null)
            return;
        //if (tooltip != "")
        //    SetTip(b, L(tooltip));

        /*var c = inst?.Color;
        if (c == null) {
            c = new() { Color = isColor };
            c.ColorChanged += response;
            b.Flyout = new Flyout() { Content = c };
        } else c.Color = isColor;*/
        inst ??= new(t, v, b);
    }
    internal static void BigComboBox<T>(ref WComboBox? inst, IoUserControl? owner, string tooltip, IEnumerable<T>? itemSource, T? selectedItem, IDataTemplate? itemTemplate = null, EventHandler<EventArgs>? response = null, bool bitmask = false, string classes = "") {
        var c = inst?.Combo;
        JustComboBox(ref c, owner, tooltip, itemSource, selectedItem, itemTemplate, response, bitmask, classes);
        if (c == null)
            return;
        var v = inst?.View;
        WrapViewbox(ref v, c);
        if (v == null)
            return;
        //if (tooltip != "")
        //    SetTip(v, L(tooltip));
        inst ??= new(c, v);
    }
    #endregion

    #region Brushes
    internal static IBrush MakeGradBrush(Color c) => new LinearGradientBrush {
        StartPoint = new RelativePoint(0, 0, RelativeUnit.Relative),
        EndPoint = new RelativePoint(0, 1, RelativeUnit.Relative),
        GradientStops = {
            new GradientStop(Color.FromRgb((byte)(c.R*0.5),(byte)(c.G*0.5),(byte)(c.B*0.5)), 0.0),
            new GradientStop(c, 0.5) }
    };
    internal static IBrush MakeLeftShade() => new LinearGradientBrush {
        StartPoint = new RelativePoint(0, 0, RelativeUnit.Relative),
        EndPoint = new RelativePoint(1, 0, RelativeUnit.Relative),
        GradientStops = {
            new GradientStop(Color.FromArgb(128,0,0,0), 0.0),
            new GradientStop(Color.FromArgb(0,0,0,0), 1.0) }};

    internal static IBrush MakeRightShade() => new LinearGradientBrush {
        StartPoint = new RelativePoint(0, 0, RelativeUnit.Relative),
        EndPoint = new RelativePoint(1, 0, RelativeUnit.Relative),
        GradientStops = {
            new GradientStop(Color.FromArgb(0,0,0,0), 0.0),
            new GradientStop(Color.FromArgb(128,0,0,0), 1.0) }
    };

    internal static IBrush MakeBottomShade() => new LinearGradientBrush {
        StartPoint = new RelativePoint(0, 0, RelativeUnit.Relative),
        EndPoint = new RelativePoint(0, 1, RelativeUnit.Relative),
        GradientStops = {
            new GradientStop(Color.FromArgb(0,0,0,0), 0.0),
            new GradientStop(Color.FromArgb(128,0,0,0), 1.0) }
    };
    #endregion

    #region Helpers
    private static void SetupTooltip(ref IoTooltips inst, IoUserControl? owner = null, string tooltip = "") {
        inst.Tooltip = tooltip;
        if (owner == null)
            return;
        inst.OnLongClick += owner.OnTooltip;
        inst.OnLongHoldStart += owner.OnTooltipStart;
        inst.OnLongHoldStop += owner.OnTooltipStop;
    }
    internal static void CancelToggle(object? o, RoutedEventArgs e)
           => ((IoToggleButton?)o)?.IsChecked = false;
    internal static void AddChild(Panel g, Control c) {
        var a = g.Tag as List<Control> ?? [];
        a.Add(c);
    }
    internal static void FinishChildren(Panel g) {
        // tried to update the children list while keeping any unchanged children attached if possible. (no detaching and reattaching of the same child)
        var a = g.Tag as List<Control> ?? [];
        for (int i = g.Children.Count; 0 <= --i;)
            if (!a.Contains(g.Children[i]))
                g.Children.RemoveAt(i);

        int gi = g.Children.Count;
        for (int i = gi; --i >= 0;) {
            var index = a.IndexOf(g.Children[i]);
            if (index < gi)
                gi = index;
            else
                g.Children.RemoveAt(i);
        }

        gi = 0;
        for (int i = 0; i < a.Count; ++i) {
            if (gi >= g.Children.Count || g.Children[gi] != a[i])
                g.Children.Insert(gi, a[i]);
            ++gi;
        }
    }
    internal static void SetTip(Control c, string tip) {
        var t = (GetProfile?.ShowTooltips ?? DPro.ShowTooltips);
        if (t <= 0) {
            ToolTip.SetTip(c, null);
            return;
        }

        // TODO this is broken and clips the text wrong at 90 and 270 rotations!
        var tipContent = new IoTextBlockInfo {
            Text = L(tip),
            RenderTransform = new RotateTransform { Angle = (GetProfile?.DisplayRotation ?? 0) * 90 },
            RenderTransformOrigin = new RelativePoint(0.5, 0.5, RelativeUnit.Relative)
        };
        // force layout swap BEFORE tooltip measures itself
        //tipContent.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
        //var size = tipContent.DesiredSize;*/
        // this dones't work either:
        //LongPressTooltip.Attach(c, t, tipContent);

        ToolTip.SetTip(c, tipContent);
        ToolTip.SetShowDelay(c, t);
    }
    #endregion

    #region Pointer Overlay
    private static void SetPointerOverlay(object? s, StyledElement c, bool set) {
        while (true) {
            if (c is IoPanel p) {
                if (set)
                    p.AddOverlayPointer(s);
                else
                    p.RemoveOverlayPointer(s);
                break;
            }
            if (c.Parent == null)
                return;
            c = c.Parent;
        }
    }
    private static void SetPointerOverlay(object? s, EventArgs e) {
        if (s is StyledElement c)
            SetPointerOverlay(s, c, true);
    }
    private static void ResetPointerOverlay(object? s, EventArgs e) {
        if (s is StyledElement c)
            SetPointerOverlay(s, c, false);
    }
    #endregion
}

/*private static void HoldTip(Control? c) {
    if (c == null)
        return;

    // TODO this is broken, the pressed event refuses to get triggered, and the lost is getting called all the time
    // also the timer in App.cs doens't correctly sho any tooltips even when this works...
    c.PointerPressed += (s, _) => {
        App.TipTimer = 3;
        App.TipSource = s;
    };
    c.PointerReleased += (s, _) => {
        if (App.TipSource == s)
            App.TipSource = null;
    };
    c.PointerCaptureLost += (s, _) => {
        if (App.TipSource == s)
            App.TipSource = null;
    };
}
public static class LongPressTooltip {
    private const int DelayMs = 500;
    static readonly Dictionary<Control, EventHandler<PointerPressedEventArgs>> controls = [];

    public static void Attach(Control control, double time, IoTextBlockInfo tooltipText) {
        DispatcherTimer? timer = null;
        bool triggered = false;
        EventHandler<PointerPressedEventArgs> pp = (_, e) => {
            triggered = false;

            timer = new DispatcherTimer {
                Interval = TimeSpan.FromMilliseconds(time)
            };

            timer.Tick += (_, _) => {
                timer?.Stop();
                triggered = true;

                // Show tooltip manually
                ToolTip.SetTip(control, tooltipText);
                ToolTip.SetIsOpen(control, true);
            };

            timer.Start();
        };
        if (controls.TryGetValue(control, out var c)) {
            control.PointerPressed -= c;
            control.PointerPressed += controls[control] = pp;
            return;
        }
        control.PointerPressed += controls[control] = pp;

        void Cancel() {
            timer?.Stop();
            timer = null;

            if (triggered)
                ToolTip.SetIsOpen(control, false);
        }

        control.PointerReleased += (_, __) => Cancel();
        control.PointerCaptureLost += (_, __) => Cancel();
        control.PointerMoved += (_, e)
        =>
        {
            // optional: cancel if finger drifts too far
            if (e.GetCurrentPoint(control).Properties.IsLeftButtonPressed == false)
                Cancel();
        };
    }
}*/