using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using IoListWallet.Core.Dock;
using System;
using System.Collections.Generic;
using static IoListWallet.App;
using static IoListWallet.Core.Static.AppSave;

namespace IoListWallet.Core;

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
    protected internal override Control _Build(bool refresh, bool relink, bool left, bool right, bool bottom) {
        if (Panel.Count > Selected && FromK(Panel[Selected]) is IoPanel c) {
            if (relink) 
                c.ParentDock = this;
            if (refresh)
                c.RefreshTitleBar();
            var amoriginal = (c.GetMyMainPanel()?.Dock.Original ?? "") == c.GetK;
            c.square.IsVisible = left || amoriginal;
            c.squareR.IsVisible = GetDisplayMode == IMode.MultiWindow && (right || amoriginal);
            return MyBuild = c;
        }
        return MyBuild = new Panel();
    }
    internal override void UnBuild(bool recursive = true) => MyBuild = null;
    internal override protected void PropagateColor(Color c) { }
}