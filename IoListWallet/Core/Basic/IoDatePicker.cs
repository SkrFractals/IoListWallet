using Avalonia.Controls;
using Avalonia.Input;
using System;

namespace IoListWallet.Core.Basic {
    internal class IoDatePicker : DatePicker {

        internal event EventHandler<DatePickerSelectedValueChangedEventArgs>? SelectedDateChangedGuarded;

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
            SelectedDateChanged += Guard;
            Cursor = new Cursor(StandardCursorType.Hand);
        }
        internal void Guard(object? sender, DatePickerSelectedValueChangedEventArgs e) {
            if (Trigger)
                SelectedDateChangedGuarded?.Invoke(sender, e);
        }
        protected override Type StyleKeyOverride => typeof(DatePicker);
    }
}