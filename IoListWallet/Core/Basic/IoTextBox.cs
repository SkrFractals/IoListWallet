using Avalonia.Controls;
using Avalonia.Input;
using System;

namespace IoListWallet.Core.Basic {
    internal class IoTextBox : TextBox {

        internal event EventHandler<EventArgs>? TextChangedGuarded;

        private EventHandler<EventArgs>? response = null, responseChange = null;

        internal void SetResponse(EventHandler<EventArgs>? newResponse) {
            if (newResponse != null) {
                if (response != null)
                    LostFocus -= response;

                LostFocus += response = newResponse;
            }
        }
        internal void SetResponseChange(EventHandler<EventArgs>? newResponse) {
            if (newResponse != null) {
                if (responseChange != null)
                    TextChangedGuarded -= responseChange;

                TextChangedGuarded += responseChange = newResponse;
            }
        }

        internal bool Trigger = true;

        // doesn't work...
        internal bool Enabled { get => IsEnabled; set { Cursor = new Cursor((IsEnabled = value) ? StandardCursorType.Hand : StandardCursorType.No); } }

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