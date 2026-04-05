using Avalonia.Media;
using IoListWallet.ElementControls;
using IoListWallet.MainControls.Panels;
using IoListWallet.Structs;
using System.Threading;
using static IoListWallet.AppSave;

namespace IoListWallet.IoPanels.Profile;
internal class ListFontControl : IoListPanel<FontFamily> {
    public ListFontControl() : base() {
        MyList.MyFilter.Filters = new IoFilter() { 
            Filters = [ // Search Formats:
                new FilterText() { Name = "Name" } // Font Name
            ], SortIndex = 0, SortIndex2 = 0, Increasing = true, Increasing2 = true
        };
    }
    internal override bool IsModal() => true;
    internal override string TrueParentType => MakeK(typeof(BookProfile), "");
    protected override bool CloseOnClick() => false;
    internal override void RefreshData(CancellationToken? token) {
        MyList.ItemSource = (System.Collections.Generic.IEnumerable<FontFamily>?)FontManager.Current.SystemFonts;
        MyList.ItemTemplate = (object? font) => [
            "T_SettingsApp.SelectControlFontEntry", ((FontFamily?)font)?.ToString() ?? "ERROR"
            ]; 
        MyList.FilterCommandTemplate = (font) => [   // Search Data:
            ((FontFamily?)font)?.ToString() ?? ""   // Font Name
            ];
        base.RefreshData(token);
    }
    protected override IoList<FontFamily> MakeList() 
        => new IoListButtons<FontFamily>(this, []);
    protected override FontFamily? Selected() => new(GetProfile?.ControlFont ?? DPro.ControlFont);
    protected override void PerformSelect(object? tag)
        => ((BookProfile?)TrueParent)?.SetControlFont(((FontFamily?)tag)?.Name, GetProfile?.ControlFontSize ?? DPro.ControlFontSize);
   
}