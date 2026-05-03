using Avalonia.Media;
using IoListWallet.Content.Abstract;
using static IoListWallet.Core.Static.AppSave;

namespace IoListWallet.IoPanels.Profile;
internal class FontControlSelect : FontSelect {
    protected override string EntryKey() => "T_ProfileContent.SelectControlFontEntry";
    protected override FontFamily? Selected(int fromList) 
        => new(GetProfile?.ControlFont ?? DPro.ControlFont);
    protected override void PerformSelect(int fromList, object? tag)
        => GetProfile?.ControlFont = ((FontFamily?)tag)?.Name ?? DPro.ControlFont;
   
}