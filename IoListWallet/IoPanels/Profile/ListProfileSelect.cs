using IoListWallet.ElementControls;
using IoListWallet.MainControls.Panels;
using IoListWallet.IoPanels.Main;
using IoListWallet.Structs;
using System;
using System.Threading;
using static IoListWallet.App;
using static IoListWallet.AppSave;

namespace IoListWallet.IoPanels.Profile;
internal class ListProfileSelect : IoListPanel<int> {
    //static string F = "Failed ListProfileSelect.";
    public ListProfileSelect() : base() {
        MyList.MyFilter.Filters = new IoFilter() {
            Filters = [ // Search Formats:
                new FilterText() { Name = "Name" } // Name 
            ], SortIndex = 0, SortIndex2 = 0, Increasing = true, Increasing2 = true
        };
    }
    internal override string TrueParentType => MakeK(typeof(ListMainMenu), "");
    internal override void RefreshData(CancellationToken? token) {
        //((IoListButtons<int>)MyList).Columns = 2; // 0 = Select Network, 1 = Edit Network
        MyList.ItemSource = GetProfiles.Keys;
        MyList.ItemTemplate = (o) => [ // Columns:
            "T_SelectProfile", IsProfile((int?)o ?? -1, out var p) && p != null ? p.Name == "" ? L("NoName") : p.Name : "", // Select Profile
            //"T_EditProfile", L("EditProfile") // Edit Profile
            ];
        MyList.FilterCommandTemplate = (o) => IsProfile((int?)o ?? -1, out var p) && p != null ? [ // Search Data:
            L(p.Name) // Name
            ] : [];
        base.RefreshData(token);
    }
    protected override IoList<int> MakeList()
        => new IoListButtons<int>(this, [("T_AddProfile", L("AddProfile"))]);
    protected override Type? Selected() => GetProfile?.Network;
    protected override void PerformSelect(object? o) {
        LoadProfile((int?)o ?? -1);
        //RefreshPanel(this);
    }
    protected override void OnClickButton(object? sender, IoList<int>.ClickButtonEventArgs e) { 
        switch (e.Index) {
            case 0: // Select Profile
                base.OnClickButton(sender, e);
                break;
            /*case 1: // Edit Profile
                var b = e.ClickedButton ?? throw new(F + "OnClickButton");
                PerformSelect(b.Button.Tag);
                // TODO open profile edit
                break;*/
        }
    }
    protected override void OnClickTool(object? s, IoList<int>.ClickToolEventArgs e) {
        switch (e.Index) {
            case 0: // Add Profile
                ushort profileId = 0;
                while (SApp.Profiles.ContainsKey(profileId))
                    ++profileId;
                LoadProfile(profileId);
                //var main = RefreshPanel((typeof(ListMainMenu), ""));
                OpenRefreshPanel(AppMainMenu, typeof(BookProfile));
                break;
        }
    }
}