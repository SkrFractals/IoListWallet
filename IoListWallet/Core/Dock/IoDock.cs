using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;

namespace IoListWallet.Core.Dock;

internal abstract class IoDock {
    internal Control? MyBuild = null;
    internal IoDock? Parent = null; // Loading should fill it it as it goes
    internal Size MinSize = new(120, 120), MySize = new(120, 120);
    internal abstract void RefreshMinSize(bool shrink = false); // called from below to bubble up into the root (TODO When a panel changes min size, it should call this on it's dock)
    internal abstract void SanitizeRatios(Size Available); // adjust any ratios that are out of bounds (window should call this on resize)
    protected internal abstract Control _Build(bool refresh, bool relink, bool left, bool right, bool bottom); // TODO when undock removes anything, it should release from it's grid's Children! Also SetRow and column to 0,0!
    internal abstract void UnBuild(bool recursive = true);
    internal abstract protected void PropagateColor(Color c);
}