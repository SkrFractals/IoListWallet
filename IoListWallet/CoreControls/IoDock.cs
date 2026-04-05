using Avalonia.Controls;
using Avalonia.Layout;
using IoListWallet.MainControls.Panels;
using static IoListWallet.App;
using static IoListWallet.AppSave;

namespace IoListWallet.MainControls;
internal abstract class IoDock {
    internal IoDock? Parent; // Loading should fill it it as it goes
}
internal class IoDockPanel : IoDock {
    [Save] internal string? Panel { get; set { field = value; IoPanel.Dirty(); } } = null; // Panel KeyId
}
internal class IoDockSplit : IoDock {
    [Save] internal Orientation Orientation { get; set { field = value; IoPanel.Dirty(); } } = Orientation.Vertical;
    [Save] internal IoDock First { get; set { field = value; IoPanel.Dirty(); } } = new IoDockPanel();
    [Save] internal IoDock Second { get; set { field = value; IoPanel.Dirty(); } } = new IoDockPanel();
    [Save] internal double SplitRatio { get; set { field = value; IoPanel.Dirty(); } } = 0.5;
}
internal class IoDockRoot : IoDock {
    internal IoMainPanel Panel; // Main Panel hosting all the IoPanels
    [Save] internal IoDock TopNonRoot { get; set { field = value; IoPanel.Dirty(); } } = new IoDockPanel(); // Root node holding the first IoPanel, or a split
    internal IoDockRoot(IoMainPanel Panel) {
        this.Panel = Panel;
        TopNonRoot.Parent = this;
        Parent = null;
    }
}
internal static class DockRenderer {
    internal static Control Build(IoDock dock) {
        if (dock is IoDockPanel panelDock) 
            return (Control?)FromK(panelDock.Panel) ?? new Panel();
        
        if (dock is IoDockSplit split) {
            var grid = new Grid();

            if (split.Orientation == Orientation.Horizontal) {
                grid.ColumnDefinitions.Add(new ColumnDefinition(split.SplitRatio, GridUnitType.Star));
                grid.ColumnDefinitions.Add(new ColumnDefinition(1, GridUnitType.Auto));
                grid.ColumnDefinitions.Add(new ColumnDefinition(1 - split.SplitRatio, GridUnitType.Star));

                var first = Build(split.First);
                var second = Build(split.Second);

                Grid.SetColumn(first, 0);
                Grid.SetColumn(second, 2);

                grid.Children.Add(first);
                grid.Children.Add(new GridSplitter { Width = 4 });
                grid.Children.Add(second);
            } else {
                grid.RowDefinitions.Add(new RowDefinition(split.SplitRatio, GridUnitType.Star));
                grid.RowDefinitions.Add(new RowDefinition(1, GridUnitType.Auto));
                grid.RowDefinitions.Add(new RowDefinition(1 - split.SplitRatio, GridUnitType.Star));

                var first = Build(split.First);
                var second = Build(split.Second);

                Grid.SetRow(first, 0);
                Grid.SetRow(second, 2);

                grid.Children.Add(first);
                grid.Children.Add(new GridSplitter { Height = 4 });
                grid.Children.Add(second);
            }

            return grid;
        }

        if (dock is IoDockRoot root) {
            return Build(root.TopNonRoot);
        }

        return new Panel();
    }
}