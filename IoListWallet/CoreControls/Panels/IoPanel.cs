using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using IoListWallet.ElementControls;
using IoListWallet.IoPanels.Main;
using System;
using System.Collections.Generic;
using System.Threading;
using static IoListWallet.App;
using static IoListWallet.AppSave;

namespace IoListWallet.MainControls.Panels;
internal class IoPanel : IoUserControl {

    // Serialized:
    internal static void Dirty() => GetProfile?.Pan.Dirty = true;
    [Save]
    internal string Key { get; set { Dirty(); field = value; } }
    [Save]
    internal List<string> ChildPanels { get; set { Dirty(); field = value; } } = [];
    [Save]
    internal string? ParentPanel { get; set; }
    internal static string MakeK(Type type, string key) => type.Name + ":" + key;
    internal static (Type Type, string Key)? SplitK(string key) {
        var i = key.IndexOf(':');
        return i >= 0 && PTypes.TryGetValue(key[..i], out var type) ? (type, key[(i + 1)..]) : null;
    }
    internal string GetK => MakeK(GetType(), Key);
    internal double ratio { get; private set; } = 1;
    internal bool IsAsync() => false;
    internal virtual bool IsModal() => false;
    internal virtual bool RefreshesWhenOpened() => false;
    internal virtual IoPanel? TrueParent => GetPanel(TrueParentType);
    internal virtual string TrueParentType => MakeK(typeof(ListMainMenu), "");
    public IoDockPanel? ParentDock { get; protected set; }

    private bool canAuto = true;
    /*internal IoPanel(string key = "") {
        key = Key;
        Init();
    }*/
    internal IoPanel() : base() {
        Key = "";
        Init();
    }

    internal virtual void Snapshot() { }
    //internal virtual void Restore() { }

    private DisplayMode GetDisplayMode => (GetProfile?.DisplayMode ?? DisplayMode.Fullscreen);
    protected virtual void Init() { IsVisible = false; ChildPanels = []; }
    // A parent panel opens this as a new child panel
    internal virtual void Open(IoPanel Parent) {
        if (!IsVisible) {
            if (GetDisplayMode == DisplayMode.MultiWindow) {
                // open a new window to dock into:
                IoWindow NewWindow = new() {
                    WindowStartupLocation = WindowStartupLocation.CenterOwner
                };
                ParentDock = (IoDockPanel)NewWindow.Dock.Dock.TopNonRoot;
                IoDock? dock = Parent?.ParentDock;
                while (dock?.Parent != null)
                    dock = dock.Parent;
                if (dock is IoDockRoot Root 
                    && (GetProfile?.Pan.OpenedWindows.TryGetValue(Root.Panel.MyWindowK, out var w) ?? false) 
                    && w is IoWindow window 
                    && window.IsVisible
                ) {
                    NewWindow.Show(window); 
                    NewWindow.OpenedFrom = window.Index;
                } else NewWindow.Show();
            } else {
                // replace myself into parent's dock:
                ParentDock = Parent.ParentDock ?? AppMainMenu?.ParentDock ?? ParentDock;
                Parent.IsVisible = false;
                Parent.Undock();
            }
            ParentDock?.Panel = GetK; // dock myself in
            Dirty();
        }
        OnOpen(); // IsVisible = true; +focus on my window
        // Only set parent if it doesn't cause a loop
        SetTitle();
        var P = Parent;
        while (P != null) {
            if (P == this) return;
            P = FromK(P.ParentPanel);
        }
        SetParent(Parent);
    }
    // call this to close the panel
    internal virtual void Close() {
        if (!IsVisible)
            return;
        if (FromK(ParentPanel) is not IoPanel p) {
            // closing mainmenu panel
            AppClose();
            return;
        }
        var Dock = ParentDock;
        Undock();
        if (GetDisplayMode != DisplayMode.MultiWindow 
            && FromK((p.ParentDock = Dock)?.Panel = ParentPanel) is IoPanel parent 
            && parent.OnOpen().RefreshesWhenOpened()
        ) RefreshPanel(parent); // redock parent into the dock I've undocked myself from
        OnClose();
        Dirty();
    }
    internal virtual void Delete() {
        GetProfile?.Pan.PinnedPanels.Remove(GetK);
        GetProfile?.Pan.LoadedPanels.Remove(GetK);
        var Pinned = GetPanel((typeof(ListPinned), ""));
        if (Pinned.IsVisible) // only if visible, because PInned is refreshing itself upon opening
            RefreshPanel(Pinned); // Refresh Pinned panel
        Close();
    }
    internal virtual void AsyncTask(ushort Job, CancellationToken? cancel, TaskBin? task) {
        switch (Job) {
            case 1:
                // run async Refresh, with cancel token so we coudl abort it if we need to refresh again before this finishes
                if(!RefreshesWhenOpened() || IsVisible) 
                    RefreshData(cancel);
                break;
        }
        task?.State = TaskState.Done; // mark it as finished
    }
    internal virtual void FinishAsyncTask(ushort Job) {
        switch (Job) {
            case 1:
                // run async Refresh, with cancel token so we coudl abort it if we need to refresh again before this finishes
                if (!RefreshesWhenOpened() || IsVisible)
                    Refreshes();
                break;
        }
    }
    internal void Undock() {
        if (Parent is Panel p)
            p.Children.Remove(this);

        if (Parent is ContentControl cc)
            cc.Content = null;

        IsVisible = false; // hide itself

        if (ParentDock == null)
            return;
        var DockParent = ParentDock.Parent;
        // If it is the only panel docked in this window in MultiWindow mode - close the window
        if (DockParent is IoDockRoot Root)
            if (( GetProfile?.Pan.OpenedWindows.TryGetValue(Root.Panel.MyWindowK, out var w) ?? false) 
                && w is IoWindow window 
                && GetDisplayMode == DisplayMode.MultiWindow 
                && !window.IsClosing
            ) window.Close();
        if (DockParent is IoDockSplit Split) {
            // Undock from a split Dock - will turn the split into a single dock containing the remaining other panel
            var SplitParent = Split.Parent;
            var Remaining = Split.First == ParentDock ? Split.Second : Split.First;
            Remaining.Parent = SplitParent;
            if (SplitParent is IoDockRoot root) {
                // parent of my split is already the root, so the remaining other half will become the new first dock under the root
                root.TopNonRoot = Remaining;
                // If only the last IoPanel remains in the window - make it est the window's title
                if (Remaining is IoDockPanel RemainingLast)
                    FromK(RemainingLast.Panel)?.SetTitle();
            }
            if (SplitParent is IoDockSplit split) {
                // the parent above my split is another split, so place the remaining other half into my previous spot on that parent split
                if (split.First == Split)
                    split.First = Remaining;
                else split.Second = Remaining;
            }
        }
        ParentDock = null;
        Dirty();
    }
    internal void SetTitle() {
        if (ParentDock?.Parent is IoDockRoot RootDock) {
            var t = L(GetType().Name) + "\n" + CustomTitle();
            RootDock.Panel.Overlay?.Text.Text = t;
            if(GetProfile?.Pan.OpenedWindows.TryGetValue(RootDock.Panel.MyWindowK, out var w) ?? false) 
                w?.Title = t;
        }
    }
    protected virtual string CustomTitle() => Key == "" ? "" : " - " + Key;
    internal void WindowClosed() {
        Undock();
        OnClose();
    }
    internal void ChangeKey(string NewKey) {
        Dirty();
        var K = MakeK(GetType(), Key);
        if (GetProfile?.Pan.LoadedPanels.ContainsKey(K) ?? false)
            GetProfile.Pan.LoadedPanels.Remove(K);
        Key = NewKey;
        GetProfile?.Pan.LoadedPanels[GetK] = this;
        SetTitle();
    }
    internal bool Cancel(CancellationToken? cancel) => cancel != null && cancel.Value.IsCancellationRequested;
    // a child modal panel can close and give this signal to the parent, returns true if consumed
    internal virtual bool OnResponse(IoPanel From, object Data) => false;
    internal void Refreshes() {
        Refresh();
        UpdateWindow();
    }

    internal void UpdateWindow() {
        if (!(IsVisible && (GetProfile?.AutomaticWindowSize ?? false)))
            return;
        if (canAuto 
            && (GetProfile?.Pan.OpenedWindows.TryGetValue(GetMyMainPanel()?.MyWindowK ?? -1, out var window) ?? false)
            && window is IoWindow w
            && GetDisplayMode != DisplayMode.Fullscreen 
            && ((w.Width == w.MinWidth && w.Height == w.MinHeight)
                || (w.MinWidth == 120 && w.MinHeight == 120))) {
            canAuto = false;
            PrepareLayout(w);
            w.SizeToContent = SizeToContent.WidthAndHeight;// = SetWindowSize();
            w.Width = w.Height = w.MinWidth = w.MinHeight = 120;
            w.LayoutUpdated += OnLayout;
        }
    }

    protected virtual void PrepareLayout(IoWindow w) { }
    protected virtual void AfterLayout(IoWindow w) { }
    private void OnLayout(object? s, EventArgs e) {
        if (s is not IoWindow w || double.IsNaN(w.Width) || double.IsNaN(w.Height))
            return;
        w.LayoutUpdated -= OnLayout;
        ratio = (double)(w.MinWidth = w.Width + 20) / (w.MinHeight = w.Height + 20);
        w.SizeToContent = SizeToContent.Manual;
        AfterLayout(w);
        canAuto = true;
    }
    
    internal virtual void Refresh() { }
    internal virtual void RefreshData(CancellationToken? cancel) { }
    
    protected IoMainPanel? GetMyMainPanel() {
        IoDock? RootDock = ParentDock ?? null;
        while (RootDock?.Parent != null)
            RootDock = RootDock.Parent;
        return ((IoDockRoot?)RootDock)?.Panel;
    }
    protected virtual IoPanel OnOpen() {
        canAuto = IsVisible = true;
        if (ParentDock == null)
            return this;
        // Find the Root Dock and Window:
        var MainPanel = GetMyMainPanel();
        if (MainPanel?.Dock == ParentDock.Parent) {
            // We are the only panel in the dock
            //if (AppDisplayMode != DisplayMode.Fullscreen)
            //    SetWindowSize();
        }
        // Update Bottom toolbar to reflect the state of the new content:
        MainPanel?.Update();
        if ((GetProfile?.Pan.OpenedWindows.TryGetValue(MainPanel?.MyWindowK ?? -1, out var w) ?? false) && w is IoWindow OpenWindow) {
            OpenWindow.Activate();
            OpenWindow.IsVisible = true;
            OpenWindow.Focus();
            if (IsModal() && Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
                foreach (var eachwindow in desktop.Windows)
                    if (eachwindow != OpenWindow)
                        eachwindow.IsEnabled = false;
        }
        return this;
    }
    // Should also be called when ParentDock's Root IoWindow gets closed:
    protected virtual void OnClose() {
        // unfreezes everything if i was modal
        if (IsModal() && Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            foreach (var w in desktop.Windows)
                w.IsEnabled = true;
    }
    // Should only be called when we actually want to lose the panel, like closing a transaction viewer for good, reopening will be reseted
    protected virtual void OnDelete() { }
    protected void SetParent(IoPanel? parent) {
        if (parent?.GetK is not string k || ParentPanel == k)
            return;
        FromK(ParentPanel)?.ChildPanels.Remove(GetK);
        ParentPanel = k;
        parent?.ChildPanels.Add(GetK);
    }
}

/*protected virtual Size SetWindowSize() {
        if(GetMyMainPanel() is not IoMainPanel MainPanel || MainPanel.Dock != ParentDock?.Parent)
            return new Size(0,0);
        double minW = 120, maxW = 4000, minH = 120, maxH = 4000;
        Size best = new(maxW, maxH);
        double test;
        for (int i = 0; i < 20; i++) {
            var size = new Size(test = (minW + maxW) / 2, best.Height);
            Measure(size);
            Arrange(new Rect(size));
            if (HasOverflow(this))
                minW = test;
            else 
                best = new(maxW = test, best.Height);
        }
        for (int i = 0; i < 20; i++) {
            var size = new Size(best.Width, test = (minH + maxH) / 2);
            Measure(size);
            Arrange(new Rect(size));
            if (HasOverflow(this))
                minH = test;
            else
                best = new(best.Width, maxH = test);
        }
        return new Size(best.Width + 16, best.Height + 16);
    }
    protected bool HasOverflow(Control? c) {
        if (c == null)
            return false;
        if (c.Bounds.Width < c.DesiredSize.Width
            || c.Bounds.Height < c.DesiredSize.Height)
            return true;
        if (c is WrapPanel w) {
            var available = w.Bounds.Size;
            // Measure as single line (no wrapping)
            w.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            var singleLine = w.DesiredSize;
            // Measure with current constraint
            w.Measure(available);
            var wrapped = w.DesiredSize;
            // If height increased - wrapping happened, if desired > available - leaking happened
            if (wrapped.Height > singleLine.Height 
                || w.DesiredSize.Height > available.Height
                || w.DesiredSize.Width > available.Width)
                return true;
        }
        if (c is ScrollViewer s) 
            return s.Extent.Width > s.Viewport.Width
                || s.Extent.Height > s.Viewport.Height
                || HasOverflow(s.Content as Control);
        if (c is Grid g) {
            foreach (var child in g.Children) {
                if (child.Bounds.Right > g.Bounds.Width 
                    || child.Bounds.Bottom > g.Bounds.Height
                    || HasOverflow(child))
                    return true;
            }
            return false;
        }
        if (c is ContentControl u) 
            return HasOverflow((Control?)u.Content);
        return false;
    }*/