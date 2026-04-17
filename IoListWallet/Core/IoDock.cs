using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using IoListWallet.Core.Panels;
using System;
using System.Collections.Generic;
using static IoListWallet.App;
using static IoListWallet.Core.Static.AppSave;

namespace IoListWallet.Core;

internal abstract class IoDock {
    internal Control? MyBuild = null;
    internal IoDock? Parent = null; // Loading should fill it it as it goes
    internal Size MinSize = new(120, 120), MySize = new(120, 120);
    internal abstract void RefreshMinSize(bool shrink = false); // called from below to bubble up into the root (TODO When a panel changes min size, it should call this on it's dock)
    internal abstract void SanitizeRatios(Size Available); // adjust any ratios that are out of bounds (window should call this on resize)
    internal abstract Control Build(bool refresh = false, bool relink = false); // TODO when undock removes anything, it should release from it's grid's Children! Also SetRow and column to 0,0!
    internal abstract void UnBuild(bool recursive = true);
    internal abstract protected void PropagateColor(Color c);
}
internal class IoDockPanels : IoDock {
    [Save] internal List<string> Panel { get; set { field = value; IoPanel.Dirty(); } } = []; // Panel KeyId
    [Save] internal int Selected { get; set { field = value; IoPanel.Dirty(); } } = 0; // Which one is selected
    internal override void RefreshMinSize(bool shrink = false) {
        MinSize = new(120, 120);
        foreach (var p in Panel)
            if (FromK(p) is IoPanel ip)
                MinSize = new(Math.Max(MinSize.Width, ip.MinSize.Width), Math.Max(MinSize.Height, ip.MinSize.Height));
        Parent?.RefreshMinSize(shrink);
    }
    internal override void SanitizeRatios(Size Available) => MySize = Available;
    internal override Control Build(bool refresh = false, bool relink = false) {
        if (Panel.Count > Selected && FromK(Panel[Selected]) is IoPanel c) {
            if (relink)
                c.ParentDock = this;    
            if (refresh)
                c.RefreshTitleBar();
            return MyBuild = c;
        }
        return MyBuild = new Panel();
    }
    internal override void UnBuild(bool recursive = true) => MyBuild = null;
    internal override protected void PropagateColor(Color c) { }
}
internal class IoDockSplit : IoDock {
    //private PointerPoint? capture;
    private Grid g;
    private double offset = 0;
    private readonly Panel split = new() {
        Background = new SolidColorBrush(Colors.Black),
        HorizontalAlignment = HorizontalAlignment.Stretch,
        VerticalAlignment = VerticalAlignment.Stretch
    };
    private double splitterSize { get => GetProfile?.DockSplitterWidth ?? DPro.DockSplitterWidth; } // Area between the docks that you could drag, TODO make it a PRO save property
    private Point bounds = new(.5, .5);
    public IoDockSplit() {
        split.PointerPressed += OnSplitterPressed;
        split.PointerMoved += OnSplitterMoved;
        split.PointerReleased += OnSplitterReleased;
        //Splitter.PointerCaptureLost += OnSplitterCaptureLost;
        MyBuild = g = new Grid();
        g.RowDefinitions.Add(new RowDefinition(1, GridUnitType.Star));
        g.ColumnDefinitions.Add(new ColumnDefinition(1, GridUnitType.Star));
    }
    [Save] // Only ever call this once!
    internal Orientation Orientation {
        get; set {
            split.Cursor = new((field = value) == Orientation.Vertical
                ? StandardCursorType.SizeNorthSouth
                : StandardCursorType.SizeWestEast);
            Reratio();
            IoPanel.Dirty();
        }
    } = Orientation.Vertical;
    [Save] internal IoDock First { get; set { field = value; IoPanel.Dirty(); } } = new IoDockPanels();
    [Save] internal IoDock Second { get; set { field = value; IoPanel.Dirty(); } } = new IoDockPanels();
    [Save] internal double SplitRatio { get; set { field = value; IoPanel.Dirty(); } } = .5;
    internal override void RefreshMinSize(bool shrink = false) {
        MinSize = Orientation == Orientation.Vertical
            ? new(12+Math.Max(First.MinSize.Width, Second.MinSize.Width),
                12+First.MinSize.Height + splitterSize + Second.MinSize.Height)
            : new(12+First.MinSize.Width + splitterSize + Second.MinSize.Width,
                12+Math.Max(First.MinSize.Height, Second.MinSize.Height));
        Parent?.RefreshMinSize(shrink);
    }
    internal override void SanitizeRatios(Size Available) {
        (double F, double S, double A) = Orientation == Orientation.Vertical
            ? (First.MinSize.Height, Second.MinSize.Height, Available.Height)
            : (First.MinSize.Width, Second.MinSize.Width, Available.Width);
        var halfSlider = splitterSize / 2;
        bounds = new((F + halfSlider) / A, 1 - (S + halfSlider) / A);

        if (bounds.X > bounds.Y) {
            /*MinSize = MySize = Orientation == Orientation.Vertical
                ? new(12+Math.Max(First.MinSize.Width,Second.MinSize.Width),12+First.MinSize.Height+Second.MinSize.Height+ splitterSize)
                : new(12+First.MinSize.Width + Second.MinSize.Width + splitterSize, 12+Math.Max(First.MinSize.Height, Second.MinSize.Height));*/
            RefreshMinSize(false);

            return;
        }

        SetRatio(SplitRatio);
        if (Orientation == Orientation.Vertical) {
            First.SanitizeRatios(new(Available.Width, Available.Height * SplitRatio - halfSlider));
            Second.SanitizeRatios(new(Available.Width, Available.Height * (1 - SplitRatio) - halfSlider));
        } else {
            First.SanitizeRatios(new(Available.Width * SplitRatio - halfSlider, Available.Height));
            Second.SanitizeRatios(new(Available.Width * (1 - SplitRatio) - halfSlider, Available.Height));
        }
    }
    internal void SetRatio(double To) { // User, or forced change in ratio
        SplitRatio = Math.Clamp(To, bounds.X, bounds.Y);
        Reratio();
    }

    private void SetRowH(Grid g, GridLength[] h) {
        var gr = g.RowDefinitions;
        for (int i = 0; i < h.Length; ++i) {
            if (gr.Count > i)
                gr[i].Height = h[i];
            else gr.Add(new RowDefinition(h[i]));
        }
        while (gr.Count > h.Length)
            gr.RemoveAt(gr.Count - 1);
    }

    private void SetColW(Grid g, GridLength[] w) {
        var gc = g.ColumnDefinitions;
        for (int i = 0; i < w.Length; ++i) {
            if (gc.Count > i)
                gc[i].Width = w[i];
            else gc.Add(new ColumnDefinition(w[i]));
        }
        while (gc.Count > w.Length)
            gc.RemoveAt(gc.Count - 1);
    }


    private void Reratio() {
        Grid g = (Grid)(MyBuild ??= new Grid());
        var halfSlider = splitterSize / 2;
        var s = Orientation == Orientation.Vertical ? MySize.Height : MySize.Width;
        GridLength[] n = [new GridLength(s * SplitRatio - halfSlider, GridUnitType.Star),
                new GridLength(splitterSize),
                new GridLength(s * (1 - SplitRatio) - halfSlider, GridUnitType.Star)];
        if (Orientation == Orientation.Vertical)
            SetRowH(g, n);
        else
            SetColW(g, n);
    }
    private void LinkGrid(Grid g, Control first, Control second) {
        if (g.Children.Count == 0) {
            g.Children.Add(first);
        } else g.Children[0] = first;
        if (g.Children.Count == 1) {
            g.Children.Add(split);
        } else g.Children[1] = split;
        if (g.Children.Count == 2) {
            g.Children.Add(second);
        } else g.Children[2] = second;
    }
    internal override Control Build(bool refresh = false, bool relink = false) {
        Reratio();
        Grid g = (Grid)(MyBuild ??= new Grid());
        var first = First.Build(refresh, relink);
        var second = Second.Build(refresh, relink);
        LinkGrid(g, first, second);
        Grid.SetRow(first, 0);
        Grid.SetColumn(first, 0);
        if (Orientation == Orientation.Vertical) {
            Grid.SetColumn(second, 0);
            Grid.SetRow(second, 2);
            Grid.SetRow(split, 1);
            return g;
        }
        Grid.SetRow(second, 0);
        Grid.SetColumn(second, 2);
        Grid.SetColumn(split, 1);
        return g;
    }
    internal override void UnBuild(bool recursive = true) {
        Grid g = (Grid)(MyBuild ??= new Grid());
        if (recursive) {
            First.UnBuild();
            Second.UnBuild();
        }
        g.Children.Clear();
    }
    private void OnSplitterPressed(object? sender, PointerPressedEventArgs e) {
        if (sender is Control c) {
            e.Pointer.Capture(c); //capture = e.GetCurrentPoint(c);
            var p = e.GetPosition(MyBuild);
            // remember offset of press from the splitter center
            offset = Orientation == Orientation.Vertical
                ? p.Y - SplitRatio * g.Bounds.Height
                : p.X - SplitRatio * g.Bounds.Width;
        }
    }
    private void OnSplitterMoved(object? sender, PointerEventArgs e) {
        if (MyBuild is Grid g && e.Pointer.Captured == sender) {
            var p = e.GetPosition(MyBuild);
            SetRatio(Orientation == Orientation.Vertical
                ? (p.Y - offset) / g.Bounds.Height
                : (p.X - offset) / g.Bounds.Width);
        }
    }
    private void OnSplitterReleased(object? sender, PointerReleasedEventArgs e)
        => e.Pointer.Capture(null);
    //private void OnSplitterCaptureLost(object? sender, PointerCaptureLostEventArgs e) => capture = null;
    internal override protected void PropagateColor(Color c) {
        split.Background = new SolidColorBrush(c);
        First.PropagateColor(c);
        Second.PropagateColor(c);
    }
}
internal class IoDockRoot : IoDock {
    internal IoMainPanel? Panel = null; // Main Panel hosting all the IoPanels
    [Save] internal IoDock TopNonRoot { get; set { field = value; IoPanel.Dirty(); } } = new IoDockPanels(); // Root node holding the first IoPanel, or a split
    [Save] internal string Original { get; set { field = value; IoPanel.Dirty(); } } = ""; // Original single panel that this dock was spawned with
    internal IoDockRoot SetupParents() => (IoDockRoot)(_SetupParents(TopNonRoot).Parent = this);
    private IoDock _SetupParents(IoDock d) {
        if (d is IoDockSplit s) {
            _SetupParents(s.First).Parent = s;
            _SetupParents(s.Second).Parent = s;
        }
        return d;
    }
    internal override void RefreshMinSize(bool shrink = false) {
        MinSize = TopNonRoot is IoDockSplit  // = Child_TopNonRoot( + WindowToolbar( = controlfont + padding) if docked)
            ? new(TopNonRoot.MinSize.Width, TopNonRoot.MinSize.Height + (GetProfile?.ControlFontSize ?? DPro.ControlFontSize) + 8)
            : TopNonRoot.MinSize;
        if (TryWindow(Panel?.MyWindowK ?? -1) is IoWindow window) { // Upsize my window if overflown
            window.SetMinSize(MinSize + new Size(24,24));
            if (shrink) {
                window.SetSize(MinSize);
                SanitizeRatios(MinSize);
                return;
            }
            // ws = window.GetSize();
            SanitizeRatios(new(Math.Max(MinSize.Width, window.Width), Math.Max(MinSize.Height, window.Height)));
        }
    }
    internal override void SanitizeRatios(Size Available)
        => TopNonRoot.SanitizeRatios(Available);
    internal override Control Build(bool refresh = false, bool relink = false) {
        SetColor();
        return MyBuild = TopNonRoot.Build(refresh, relink);
    }
    internal override void UnBuild(bool recursive = true) {
        MyBuild = null;
        if (recursive) TopNonRoot.UnBuild(recursive);
    }
    internal void SetColor() {
        Color c = FromK(Original)?.TitleColor ?? DPro.TitleColor;
        TopNonRoot.PropagateColor(c);
    }
    protected internal override void PropagateColor(Color c) { }
}