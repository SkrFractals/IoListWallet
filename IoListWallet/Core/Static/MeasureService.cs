using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using System;

namespace IoListWallet.Core.Static;

internal static class MeasureService {
    private static ContentControl host = new() { IsVisible = false };
    private static TopLevel? mainTop = null;
    public static Size Measure(Control? control) {
        if (control == null)
            return new(0, 0);
        // AppSave.AppMainPanel is the first content ever created (hosting main menu panel), and a permanent one (closing it closes the app), so let's use that:
        var top = AppSave.AppFakePanel == null ? TopLevel.GetTopLevel(AppSave.AppMainPanel) : TopLevel.GetTopLevel(AppSave.AppFakePanel)
            ?? throw new("No TopLevel available.");
        try {
            if (top != mainTop && mainTop != null)
                OverlayLayer.GetOverlayLayer(mainTop)?.Children.Remove(host);
            mainTop = top;
            if (mainTop == null)
                return new(120, 120);

            var overlay = OverlayLayer.GetOverlayLayer(mainTop)
                        ?? throw new("No Overlay available");
            if (!overlay.Children.Contains(host)) {
                //if (host.Parent is OverlayLayer o)
                //    o.Children.Remove(host);
                overlay.Children.Add(host);
            }
            host.Content = control;
            //control.Arrange(new(new(0, 0), new Size(120, 120)));
            control.Measure(Size.Infinity);

            host.Content = null;

            return control.DesiredSize;
        } catch (Exception) { 
            return new Size(120, 120); 
        }
    }
}