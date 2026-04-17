using Avalonia.Controls;
using Avalonia.Media;
using IoListWallet.Core.Panels;
using IoListWallet.Core.Static;
using static IoListWallet.App;

namespace IoListWallet.Core;
internal class IoMainPanel : IoUserControl {
    [AppSave.Save]
    internal IoDockRoot Dock { get; set { 
            (field = value).SetupParents().Panel = this; // relink pointers
            //value.SanitizeRatios(new(120, 120)); // no width and height exist yet when assigning this
            //value.Build();
            IoPanel.Dirty(); } }
    [AppSave.Save]
    internal int MyWindowK { get; set { field = value; IoPanel.Dirty(); } } = -1; // It can lack window, if app is in fullscreen mode

    
    internal ContentControl DockHost { get; private set; }
    internal ContentControl Titlebar { get; private set; }
    internal Grid OverlayGrid { get; private set; }
    internal (TextBlock Text, Viewbox View)? OverlayX { get; set; }
    private Grid grid;
    internal bool shrinkRefresh = false;
    public IoMainPanel() {
        Dock = new();
        grid ??= new Grid() {
            ColumnDefinitions = [new ColumnDefinition(GridLength.Star)],
            RowDefinitions = [new RowDefinition(GridLength.Auto), new RowDefinition(GridLength.Star)]
        };
        DockHost = new();
        Titlebar = new();

        OverlayGrid = new();
        OverlayGrid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
        OverlayGrid.RowDefinitions.Add(new RowDefinition(GridLength.Star));
        OverlayX = MakeTextBlock("overlayX", "", "·") ?? throw new("overlayX");
        OverlayGrid.Children.Add(OverlayX.Value.View);
        OverlayGrid.Background = new SolidColorBrush(Colors.Black, OverlayX.Value.Text.Opacity = 1);
        OverlayX.Value.Text.IsHitTestVisible = OverlayX.Value.View.IsHitTestVisible
            = OverlayGrid.IsHitTestVisible = false;
        OverlayGrid.IsVisible = false;
        Grid.SetRowSpan(OverlayGrid, 2);

        grid.Children.Add(DockHost);
        grid.Children.Add(Titlebar);
        grid.Children.Add(OverlayGrid);

        Content = new RotationHost { Child = grid };
    }
    internal void RefreshDock() => DockHost.Content = Dock.Build();
    
    internal bool Update(bool total = false) {
        var mw = AppSave.GetDisplayMode == AppSave.DisplayMode.MultiWindow;

        Grid.SetRow(DockHost, mw ? 1 : 0);
        Grid.SetRow(Titlebar, mw ? 0 : 1);

        grid.RowDefinitions[0].Height = mw ? GridLength.Auto : GridLength.Star;
        grid.RowDefinitions[1].Height = mw ? GridLength.Star : GridLength.Auto;

        var myPanel = FromK(mw ? Dock.Original : ((IoDockPanels)Dock.TopNonRoot).Panel[0]);
        if (myPanel == null)
            return false;

        if (myPanel.Titlebar.Parent is ContentControl c)
            c.Content = null;
        Titlebar.Content = myPanel.Titlebar;
        DockHost.Content = null;
        Dock.Build();
        DockHost.Content = Dock.MyBuild;
        return true;
    }
}