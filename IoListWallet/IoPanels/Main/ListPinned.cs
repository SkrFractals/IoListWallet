using IoListWallet.ElementControls;
using IoListWallet.MainControls;
using IoListWallet.MainControls.Panels;
using IoListWallet.Structs;
using IoListWallet.Structs.Crypto;
using System;
using System.Threading;
using static IoListWallet.App;
using static IoListWallet.AppSave;

namespace IoListWallet.IoPanels.Main;
internal class ListPinned : IoListPanel<IoPanel> {
    public ListPinned() : base() { }
    internal override bool IsModal() => true;
    internal override void RefreshData(CancellationToken? token) {
        if (GetProfile is not IoProfile profile)
            return;
        ((IoListButtons<IoPanel>)MyList).Columns = 2; // 0 = Select Network, 1 = Edit Network
        System.Collections.Generic.List<IoPanel> Panels = [];
        foreach (var p in profile.Pan.PinnedPanels) { Panels.Add(GetPanel(p)!); }
        MyList.ItemSource = Panels;
        MyList.ItemTemplate = (panel) => [ // Columns:
                "T_OpenPinned", panel is IoPanel p ? p.Key == "" ? L(p.GetType().Name) : L(p.GetType().Name) + " - " + p.Key : "", // Goto Panel
                "T_UnpinPinned", L("UnpinPinned") // Unpin
                ];
        MyList.FilterCommandTemplate = (panel) => panel is IoPanel p ? [L(p.GetType().Name), L(p.Key)] : [];
        MyList.MyFilter.Filters = new IoFilter() {
            Filters = [ // Search Formats:
                new FilterText() { Name = "Panel" }, // Panel Type 
                new FilterText() { Name = "Key" }, // Instance Key 
            ], SortIndex = 0, SortIndex2 = 0, Increasing = true, Increasing2 = true
        };
        base.RefreshData(token);
    }
    protected override IoList<IoPanel> MakeList() 
        => new IoListButtons<IoPanel>(this, []);
    protected override object? Selected() => null;
    protected override void PerformSelect(object? o) {
        Close();
        var To = FromK(((Type, string)?)o);
        if (ParentPanel != null && To != null)
            OpenPanel(FromK(ParentPanel) ?? AppMainMenu, To.GetType(), To.Key);
    }
    protected override void OnClickButton(object? s, IoList<IoPanel>.ClickButtonEventArgs e) {
        if (GetProfile is not IoProfile profile)
            return;
        switch (e.Index) {
            case 0: // Select Pinned
                base.OnClickButton(s, e);
                break;
            case 1: // Unpin
                var b = e.ClickedButton ?? throw new("Failed LastPinned.OnClickButton");
                var To = (IoPanel?)b.Button.Tag;
                if (To != null) {
                    IoDock? d = To.ParentDock;
                    while (d?.Parent != null)
                        d = d.Parent;
                    if (d is IoDockRoot root) 
                        root.Panel.Update();
                    profile.Pan.PinnedPanels.Remove(To.GetK);
                    profile.Pan.Dirty = true;
                }
                RefreshPanel(this);
                break;
        }
    }
}