using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using IoListWallet.Core.Static;
using System;
using static IoListWallet.Core.Static.AppSave;

namespace IoListWallet.Core.Dock;

internal class IoDockSplit : IoDock {
    //private PointerPoint? capture;
    private Grid g;
    private double offset = 0;
    private readonly Grid split = new() {
        Background = new SolidColorBrush(Colors.Black),
        HorizontalAlignment = HorizontalAlignment.Stretch,
        VerticalAlignment = VerticalAlignment.Stretch
    };
    Border squareLeft = new() {
        Child = new TextBlock() { Text = ""},
        Background = ConstructionHelpers.MakeLeftShade(),
        VerticalAlignment = VerticalAlignment.Stretch,
        HorizontalAlignment = HorizontalAlignment.Left,
        MinWidth = 20,
        IsHitTestVisible = false
    };
    Border squareRight = new() {
        Child = new TextBlock() { Text = "" },
        Background = ConstructionHelpers.MakeRightShade(),
        VerticalAlignment = VerticalAlignment.Stretch,
        HorizontalAlignment = HorizontalAlignment.Right,
        MinWidth = 20,
        IsHitTestVisible = false
    };
    Border squareBottom = new() {
        Child = new TextBlock() { Text = "            " },
        Background = ConstructionHelpers.MakeBottomShade(),
        VerticalAlignment = VerticalAlignment.Stretch,
        HorizontalAlignment = HorizontalAlignment.Stretch,
        MinHeight = 20,
        IsHitTestVisible = false
    };

    private double splitterSize { get => GetProfile?.DockSplitterWidth ?? DPro.DockSplitterWidth; } // Area between the docks that you could drag, TODO make it a PRO save property
    private Point bounds = new(.5, .5);
    public IoDockSplit() {
        split.Children.Add(squareLeft);
        split.Children.Add(squareRight);
        split.Children.Add(squareBottom);
        Grid.SetRow(squareBottom, 1);
        Grid.SetColumn(squareRight, 2);
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
            split.RowDefinitions.Add(new RowDefinition(GridLength.Star));
            //split.RowDefinitions = [new RowDefinition(GridLength.Auto)];
            split.ColumnDefinitions = [new ColumnDefinition(GridLength.Auto)];
            if ((field = value) == Orientation.Vertical) {
                split.Cursor = new(StandardCursorType.SizeNorthSouth);
                split.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
                split.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Auto));
            } else {
                split.Cursor = new(StandardCursorType.SizeWestEast);
                //split.RowDefinitions.Add(new RowDefinition(GridLength.Star));
                split.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
            }
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
            RefreshMinSize(false);
            return;
        }
        MySize = Available;
        //MySize = new(Math.Max(MinSize.Width, MySize.Width), Math.Max(MinSize.Height, MySize.Height));

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
    protected internal override Control _Build(bool refresh, bool relink, bool left, bool right, bool bottom) {
        Reratio();
        Grid g = (Grid)(MyBuild ??= new Grid());
        var first = First._Build(refresh, relink, left, 
            squareRight.IsVisible = right && Orientation == Orientation.Vertical, 
            squareBottom.IsVisible = bottom && Orientation == Orientation.Horizontal);
        var second = Second._Build(refresh, relink, 
            squareLeft.IsVisible = left && Orientation == Orientation.Vertical, right, bottom);
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