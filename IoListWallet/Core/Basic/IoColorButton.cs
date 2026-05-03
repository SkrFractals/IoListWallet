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

    internal IoTooltips Tooltip;
    internal Color? isColor;

    private EventHandler<ColorChangedEventArgs>? response = null;
    private ColorPicker c;

    internal void SetResponse(EventHandler<ColorChangedEventArgs>? newResponse) {
        if (newResponse != null) {
            if (response != null)
                ClickGuarded -= response;

            ClickGuarded += response = newResponse;
        }
    }
    internal IoColorButton() : base() {
        Tooltip = new(this, false);
        Cursor = new Cursor(StandardCursorType.Hand);
        //Click += GuardE;
        PropertyChanged += (_, e) => {
            if (e.Property == BoundsProperty) {
                var bounds = (Rect)e.NewValue!;
                CornerRadius = new CornerRadius(bounds.Height / 4);
            }
        };
        // when it's here the flyout and picker shows out, but the picker disappears when i touch it
        c = new() { Color = isColor ?? AppSave.DPro.TitleColor };
        c.ColorChanged += ColorChanged;

        Flyout = new Flyout() { Content = c };
    }
    private void ColorChanged(object? sender, ColorChangedEventArgs e) {
        ClickGuarded?.Invoke(sender, e);
        c.ColorChanged -= ColorChanged;
    }
    protected override void OnPointerReleased(PointerReleasedEventArgs e) {
        if (Tooltip.Release()) {
            e.Handled = true;
            return;
        }
        base.OnPointerReleased(e);
    }
    protected override Type StyleKeyOverride => typeof(ToggleButton);
}
internal class WColorButton(IoTextBlockBase text, Viewbox view, IoColorButton button) : WSColorButton(text, button) {
    internal Viewbox View = view;
}
internal class WSColorButton(IoTextBlockBase text, IoColorButton button) {
    internal IoTextBlockBase Text = text;
    internal IoColorButton Button = button;
}