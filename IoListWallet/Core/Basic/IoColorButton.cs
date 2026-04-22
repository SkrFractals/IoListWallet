using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Media;
using IoListWallet.Core.Static;
using System;

namespace IoListWallet.Core.Basic;
internal class IoColorButton : Button {

    internal event EventHandler<ColorChangedEventArgs>? ClickGuarded;

    internal Color? isColor;

    private EventHandler<ColorChangedEventArgs>? response = null;
    ColorPicker c;
    internal void SetResponse(EventHandler<ColorChangedEventArgs>? newResponse) {
        if (newResponse != null) {
            if (response != null)
                ClickGuarded -= response;

            ClickGuarded += response = newResponse;
        }
    }

    // doesn't work...
    //internal bool Enabled { get => IsEnabled; set { Cursor = new Cursor((IsEnabled = value) ? StandardCursorType.Hand : StandardCursorType.No); } }


    internal IoColorButton() : base() {
        Cursor = new Cursor(StandardCursorType.Hand);
        //Click += GuardE;
        PropertyChanged += (_, e) => {
            if (e.Property == BoundsProperty) {
                var bounds = (Rect)e.NewValue!;
                CornerRadius = new CornerRadius(bounds.Height / 4);
            }
        };
        // when it's here th eflyout and picker shows out, but the picker disappears when i touch it
        c = new() { Color = isColor ?? AppSave.DPro.TitleColor };
        c.ColorChanged += ColorChanged;

        Flyout = new Flyout() { Content = c };
    }
    //private void GuardE(object? sender, RoutedEventArgs e) {
    //    c.ColorChanged += ColorChanged;
        // when it's here the flyout won't even appear
        //ColorPicker c = new() { Color = isColor ?? AppSave.DPro.TitleColor };
        //Flyout = new Flyout() { Content = c };
    //}
    private void ColorChanged(object? sender, ColorChangedEventArgs e) {
        ClickGuarded?.Invoke(sender, e);
        c.ColorChanged -= ColorChanged;
    }
    protected override Type StyleKeyOverride => typeof(ToggleButton);
}