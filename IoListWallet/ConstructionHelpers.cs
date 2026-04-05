using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Layout;
using Avalonia.Media;
using IoListWallet.ElementControls.Basic;
using System;
using System.Collections.Generic;
using static IoListWallet.AppSave;
using static IoListWallet.App;
using Avalonia.Interactivity;
using IoListWallet.Structs.Crypto;
using Avalonia.Input;

namespace IoListWallet;

internal static class ConstructionHelpers {
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
    internal static void AddChild(Panel g, Control c) {
        var a = g.Tag as List<Control> ?? [];
        a.Add(c);
        /*int i = (int?)g.Tag ?? 0;
        while (i < g.Children.Count && g.Children[i] != c)
            g.Children.RemoveAt(i);
        g.Children.Add(c);*/
    }
    internal static void FinishChildren(Panel g) {
        var a = g.Tag as List<Control> ?? [];
        for (int i = g.Children.Count; 0 <= --i;)
            if (!a.Contains(g.Children[i]))
                g.Children.RemoveAt(i);

        // Simple (doesn't keep order)
        /*for (int i = a.Count; 0 <= --i;)
            if (!g.Children.Contains(a[i])) {
                if (a[i].Parent is Panel p)
                    p.Children.Remove(a[i]);
                //if (a[i].Tag is Controls c)
                //    c.Remove(a[i]);
                g.Children.Add(a[i]);
                //a[i].Tag = g.Children;
            }*/

        // ???
        /*int gi = -1;
        foreach (var inside in g.Children) {
            var index = a.IndexOf(inside);
            if (index > gi) {
                gi = index;
            } else {
                gi = -1;
                break;
            }
        }*/

        // LATEST: bugged when changing book elements

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
    internal static void JustWrapPanel(ref WrapPanel? inst) {
        inst ??= new WrapPanel {
            ClipToBounds = false,
            Orientation = Orientation.Horizontal,
            Margin = new Thickness(10),
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Top
        };
        inst.Tag = new List<Control>();
        //inst.Children.Clear();
    }
    internal static void JustScroll(ref ScrollViewer? inst) {
        inst ??= new ScrollViewer {
            ClipToBounds = false,
            VerticalAlignment = VerticalAlignment.Stretch,
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
            HorizontalScrollBarVisibility = ScrollBarVisibility.Auto
        };
    }
    internal static void JustTextBlock(ref TextBlock? inst, string tooltip, string text, FontWeight weight = FontWeight.Normal) {
        if (inst != null) {
            inst.Text = text;
            inst.FontWeight = weight;
        } else {
            inst = new TextBlock {
                Text = text,
                ClipToBounds = false,
                FontWeight = weight,
                TextAlignment = TextAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center
            };
        }
        if (tooltip != "")
            SetTip(inst, L(tooltip));
    }
    internal static void JustNumeric(ref IoNumericLeftRight? inst, string tooltip, ulong value = 0, ulong minimum = 0, ulong maximum = 0, ulong increment = 1, ulong speed = 1, string watermarkLocaleId = "", bool stretch = true, string textLeft = "", string textRight = "", EventHandler<EventArgs>? response = null) {
        if (inst != null) {
            inst.Trigger = false;
            inst.Minimum = minimum;
            inst.Maximum = maximum;
            inst.Value = value;
            inst.Increment = increment;
            inst.Stretch = stretch;
            inst.TextLeft = textLeft;
            inst.TextRight = textRight;
            inst.Speed = speed;
            inst.Refresh();
            inst.Trigger = true;
        } else {
            inst = new IoNumericLeftRight(tooltip, value, minimum, maximum, increment, speed, watermarkLocaleId, stretch, textLeft, textRight, response);
        }
        if (tooltip != "")
            SetTip(inst, L(tooltip));
    }
    internal static void JustDatePick(ref IoDatePicker? inst, string tooltip, DateTime value, EventHandler<DatePickerSelectedValueChangedEventArgs>? response = null) {
        if (inst != null) {
            inst.Trigger = false;
            inst.SelectedDate = value;
            inst.Trigger = true;
        } else {
            inst = new IoDatePicker {
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center,
                SelectedDate = value
            };
            if (response != null)
                inst.SelectedDateChangedGuarded += response;
        }
        if (tooltip != "")
            SetTip(inst, L(tooltip));
    }
    internal static void JustCheckBox(ref IoCheckBox? inst, string tooltip, EventHandler<EventArgs>? response, bool isChecked) {
        if (inst != null) {
            inst.Trigger = false;
            inst.IsChecked = isChecked;
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
            if (response != null)
                inst.IsCheckedChangedGuarded += response;
        }
        if (tooltip != "")
            SetTip(inst, L(tooltip));
    }
    internal static void JustTextBox(ref IoTextBox? inst, string tooltip, string text = "", string watermarkLocaleId = "", EventHandler<EventArgs>? response = null, EventHandler<EventArgs>? responseChange = null) {
        if (inst != null) {
            inst.Trigger = false;
            inst.Text = text;
            inst.Watermark = watermarkLocaleId;
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
            if (response != null) {
                inst.LostFocus += response;
                inst.KeyDown += static (sender, e) => {
                    if (e.Key == Key.Enter && sender is TextBox tb) {
                        // Clears focus of this textbox after hitting enter, which will furhter tigger submit of the edited text
                        // DEPRECATED This deprecated call is the only thing that works like I want. Hopefully we'll figure out a different and long-term supported way!
                        var topLevel = TopLevel.GetTopLevel(tb);
                        topLevel?.FocusManager?.ClearFocus();
                        //topLevel?.Focus();
                        //if (tb.GetVisualRoot() is IInputElement inputRoot) inputRoot.Focus();
                    }
                };
            }
            if (responseChange != null)
                inst.TextChangedGuarded += responseChange;
        }
        if (tooltip != "")
            SetTip(inst, L(tooltip));
    }
    /*internal static void JustPasswordBox(ref IoPasswordBox? inst, string tooltip, string text = "", string watermarkLocaleId = "", EventHandler<EventArgs>? response = null, EventHandler<EventArgs>? responseChange = null) {
        if (inst != null) {
            inst.Trigger = false;
            inst.Text = text;
            inst.Watermark = watermarkLocaleId;
            inst.Trigger = true;
        } else {
            inst = new IoPasswordBox {
                Text = text,
                Watermark = watermarkLocaleId,
                TextAlignment = TextAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalContentAlignment = VerticalAlignment.Center,
                HorizontalContentAlignment = HorizontalAlignment.Center,
                Padding = new Thickness(0),
                BorderThickness = new Thickness(0)
            };
            if (response != null) {
                inst.LostFocus += response;
                inst.KeyDown += static (sender, e) => {
                    if (e.Key == Key.Enter && sender is TextBox tb) {
                        // Clears focus of this textbox after hitting enter, which will furhter tigger submit of the edited text
                        // DEPRECATED This deprecated call is the only thing that works like I want. Hopefully we'll figure out a different and long-term supported way!
                        var topLevel = TopLevel.GetTopLevel(tb);
                        topLevel?.FocusManager?.ClearFocus();
                        //topLevel?.Focus();
                        //if (tb.GetVisualRoot() is IInputElement inputRoot) inputRoot.Focus();
                    }
                };
            }
            if (responseChange != null)
                inst.TextChangedGuarded += responseChange;
        }
        if (tooltip != "")
            SetTip(inst, L(tooltip));
    }*/
    internal static void JustButton(ref (TextBlock Text, IoToggleButton Button)? inst, string tooltip, string text = "", EventHandler<RoutedEventArgs>? response = null, bool isChecked = false, FontWeight weight = FontWeight.Normal, string? areYouSure = null) {
        var t = inst?.Text;
        JustTextBlock(ref t, "", text, weight);
        if (t == null)
            return;
        var b = inst?.Button;
        WrapButton(ref b, t, t, isChecked, response, areYouSure);
        if (b == null)
            return;
        if (tooltip != "")
            SetTip(b, L(tooltip));
        inst = (t, b);
    }
    /*internal static void JustRoundButton(ref (TextBlock Text, IoToggleButton Button, Viewbox Round)? inst, string tooltip, string text = "", EventHandler<RoutedEventArgs>? response = null, bool isChecked = false, FontWeight weight = FontWeight.Normal, bool uncheckOnClick = true) {
        var t = inst?.Text;
        JustTextBlock(ref t, "", text, weight);
        if (t == null)
            return;
        var b = inst?.Button;
        WrapButton(ref b, t, isChecked, response, uncheckOnClick);
        if (b == null)
            return;
        var v = inst?.Round;
        WrapViewbox(ref v, b);
        if (v == null)
            return;
        v.Stretch = Stretch.Fill;
        if (tooltip != "")
            SetTip(v, L(tooltip));
        inst = (t, b, v);
    }*/
    internal static void JustComboBox<T>(ref IoComboBox? inst, string tooltip, IEnumerable<T>? itemSource, T? selectedItem, IDataTemplate? itemTemplate = null, EventHandler<EventArgs>? response = null) {
        if (inst != null) {
            inst.Trigger = false;
            //inst.ItemSource = ItemSource; // cannot update this
            inst.SelectedItem = selectedItem;
            //inst.ItemTemplate = ItemTemplate; // this should not be required again for the same combobox, unless it actaully changes its behavior
            inst.Trigger = true;
        } else {
            inst = new IoComboBox {
                ItemsSource = itemSource,
                ClipToBounds = false,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
                ItemTemplate = itemTemplate,
                SelectedItem = selectedItem
            };
            if (response != null)
                inst.SelectionChangedGuarded += response;
        }
        if (tooltip != "")
            SetTip(inst, L(tooltip));
    }
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
    /*internal class HeightToCornerRadiusConverter : IValueConverter {
        internal object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if (value is double h)
                return new CornerRadius(h / 2);

            return new CornerRadius(0);
        }

        internal object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }*/
    // areYouSure == "": the button will toggle, null: will reset to false and need to be set isChecked after, "XXX": will toggle to are you sure text, timeout back to normal, only clicking when checked will fire guraded clicked event
    internal static void WrapButton(ref IoToggleButton? inst, TextBlock text, Control control, bool isChecked = false, EventHandler<RoutedEventArgs>? response = null, string? areYouSure = null) {
        if (inst != null) {
            inst.Trigger = false;
            inst.IsChecked = isChecked;
            inst.AreYouSure = areYouSure;
            inst.Text = text.Text ?? "";
            inst.Trigger = true;
        } else {
            inst = new IoToggleButton(text) {
                Content = control,
                ClipToBounds = false,
                //CornerRadius = "{Binding $self.ActualHeight}",
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
                HorizontalContentAlignment = HorizontalAlignment.Stretch,
                VerticalContentAlignment = VerticalAlignment.Stretch,
                AreYouSure = areYouSure,
                IsChecked = isChecked,
                Text = text.Text ?? ""
            };
            if (response != null)
                inst.ClickGuarded += response;
        }
    }
    internal static void BigTextBlock(ref (TextBlock Text, Viewbox View)? inst, string tooltip, string text, FontWeight weight = FontWeight.Normal) {
        var t = inst?.Text;
        JustTextBlock(ref t, "", text, weight);
        if (t == null)
            return;
        var v = inst?.View;
        WrapViewbox(ref v, t);
        if (v == null)
            return;
        if (tooltip != "")
            SetTip(v, L(tooltip));
        inst = (t, v);
    }
    internal static void BigCheckBox(ref (IoCheckBox Check, Viewbox View)? inst, string tooltip, bool isChecked = false, EventHandler<EventArgs>? response = null) {
        var c = inst?.Check;
        JustCheckBox(ref c, "", response, isChecked);
        if (c == null)
            return;
        var v = inst?.View;
        WrapViewbox(ref v, c);
        if (v == null)
            return;
        if (tooltip != "")
            SetTip(v, L(tooltip));
        inst = (c, v);
    }
    internal static void BigDatePick(ref (IoDatePicker Date, Viewbox View)? inst, string tooltip, DateTime value, EventHandler<DatePickerSelectedValueChangedEventArgs>? response = null) {
        var d = inst?.Date;
        JustDatePick(ref d, "", value, response);
        if (d == null)
            return;
        var v = inst?.View;
        WrapViewbox(ref v, d);
        if (v == null)
            return;
        if (tooltip != "")
            SetTip(v, L(tooltip));
        inst = (d, v);
    }
    internal static void BigTextBox(ref (IoTextBox Text, Viewbox View, Border Border)? inst, string tooltip, string text = "", string watermarkLocaleId = "", EventHandler<EventArgs>? response = null, EventHandler<EventArgs>? responseChange = null) {
        var t = inst?.Text;
        JustTextBox(ref t, "", text, watermarkLocaleId, response, responseChange);
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
        if (tooltip != "")
            SetTip(b, L(tooltip));
        inst = (t, v, b);
    }
    /*internal static void BigPasswordBox(ref (IoPasswordBox Text, Viewbox View, Border Border)? inst, string tooltip, string text = "", string watermarkLocaleId = "", EventHandler<EventArgs>? response = null, EventHandler<EventArgs>? responseChange = null) {
        var t = inst?.Text;
        JustPasswordBox(ref t, "", text, watermarkLocaleId, response, responseChange);
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
        if (tooltip != "")
            SetTip(b, L(tooltip));
        inst = (t, v, b);
    }*/
    internal static void BigButton(ref (TextBlock Text, Viewbox View, IoToggleButton Button)? inst, string tooltip = "", string text = "", EventHandler<RoutedEventArgs>? response = null, bool isChecked = false, FontWeight weight = FontWeight.Normal, string? areYouSure = null) {
        var t = inst?.Text;
        JustTextBlock(ref t, "", text, weight);
        if (t == null)
            return;
        var v = inst?.View;
        WrapViewbox(ref v, t);
        if (v == null)
            return;
        var b = inst?.Button;
        WrapButton(ref b, t, v, isChecked, response, areYouSure);
        if (b == null)
            return;
        if (tooltip != "")
            SetTip(b, L(tooltip));
        inst = (t, v, b);
    }
    /*internal static void BigRoundButton(ref (TextBlock Text, Viewbox View, IoToggleButton Button, Viewbox Round)? inst, string tooltip = "", string text = "", EventHandler<RoutedEventArgs>? response = null, bool isChecked = false, FontWeight weight = FontWeight.Normal, bool uncheckOnClick = true) {
        var t = inst?.Text;
        JustTextBlock(ref t, "", text, weight);
        if (t == null)
            return;
        var v = inst?.View;
        WrapViewbox(ref v, g);
        if (v == null)
            return;
        var b = inst?.Button;
        WrapButton(ref b, v, isChecked, response, uncheckOnClick);
        if (b == null)
            return;
        var v2 = inst?.Round;
        WrapViewbox(ref v2, b);
        if (v2 == null)
            return;
        v2.Stretch = Stretch.Uniform;
        if (tooltip != "")
            SetTip(b, L(tooltip));
        inst = (t, v, b, v2);
    }*/
    internal static void BigComboBox<T>(ref (IoComboBox Combo, Viewbox View)? inst, string tooltip, IEnumerable<T>? itemSource, T? selectedItem, IDataTemplate? itemTemplate = null, EventHandler<EventArgs>? response = null) {
        var c = inst?.Combo;
        JustComboBox(ref c, "", itemSource, selectedItem, itemTemplate, response);
        if (c == null)
            return;
        var v = inst?.View;
        WrapViewbox(ref v, c);
        if (v == null)
            return;
        if (tooltip != "")
            SetTip(v, L(tooltip));
        inst = (c, v);
    }
    internal static void SetTip(Control c, string tip) {
        var t = (GetProfile?.ShowTooltips ?? DPro.ShowTooltips);
        ToolTip.SetTip(c, t > 0 ? new IoInfoTextBlock { Text = L(tip) } : null);
        ToolTip.SetShowDelay(c, t);
    }
}