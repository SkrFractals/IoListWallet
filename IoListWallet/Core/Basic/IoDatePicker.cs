using Avalonia.Controls;
using Avalonia.Input;
using System;

namespace IoListWallet.Core.Basic;

internal class IoDatePicker : DatePicker {

    internal event EventHandler<DatePickerSelectedValueChangedEventArgs>? SelectedDateChangedGuarded;

    internal IoTooltips Tooltip;
    internal bool Trigger = true;

    private EventHandler<DatePickerSelectedValueChangedEventArgs>? response = null;

    internal void SetResponse(EventHandler<DatePickerSelectedValueChangedEventArgs>? newResponse) {
        if (newResponse != null) {
            if (response != null)
                SelectedDateChangedGuarded -= response;
            SelectedDateChangedGuarded += response = newResponse;
        }
    }
    internal IoDatePicker() : base() {
        Tooltip = new(this, false);
        SelectedDateChanged += Guard;
        Cursor = new Cursor(StandardCursorType.Hand);
    }
    internal void Guard(object? sender, DatePickerSelectedValueChangedEventArgs e) {
        if (Trigger)
            SelectedDateChangedGuarded?.Invoke(sender, e);
    }
    protected override void OnPointerReleased(PointerReleasedEventArgs e) {
        if (Tooltip.Release()) {
            e.Handled = true;
            return;
        }
        base.OnPointerReleased(e);
    }
    protected override Type StyleKeyOverride => typeof(DatePicker);
}
internal class WDatePicker(IoDatePicker date, Viewbox view) {
    internal IoDatePicker Date = date;
    internal Viewbox View = view;
}