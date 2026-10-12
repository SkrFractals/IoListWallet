using IoListWallet.Content.Abstract;
using IoListWallet.Core;
using IoListWallet.Core.Basic;
using IoListWallet.Core.Lists;
using IoListWallet.Structs.Crypto;
using System.Threading;
using static IoListWallet.App;
using static IoListWallet.Core.Static.AppSave;

namespace IoListWallet.Content.Main;
internal class PinnedPanel : IoListPanel {
    internal override bool IsModal() => true;
    public PinnedPanel() : base()
        => MakeList<IoPanel>(new IoFilter() {
            Filters = [ // Search Formats:
                new FilterText() { Name = "Name" }, // User-made Nickname 
                new FilterText() { Name = "Panel" }, // Panel Type 
                new FilterText() { Name = "Key" } // Instance Key 
               
            ], SortIndex = 0, SortIndex2 = 0, Increasing = true, Increasing2 = true
        }, new Gen_Buttons(2), []);
    //internal override bool IsModal() => true;
    internal override void RefreshData(int job, CancellationToken? token) {
        if (GetProfile is not IoProfile profile)
            return;
        // Fetch PinnedPanels list:
        System.Collections.Generic.List<IoPanel> Panels = [];
        foreach (var p in profile.Pan.PinnedPanels) { Panels.Add(GetPanel(p)!); }

        MyList[0].RefreshTemplate(Panels, (panel) => {
            var p = panel as IoPanel;
            var k = p != null ? p.Key == "" ? L(p.GetType().Name) : L(p.GetType().Name) + " - " + p.Key : "";
            return new Gen_Buttons.GenData([ // Columns:
                "T_OpenPinned", p != null && p.UserTitle != "" ? p.UserTitle + "\n" + k : k, // Goto Panel
                "T_UnpinPinned", L("UnpinPinned") // Unpin
                ]);
        }, (panel) => panel is IoPanel p ? [p.UserTitle, L(p.GetType().Name), L(p.Key)] : []);
        base.RefreshData(job, token);
    }
    protected override bool CloseOnClick(int fromList) => true;
    protected override object? Selected(int fromList) => null;
    protected override void PerformSelect(int fromList, object? o) {
        Close();
        var To = (IoPanel?)o;
        if (ParentPanel != null && To != null)
            _ = OpenPanel(FromK(ParentPanel) ?? AppMainMenu, To.GetType(), To.Key);
    }
    protected override void OnClickRow(object? s, IoListBase.ClickRowEventArgs e) {
        if (GetProfile is not IoProfile profile)
            return;
        switch (e.Index) {
            case 0: // Select Pinned
                base.OnClickRow(s, e);
                break;
            case 1: // Unpin
                var b = e.ClickedRow as WToggleButton ?? throw new("Failed LastPinned.OnClickButton");
                var To = (IoPanel?)b.Button.Tag;
                if (To != null) {
                    profile.Pan.PinnedPanels.Remove(To.GetK);
                    To.Refresh(0); // this ensures the pin icon updates on the actual panel
                    profile.Pan.Dirty = true;
                }
                _ = RefreshPanel(this, 1, true);
                break;
        }
    }
}