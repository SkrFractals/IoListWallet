/*using Avalonia.Controls;
using System;

namespace IoListWallet.ElementControls.Basic {
    internal class IoPasswordBox : PasswordBox {

        internal event EventHandler<EventArgs>? TextChangedGuarded;

        internal bool Trigger = true;

        internal IoPasswordBox() : base() {
            TextChanged += Guard;
            //Classes.Add("textbox");
        }
        internal void Guard(object? sender, EventArgs e) {
            if (Trigger)
                TextChangedGuarded?.Invoke(sender, e);
        }
        protected override Type StyleKeyOverride => typeof(PasswordBox);
    }
}*/