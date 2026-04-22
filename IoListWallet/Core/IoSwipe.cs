// faster but broken
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using IoListWallet.Core.Basic;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using static IoListWallet.Core.Static.AppSave;
using static IoListWallet.Core.Static.ConstructionHelpers;

namespace IoListWallet.Core;

internal class IoSwipe : UserControl {
    internal class SwipeEventArgs : EventArgs {
        internal (int X, int Y) ToPage;
        internal bool Horizontal, Vertical;
    }
    internal class UpdateTransformEventArgs : EventArgs {
        internal Point Prev, Current;
    }
    internal bool isSnapBack { get; private set; }
    internal Point displaceNow { get; private set; }

    internal event EventHandler<UpdateTransformEventArgs>? OnUpdateTransform;
    internal event EventHandler<EventArgs>? AnimationFinished;
    internal event EventHandler<EventArgs>? OnStartAnimate;
    internal event EventHandler<EventArgs>? OnRightEvent;
    internal event EventHandler<EventArgs>? OnLeftEvent;
    internal event EventHandler<EventArgs>? OnTopEvent;
    internal event EventHandler<EventArgs>? OnBottomEvent;
    internal event EventHandler<EventArgs>? OnMove;
    internal event EventHandler<SwipeEventArgs>? TriggerSwipe;
    internal event EventHandler<SwipeEventArgs>? OnSwipeStarted;

    internal double Time = 1;
    internal (int X, int Y) Page = (0, 0), PrevPage = (0, 0);
    internal (int IncMinX, int IncMinY, int ExcMaxX, int ExcMaxY)
        TransformBounds = (0, 0, 1, 1); // Inclusive Min, Exclusive Max, The Owner will set this up

    private IPointer? _pointer;
    private TranslateTransform transformCurrent, transformPrevious;
    internal bool IsDragging { get; private set; }
    internal bool Horizontal, Vertical;
    private Point dragOrigin, displaceFrom, displaceStart, displaceNowT, dragVelocity0;
    private double lastTime = 0, lastTimeT = 0;
    private CancellationTokenSource cancellationTokenSource;
    private CancellationToken cancellationToken;
    private Grid host;
    //private Grid? gridPrevious, gridCurrent;
    private ScrollViewer scrollCurrent, scrollPrevious;
    private Stopwatch stopWatch, stopWatchT;
    private Border inputBlocker;
    //private RenderTargetBitmap? BmpPrev, BmpCur;

    private double GetDeltaTimeT => (-lastTimeT + (lastTimeT = stopWatchT.ElapsedMilliseconds / 1000.0)) * 1000.0 / GetAniTime;
    private double GetDeltaTime => (-lastTime + (lastTime = stopWatch.ElapsedMilliseconds / 1000.0)) * 1000.0 / GetAniTime;
    private double SwipeBoundsW => Math.Max(240, host.Bounds.Width > 0 ? host.Bounds.Width : host.DesiredSize.Width);
    private double SwipeBoundsH => Math.Max(240, host.Bounds.Height > 0 ? host.Bounds.Height : host.DesiredSize.Height);
    private double GetSensitivity => (GetProfile?.SwipeSensitivity ?? DPro.SwipeSensitivity);
    private double GetAniTime => (GetProfile?.SwipeTime ?? DPro.SwipeTime);
    internal IoSwipe(bool horizontal, bool vertical) : base() {
        App.SwipeCaptured += SwipeCaptured;
        HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch;
        VerticalAlignment = Avalonia.Layout.VerticalAlignment.Stretch;
        Horizontal = horizontal;
        Vertical = vertical;
        ClipToBounds = false;
        // Host
        Grid? h = null;
        JustGrid(ref h);
        if (h == null)
            throw new("Failed SwipePanel.host");
        (host = h).AttachedToVisualTree += (_, _) => host.LayoutUpdated += OnLayoutUpdated;
        h.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
        h.RowDefinitions.Add(new RowDefinition(GridLength.Star));
        h.AddHandler(PointerPressedEvent, OnPressed, RoutingStrategies.Tunnel);
        h.AddHandler(PointerMovedEvent, OnMoved, RoutingStrategies.Tunnel);
        h.AddHandler(PointerReleasedEvent, OnReleased, RoutingStrategies.Tunnel);
        h.AddHandler(PointerCaptureLostEvent, OnUncaptured, RoutingStrategies.Tunnel);

        ScrollViewer? n = null;
        JustScroll(ref n);
        if (n == null)
            throw new("Failed SwipePanel.host");
        scrollCurrent = n;
        transformCurrent = new TranslateTransform(0, 0);

        ScrollViewer? p = null;
        JustScroll(ref p);
        if (p == null)
            throw new("Failed SwipePanel.host");
        scrollPrevious = p;
        transformPrevious = new TranslateTransform(0, 0);

        cancellationTokenSource = new CancellationTokenSource();
        cancellationToken = cancellationTokenSource.Token;
        inputBlocker = new Border {
            Background = Brushes.Transparent, // invisible
            IsHitTestVisible = true // takes clicks
        };
        stopWatch = new();
        stopWatchT = new();
        Content = host;
    }
    internal void ClearHost(Grid? previousGrid, Grid? currentGrid) {
        scrollCurrent.Content = null;
        scrollPrevious.Content = null;
        inputBlocker.Child = null;
        //gridPrevious = null;
        //gridCurrent = null;
    }
    internal void RefreshHost(Grid? previousGrid, Grid? currentGrid) {

        scrollCurrent.Content = null;
        scrollPrevious.Content = null;
        scrollCurrent.Content = null;
        scrollPrevious.Content = null;
        inputBlocker.Child = null;

        // adding my screens to scrollviewers, but technically, they should not be needed since the grids will always fit the screen, but just to be sure...
        scrollCurrent.Content = currentGrid;//currentGrid;
        scrollPrevious.Content = previousGrid;// previousGrid;
        // I am translating these scroll viewers, to make it look like, the currentGrid swipes into the screen, pushing the previousGrid away.
        scrollCurrent.RenderTransform = transformCurrent;
        scrollPrevious.RenderTransform = transformPrevious;
        // this part is basically just a wrapper safely adding scrollCurrent, scrollPrevious into host.Children
        host.Tag = new List<Control>();
        AddChild(host, scrollCurrent);
        if (previousGrid != null) {
            AddChild(host, scrollPrevious);
            AddChild(host, inputBlocker); // invisible hittestable overlay so you can't click anything in the grids until animation finishes
        }
        FinishChildren(host);
        UpdateTransforms(displaceNow);
    }
    private void UpdateTransforms(Point displace) {
        // this is called each tick of the animation to update the translates of the scrollviewers.
        // Time goes from 0 to 1 (0=previous on screen, 0.5=halfway, 1=current on screen)
        //text.Text = "DisplaceNow: " + displaceNow.X.ToString() + " - " + displaceNow.Y.ToString();
        double w = SwipeBoundsW, h = SwipeBoundsH;
        transformCurrent.X = displace.X * w;
        transformCurrent.Y = displace.Y * h;
        var ds = displace - displaceStart;
        transformPrevious.X = ds.X * w;
        transformPrevious.Y = ds.Y * h;
        scrollPrevious.Opacity = (1 - Time) * (1 - Time);
        scrollCurrent.Opacity = Page == PrevPage ? 1 : Time * Time;
        OnUpdateTransform?.Invoke(this, new UpdateTransformEventArgs() { Prev = ds, Current = displace });
    }
    internal bool SwipeTo((int X, int Y) NewPage) {
        PrevPage = Page;
        if (NewPage == PrevPage)
            return false;
        Page = NewPage;
        double dx = Math.Sign(NewPage.X - PrevPage.X), dy = Math.Sign(NewPage.Y - PrevPage.Y);
        displaceStart = new(dx, dy);
        displaceNow += displaceStart;
        isSnapBack = false;
        OnSwipeStarted?.Invoke(this, new SwipeEventArgs() { Horizontal = dx != 0, Vertical = dy != 0, ToPage = NewPage });
        StartAnimate();
        return true;
    }

    private void OnLayoutUpdated(object? sender, EventArgs e) {
        if (host.Bounds.Width > 0 && host.Bounds.Height > 0) {
            UpdateTransforms(displaceNow);
            host.LayoutUpdated -= OnLayoutUpdated;
        }
    }
    private Point Damp(Point PointerPixels) {

        double PX = PointerPixels.X / SwipeBoundsW, APX = Math.Abs(PX), PY = PointerPixels.Y / SwipeBoundsH, APY = Math.Abs(PY);
        double a = 1 - Math.Max(APX, APY) * GetSensitivity / 6.0;
        if (a >= 1)
            return new(0, 0);
        Point N = Horizontal
            ? Vertical
                ? a <= 0.0
                    ? APX > APY ? new(PX * (1 - APY / APX), 0.0) : new(0.0, PY * (1 - APX / APY)) // Outside - zeroing the smaller axis, and shrinking the larger as you approach diagonal
                    : APX > APY ? new(((1 - APY / APX) * (1 - a) + a) * PX, a * PY) : new(a * PX, PY * ((1 - APX / APY) * (1 - a) + a)) // central region where mapping is 1:1
                : new(PX, 0.0)// only horizontal - pass only x untransformed
            : new(0.0, Vertical ? PY : 0.0); // only vertical - pass only y untransform. OR none of them and then pass (0,0)

        return new Point(
            N.X > 0
                ? CanLeft ? N.X : Math.Log(Math.Log(1e8) * N.X + 1.0, 1e8)
                : CanRight ? N.X : -Math.Log(-Math.Log(1e8) * N.X + 1.0, 1e8),
            N.Y > 0
                ? CanUp ? N.Y : Math.Log(Math.Log(1e8) * N.Y + 1.0, 1e8)
                : CanDown ? N.Y : -Math.Log(-Math.Log(1e8) * N.Y + 1.0, 1e8));
    }
    private Point InvDamp(Point DisplaceCoords, out bool snapH, out bool snapW) {
        var L = Math.Log(1e8);
        double PX = DisplaceCoords.X, APX = Math.Abs(PX), PY = DisplaceCoords.Y, APY = Math.Abs(PY);
        Point N = new(
            DisplaceCoords.X > 0
                ? (Math.Pow(1e8, DisplaceCoords.X) - 1.0) / L
                : -(Math.Pow(1e8, -DisplaceCoords.X) - 1.0) / L,
            DisplaceCoords.Y > 0
                ? (Math.Pow(1e8, DisplaceCoords.Y) - 1.0) / L
                : -(Math.Pow(1e8, -DisplaceCoords.Y) - 1.0) / L);
        snapH = false; snapW = false;
        if (APX > 0) {
            if (APY > 0) {
                double k = GetSensitivity / 6.0, nx = N.X, ny = N.Y,
                    anx = Math.Abs(nx), any = Math.Abs(ny), lo = 1e-6, hi = 1.0;
                bool xDominant = anx > any;
                for (int i = 0; i < 30; i++) {
                    double a = (lo + hi) * 0.5, px, py;
                    if (xDominant) {
                        double apx = Math.Abs(px = Math.Sign(nx) * (1 - a) / k),
                            apy = Math.Abs(py = ny / a),
                            nx_est = ((1 - apy / apx) * (1 - a) + a) * px;
                        if (nx_est > nx)
                            hi = a;
                        else
                            lo = a;
                    } else {
                        double apx = Math.Abs(px = nx / a),
                            apy = Math.Abs(py = Math.Sign(ny) * (1 - a) / k),
                            ny_est = ((1 - apx / apy) * (1 - a) + a) * py;
                        if (ny_est > ny)
                            hi = a;
                        else
                            lo = a;
                    }
                }
                double aa = (lo + hi) * 0.5;
                if (xDominant) {
                    PX = Math.Sign(nx) * (1 - aa) / k;
                    PY = ny / aa;
                } else {
                    PY = Math.Sign(ny) * (1 - aa) / k;
                    PX = nx / aa;
                }

                return new Point(PX * SwipeBoundsW, PY * SwipeBoundsH);
            } else {
                snapH = true;
                return new(N.X * SwipeBoundsW, 0);
            }
        } else {
            if (APY > 0) {
                snapW = true;
                return new(0, N.Y * SwipeBoundsH);
            } else {
                snapW = snapH = true;
                return new(0, 0);
            }
        }
    }
    internal void StartSwipe((int, int) ToPage, bool horizontal, bool vertical) {
        if (!IsDragging)
            return;
        StartSwipeForced(ToPage, horizontal, vertical);
    }
    internal void StartSwipeForced((int, int) ToPage, bool horizontal, bool vertical) {
        stopWatch.Stop();
        _pointer?.Capture(null);
        _pointer = null;
        Cursor = new Cursor(StandardCursorType.Arrow);
        IsDragging = false;
        SwipeEventArgs e = new SwipeEventArgs() { ToPage = ToPage, Horizontal = horizontal, Vertical = vertical };
        TriggerSwipe?.Invoke(this, e); // This should prompt the Owner to immediately switch my Page to ToPage, and call my RefreshHost with the two content i want to swipe between
    }

    #region Pointer Processor
    internal void Delete() => App.SwipeCaptured -= SwipeCaptured;
    protected void SwipeCaptured(object? s, EventArgs e) { if (s != this) CancelDrag(); }
    private void OnPressed(object? s, PointerPressedEventArgs e) {
        // do not allow another drag while swipe animation is running! Snap back animation is allowed to interrupt though.
        if (Time < 1 && Page != PrevPage)
            return;
        if (GetSensitivity <= 0)
            return;
        if (IsClickInsideFocusedTextBoxOrScrollBar(e.Source)) {
            Cursor = new Cursor(StandardCursorType.Arrow);
            IsDragging = false;
            _pointer = null;
            return;
        }
        Cursor = new Cursor(Horizontal ? Vertical ? StandardCursorType.SizeAll : StandardCursorType.SizeWestEast : Vertical ? StandardCursorType.SizeNorthSouth : StandardCursorType.No);
        IsDragging = true;
        StopAnimation();
        stopWatch.Restart();
        lastTime = stopWatch.ElapsedMilliseconds / 1000.0;
        Time = 1;
        // when grabbed before snapback fully returned, we'll add this offset to start from that offset, letting us catch that animation state
        var P = e.GetPosition(host);
        dragOrigin = P - InvDamp(displaceNow = displaceNowT, out bool snapH, out bool snapW);
        if (snapW)
            dragOrigin = new(P.X, dragOrigin.Y);
        if (snapH)
            dragOrigin = new(dragOrigin.X, P.Y);
        dragVelocity0 = new();
        UpdateTransforms(displaceNow);
    }
    private bool CanRight { get => Page.X < TransformBounds.ExcMaxX - 1; }
    private bool CanLeft { get => Page.X > TransformBounds.IncMinX; }
    private bool CanDown { get => Page.Y < TransformBounds.ExcMaxY - 1; }
    private bool CanUp { get => Page.Y > TransformBounds.IncMinY; }
    
    
    private void OnMoved(object? s, PointerEventArgs e) {
        if (!IsDragging)
            return;
        Point normalized = Damp(/*Rotate(*/e.GetPosition(host)/*)*/ - dragOrigin);
        var disS = 12.0 / GetSensitivity;
        var spdS = 120.0 / GetSensitivity;
        dragVelocity0 = (normalized - displaceNow) / Math.Max(GetDeltaTime, .01);
        dragVelocity0 = new(Horizontal ? dragVelocity0.X : 0, Vertical ? dragVelocity0.Y : 0);
        UpdateTransforms(displaceNow = normalized);
        if (.5 * disS < Math.Max(Math.Abs(displaceNow.X), Math.Abs(displaceNow.Y))
            || .5 * spdS < Math.Max(Math.Abs(dragVelocity0.X), Math.Abs(dragVelocity0.Y))
        ) {
            App.UnpressNumerics(this);
            (_pointer = e.Pointer).Capture(host);
            App.FireSwipeCaptured(this);
        }

        if (Horizontal) {
            // Make dragging over sensitivity threshold trigger a swipe
            // Make high velocities also trigger a swipe (if the users dones't drag much, but pushes it quickly instead)
            if (displaceNow.X > disS || dragVelocity0.X > spdS) {
                if (CanLeft)
                    StartSwipe((Page.X - 1, Page.Y), true, false);
                else OnLeftEvent?.Invoke(this, new EventArgs());
            }
            if (displaceNow.X < -disS || dragVelocity0.X < -spdS) {
                if (CanRight)
                    StartSwipe((Page.X + 1, Page.Y), true, false);
                else OnRightEvent?.Invoke(this, new EventArgs());
            }

        }
        if (Vertical) {
            if (displaceNow.Y > disS || dragVelocity0.Y > spdS) {
                if (CanUp)
                    StartSwipe((Page.X, Page.Y - 1), false, true);
                else OnTopEvent?.Invoke(this, new EventArgs());
            }
            if (displaceNow.Y < -disS || dragVelocity0.Y < -spdS) {
                if (CanDown)
                    StartSwipe((Page.X, Page.Y + 1), false, true);
                else OnBottomEvent?.Invoke(this, new EventArgs());
            }
        }
        OnMove?.Invoke(this, new EventArgs());
    }
    private void OnReleased(object? s, PointerReleasedEventArgs e) => CancelDrag();
    private void OnUncaptured(object? s, PointerCaptureLostEventArgs e) => CancelDrag();
    internal void CancelDrag() {
        if (!IsDragging)
            return;
        stopWatch.Stop();
        // Snap back animation:
        isSnapBack = true;
        StartAnimate();
    }
    bool IsClickInsideFocusedTextBoxOrScrollBar(object? source) {
        while (source is Control c) {
            if (c is TextBox tb)
                return tb.IsFocused;
            if (c is ScrollBar sb)
                return true;
            if (c is IoNumericLeftRight n)
                if (!(n.prevReleaseAction || n.nextReleaseAction))
                    if (n.prevHeld > 0 || n.nextHeld > 0)
                        n.Release();
            source = c.Parent;
        }
        return false;
    }
    #endregion

    #region Animation
    private void StartAnimate() {
        StopAnimation();
        Time = 0;
        Cursor = new Cursor(StandardCursorType.Arrow);
        IsDragging = false;
        _pointer?.Capture(null);
        _pointer = null;
        displaceNowT = displaceFrom = displaceNow;
        OnStartAnimate?.Invoke(this, new EventArgs());
        if (GetAniTime <= 0)
            FinishAnimation();
        else
            StartRenderLoop();
    }
    private void StopAnimation() {
        cancellationTokenSource.Cancel();
        cancellationTokenSource = new CancellationTokenSource();
        cancellationToken = cancellationTokenSource.Token;
    }
    private void StartRenderLoop() {
        stopWatchT.Restart();
        lastTimeT = stopWatchT.Elapsed.TotalSeconds;
        RequestNextFrame();
    }
    private void RequestNextFrame() => TopLevel.GetTopLevel(AppMainPanel)?.RequestAnimationFrame(OnRenderingFrame);

    private void OnRenderingFrame(TimeSpan timestamp) {
        if (cancellationToken.IsCancellationRequested) return;
        if (AnimateFrame())
            RequestNextFrame(); // only schedule next frame if still animating
    }
    private void FinishAnimation() {
        isSnapBack = false;
        Time = 1;
        dragVelocity0 = displaceNow = new(0, 0);
        PrevPage = Page;
        scrollPrevious.Content = null;
        stopWatchT.Stop();
        host.Children.Remove(scrollPrevious);
        host.Children.Remove(inputBlocker);
        AnimationFinished?.Invoke(this, new());
        displaceNowT = new(0, 0);
    }
    private bool AnimateFrame() {
        if ((Time += GetDeltaTimeT) >= 1) {
            displaceNowT = new(0, 0);
            FinishAnimation();
        } else // Animates with Cosine sigmoid, also blends in the initial drag velocity (with exponential decay for safety) at the beginning of the animation, for seamless continuity.
            displaceNowT = (Math.Cos(Math.PI * Time) * .5 + .5) * (displaceFrom + new Point(0,
                Math.Sign(dragVelocity0.Y) * (1 - Math.Exp(-Time * Math.Abs(dragVelocity0.Y) * GetAniTime / 1000.0))));
        if (!IsDragging)
            UpdateTransforms(displaceNowT);
        return Time < 1;
    }

    #endregion
}


/*VisualBrush? 
    brushCur = currentGrid == null ? null : new(currentGrid),
    brushPrev = previousGrid == null ? null : new(previousGrid);

Rectangle rectCur = new() {
    Width = Bounds.Width,
    Height = Bounds.Height,
    Fill = brushCur
}, rectPrev = new() {
    Width = Bounds.Width,
    Height = Bounds.Height,
    Fill = brushPrev
}*/

/*BmpCur = RenderControlToBitmap(gridCurrent = currentGrid);
BmpPrev = RenderControlToBitmap(gridPrevious = previousGrid);

var ImgCur = BmpCur == null ? null : new Image {
    Source = BmpCur,
    Width = Bounds.Width,
    Height = Bounds.Height,
    Stretch = Stretch.Fill
};
var ImgPrev = BmpPrev == null ? null : new Image {
    Source = BmpPrev,
    Width = Bounds.Width,
    Height = Bounds.Height,
    Stretch = Stretch.Fill
};


// Make Page Scroll:
scrollCurrent.Content = (object?)ImgCur ?? currentGrid;//currentGrid;
scrollPrevious.Content = (object?)ImgPrev ?? previousGrid;// previousGrid;
*/

//Dispatcher.UIThread.InvokeAsync(() => { }, DispatcherPriority.Render);
