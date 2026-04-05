using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using System;
using System.Threading.Tasks;

namespace IoListWallet.ElementControls.Basic {
    internal class IoToggleButton : ToggleButton {

        internal event EventHandler<RoutedEventArgs>? IsCheckedChangedGuarded;
        internal event EventHandler<RoutedEventArgs>? ClickGuarded;

        internal bool Trigger = true;
        internal string? AreYouSure = null;
        internal string Text = "";

        private ulong processID = 1;
        private TextBlock myText;

        internal IoToggleButton(TextBlock myText) : base() {
            //Classes.Add(":togglebutton");
            this.myText = myText;
            IsCheckedChanged += GuardE;
            Click += ClickE;
            PropertyChanged += (_, e) => {
                if (e.Property == BoundsProperty) {
                    var bounds = (Rect)e.NewValue!;
                    CornerRadius = new CornerRadius(bounds.Height / 4);
                }
            };
        }
        internal void GuardE(object? sender, RoutedEventArgs e) {
            if (Trigger)
                IsCheckedChangedGuarded?.Invoke(sender, e);
        }
        internal async void ClickE(object? sender, RoutedEventArgs e) {
            if (Trigger) {
                Trigger = false;
                myText.Text = Text;
                if (AreYouSure == null) 
                    IsChecked = false;
                Trigger = true;
                if (AreYouSure == null || AreYouSure == "" || (!IsChecked ?? true)) {
                    ClickGuarded?.Invoke(sender, e);
                    return;
                }
                ulong Id = ++processID;
                myText.Text = AreYouSure;
                await Task.Delay(3000);
                if (processID == Id) {
                    Trigger = false;
                    myText.Text = Text;
                    IsChecked = false;
                    Trigger = true;
                }
            }       
        }
        protected override Type StyleKeyOverride => typeof(ToggleButton);
    }
}