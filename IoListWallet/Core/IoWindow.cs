using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using IoListWallet.Core.Panels;
using IoListWallet.Core.Static;
using System;
using static IoListWallet.App;
using static IoListWallet.Core.Static.AppSave;

namespace IoListWallet.Core;
internal class IoWindow : Window {
    [Save]
    internal IoMainPanel MyMainPanel { get; set { field = value; IoPanel.Dirty(); } }
    [Save]
    internal int Index { get; set { field = value; IoPanel.Dirty(); } } = 0;
    [Save] internal int OpenedFrom { get; set { Owner = TryWindow(field = value); IoPanel.Dirty(); } } = -1;
    [Save]
    internal double MyWidth { get; set { Width = field = value; IoPanel.Dirty(); } } = 120;
    [Save]
    internal double MyHeight { get; set { Height = field = value; IoPanel.Dirty(); } } = 120;

    /*[Save] internal bool WindowVisible { get; set { 
            if (field = value) {
                if (IsWindow(OpenedFrom, out var o)) 
                    Show(o!); 
                else 
                    Show(); 
            } else 
                Hide(); 
            IoPanel.Dirty(); 
        } } = true;*/
    internal bool IsClosing = false, isDragging = false, isDockDragging = false;

    //private bool _dragging;
    //private PixelPoint _lastPosition;
    //private DateTime _lastMoveTime;

    internal void ReLink() {
        MyMainPanel.DockHost.Content = MyMainPanel.Dock.Build(true, true);
    }

    public void SetMinSize(Size n){
        if (((GetProfile?.DisplayRotation ?? DPro.DisplayRotation) & 1) == 1) {
            MinHeight = n.Width;
            MinWidth = n.Height;
            return;
        }
        MinWidth = n.Width;
        MinHeight = n.Height;
    }
    public void SetSize(Size n) {
        if (((GetProfile?.DisplayRotation ?? DPro.DisplayRotation) & 1) == 1) {
            Height = n.Width;
            Width = n.Height;
            return;
        }
        Width = n.Width;
        Height = n.Height;
    }
    public Size GetMinSize() =>((GetProfile?.DisplayRotation ?? DPro.DisplayRotation) & 1) == 1 ? new(MinHeight, MinWidth) : new(MinWidth, MinHeight);
    public Size GetSize() => ((GetProfile?.DisplayRotation ?? DPro.DisplayRotation) & 1) == 1 ? new(Height, Width) : new(Width, Height);

    bool _clamping = false;
    void ClampToScreen(object? s, SizeChangedEventArgs e) {
        if (_clamping) 
            return;
        _clamping = true;
        var screen = Screens.ScreenFromWindow(this)?.WorkingArea
                     ?? Screens.Primary?.WorkingArea;
        if (screen == null)
            return;
        int x = Position.X;
        int y = Position.Y;
        //double w = Width;
        //double h = Height;
        //if (double.IsNaN(w) || double.IsNaN(h))
        //   return;

        var pSize = e.PreviousSize;
        var nSize = e.NewSize;
        var w = nSize.Width;
        var h = nSize.Height;

        if (pSize.Width < nSize.Width) {
            // right edge
            if (x + w > screen.Value.Right)
                w = screen.Value.Right - x;
            // left edge
            if (x < screen.Value.X)
                x += (int)(nSize.Width - (w = Math.Min(w, pSize.Width)));
        }
        if (pSize.Height < nSize.Height) {
            // bottom edge
            if (y + h > screen.Value.Bottom)
                h = screen.Value.Bottom - y;
            // top edge
            if (y < screen.Value.Y) 
                y += (int)(nSize.Height - (h = Math.Min(h, pSize.Height)));
        }
        Position = new PixelPoint(x, y);
        Width = pSize.Width > nSize.Width ? Math.Max(nSize.Width, MinWidth) : Math.Max(pSize.Width, Math.Max(w, MinWidth));
        Height = pSize.Height > nSize.Height ? Math.Max(nSize.Height, MinHeight) : Math.Max(pSize.Height, Math.Max(h, MinHeight));
        _clamping = false;
    }

    public IoWindow() {

        //this.GetObservable(ClientSizeProperty).Subscribe(_ => ClampToScreen());
        //this.PositionChanged += (_, __) => ClampToScreen();

        SizeChanged += ClampToScreen;

        SystemDecorations = GetDisplayMode == DisplayMode.MultiWindow 
            ? SystemDecorations.BorderOnly : SystemDecorations.Full;
        SetMinSize(new(120, 120));
        for (Index = 0; GetProfile?.Pan.OpenedWindows.ContainsKey(Index) ?? false; ++Index) ;
        GetProfile?.Pan.OpenedWindows[Index] = this;
        MyMainPanel = new() { MyWindowK = Index };
       
        Content = MyMainPanel;
        Closing += IoWindow_Closing;
    }

    internal void DragStarted() {
        isDragging = true;
        if (GetDisplayMode != DisplayMode.MultiWindow || MyMainPanel == AppMainPanel || (FromK(MyMainPanel.Dock.Original)?.IsModal() ?? true))
            return;
        MyMainPanel.OverlayGrid.IsVisible = isDockDragging = true;
        Opacity = 0.25f;

    }
    internal void DragEnded() {
        isDockDragging = isDragging = false;
        MyMainPanel.OverlayGrid.IsVisible = false;
        Opacity = 1.0f;
    }
    internal void DoubleTap() {
        if (GetDisplayMode != DisplayMode.MultiWindow)
            return;
       
        WindowState = WindowState == WindowState.Normal ? WindowState.Maximized : WindowState.Normal;
        DragEnded();
    }

    private void IoWindow_Closing(object? sender, WindowClosingEventArgs e) {
        MyMainPanel.DockHost.Content = null;
        if (IsClosing || !App.appRunning)
            return;
        GetProfile?.Pan.OpenedWindows.Remove(Index);
        bool MainClose = MyMainPanel.Dock == AppDockRoot;
        IsClosing = true;
        if (MainClose)
            AppClose();
        else CloseDock(MyMainPanel.Dock);
       
    }
    protected override void OnSizeChanged(SizeChangedEventArgs e) {
        base.OnSizeChanged(e);
        MyMainPanel.Dock.RefreshMinSize();
        MyWidth = Width;
        MyHeight = Height;
    }
    private static void CloseDock(IoDock dock) {
        if (dock is IoDockPanels panelDock)
            foreach(var p in panelDock.Panel)
                FromK(p)?.WindowClosed();
        else if (dock is IoDockRoot root)
            CloseDock(root.TopNonRoot);
        else if (dock is IoDockSplit split) {
            CloseDock(split.First);
            CloseDock(split.Second);
        }
    }
}