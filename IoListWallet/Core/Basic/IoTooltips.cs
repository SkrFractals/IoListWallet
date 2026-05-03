using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using System;
using System.Threading.Tasks;

namespace IoListWallet.Core.Basic;

internal class IoTooltips {

    public IoTooltips(Control c, bool selfRelease = true) => AddHandlers(control = c, selfRelease);
 
    internal event EventHandler<IoUserControl.ToolTipEventArgs>? OnLongClick;
    internal event EventHandler<EventArgs>? OnLongHoldStart;
    internal event EventHandler<EventArgs>? OnLongHoldStop;
    internal string Tooltip = "";

    internal bool longClick { get; private set; } = false;
    private ulong processClickID = 1;
    private Point? startPointer;
    private Control control;

    private void AddHandlers(Control c, bool selfRelease = true) {
        c.AddHandler(InputElement.PointerPressedEvent, OnPress, RoutingStrategies.Tunnel);
        c.AddHandler(InputElement.PointerMovedEvent, OnMoved, RoutingStrategies.Tunnel);
        if (selfRelease)
            c.AddHandler(InputElement.PointerReleasedEvent, OnRelease, RoutingStrategies.Tunnel);
    }
    internal async void OnPress(object? s, PointerPressedEventArgs e) {
        if (Tooltip == "")
            return;
        OnLongHoldStop?.Invoke(this, new EventArgs());
        startPointer = e.GetPosition(null);
        longClick = false;
        ulong Id = ++processClickID;
        await Task.Delay(Static.AppSave.GetProfile?.ShowTooltips ?? Static.AppSave.DPro.ShowTooltips);
        if (processClickID == Id) {
            longClick = true;
            OnLongHoldStart?.Invoke(this, new EventArgs());
        }
    }
    internal bool Release() {
        var r = longClick;
        if (r)
            OnLongClick?.Invoke(this, new IoUserControl.ToolTipEventArgs() { tooltip = Tooltip });
        LongStop();
        return r;
    }
    internal void OnRelease(object? s, PointerReleasedEventArgs e) => Release();
    internal async void OnMoved(object? s, PointerEventArgs e) {
        if (Tooltip == "")
            return;
        var delta = e.GetPosition(null);
        var start = startPointer ?? delta;
        startPointer = start;
        delta -= start;
        var screen = Static.AppSave.AppMainWindow?.Screens.ScreenFromPoint(control.PointToScreen(e.GetPosition(control)));
        var t = screen == null ? 100 : Math.Clamp((int)(Math.Min(screen.WorkingArea.Width, screen.WorkingArea.Height) * 0.04), 32, 128);
        if (delta.X * delta.X + delta.Y * delta.Y > t * t) 
            LongStop();
    }
    private void LongStop() {
        ++processClickID;
        longClick = false;
        OnLongHoldStop?.Invoke(this, new EventArgs());
    }
}