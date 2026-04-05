using Avalonia.Controls;
using IoListWallet.MainControls.Panels;
using static IoListWallet.App;
using static IoListWallet.AppSave;

namespace IoListWallet.MainControls;
internal class IoWindow : Window {
    [Save]
    internal IoMainPanel Dock { get; set { field = value; IoPanel.Dirty(); } }
    [Save]
    internal int Index { get; set { field = value; IoPanel.Dirty(); } } = 0;
    [Save] internal int OpenedFrom { get; set { field = value; IoPanel.Dirty(); } } = -1;

    internal bool IsClosing = false;

    internal IoWindow() {
        MinWidth = MinHeight = 120;
        for (Index = 0; GetProfile?.Pan.OpenedWindows.ContainsKey(Index) ?? false; ++Index) ;
        GetProfile?.Pan.OpenedWindows[Index] = this;
        Dock = new() { MyWindowK = Index };
        Content = Dock;
        Closing += IoWindow_Closing;
    }
    private void IoWindow_Closing(object? sender, WindowClosingEventArgs e) {
        if (IsClosing)
            return;
        GetProfile?.Pan.OpenedWindows.Remove(Index);
        bool MainClose = Dock.Dock == AppDockRoot;
        IsClosing = true;
        CloseDock(Dock.Dock);
        if (MainClose)
            AppClose();
    }
    private static void CloseDock(IoDock dock) {
        if (dock is IoDockPanel panelDock)
            FromK(panelDock.Panel)?.WindowClosed();
        else if (dock is IoDockRoot root)
            CloseDock(root.TopNonRoot);
        else if (dock is IoDockSplit split) {
            CloseDock(split.First);
            CloseDock(split.Second);
        }
    }
}