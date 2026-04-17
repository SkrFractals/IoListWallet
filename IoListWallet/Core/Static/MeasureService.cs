using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;

namespace IoListWallet.Core.Static;

internal static class MeasureService {
    static ContentControl host = new() { IsVisible = false };
    public static Size Measure(Control control) {
        // AppSave.AppMainPanel is the first content ever created (hosting main menu panel), and a permanent one (closing it closes the app), so let's use that:
        var top = TopLevel.GetTopLevel(AppSave.AppMainPanel) 
            ?? throw new("No TopLevel available.");
        var overlay = OverlayLayer.GetOverlayLayer(top) 
            ?? throw new("No Overlay available");



        if (!overlay.Children.Contains(host)) {
            if (host.Parent is OverlayLayer o)
                o.Children.Remove(host);
            overlay.Children.Add(host);
        }
        host.Content = control;
        //control.Arrange(new(new(0, 0), new Size(120, 120)));
        control.Measure(Size.Infinity);
        
        host.Content = null;
        return control.DesiredSize;
    }
}