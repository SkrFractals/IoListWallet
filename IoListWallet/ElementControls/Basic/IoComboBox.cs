using Avalonia.Controls;
using System;

namespace IoListWallet.ElementControls.Basic {
    internal class IoComboBox : ComboBox {

        internal event EventHandler<EventArgs>? SelectionChangedGuarded;

        internal bool Trigger = true;

        internal IoComboBox() : base() 
            => SelectionChanged += Guard; 
        internal void Guard(object? sender, EventArgs e) {
            if (Trigger)
                SelectionChangedGuarded?.Invoke(sender, e);
        }
        protected override Type StyleKeyOverride => typeof(ComboBox);
    }
}