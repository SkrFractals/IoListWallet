using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using IoListWallet.Core.Dock;
using IoListWallet.Core.Panels;
using System;
using static IoListWallet.App;
using static IoListWallet.Core.Static.AppSave;

namespace IoListWallet.Core;
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

        var titleSize = Static.MeasureService.Measure(FromK(Original)?.Titlebar);
        MinSize = new(Math.Max(titleSize.Width, MinSize.Width), titleSize.Height + MinSize.Height);

        if (TryWindow(Panel?.MyWindowK ?? -1) is IoWindow window) { // Upsize my window if overflown
            window.SetMinSize(MinSize);
            if (shrink) {
                window.SetSize(MinSize);
                SanitizeRatios(MinSize);
                return;
            }
            SanitizeRatios(new(Math.Max(MinSize.Width, window.Width), Math.Max(MinSize.Height, window.Height)));
        }
    }
    internal override void SanitizeRatios(Size Available)
        => TopNonRoot.SanitizeRatios(MySize = Available);
    protected internal override Control _Build(bool refresh, bool relink, bool left, bool right, bool bottom) {
        SetColor();
        return MyBuild = TopNonRoot._Build(refresh, relink, left, right, bottom);
    }
    internal Control Build(bool refresh = false, bool relink = false) => _Build(refresh, relink, true, true, true);
    internal override void UnBuild(bool recursive = true) {
        MyBuild = null;
        if (recursive) TopNonRoot.UnBuild(recursive);
    }
    internal void SetColor() {
        TopNonRoot.PropagateColor(FromK(Original)?.GetTitleColor ?? DPro.TitleColor);
    }
    protected internal override void PropagateColor(Color c) { }
}