using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Controls.Templates;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using IoListWallet.Content.Abstract;
using IoListWallet.Core.Basic;
using IoListWallet.Core.Dock;
using IoListWallet.Core.Static;
using IoListWallet.Content.Main;
using IoListWallet.Structs.Crypto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using static IoListWallet.App;
using static IoListWallet.Core.Static.AppSave;

namespace IoListWallet.Core;
internal abstract class IoPanel : IoUserControl {

    Viewbox MyView = new() { Stretch = Stretch.Uniform, StretchDirection = StretchDirection.DownOnly };

    internal IoPanel() : base() {
        Background = GetProfile?.BackgroundBrush;
        // Content grid:
        IsVisible = false;
        //ChildPanels = [];
        Content = MyGrid = new();
        MyGrid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));

        // titlebar
        Titlebar ??= new Grid {
            HorizontalAlignment = HorizontalAlignment.Stretch,
            RowDefinitions = [new RowDefinition(GridLength.Auto)]
        };
        titleTextborder.Child = titleText = new IoTextBlockControl() {
            TextAlignment = TextAlignment.Left, 
            PaddingMultiplier = 5 };

        titleTextborder.DoubleTapped += TitleBarDoubleTapped;
        /*Titlebar.PointerPressed += TitleBarPressed;
        Titlebar.PointerReleased += TitleBarReleased;
        Titlebar.PointerCaptureLost += TitleBarLost;*/
        Titlebar.AddHandler(PointerPressedEvent, TitleBarPressed, RoutingStrategies.Tunnel);
        Titlebar.AddHandler(PointerReleasedEvent, TitleBarReleased, RoutingStrategies.Tunnel);
        Titlebar.AddHandler(PointerCaptureLostEvent, TitleBarLost, RoutingStrategies.Tunnel);
        Titlebar.AddHandler(PointerMovedEvent, TitleBarMoved, RoutingStrategies.Tunnel);

        // i want a square column 0 (titleTextborder spanning further to 1, and on top of it my square), the star column 1 (only occupied by titleTextborder), then auto column 2 (occuptied by edit)
        titleTextBox = MakeSmallTextBox("TitleTextBox", this, "T_TitleTextBox", "", "TypeCustomTitle", TitleTextBox_LostFocus);
        titleTextContent = new() { 
            RowDefinitions = [new RowDefinition(GridLength.Star)], 
            ColumnDefinitions = [new ColumnDefinition(GridLength.Auto), new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Auto)] 
        };
        var innerGrid = new Grid {
            RowDefinitions = { new RowDefinition(GridLength.Star) },
            ColumnDefinitions = { new ColumnDefinition(GridLength.Star) }
        };
        square = new Border {
            Child = new TextBlock() { Text = "" },
            Background = ConstructionHelpers.MakeLeftShade(),
            VerticalAlignment = VerticalAlignment.Stretch,
            HorizontalAlignment = HorizontalAlignment.Left,
            Width = 20,
            IsHitTestVisible = false
        };
        squareR = new Border {
            Child = new TextBlock() { Text = "" },
            Background = ConstructionHelpers.MakeRightShade(),
            VerticalAlignment = VerticalAlignment.Stretch,
            HorizontalAlignment = HorizontalAlignment.Right,
            Width = 20,
            IsHitTestVisible = false
        };
        innerGrid.Children.Add(square);
        titleTextContent.Children.Add(innerGrid);
        titleTextContent.Children.Add(titleTextborder);
        titleTextContent.Children.Add(editTitle = MakeSmallButton("editTitle", this, "T_EditTitle", "✎", EditTitle_Click)?.Button!);

        Grid.SetColumn(editTitle, 2);
        Grid.SetColumnSpan(titleTextborder, 2);


        titleBox = new() { Content = titleTextContent }; // titleBox = titleTextContent / TitleTextBox

        colorPickB = MakeSmallButton("colorPickB", this, "T_ColorPick", "🎨");
        colorPickB.Button.Flyout = new Flyout() { Content = colorPick };
        colorPick.ColorChanged += ColorPick_ColorChanged;
        colorPick.Color = TitleColor;
        TitleColor = TitleColor; // this is called to perform the actions in the setter
        pin = MakeToolbarButton(L("Pin"));
        pin.Click += Pin_Click;
        pin.AreYouSure = ""; // make it togglable in color
        pinned = MakeToolbarButton(L("Pinned"));
        pinned.Click += (s, e) => ConstructionHelpers.CancelToggle(s, e);
        pinned.Click += (_, _) => Pinned_Click();
        //ConstructionHelpers.SetTip(pinned, "T_Pinned");

        home = MakeToolbarButton(L("Home"));
        home.Click += (s, e) => ConstructionHelpers.CancelToggle(s, e);
        home.Click += (_, _) => Home_Click();
        home.Text = L("Home");
        //ConstructionHelpers.SetTip(pinned, "T_Home");
        hide = MakeToolbarButton("");
        hide.Click += ConstructionHelpers.CancelToggle;
        hide.Click += HideButton_Click;
        min = MakeToolbarButton("_");
        min.Click += ConstructionHelpers.CancelToggle;
        min.Click += MinButton_Click;
        win = MakeToolbarButton(L("Undock"));
        win.Click += ConstructionHelpers.CancelToggle;
        win.Click += WindowButton_Click;
        back = MakeToolbarButton(L("Back"));
        back.Click += ConstructionHelpers.CancelToggle;
        back.Click += Back_Click;

        // overlay
        TooltipGrid = new() { IsVisible = false };
        TooltipGrid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
        TooltipGrid.RowDefinitions.Add(new RowDefinition(GridLength.Star));
        Tooltip = MakeButton("tooltip", this, "", "", (_, _) => TooltipGrid.IsVisible = false);
        Tooltip.View.StretchDirection = StretchDirection.Both;
        TooltipGrid.Children.Add(Tooltip.Button);
        TooltipGrid.Background = new SolidColorBrush(Colors.Black, Tooltip.Button.Opacity = .9);
        Grid.SetRowSpan(TooltipGrid, 2);

        // overlay
        Overlay = MakeTextBlock("overlay", this, "", "");
        OverlayGrid.Children.Add(Overlay.View);
        OverlayGrid.Children.Add(rects);
        OverlayGrid.Background = new SolidColorBrush(Colors.Black, Overlay.Text.Opacity = .8);
        Overlay.Text.IsHitTestVisible = Overlay.View.IsHitTestVisible = false;
        Grid.SetRowSpan(OverlayGrid, 2);

        // Hold
        var holdText = MakeTextBlock("holdText", this, "", "???");
        tooltipHold.Children.Add(holdText.View);
        holdText.Text.IsHitTestVisible = holdText.View.IsHitTestVisible = false;
        Grid.SetRowSpan(tooltipHold, 2);
    }

    private const string F = "Failed IoPanel.";

    #region Serialized
    internal static void Dirty() => GetProfile?.Pan.Dirty = true; // Somethign was edited in me - dirty the panel.json save file for saving

    [Save] // My dictionary key, if another panel will share my key it will point to me, only one isngleton with the same key can exist
    internal string Key { get; set { field = value; Init(Available); Dirty(); } } = "";
    [Save] // Where I have been opened from recently?
    internal string? ParentPanel { get; set { field = value; Dirty(); } } = null;
    [Save] // In multiwindow mode, you can write this in the editable title after lciking the Edittitle button
    internal string UserTitle { get; set { 
            titleText.Text = (field = value) == "" || !(GetProfile?.TitleText ?? false) 
                ? MyTitle 
                : value;
            titleTextBox?.Text = field = value;
            Dirty();
            var k = (typeof(PinnedPanel), "");
            if(IsOpened(k))
                RefreshPanel(k, true); 
        } } = "";
    [Save] // In multiwindow mode, you set the titlebar color
    internal Color TitleColor { get; set {
            if (GetMyMainPanel()?.Dock.Original == GetK)
                GetMyMainPanel()?.Dock.SetColor();
            colorPick.Color = field = value;
            square.Background = ConstructionHelpers.MakeLeftShade();
            SetTitleBarBackground();
            Dirty(); } } = GetProfile?.TitleColor ?? DPro.TitleColor; // default titlebar color (only show in setting if multiwindow)
    [Save] // Hide sensitive numbers
    internal bool HideNumbers {
        get; set {
            if (field == value)
                return;
            field = value;
            RefreshAll(false);
            Dirty();
        }
    } = false;

    protected bool /*notConst = true, */dragOut = false;
    internal int OriginalMainWindow;
    #endregion

    #region Properties
    internal virtual bool IsModal() => false;
    internal virtual int RefreshesWhenOpened() => 0;
    internal virtual string TrueParentType => MakeK(typeof(MainMenu), "");
    internal virtual bool DeleteWhenClosed() => false;
    internal Color GetTitleColor { get => (GetProfile?.TitleUnlock ?? false) ? TitleColor : GetProfile?.TitleColor ?? DPro.TitleColor; }
    internal string GetTitle() => UserTitle == "" || !(GetProfile?.TitleText ?? false) ? MyTitle : UserTitle;
    protected bool GetHidden() => HideNumbers || (FromK(ParentPanel) is IoPanel p && p.GetHidden());
    #endregion

    #region Pointers
    private string MyTitle = "";
    internal static string MakeK(Type type, string key) => type.Name + ":" + key;
    internal static (Type Type, string Key)? SplitK(string key) {
        var i = key.IndexOf(':');
        return i >= 0 && PTypes.TryGetValue(key[..i], out var type) ? (type, key[(i + 1)..]) : null;
    }
    internal string GetK => MakeK(GetType(), Key);
    internal virtual IoPanel? TrueParent => GetPanel(TrueParentType);
    internal IoDockPanels? ParentDock;
    internal Grid MyGrid { get; private set; }
    internal Grid OverlayGrid { get; private set; } = new() {
        ColumnDefinitions = [new ColumnDefinition(GridLength.Star)],
        RowDefinitions = [new RowDefinition(GridLength.Star)],
        IsHitTestVisible = false
    };
    internal Grid TooltipGrid { get; private set; }
    private WToggleButton Tooltip;
    private Grid tooltipHold = new() {
        ColumnDefinitions = [new ColumnDefinition(GridLength.Star)],
        RowDefinitions = [new RowDefinition(GridLength.Star)],
        Background = new SolidColorBrush(Colors.Black, .8),
        IsHitTestVisible = false,
        IsVisible = false
    };
    protected UserControl MyContent = new();
    //internal Bitmap? ProBackground { get; set { field = value; } } = null;
    internal IoMainPanel? GetMyMainPanel() {
        IoDock? RootDock = ParentDock ?? null;
        while (RootDock?.Parent != null)
            RootDock = RootDock.Parent;
        return ((IoDockRoot?)RootDock)?.Panel;
    }
    internal override void OnTooltip(object? s, ToolTipEventArgs e) {
        Tooltip.Text.Text = L(e.tooltip);
        TooltipGrid.IsVisible = true;
    }
    internal override void OnTooltipStart(object? s, EventArgs e) => tooltipHold.IsVisible = true;
    internal override void OnTooltipStop(object? s, EventArgs e) => tooltipHold.IsVisible = false;
    //protected void SetParent(IoPanel? parent) {
    //    if (parent?.GetK is not string k || ParentPanel == k)
    //        return;
    //FromK(ParentPanel)?.ChildPanels.Remove(GetK);
    //    ParentPanel = k;
    //parent?.ChildPanels.Add(GetK);
    //}
    #endregion

    #region Overlay
    internal WTextBlock? Overlay { get; set; }
    internal bool IsCursorOver { get => PointerOverOverlay.Count > 0 || IsPointerOver || GetDisplayMode == IMode.Fullscreen; }
    private readonly HashSet<object> PointerOverOverlay = [];
    internal void AddOverlayPointer(object? c) {
        if (c != null)
            PointerOverOverlay.Add(c);
    }
    internal void RemoveOverlayPointer(object? c) {
        if (c != null)
            PointerOverOverlay.Remove(c);
    }
    public class DockPreviewOverlay : Control {
        private Point RectPos = new(0, 0);
        private Size RectSize = new(0, 0);
        internal Color TitleColor;
        internal void Update(double dt, Point NewPos, Size NewSize) {
            if (NewPos.X == 0 && NewPos.Y == 0)
                NewPos = new(Bounds.Size.Width / 2, Bounds.Size.Height / 2);
            if (dt > 0) {
                var e = Math.Exp(-12 * dt);
                RectPos = RectPos * e + NewPos * (1 - e);
                RectSize = RectSize * e + NewSize * (1 - e);
            } else {
                RectPos = NewPos;
                RectSize = NewSize;
            }
            InvalidateVisual(); // request redraw
        }
        public override void Render(DrawingContext context) {
            base.Render(context);
            if (RectSize.Width > 0 && RectSize.Height > 0) 
                context.DrawRectangle(new SolidColorBrush(TitleColor, .25), 
                    new Pen(Brushes.LightBlue, 8, DashStyle.Dash, PenLineCap.Round), 
                    new Rect(RectPos, RectSize), 8);
        }
    }
    Point _dS;
    short Hor = 0, Ver = 0;

    internal (IoPanel?, IoWindow?) UpdateOverlay(Color c, double dt, double hide, IoWindow? w, PixelPoint dS) {
        if (Overlay?.Text is not IoTextBlockControl t /*|| OverlayX?.Text is not TextBlock tx*/)
            return (null,null);

        var window = TryWindow(GetMyMainPanel()?.MyWindowK ?? -1);

        var disabled = !(window?.IsEnabled ?? true);
        if (disabled) {
            dt = 1000000;
            hide = .01 * (GetProfile?.MaskOpacity ?? DPro.MaskOpacity);
        }

        // If unconvered, jump to the full unmask period "instantly":
        if (t.Opacity <= 0 && hide <= 0)
            dt = 1000000;

        OverlayGrid.IsVisible = (t.Opacity = Math.Clamp(hide, t.Opacity - dt, t.Opacity + dt)) > 0.02;
        t.IsVisible = window != null && !window.isDockDragging;
        MyContent.IsVisible = t.IsVisible || window == null;
        OverlayGrid.Background = new SolidColorBrush(disabled ? GetTitleColor : Colors.Black, t.Opacity);

        if (w == null || (dS.X == 0 && dS.Y == 0))  {
            rects.Update(0, new(0,0), new(0,0));
        } else {
            rects.TitleColor = c;
            PixelPoint minS = w.PointToScreen(Bounds.Position);
            PixelPoint maxS = w.PointToScreen(Bounds.BottomRight);
            _dS = Rotate(new((double)(dS.X - minS.X) / (maxS.X - minS.X) - .5, (double)(dS.Y - minS.Y) / (maxS.Y - minS.Y) - .5));
            if (Math.Abs(_dS.X) >= .5 || Math.Abs(_dS.Y) >= .5) {
                rects.Update(0, new(0,0), new(0, 0));
            } else return (this,w);
        }
        
        return (null,null);
    }
    internal void UpdateOverlay2(double dt, bool allowBook = true) {
        if (allowBook && Math.Abs(_dS.X) < .25 && Math.Abs(_dS.Y) < .25 && GetMyMainPanel()?.Dock.Original != GetK) {
            Hor = Ver = 0;
            var size = Bounds.Size * .25;
            rects.Update(dt, /*Bounds.Position +*/ new Point(size.Width, size.Height), size * 2.0);
        } else if (Math.Abs(_dS.X) > Math.Abs(_dS.Y)) {
            Ver = 0;
            var s = new Size(Bounds.Size.Width / 2 - 12, Bounds.Size.Height - 24);
            if (_dS.X > 0) {// right
                Hor = 1;
                rects.Update(dt, /*Bounds.Position +*/ new Point(Bounds.Size.Width / 2, 12), s);
            } else {// left
                Hor = -1;
                rects.Update(dt, /*Bounds.Position +*/ new Point(12, 12), s);
            }
        } else {
            Hor = 0;
            var s = new Size(Bounds.Size.Width - 24, Bounds.Size.Height / 2 - 12);
            if (_dS.Y > 0) {// bottom
                Ver = 1;
                rects.Update(dt, /*Bounds.Position +*/ new Point(12, Bounds.Size.Height / 2), s);
            } else {// top
                Ver = -1;
                rects.Update(dt, /*Bounds.Position +*/ new Point(12, 12), s);
            }
        }
    }
    #endregion

    #region Toolbar
    internal Grid Titlebar { get; private set; }
    private UserControl titleBox;
    private ContentControl titleBarContainer = new();
    private IoTextBox? titleTextBox; 
    private Grid titleTextContent; 
    private IoTextBlockControl titleText;
    private Border titleTextborder = new() { HorizontalAlignment = HorizontalAlignment.Stretch, VerticalAlignment = VerticalAlignment.Stretch, IsHitTestVisible = true, Background = new SolidColorBrush(Colors.Transparent) };
    private IoToggleButton editTitle;
    private ColorPicker colorPick = new();
    private WSToggleButton? colorPickB;
    private IoToggleButton pin;
    private IoToggleButton hide;
    private IoToggleButton min; 
    private IoToggleButton win;
    private IoToggleButton back;
    //private TextBlock debug = new();
    internal Border square { get; private set; }
    internal Border squareR { get; private set; }
    internal DockPreviewOverlay rects { get; private set; } = new();
    private WComboBox? DockBook = null;
    private IoToggleButton home, pinned;
    private void TitleTextBox_LostFocus(object? s, EventArgs e) {
        titleBox.Content = titleTextContent;
        UserTitle = titleTextBox?.Text ?? "";
    }
    private void EditTitle_Click(object? s, RoutedEventArgs e) => titleBox.Content = titleTextBox;
    private void Pin_Click(object? s, EventArgs e) {
        if (pin == null)
            throw new(F + "Pin_Click");
        if (IsModal() || GetProfile?.Pan is not IoProfile.SaveFilePan Pan)
            return; // modal panels don't allow getting pinned
        if (Pan.PinnedPanels.Remove(GetK)) {
            //pin.Content = L("Pin");
            pin.IsChecked = false;
        } else {
            Pan.PinnedPanels.Add(GetK);
            //pin.Content = L("Unpin");
            pin.IsChecked = true;
        }
        Pan.Dirty = true;
    }
    private void Pinned_Click() {
        // modal panels don't allow going to a different panel
        if(!IsModal() && GetProfile?.Pan.PinnedPanels.Count > 0)//if (FromK(GetMyMainPanel()?.Dock.Original) is IoPanel MyPanel && !MyPanel.IsModal())
            OpenRefreshPanel(this, typeof(PinnedPanel));
    }
    private void Home_Click() {
        if(!IsModal())//if (FromK(GetMyMainPanel()?.Dock.Original) is IoPanel MyPanel && !MyPanel.IsModal())
            OpenPanel(this, typeof(MainMenu));
    }
    private void ColorPick_ColorChanged(object? s, EventArgs e) {
        if(TitleColor != colorPick.Color)
            TitleColor = colorPick.Color;
    }
    private void HideButton_Click(object? s, EventArgs e) => HideNumbers = !HideNumbers;
    private void MinButton_Click(object? s, EventArgs e) {
        if (TryWindow(GetMyMainPanel()?.MyWindowK ?? -1) is IoWindow w)
            w.WindowState = WindowState.Minimized; // TODO maybe returning from here should fix border properties...?
    }
    private void WindowButton_Click(object? s, EventArgs e) {
        var mp = GetMyMainPanel();
        if (mp?.Dock.Original == GetK) {
            // I am the Original, so my titlebar is the window's titlebar, so this will be the un/maximize button
            if (TryWindow(mp?.MyWindowK ?? -1) is IoWindow w)
                w.ToggleMaximize();
            Unmask();
            return;
        }
        // undock back into my original window

        var prev = ParentDock?.Parent;
        UndockMe();
        prev?.RefreshMinSize();

        OpenRefreshPanel(TrueParent, GetType(), Key);

        // TODO this is broken, the undocked isn't showing up
        //mp?.DockHost.Content = mp.Dock.Build();
    }
    private void Back_Click(object? s, EventArgs e) {
        if (this is MainMenu) AppClose(); else Close();
    }
    #endregion

    #region Lifetime
    private Point? startPointer;
    //private PixelPoint startPos;
    private bool IsDockedPanel() => GetMyMainPanel() is IoMainPanel myMain && myMain.Dock.Original != GetK;
    bool unmax;
    bool IsClickInsideControl(object? source) {
        while (source is Control c) {
            if (c is TextBox tb || c is Button sb || c is ComboBox cb)
                return true;
            source = c.Parent;
        }
        return false;
    }
    private void TitleBarPressed(object? s, PointerPressedEventArgs e) {

        if (IsClickInsideControl(e.Source))
            return;

        var w = TryWindow(OriginalMainWindow);
        unmax = w != null && (w.WindowState == WindowState.Maximized || w.Snapped);
        if (IsDockedPanel() || unmax) {
            // TODO might be nice if I made this work, puling a panel out of dock by its titlebar
            if (GetDisplayMode != IMode.MultiWindow)
                return;
            startPointer = e.GetPosition(null);
            dragOut = true;
            return;
        }
        if (w == null)
            return;
        startPointer = e.GetPosition(null);
        w.DragStarted();
    }
    void TitleBarMoved(object? s, PointerEventArgs e) {
        var delta = e.GetPosition(null);
        var start = startPointer ?? delta;
        startPointer = start;
        delta -= start;
        if (dragOut) {

            // threshold to drag out
            var screen = AppMainWindow?.Screens.ScreenFromPoint(this.PointToScreen(e.GetPosition(this)));
            var t = screen == null ? 100 : Math.Clamp((int)(Math.Min(screen.WorkingArea.Width, screen.WorkingArea.Height) * 0.04), 32, 128);

            if (delta.X * delta.X + delta.Y * delta.Y > t*t) {
                dragOut = false;

                // removes myself from the other window's dock
                var c = e.Pointer.Captured;
                startPointer = null;
                var screenPoint = Titlebar.PointToScreen(e.GetPosition(Titlebar));

                IoWindow draggedOut;
                var prev = ParentDock?.Parent;
                if (unmax) {
                    unmax = false;
                    if (TryWindow(GetMyMainPanel()?.MyWindowK ?? -1) is not IoWindow d)
                        return;
                    if (d.Snapped) {
                        // unsnap
                        d.Snapped = false;
                        d.Position = new(d.MyX, d.MyY);
                        d.Width = d.MyWidth;
                        d.Height = d.MyHeight;
                    } else d.ToggleMaximize(); // unmaximize
                    
                    draggedOut = d;
                } else {
                    // undock
                    UndockMe();
                    prev?.RefreshMinSize();
                    // respawn my window and attach myself to it's content
                    OpenRefreshPanel(TrueParent, GetType(), Key);
                    if (TryWindow(GetMyMainPanel()?.MyWindowK ?? -1) is not IoWindow d)
                        return;
                    draggedOut = d;
                }

                // currently i am putting the new window's top left corner to the cursor, mimicking the original position would be hard to clamp because i can't predict how much space will be avaiable in the new titlebar
                var offset = (int)(GetProfile?.ControlFontSize ?? DPro.ControlFontSize);
                //draggedOut.Position = new(screenPoint.X - offset/*(int)Math.Min(titlePosition.X, draggedOut.Width / 2)*/, screenPoint.Y - offset/*(int)titlePosition.Y*/);
                draggedOut.Position = new(screenPoint.X - offset, screenPoint.Y - offset);
                startPointer = new(offset, offset);// this actually works precisely, and transfers it perfectly no matter how fast i drag

                draggedOut.DragStarted();
                e.Pointer.Capture(c); // this transfers the pointer hold to the new window, but i was warned it might be fragile on other platforms...?
                
            }
            return;
        }
        if (IsDockedPanel() || TryWindow(OriginalMainWindow) is not IoWindow w || !w.isDragging || w.WindowState == WindowState.Maximized)
            return;
        w.Position += new PixelPoint((int)delta.X, (int)delta.Y);
        // ghost snap preview window:
        if (FakeWindow == null)
            return;
        FakeWindow.TargetOpacity = SnapLogic(this.PointToScreen(e.GetPosition(this)), FakeWindow, TryWindow(GetMyMainPanel()?.MyWindowK ?? -1)) ? IoFakeWindow.FullOpacity : 0;

        // DEBUG
        /*debug.Text = "O " + FakeWindow.TargetOpacity.ToString() + " W " + FakeWindow.TargetWidth.ToString() + " H " + FakeWindow.TargetHeight.ToString() + "\n P " + FakeWindow.TargetPosition.ToString()
            + "\n O " + FakeWindow.TransientOpacity.ToString() + " TW " + FakeWindow.TransientWidth.ToString() + " TH " + FakeWindow.TransientHeight.ToString() + "\n V " + (FakeWindow.IsVisible ? "I" : "X")
            + "\n O " + FakeWindow.r.RectOpacity.ToString() + " RP " + FakeWindow.r.RectPos.ToString() + " RS " + FakeWindow.r.RectSize.ToString() + "\n TC " + FakeWindow.r.TitleColor.ToString()
            + "\n Child " + ((Panel)FakeWindow.Content).Children.Count.ToString();*/

    }
    private void TitleBarReleased(object? s, PointerReleasedEventArgs e) => 
        EndDrag(e.Pointer, this.PointToScreen(e.GetPosition(this)));
    private void TitleBarLost(object? s, PointerCaptureLostEventArgs e) => 
        EndDrag(e.Pointer, null);
    private void TitleBarDoubleTapped(object? s, TappedEventArgs e) {
        if (IsDockedPanel() || TryWindow(OriginalMainWindow) is not IoWindow w)
            return;
        w.DoubleTap();
    }
    private void EndDrag(IPointer e, PixelPoint? EndPoint) {
        dragOut = unmax = false;
        FakeWindow?.IsVisible = false;
        if (IsDockedPanel() || TryWindow(OriginalMainWindow) is not IoWindow w || !w.isDragging)
            return;
        w.DragEnded();
        if (EndPoint == null || w.WindowState != WindowState.Normal || w.Snapped)
            return;
        SnapLogic(EndPoint.Value, w);
    }
    private Avalonia.Platform.Screen? screen = null;
    private bool SnapLogic(PixelPoint pix, Window win, IoWindow? refWindow = null) {
        var _screen = win.Screens.ScreenFromPoint(pix);
        if (_screen != null)
            screen = _screen;
        if(screen == null)
            return true;
        var a = screen.WorkingArea;
        // clamping because the very edge might be glitchy
        var loc = new PixelPoint(Math.Clamp(pix.X - a.X, 1, a.Width - 2), Math.Clamp(pix.Y - a.Y, 1, a.Height - 2));

        // W,H - division of the remaining width and height (2 will stretch from the location halway to the border, 1 will fill towards the border)
        bool Snap(PixelPoint L, int W, int H) {
            var w = (a.Right - L.X) / W;
            var h = (a.Bottom - L.Y) / H;
            PixelPoint toPos = L;
            double toW = w, toH = h;

            if (win is IoWindow iw) {
                if (W == 1 && H == 1 && L == a.Position) {
                    iw.ToggleMaximize();
                    return true;
                }
                iw.Snapped = true;
                // remember original size to return to when unsnapped
                (iw.MyX, iw.MyY) = (win.Position.X, win.Position.Y);
                iw.MyWidth = win.Width;
                iw.MyHeight = win.Height;
            } else if (win is IoFakeWindow fw && refWindow != null) {
                fw.SetColor(FromK(refWindow.MyMainPanel.Dock.Original)?.TitleColor ?? Colors.White);
                fw.TargetOpacity = 0.5;
                fw.TargetPosition = L;
                fw.TargetWidth = L.X + w;
                fw.TargetHeight = L.Y + h;
                if (/*fw.WindowState != WindowState.Normal*/!fw.IsVisible) {
                    fw.IsVisible = true;
                    //fw.Opacity = 1;
                    //fw.WindowState = WindowState.Normal;
                    fw.TransientPosition = refWindow.Position;
                    fw.TransientWidth = refWindow.Position.X + refWindow.Width;
                    fw.TransientHeight = refWindow.Position.Y + refWindow.Height;
                } else if (fw.TransientPosition != fw.TargetPosition 
                    || fw.TransientWidth != fw.TargetWidth 
                    || fw.TransientHeight != fw.TargetHeight) {
                    toPos = new(Math.Min(fw.Position.X, L.X), Math.Min(fw.Position.Y, L.Y));
                    toW = Math.Max(w + L.X - toPos.X, fw.Width) ;
                    toH = Math.Max(h + L.Y - toPos.Y, fw.Height);
                }
            }
            if (win.Position != toPos)
                win.Position = toPos;
            if (win.Width != toW)
                win.Width = toW;
            if (win.Height != toH)
                win.Height = toH;
            return true;
        }
        var t = Math.Clamp((int)(Math.Min(a.Width, a.Height) * 0.02), 16, 64); // threshold to snap
        if (loc.X < t) {
            if (loc.Y < t)
                return Snap(a.Position, 2, 2); // top-left
            else if (loc.Y >= a.Height - t)
                return Snap(new(a.X, a.Y + a.Height / 2), 2, 1); // bottom-left
            else {
                return Snap(a.Position, 2, 1); // left
                // TODO  maybe also trigger that system picker of what to put on the right like windows does?
            }
        } else if (loc.X >= a.Width - t) {
            if (loc.Y < t)
                return Snap(new(a.X + a.Width / 2, a.Y), 1, 2); // top-right
            else if (loc.Y >= a.Height - t) {
                return Snap(new(a.X + a.Width / 2, a.Y + a.Height / 2), 1, 1); // bottom-right
            } else {
                return Snap(new(a.X + a.Width / 2, a.Y), 1, 1); // right
                // TODO maximize to right half (and maybe also trigger that system picker of what to put on the left like windows does?)
            }
        } else if (loc.Y < t)
            return Snap(a.Position, 1, 1); // total maximize
        return false;
    }
    internal virtual void Init(Size available) { }
    internal virtual void Snapshot() { }
    // A parent panel opens this as a new child panel
    internal virtual void Open(IoPanel? Parent) {
        if (!IsVisible) {
            if (GetDisplayMode == IMode.MultiWindow) {
                // open a new window to dock into:
                IoWindow NewWindow = new() {
                    WindowStartupLocation = WindowStartupLocation.CenterOwner
                };
                ParentDock = (IoDockPanels)NewWindow.MyMainPanel.Dock.TopNonRoot;
                IoDock? dock = Parent?.ParentDock;
                while (dock?.Parent != null)
                    dock = dock.Parent;
                if (dock is IoDockRoot Root
                    && TryWindow(Root.Panel?.MyWindowK ?? -1) is IoWindow window
                    && window.IsVisible
                ) {
                    NewWindow.Show(window);
                    NewWindow.OpenedFrom = window.Index;
                } else NewWindow.Show();
                OriginalMainWindow = NewWindow.Index;
                if (this is MainMenu)
                    AppMainWindow = NewWindow;
            } else {
                if (this is MainMenu thisMenu) {
                    Register(GetK, AppMainMenu = thisMenu);
                    IoDockPanels? Dock = null;
                    if (Desktop != null) {
                        if (GetDisplayMode == IMode.SingleWindow) {
                            AppMainPanel = (AppMainWindow = new()).MyMainPanel;
                            Dock = (IoDockPanels)AppMainWindow.MyMainPanel.Dock.TopNonRoot;
                        } else {
                            Dock = (IoDockPanels)(AppMainPanel = new()).Dock.TopNonRoot;
                        }
                    } else {
                        AppMainPanel = new();
                        Dock = (IoDockPanels)AppMainPanel.Dock.TopNonRoot;
                    }
                    (ParentDock = Dock).Panel = [GetK];
                    AppDockRoot = ParentDock?.Parent as IoDockRoot;
                    AppDockRoot?.Original = GetK;
                } else {
                    // replace myself into parent's dock:
                    ParentDock = Parent?.ParentDock ?? AppMainMenu?.ParentDock ?? ParentDock;
                    Parent?.IsVisible = false;
                    Parent?.UndockMe();
                }

              
            }
            ParentDock?.Panel = [GetK]; // dock myself in
            GetMyMainPanel()?.Dock.Original = GetK;
            Dirty();
        }
        OnOpen(); // IsVisible = true; +focus on my window
        Load(Parent);
    }
    internal void Load(IoPanel? Parent) {
        // Only set parent if it doesn't cause a loop
        SetTitle();
        var P = Parent;
        while (P != null) {
            if (P == this) return;
            P = FromK(P.ParentPanel);
        }
        ParentPanel = Parent?.GetK;
    }
    internal void Unmask() => UpdateOverlay(Colors.Transparent, 1000, -(GetProfile?.UnMaskTime ?? DPro.UnMaskTime), null, new(0, 0));
    internal virtual IoPanel OnOpen() {
        // Focus on me:
        for (var i = 0; i < ParentDock?.Panel.Count; ++i)
            if (ParentDock.Panel[i] == GetK)
                ParentDock?.Selected = i;
        Unmask();
        IsVisible = true;
        if (ParentDock == null)
            return this;
        // Find the Root Dock and Window:
        var MainPanel = GetMyMainPanel();
        if (MainPanel?.Dock == ParentDock.Parent) {
            // We are the only panel in the dock
        }
        // Update Bottom toolbar to reflect the state of the new content:
        MainPanel?.Update();
        if ((GetProfile?.Win.OpenedWindows.TryGetValue(MainPanel?.MyWindowK ?? -1, out var w) ?? false) && w is IoWindow OpenWindow) {
            OpenWindow.Activate();
            OpenWindow.IsVisible = true;
            OpenWindow.Focus();
            if (IsModal() && Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop) 
                foreach (var eachwindow in desktop.Windows)
                    if (eachwindow != OpenWindow) {
                        // This donesn't work, for some reason, it gives a Hand icon...?
                        //eachwindow.Cursor = new Cursor(StandardCursorType.No);
                        eachwindow.IsEnabled = false;
                    }
            // If i am not selected in a dockBook, sleect me, and refresh it, so that it shows me
            if (ParentDock.Panel[ParentDock.Selected] != GetK) {
                ParentDock.Selected = ParentDock.Panel.IndexOf(GetK);
                OpenWindow.MyMainPanel.RefreshDock();
            }
        }
        return this;
    }
    internal void DockInto(IoWindow Incoming) {
        if (GetMyMainPanel() is not IoMainPanel main)
            return;
        //ParentDock ??= new IoDockPanels() { Panel = [GetK], Selected = 0 };
        main.Dock.UnBuild(/*true*/);
        
        main.DockHost.Content = null;

        // TODO Closing crashes?
        //Incoming.Hide();
        var IncomingDock = Incoming.MyMainPanel.Dock.TopNonRoot;
        
        if (ParentDock == null)
            return;
        bool success = true;
        if (Hor != 0 || Ver != 0) {
            var s = new IoDockSplit() { SplitRatio = .5, Orientation = Hor != 0 ? Orientation.Horizontal : Orientation.Vertical };
            (s.First, s.Second) = (Hor != 0 ? Hor > 0 : Ver > 0) ? ((IoDock)ParentDock, IncomingDock) : (IncomingDock, ParentDock);
            s.Parent = ParentDock.Parent;
            var parentRoot = ParentDock.Parent as IoDockRoot;
            parentRoot?.TopNonRoot = s;
            var parentSplit = ParentDock.Parent as IoDockSplit;
            IoDock? fir = null, sec = null;
            if (parentSplit != null) {
                fir = parentSplit.First;
                sec = parentSplit.Second;
                (parentSplit.First, parentSplit.Second) = parentSplit.First == ParentDock ? ((IoDock)s, parentSplit.Second) : (parentSplit.First, s);
            }
            ParentDock.Parent = IncomingDock.Parent = s;
            ParentDock.RefreshMinSize();

            if (TryWindow(main.MyWindowK) is IoWindow w && (w.MinWidth > Available.Width || w.MinHeight > Available.Height)) {
                // Got too big - Revert Split
                ParentDock.Parent = s.Parent;
                IncomingDock.Parent = Incoming.MyMainPanel.Dock;
                if (parentSplit != null) {
                    parentSplit.First = fir!;
                    parentSplit.Second = sec!;
                }
                parentRoot?.TopNonRoot = ParentDock;
                s.MyBuild = null;
                success = false;
                
            }
        } else {
            if (IncomingDock is not IoDockPanels incomingPanels || incomingPanels.Panel.Count != 1) // TODO maybe i could extend that one day?
                throw new("Only single panels are allowed into a book!");
            var prevSelected = ParentDock.Selected;

            ParentDock.Selected = ParentDock.Panel.Count;
            var incomingPanel = incomingPanels.Panel[0];
            ParentDock.Panel.Add(incomingPanel);
            var incPan = FromK(incomingPanel) ?? throw new("No Panel?");
            var prevPar = incPan.ParentDock;
            incPan.ParentDock = ParentDock;
            ParentDock.RefreshMinSize();

            if (TryWindow(main.MyWindowK) is IoWindow w && (w.MinWidth > Available.Width || w.MinHeight > Available.Height)) {
                // Got too big - Revert Book
                ParentDock.Selected = prevSelected;
                ParentDock.Panel.RemoveAt(ParentDock.Panel.Count - 1);
                incPan.ParentDock = prevPar;
                success = false;
            }
        }


        if (success) {
            Incoming.MyMainPanel.Dock.UnBuild(true);
            Incoming.MyMainPanel.Content = null;
            //Incoming.MyMainPanel.Dock.MyBuild = Incoming.MyMainPanel.Dock.TopNonRoot.MyBuild = null;
            Incoming.MyMainPanel.DockHost.Content = null;
            (Incoming.MyMainPanel.Dock.TopNonRoot = new IoDockPanels()).Parent = Incoming.MyMainPanel.Dock;
            Incoming.Close();
        } else {
            ParentDock.RefreshMinSize();
            Incoming.MyMainPanel.Dock.RefreshMinSize(true);
            Incoming.MyMainPanel.DockHost.Content = Incoming.MyMainPanel.Dock.Build();
        }

        main.DockHost.Content = main.Dock.Build(true);
        Unmask();
    }
    internal void UndockMe(bool BackToWindow = false) {
        if (Parent is Panel p)
            p.Children.Remove(this);
        if (Parent is ContentControl cc)
            cc.Content = null;
        IsVisible = BackToWindow; // hide itself
        if (ParentDock == null)
            return;
        ParentDock.MyBuild = null;
        var myMain = GetMyMainPanel();
        myMain?.Dock.UnBuild();
        myMain?.DockHost.Content = null;

        void CloseWin() {
            // If it is the only panel docked in this window in MultiWindow mode - close the window
            if (myMain?.Dock.Original == GetK) //ParentDock?.Parent is IoDockRoot Root)
                if (TryWindow(myMain?.MyWindowK ?? -1) is IoWindow window//Root.Panel?.MyWindowK ?? -1) is IoWindow window
                    && GetDisplayMode == IMode.MultiWindow
                    && !window.IsClosing
                ) 
                    window.Close();
                
        }

        // If I was the Original in the docking window, then release them back to their windows (make sure to keep the original IoWindow of each around, only as hidden, with the content detached, that will get reshown and reattached)
        if (myMain?.Dock.Original == GetK) {
            void UndockAll(IoDock? d) {
                if (d == null)
                    return;
                if (d is IoDockSplit s) {
                    UndockAll(s.First);
                    UndockAll(s.Second);
                    return;
                }
                if (d is IoDockPanels ip)
                    foreach (var p in ip.Panel)
                        if (p != GetK)
                            FromK(p)?.UndockMe(true);
            }
            UndockAll(myMain?.Dock.TopNonRoot);
            if(GetDisplayMode == IMode.MultiWindow)
                ParentDock.Panel = [];
            CloseWin();

        } else {

            if (ParentDock.Panel.Count() > 1) {
                bool wasSelected = ParentDock.Panel[ParentDock.Selected] == GetK;
                ParentDock.Panel.Remove(GetK);
                ParentDock.Selected %= ParentDock.Panel.Count();
                if (wasSelected) {
                    ParentDock.RefreshMinSize();
                    FromK(ParentDock.Panel[ParentDock.Selected])?.RefreshTitleBar();
                    //ParentDock.Build(); // is Build needed?
                }
            } else {
                CloseWin();
                var DockParent = ParentDock.Parent;
                if (DockParent is IoDockSplit Split) {
                    Split.UnBuild(false);
                    // Undock from a split Dock - will turn the split into a single dock containing the remaining other panel
                    var SplitParent = Split.Parent;
                    SplitParent?.UnBuild(false);
                    var Remaining = Split.First == ParentDock ? Split.Second : Split.First;
                    Remaining.Parent = SplitParent;
                    // the parent can be either a root or a split, handle these cases:
                    if (SplitParent is IoDockRoot root) {
                        // parent of my split is already the root, so the remaining other half will become the new first dock under the root
                        root.TopNonRoot = Remaining;
                        // If only the last IoPanel remains in the window - make it the window's title
                        //if (Remaining is IoDockPanels RemainingLast)
                        //    FromK(RemainingLast.Panel)?.SetTitle();
                    }
                    if (SplitParent is IoDockSplit split) {
                        // the parent above my split is another split, so place the remaining other half into my previous spot on that parent split
                        if (split.First == Split)
                            split.First = Remaining;
                        else split.Second = Remaining;
                    }
                }
            }
            if (this is not MainMenu && GetDisplayMode == IMode.MultiWindow)
                myMain?.DockHost.Content = myMain.Dock.Build();
        }
        /*if (BackToWindow && (TryWindow(OriginalMainWindow)) is IoWindow w) {
            w.Show();
            var t = new IoDockPanels();
            var md = w.MyMainPanel.Dock;
            md.TopNonRoot = t;
            t.Panel = [GetK];
            w.MyMainPanel.DockHost.Content = (t.Parent = md).Build();
        } else*/// ParentDock = null;
        //if(TryWindow(myMain?.MyWindowK ?? -1) != null && GetDisplayMode == DisplayMode.MultiWindow)
        //if(this is not MainMenu)
        //    myMain?.DockHost.Content = myMain.Dock.Build();
        Dirty();
        Unmask();
    }
    // call this to close the panel
    internal virtual void Close(bool goBack = true) {
        if (!IsVisible)
            return;
        var p = FromK(ParentPanel);
        if (p == null && !CanCloseMain) {
            // closing mainmenu panel
            AppClose();
            return;
        }
        var M = GetMyMainPanel();
        var Dock = ParentDock;
        var prev = Dock?.Parent;
        UndockMe();
        if(M?.Dock.Original != GetK)
            prev?.RefreshMinSize();
        int refreshMask;
        if (goBack) {
            if (GetDisplayMode == IMode.MultiWindow)
                FromK(ParentPanel)?.OnOpen();
            else if (ParentPanel != null && p != null && FromK((p.ParentDock = Dock)?.Panel[0] = ParentPanel) is IoPanel parent) {

                if((refreshMask = parent.OnOpen().RefreshesWhenOpened()) != 0)
                    RefreshPanel(parent, refreshMask, true); // redock parent into the dock I've undocked myself from
                parent.UpdateMinSize();
            }
        }
        OnClose();
        if (DeleteWhenClosed())
            Delete();
        Dirty();
    }
    // Should also be called when ParentDock's Root IoWindow gets closed:
    protected virtual void OnClose() {
        // unfreezes everything if i was modal
        static void Unlock(Window w) {
            w.IsEnabled = true;
            w.Cursor = new Cursor(StandardCursorType.Arrow);
        }
        if (IsModal() && Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop) {
            if ((FromK(ParentPanel)?.IsModal() ?? false) && TryWindow(FromK(ParentPanel)?.GetMyMainPanel()?.MyWindowK ?? -1) is Window parentWindow)
                Unlock(parentWindow);
            else foreach (var eachwindow in desktop.Windows)
                Unlock(eachwindow);
        }
    }
    internal void WindowClosed() => Close(false);
    internal virtual void Delete(bool refresh = true) {
        ParentDock?.MyBuild = null;
        Content = null;
        GetProfile?.Pan.PinnedPanels.Remove(GetK);
        GetProfile?.Pan.LoadedPanels.Remove(GetK);
        if (refresh) {
            var Pinned = GetPanel((typeof(PinnedPanel), ""));
            int refreshMask = Pinned.RefreshesWhenOpened();
            if (Pinned.IsVisible) // only if visible, because Pinned is refreshing itself upon opening
                RefreshPanel(Pinned, refreshMask, true); // Refresh Pinned panel
            Close();
        }
    }
    // Should only be called when we actually want to lose the panel, like closing a transaction viewer for good, reopening will be reseted
    protected virtual void OnDelete() { }
    #endregion

    #region Async
    internal virtual int IsAsync() => 0;
    internal bool Cancel(CancellationToken? cancel)
        => cancel != null && cancel.Value.IsCancellationRequested;
    internal virtual void AsyncTask(int Job, CancellationToken? cancel, TaskBin? task) {
        // run async Refresh, with cancel token so we coudl abort it if we need to refresh again before this finishes
        if (((RefreshesWhenOpened() & Job) == 0) || IsVisible)
            RefreshData(Job, cancel);
        task?.State = TaskState.Done; // mark it as finished
    }
    internal virtual void FinishAsyncTask(int Job) {
        // run async Refresh, with cancel token so we coudl abort it if we need to refresh again before this finishes
        if (((RefreshesWhenOpened() & Job) == 0) || IsVisible)
            Refresh(Job);
    }
    #endregion

    #region Layout
    internal void UpdateMax() {
        win.Text = GetMyMainPanel()?.Dock.Original == GetK
            && TryWindow(GetMyMainPanel()?.MyWindowK ?? -1) is IoWindow w
            && w.WindowState == WindowState.Normal
            ? "🗖" : "⧉";
        dragOut = unmax = false;
        startPointer = null;
    }
    internal void SetTitle() {
        MyTitle = L(GetType().Name) + CustomTitle();
        var t = GetTitle();
            Overlay?.Text.Text = t.Replace("-", "\n");
            if(IsWindow(OriginalMainWindow, out var w))
                w?.Title = t;
        titleText.Text = t;
    }
    protected virtual string CustomTitle()
        => Key == "" ? "" : "-" + Key;
    internal void UpdateMinSize(bool shrink = false, bool total = false) {
        if (GetProfile is not IoProfile profile)
            return;
        if(total && this is IoBookPanel b)
            b.Book.PrepareLayout(Available);
        MinSize = profile.InterfaceMode == IMode.Fullscreen ? App.Available : PrepareLayout(App.Available);
        /*if (profile.DisplayMode != DisplayMode.Fullscreen && GetMyMainPanel()?.Titlebar is Control c) {
            var titlebarSize = MeasureService.Measure(c);
            MinSize = new(Math.Max(MinSize.Width, titlebarSize.Width), MinSize.Height + titlebarSize.Height);
        }*/

        ParentDock?.MinSize = MinSize;
        if (((GetMyMainPanel()?.shrinkRefresh ?? false) || shrink) && GetDisplayMode != IMode.Fullscreen) {
            GetMyMainPanel()?.shrinkRefresh = false;
            ParentDock?.RefreshMinSize(true);
        } else ParentDock?.RefreshMinSize();
    }
    internal override Size PrepareLayout(Size available) 
        => GetMyMainPanel() is IoMainPanel myMain && myMain.Dock.Original != GetK && GetDisplayMode == IMode.MultiWindow
        ? MeasureService.Measure(Titlebar) : new(0, 0);

    internal virtual void Refresh(int job) {
        RefreshTitleBar();
        //UpdateMinSize();
    }
    private void SetTitleBarBackground(){
        var amNotOriginal = GetMyMainPanel() is IoMainPanel myMain && myMain.Dock.Original != GetK;

        Titlebar.Background = GetDisplayMode == IMode.MultiWindow ? amNotOriginal ? new SolidColorBrush(GetTitleColor) : ConstructionHelpers.MakeGradBrush(GetTitleColor) : null;
    }

    internal void RefreshTitleBar() {
        if (ParentDock == null || GetProfile is not IoProfile profile)
            return;
        string sBack = this is MainMenu
        ? "CloseApp"
        : GetDisplayMode == IMode.MultiWindow
            ? "CloseWindow" : "Back";
        //ConstructionHelpers.SetTip(back, "T_" + sBack);
        bool ispinned = GetProfile?.Pan.PinnedPanels.Contains(GetK) ?? false;
        string sPin = ispinned ? "Unpin" : "Pin";
        string sHide = HideNumbers ? "🕶️" : "👁️";
        pin.FontWeight = ispinned ? FontWeight.Bold : FontWeight.Regular;
        //pin.Content = L(sPin);
        pin.Text = "📌";
        hide.Text = sHide;
        pin.IsChecked = ispinned;
        //ConstructionHelpers.SetTip(pin, "T_" + sPin);

        UpdateMax();
        var mw = GetDisplayMode == IMode.MultiWindow;
        SetTitleBarBackground();

        var col = 0;

        //MyGrid.Children.Clear();
        foreach (var child in MyGrid.Children.ToList()) 
            MyGrid.Children.Remove(child);
        if (titleBarContainer.Content == Titlebar)
            titleBarContainer.Content = null;

        Titlebar.Children.Clear();
        Titlebar.ColumnDefinitions.Clear();
        MyGrid.RowDefinitions.Clear();
        if (mw) {
            titleText.Text = UserTitle == "" || !profile.TitleText ? MyTitle : UserTitle;

            back.Text = "X";
            home.Text = "🏠︎";
            pinned.Text = "🕮";

            editTitle.IsVisible = profile.TitleText;

            Titlebar.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
            Titlebar.Children.Add(titleBox); // Title + EditTitle
            Grid.SetColumn(titleBox, col++);

            if (colorPickB != null && profile.TitleUnlock) {
                Titlebar.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Auto));
                Titlebar.Children.Add(colorPickB.Button); // ColorPick
                Grid.SetColumn(colorPickB.Button, col++);
            }
        } else {
            pin.Text = L(sPin);
            back.Text = L(sBack);
            home.Text = L("Home");
            pinned.Text = L("Pinned");
        }
        var gl = mw ? GridLength.Auto : GridLength.Star;
        if (!IsModal() && profile.Pins) {

            Titlebar.ColumnDefinitions.Add(new ColumnDefinition(gl));
            Titlebar.Children.Add(pin); // Pin/Unpin
            Grid.SetColumn(pin, col++);
        }
        var amNotOriginal = GetMyMainPanel() is IoMainPanel myMain && myMain.Dock.Original != GetK;

        /*if ((amNotOriginal && NotLeft) || !mw) {
            square.IsVisible = false;
        } else {
            square.IsVisible = true;
            
        }*/
       

        if (!(IsModal() || amNotOriginal)) {

            if (profile.Pins) {
                // Pinned
                Titlebar.ColumnDefinitions.Add(new ColumnDefinition(gl));
                Titlebar.Children.Add(pinned);
                Grid.SetColumn(pinned, col++);
            }
            if (profile.Homes) {
                // Home button anywhere that isn't already home:
                if (AppMainPanel != GetMyMainPanel()) {
                    Titlebar.ColumnDefinitions.Add(new ColumnDefinition(gl));
                    Titlebar.Children.Add(home);
                    Grid.SetColumn(home, col++);
                }
            }
        }

        Titlebar.ColumnDefinitions.Add(new ColumnDefinition(gl));
        Titlebar.Children.Add(hide);
        Grid.SetColumn(hide, col++);

        if (amNotOriginal && mw) {
            if (Titlebar.Parent is ContentControl c)
                c.Content = null;
            titleBarContainer.Content = Titlebar;

            MyGrid.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
            MyGrid.Children.Add(titleBarContainer);
            Grid.SetRow(MyContent, 1);
        } else {
            Grid.SetRow(MyContent, 0);
            if (mw) {
                // Minimize
                Titlebar.ColumnDefinitions.Add(new ColumnDefinition(gl));
                Titlebar.Children.Add(min);
                Grid.SetColumn(min, col++);
            }
        }
        if (mw) {
            // Undock/UnMaximize/Maximize
            Titlebar.ColumnDefinitions.Add(new ColumnDefinition(gl));
            Titlebar.Children.Add(win);
            Grid.SetColumn(win, col++);
        }
       

        Titlebar.ColumnDefinitions.Add(new ColumnDefinition(gl));
        Titlebar.Children.Add(squareR); // squareR
        Titlebar.Children.Add(back); // Close/Back
        Grid.SetColumn(squareR, col);
        Grid.SetColumn(back, col++);

        if (amNotOriginal && mw && ParentDock.Panel.Count > 1) {

            List<byte?> ToPage = [];
            for (byte i = 0; i < ParentDock.Panel.Count; ++i)
                if (i != ParentDock.Selected)
                    ToPage.Add(i);

            void onToggleBook(object? s, EventArgs e) {
                if (ParentDock == null)
                    return;
                if (s is not IoToggleButton b)
                    return;
                if (b.Tag is byte bit) {
                    ParentDock.Selected = bit;
                    GetMyMainPanel()?.DockHost.Content = GetMyMainPanel()?.Dock.Build();
                    FromK(ParentDock.Panel[ParentDock.Selected])?.RefreshTitleBar();
                    //RefreshPanel(ParentDock.Panel[ParentDock.Selected], true);
                    return;
                }
                if (dCombos.TryGetValue("panelBook", out var d) && d?.Combo is ComboBox c) //if(b.FindAncestorOfType<IoComboBox>() is IoComboBox c)
                    c.IsDropDownOpen ^= true; // clicked the null button: toggle the dropdown
            }

            DockBook = MakeBitmaskBox("panelBook", this, "T_PanelBook", ToPage,
                new FuncDataTemplate<byte?>((bit, _) => GetComboButtons(this, "panelBook", "", bit, ParentDock, onToggleBook)), "flat");
            //panelBook.View.Stretch = Stretch.None;
            //panelBook.Combo.Padding = new Thickness(0);
            //panelBook.Combo.MinWidth = 0;
            //panelBook.Combo.Width = panelBook.Combo.MinWidth;
            Titlebar.ColumnDefinitions.Add(new ColumnDefinition(gl));
            Titlebar.Children.Add(DockBook.View); // Book
            Grid.SetColumn(squareR, col);
            Grid.SetColumn(DockBook.View, col++);
            
        }

        MyGrid.RowDefinitions.Add(new RowDefinition(GridLength.Star));
        MyGrid.Children.Add(MyContent);
        // weird, when i starch searching fonts, it crashes with any of these following, even thought they are all null
        // it's almost like MyContent get corrupted somehow.
        
        MyGrid.Children.Add(TooltipGrid);
        MyGrid.Children.Add(OverlayGrid);
        MyGrid.Children.Add(tooltipHold);
        //MyGrid.Children.Add(debug);
    }
    internal virtual void RefreshData(int job, CancellationToken? cancel) { }
    #endregion
}


/*public class IoPanelSave {
    [Save] // In multiwindow mode, you can write this in the editable title after lciking the Edittitle button
    internal string UserTitle { get; set { 
            AppMainMenu?.UserTitle = value; field = value; Dirty(); } } = "";
    [Save] // In multiwindow mode, you set the titlebar color
    internal Color TitleColor {
        get; set {
            AppMainMenu?.TitleColor = value; field = value; Dirty();
        }
    } = Colors.Black; // default titlebar color (only show in setting if multiwindow)
}*/

/*internal void Copy(IoPanelSave p) {
//Key = p.Key;
notConst = false;
//ChildPanels = p.ChildPanels;
UserTitle = p.UserTitle;
TitleColor = p.TitleColor;
notConst = true;
SetTitle();
}*/


/*public override void Render(DrawingContext context) {
    if (ProBackground != null) 
        context.DrawImage(ProBackground, new Rect(0, 0, ProBackground.PixelSize.Width, ProBackground.PixelSize.Height), new Rect(0, 0, Bounds.Width, Bounds.Height));
    base.Render(context);
}*/



/*internal void ChangeKey(string NewKey) {
        Dirty();
        var K = MakeK(GetType(), Key);
        if (GetProfile?.Pan.LoadedPanels.ContainsKey(K) ?? false)
            GetProfile.Pan.LoadedPanels.Remove(K);
        Key = NewKey;
        GetProfile?.Pan.LoadedPanels[GetK] = this;
        SetTitle();
    }*/