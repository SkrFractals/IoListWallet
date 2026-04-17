using Avalonia.Controls;
using Avalonia.Input;
using System;

namespace IoListWallet.Core.Basic {
    internal class IoCheckBox : CheckBox {

        internal event EventHandler<EventArgs>? IsCheckedChangedGuarded;

        private EventHandler<EventArgs>? response = null;

        internal void SetResponse(EventHandler<EventArgs>? newResponse) {
            if (newResponse != null) {
                if (response != null)
                    IsCheckedChangedGuarded -= response;

                IsCheckedChangedGuarded += response = newResponse;
            }
        }

        internal bool Trigger = true;

        internal IoCheckBox() : base() {
            Cursor = new Cursor(StandardCursorType.Hand);
            IsCheckedChanged += Guard;
        }
        internal void Guard(object? sender, EventArgs e) {
            if (Trigger)
                IsCheckedChangedGuarded?.Invoke(sender, e);
        }
        protected override Type StyleKeyOverride => typeof(CheckBox);
    }
}