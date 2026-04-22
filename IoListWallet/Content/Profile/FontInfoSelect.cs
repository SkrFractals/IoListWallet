using Avalonia.Media;
using static IoListWallet.Core.Static.AppSave;

namespace IoListWallet.IoPanels.Profile;
internal class FontInfoSelect : FontSelect {
    protected override string EntryKey() => "T_ProfileContent.SelectInfoFontEntry";
    protected override FontFamily? Selected(int fromList) 
        => new(GetProfile?.InfoFont ?? DPro.InfoFont);
    protected override void PerformSelect(int FromList, object? tag) 
        => GetProfile?.InfoFont = ((FontFamily?)tag)?.Name ?? DPro.InfoFont;
}