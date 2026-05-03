using Avalonia.Controls;
using Avalonia.Input;
using System;

namespace IoListWallet.Core.Basic;

internal class IoCheckBox : CheckBox {

    internal event EventHandler<EventArgs>? IsCheckedChangedGuarded;

    internal IoTooltips Tooltip;
    internal bool Trigger = true;

    private EventHandler<EventArgs>? response = null;

    internal void SetResponse(EventHandler<EventArgs>? newResponse) {
        if (newResponse != null) {
            if (response != null)
                IsCheckedChangedGuarded -= response;
            IsCheckedChangedGuarded += response = newResponse;
        }
    }
    internal IoCheckBox() : base() {
        Tooltip = new(this, false);
        Cursor = new Cursor(StandardCursorType.Hand);
        IsCheckedChanged += Guard;
    }
    internal void Guard(object? sender, EventArgs e) {
        if (Trigger)
            IsCheckedChangedGuarded?.Invoke(sender, e);
    }
    protected override void OnPointerReleased(PointerReleasedEventArgs e) {
        if (Tooltip.Release()) {
            e.Handled = true;
            return;
        }
        base.OnPointerReleased(e);
    }
    protected override Type StyleKeyOverride => typeof(CheckBox);
}
internal class WCheckBox(IoCheckBox check, Viewbox view) {
    internal IoCheckBox Check = check;
    internal Viewbox View = view;
}