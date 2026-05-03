using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using System;
using System.Threading.Tasks;

namespace IoListWallet.Core.Basic;
internal class IoToggleButton : ToggleButton {

    internal event EventHandler<RoutedEventArgs>? IsCheckedChangedGuarded;
    internal event EventHandler<RoutedEventArgs>? ClickGuarded;
    //internal event EventHandler<IoUserControl.ToolTipEventArgs>? OnLongClick;

    internal bool Trigger = true;//, longClick = false;
    internal IoTooltips Tooltip;
    internal string? AreYouSure { get; set { if (mySure.Text == field && value != null) mySure.Text = value; field = value; InvalidateMeasure(); } } = null;
    internal string Text { get; set { if (myText.Text != AreYouSure) myText.Text = value; field = value; UpdateCorner(); InvalidateMeasure(); } } = "";
    //internal string Text { get; set { if (myText.Text != AreYouSure) myText.Text = value; field = value;  } } = "";
    internal double CornerRatio { get; set { if (field == value) return; field = value; } } = 0.25;

    private ulong processID = 1;//, processClickID = 1;
    private IoTextBlockBase myText;
    private IoTextBlockBase mySure;

    private EventHandler<RoutedEventArgs>? response = null;

    private Rect MyBounds;

    internal void SetResponse(EventHandler<RoutedEventArgs>? newResponse) {
        if (newResponse != null) {
            if (response != null)
                ClickGuarded -= response;

            ClickGuarded += response = newResponse;
        }
    }

    // doesn't work...
    //internal bool Enabled { get => IsEnabled; set { Cursor = new Cursor((IsEnabled = value) ? StandardCursorType.Hand : StandardCursorType.No); } }

    private void UpdateCorner() {
        var c = 1 + Text.Split('\n').Length;
        CornerRadius = new CornerRadius(MyBounds.Height / c * CornerRatio);
    }
    internal IoToggleButton(IoTextBlockBase myText, IoTextBlockBase mySure) : base() {
        Tooltip = new(this, false);
        Cursor = new Cursor(StandardCursorType.Hand);
        //Classes.Add(":togglebutton");
        this.myText = myText;
        this.mySure = mySure;
        Cursor = new Cursor(StandardCursorType.Hand);
        IsCheckedChanged += GuardE;
        //AddHandler(PointerPressedEvent, OnPress, RoutingStrategies.Tunnel);
        //AddHandler(PointerCaptureLostEvent, OnLost, RoutingStrategies.Tunnel);
        //PointerPressed += OnPress;
        Click += ClickE;
        PropertyChanged += (_, e) => {
            if (e.Property == BoundsProperty) {
                MyBounds = (Rect)e.NewValue!;
                UpdateCorner();
            }
        };
    }
    internal void GuardE(object? sender, RoutedEventArgs e) {
        if (Trigger)
            IsCheckedChangedGuarded?.Invoke(sender, e);
    }
    /*internal async void OnPress(object? s, PointerPressedEventArgs e) {
        if (Tooltip == "")
            return;
        longClick = false;
        ulong Id = ++processClickID;
        await Task.Delay(Static.AppSave.GetProfile?.ShowTooltips ?? Static.AppSave.DPro.ShowTooltips);
        if (processClickID == Id) 
            longClick = true;
    }*/
    internal async void ClickE(object? sender, RoutedEventArgs e) {
        //e.Handled = true;
        if (Trigger) {
            Trigger = false;
            myText.Opacity = 1;
            mySure.Opacity = 0;
            if (AreYouSure == null)
                IsChecked = false;
            Trigger = true;

            if (Tooltip.Release()) 
                return;

            if (AreYouSure == null || AreYouSure == "" || (!IsChecked ?? true)) {
                ClickGuarded?.Invoke(sender, e);
                return;
            }
            ulong Id = ++processID;
            mySure.Opacity = 1;
            myText.Opacity = 0;
            await Task.Delay(3000);
            if (processID == Id) {
                Trigger = false;
                myText.Opacity = 1;
                mySure.Opacity = 0;
                IsChecked = false;
                Trigger = true;
            }
        }
       
    }
    protected override Type StyleKeyOverride => typeof(ToggleButton);
}
internal class WToggleButton(IoTextBlockBase text, IoTextBlockBase sure, Viewbox view, Panel panel, IoToggleButton button) : WSToggleButton(text, sure, panel, button) {
    internal Viewbox View = view;
}
internal class WSToggleButton {
    internal IoTextBlockBase Text;
    internal IoTextBlockBase Sure;
    internal Panel Panel;
    internal IoToggleButton Button;
    public WSToggleButton(IoTextBlockBase text, IoTextBlockBase sure, Panel panel, IoToggleButton button) {
        Button = button;
        Panel = panel;
        Text = text;
        (Sure = sure).Opacity = 0;
    }
}