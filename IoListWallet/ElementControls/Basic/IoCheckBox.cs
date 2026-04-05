using Avalonia.Controls;
using System;

namespace IoListWallet.ElementControls.Basic {
    internal class IoCheckBox : CheckBox {

        internal event EventHandler<EventArgs>? IsCheckedChangedGuarded;

        internal bool Trigger = true;

        internal IoCheckBox() : base() 
            => IsCheckedChanged += Guard;
        internal void Guard(object? sender, EventArgs e) {
            if (Trigger)
                IsCheckedChangedGuarded?.Invoke(sender, e);
        }
        protected override Type StyleKeyOverride => typeof(CheckBox);
    }
}