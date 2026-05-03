using Avalonia.Controls;
using Avalonia.Input;
using System;

namespace IoListWallet.Core.Basic;

internal class IoComboBox : ComboBox {

    internal event EventHandler<EventArgs>? SelectionChangedGuarded;

    internal IoTooltips Tooltip;
    internal bool Trigger = true;

    private EventHandler<EventArgs>? response = null;

    internal void SetResponse(EventHandler<EventArgs>? newResponse) {
        if (newResponse != null) {
            if (response != null)
                SelectionChangedGuarded -= response;
            SelectionChangedGuarded += response = newResponse;
        }
    }
    internal IoComboBox() : base() {
        Tooltip = new(this, false);
        SelectionChanged += Guard;
    }
    internal void Guard(object? sender, EventArgs e) {
        if (Trigger)
            SelectionChangedGuarded?.Invoke(sender, e);
    }
    protected override void OnPointerReleased(PointerReleasedEventArgs e) {
        if (Tooltip.Release()) {
            e.Handled = true;
            return;
        }
        base.OnPointerReleased(e);
    }

    protected override Type StyleKeyOverride => typeof(ComboBox);
}
internal class WComboBox(IoComboBox combo, Viewbox view) {
    internal IoComboBox Combo = combo;
    internal Viewbox View = view;
}