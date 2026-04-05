using Avalonia.Controls;
using System;

namespace IoListWallet.ElementControls.Basic {
    internal class IoDatePicker : DatePicker {

        internal event EventHandler<DatePickerSelectedValueChangedEventArgs>? SelectedDateChangedGuarded;

        internal bool Trigger = true;

        internal IoDatePicker() : base() 
            => SelectedDateChanged += Guard;
        internal void Guard(object? sender, DatePickerSelectedValueChangedEventArgs e) {
            if (Trigger)
                SelectedDateChangedGuarded?.Invoke(sender, e);
        }
        protected override Type StyleKeyOverride => typeof(DatePicker);
    }
}