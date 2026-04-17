using Avalonia.Controls;
using System;

namespace IoListWallet.Core.Basic {
    internal class IoComboBox : ComboBox {

        internal event EventHandler<EventArgs>? SelectionChangedGuarded;

        internal bool Trigger = true;

        private EventHandler<EventArgs>? response = null;

        internal void SetResponse(EventHandler<EventArgs>? newResponse) {
            if (newResponse != null) {
                if (response != null)
                    SelectionChangedGuarded -= response;

                SelectionChangedGuarded += response = newResponse;
            }
        }
        internal IoComboBox() : base() 
            => SelectionChanged += Guard; 
        internal void Guard(object? sender, EventArgs e) {
            if (Trigger)
                SelectionChangedGuarded?.Invoke(sender, e);
        }
        protected override Type StyleKeyOverride => typeof(ComboBox);
    }
}