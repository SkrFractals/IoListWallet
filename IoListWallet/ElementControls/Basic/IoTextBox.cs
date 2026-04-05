using Avalonia.Controls;
using System;

namespace IoListWallet.ElementControls.Basic {
    internal class IoTextBox : TextBox {

        internal event EventHandler<EventArgs>? TextChangedGuarded;

        internal bool Trigger = true;

        internal IoTextBox() : base() {
            TextChanged += Guard;
            //Classes.Add("textbox");
        }
        internal void Guard(object? sender, EventArgs e) {
            if (Trigger)
                TextChangedGuarded?.Invoke(sender, e);
        }
        protected override Type StyleKeyOverride => typeof(TextBox);
    }
}