using Avalonia.Media;
using IoListWallet.Core;
using IoListWallet.Core.Lists;
using IoListWallet.Core.Panels;
using System.Threading;
using static IoListWallet.Core.Static.AppSave;

namespace IoListWallet.IoPanels.Profile;
internal class FontControlSelect : IoListPanel {
    public FontControlSelect() : base() {
        (MyList[0] = new IoListButtons<FontFamily>(this, []) { MyIndex = 0 })
            .MyFilter.Filters = new IoFilter() { 
            Filters = [ // Search Formats:
                new FilterText() { Name = "Name" } // Font Name
            ], SortIndex = 0, SortIndex2 = 0, Increasing = true, Increasing2 = true
        };
        MyList[0].MyFilter.MaxSearch = 1;
        //MyContent.Content = MyList[0].Content;
    }
    internal override bool IsModal() => true;
    internal override string TrueParentType => MakeK(typeof(ProfileContent), "");
    protected override bool CloseOnClick(int fromList) => false;
    internal override void RefreshData(int job, CancellationToken? token) {
        if (MyList[0] is not IoListButtons<FontFamily> list)
            return;
        list.ItemSource = (System.Collections.Generic.IEnumerable<FontFamily>?)FontManager.Current.SystemFonts;
        list.ItemTemplate = (object? font) => [
            "T_ProfileContent.SelectControlFontEntry", ((FontFamily?)font)?.ToString() ?? "ERROR"
            ];
        list.FilterCommandTemplate = (font) => [   // Search Data:
            ((FontFamily?)font)?.ToString() ?? ""   // Font Name
            ];
        base.RefreshData(job, token);
    }
    protected override FontFamily? Selected(int fromList) => new(GetProfile?.ControlFont ?? DPro.ControlFont);
    protected override void PerformSelect(int fromList, object? tag)
        => ((ProfileContent?)TrueParent)?.SetControlFont(((FontFamily?)tag)?.Name, GetProfile?.ControlFontSize ?? DPro.ControlFontSize);
   
}